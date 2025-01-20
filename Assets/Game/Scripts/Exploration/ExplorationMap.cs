using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.AddressableAssets;
using MessagePack;

// 32*32のマップを表すクラス
public class ExplorationMap
{
    // セットアップパラメータクラス
    public struct SetupParam
    {
        public int dungeonId;
        public string dugeonName;
    }

    public struct FindResult
    {
        public bool success;
        public Vector2Int position;
        public int distance;
    }

    // マスの配列
    public Mass[,] masses { get; private set; }

    // マップのマス数
    public int mapSizeX { get; private set; } = MapDef.MassMaxX;
    public int mapSizeY { get; private set; } = MapDef.MassMaxY;
    public Vector2Int mapSize { get; private set; } = new Vector2Int(MapDef.MassMaxX, MapDef.MassMaxY);
    // 開始マス
    public Vector2Int startPosition { get; private set; } = new Vector2Int(0, 0);

    // アイテムがあるマスのリスト
    public List<Mass> itemMasses { get { return _itemMasses; } }
    // トラップがあるマスのリスト
    public List<Mass> trapMasses { get { return _trapMasses; } }
    // ボスがいるマス
    public Mass bossMass { get { return _bossMass; } }
    // シンボルユニットのリスト
    public List<IUnit> unitSymbols { get { return _unitSymbols; } }

    // マップデータ
    private MapData _mapData;

    // コンストラクタ
    public ExplorationMap()
    {
        masses = new Mass[MapDef.MassMaxX, MapDef.MassMaxY];
        for (int x = 0; x < MapDef.MassMaxX; x++)
        {
            for (int y = 0; y < MapDef.MassMaxY; y++)
            {
                masses[x, y] = new Mass(x, y); // 初期状態では全てのマスが存在しない
            }
        }
    }

    // セットアップ
    public void Setup(SetupParam param)
    {
        Debug.Log($"Map Setup {param.dugeonName}");
        _mapData = GameManager.Instance.GetMapData(param.dugeonName);
        for (int y = 0; y < MapDef.MassMaxY; y++)
        {
            for (int x = 0; x < MapDef.MassMaxX; x++)
            {
                Debug.Log($"Mass Setup: ({x}, {y})");
                masses[x, y].Setup(_mapData.GetMass(new Vector2Int(x, y)));
            }
        }

        // マスのエラーチェック
        for (int x = 0; x < MapDef.MassMaxX; x++)
        {
            for (int y = 0; y < MapDef.MassMaxY; y++)
            {
                var error = CheckMassError(x, y);
                if (error != MassError.None)
                {
                    Debug.LogError($"Mass Error: ({x}, {y}) : " + error.ToString());
                }
            }
        }

        // 壁の設定
        for (int x = 0; x < MapDef.MassMaxX; x++)
        {
            for (int y = 0; y < MapDef.MassMaxY; y++)
            {
                Vector2Int mapIndex = new Vector2Int(x, y);
                if (TryGetMassIfExist(mapIndex, out var mass))
                {
                    var upIndex = MapUtil.GetWallIndex(mapIndex, MapDef.Direction.Up);
                    if (_mapData.GetHorizontalWall(upIndex))
                    {
                        mass.SetWall(MapDef.Direction.Up);
                    }
                    var downIndex = MapUtil.GetWallIndex(mapIndex, MapDef.Direction.Down);
                    if (_mapData.GetHorizontalWall(downIndex))
                    {
                        mass.SetWall(MapDef.Direction.Down);
                    }
                    var leftIndex = MapUtil.GetWallIndex(mapIndex, MapDef.Direction.Left);
                    if (_mapData.GetVerticalWall(leftIndex))
                    {
                        mass.SetWall(MapDef.Direction.Left);
                    }
                    var rightIndex = MapUtil.GetWallIndex(mapIndex, MapDef.Direction.Right);
                    if (_mapData.GetVerticalWall(rightIndex))
                    {
                        mass.SetWall(MapDef.Direction.Right);
                    }
                }
            }
        }

        startPosition = _mapData.startMass;
        SetReachableAllMass(startPosition);
    }

    // マップインデックスでマスの存在を取得するメソッド
    public bool GetMassExists(Vector2Int mapIndex)
    {
        if (TryGetMass(mapIndex, out Mass mass))
        {
            return mass.mass.exist;
        }
        return false;
    }

    // マップインデックスでマスを取得するメソッド
    public Mass GetMass(Vector2Int mapIndex)
    {
        if (MapData.IsValidMapIndex(mapIndex))
        {
            return masses[mapIndex.x, mapIndex.y];
        }
        return null;
    }

    // マップインデックスでマスの取得を試みる
    public bool TryGetMass(Vector2Int mapIndex, out Mass mass)
    {
        mass = GetMass(mapIndex);
        return mass != null;
    }

    // マップインデックスでマスが存在するときだけ取得を試みる
    public bool TryGetMassIfExist(Vector2Int mapIndex, out Mass mass)
    {
        if (TryGetMass(mapIndex, out mass))
        {
            return mass.mass.exist;
        }
        return false;
    }

    // マップインデックスとMapDef.Directionで隣接するマスに移動できるかを判定するメソッド
    public bool CanMove(Vector2Int mapIndex, MapDef.Direction direction)
    {
        Vector2Int nextMapIndex = mapIndex + MapUtil.DirectionToVector2Int(direction);
        // 移動先がマップ外のときは移動できない
        if (nextMapIndex.x < 0 || nextMapIndex.x >= MapDef.MassMaxX || nextMapIndex.y < 0 || nextMapIndex.y >= MapDef.MassMaxY)
        {
            return false;
        }
        // 移動先が存在しないときは移動できない
        if (!masses[nextMapIndex.x, nextMapIndex.y].mass.exist)
        {
            return false;
        }
        // 移動先に壁があるときは移動できない
        var checkWaallBit = MapUtil.DirectionToWallBit(direction);
        if ((masses[mapIndex.x, mapIndex.y].wall & checkWaallBit) != 0)
        {
            return false;
        }

        return true;
    }

    // マスに移動できるかを判定して、移動できるならマスを取得するメソッド
    public Mass GetMovableMass(Vector2Int mapIndex, MapDef.Direction direction)
    {
        if (CanMove(mapIndex, direction))
        {
            return GetMass(mapIndex + MapUtil.DirectionToVector2Int(direction));
        }
        return null;
    }

    // 水平壁があるかを判定するメソッド
    public bool IsHorizontalWall(Vector2Int mapIndex, MapDef.Direction direction)
    {
        // 水平壁は上下のみ
        if (direction == MapDef.Direction.Up || direction == MapDef.Direction.Down)
        {
            return _mapData.GetHorizontalWall(MapUtil.GetWallIndex(mapIndex, direction));
        }
        return false;
    }

    // 垂直壁があるかを判定するメソッド
    public bool IsVerticalWall(Vector2Int mapIndex, MapDef.Direction direction)
    {
        // 垂直壁は左右のみ
        if (direction == MapDef.Direction.Left || direction == MapDef.Direction.Right)
        {
            return _mapData.GetVerticalWall(MapUtil.GetWallIndex(mapIndex, direction));
        }
        return false;
    }

    /// <summary>
    /// 指定座標から一番近い未探索マスを取得するメソッド
    /// </summary>
    /// <param name="mapIndex">指定マップインデックス</param>
    /// <param name="includeHidden">隠されているマスも含めて探索するかのフラグ</param>
    /// <returns></returns>
    public FindResult FindNearestUnexploredMass(Vector2Int mapIndex, bool includeHidden)
    {
        FindResult findResult = new FindResult();
        findResult.distance = int.MaxValue;

        int beginX = mapIndex.x - 1;
        int endX = mapIndex.x + 1;
        int beginY = mapIndex.y - 1;
        int endY = mapIndex.y + 1;

        while (findResult.success == false)
        {
            for (int x = beginX; x <= endX; x++)
            {
                for (int y = beginY; y <= endY; y++)
                {
                    // 存在しないマス
                    if (!masses[x, y].mass.exist)
                    {
                        continue;
                    }
                    // 到達できないマス
                    if (!masses[x, y].reachable)
                    {
                        continue;
                    }
                    // 探索済みマス
                    if (masses[x, y].explored)
                    {
                        continue;
                    }
                    // 隠れているマス
                    if (!includeHidden && masses[x, y].hidden)
                    {
                        continue;
                    }
                    int distance = Mathf.Abs(mapIndex.x - x) + Mathf.Abs(mapIndex.y - y);
                    if (distance < findResult.distance)
                    {
                        findResult.position.Set(x, y);
                        findResult.success = true;
                        findResult.distance = distance;
                        break;
                    }
                }
            }
            beginX--;
            endX++;
            beginY--;
            endY++;

            if (beginX < 0) beginX = 0;
            if (endX >= MapDef.MassMaxX) endX = MapDef.MassMaxX - 1;
            if (beginY < 0) beginY = 0;
            if (endY >= MapDef.MassMaxY) endY = MapDef.MassMaxY - 1;
        }

        return findResult;
    }

    // マスに到達できるかを設定するメソッド
    private void SetReachableAllMass(Vector2Int startMapIndex)
    {
        Queue<Mass> queue = new Queue<Mass>();
        var startMass = GetMass(startMapIndex);
        if (!startMass.mass.exist)
        {
            return;
        }
        queue.Enqueue(startMass);

        while (queue.Count > 0)
        {
            var mass = queue.Dequeue();
            mass.reachable = true;

            // 4方向に対して移動できるならキューに追加
            foreach (var direction in MapDef.Directions)
            {
                var nextMass = GetMovableMass(mass.mass.mapIndex, direction);
                if (nextMass != null)
                {
                    // まだ到達していないマスならキューに追加
                    if (!nextMass.reachable)
                    {
                        if (queue.Contains(nextMass) == false)
                        {
                            queue.Enqueue(nextMass);
                        }
                    }
                }
            }
        }
    }

    enum MassError
    {
        None,
        NoMassAround,
        ItemAndTrap
    }

    // マスのエラーチェックを行うメソッド
    private MassError CheckMassError(int x, int y)
    {
        // マスが存在しないときは何もしない
        if (!masses[x, y].mass.exist)
        {
            return MassError.None;
        }

        // 上下左右のどこかにもマスが存在しないときはエラー
        if (!CheckMassExists(x, y))
        {
            return MassError.NoMassAround;
        }

        return MassError.None;
    }

    // 上下左右のどこかにマスが存在するかを判定するメソッド
    private bool CheckMassExists(int x, int y)
    {
        if (x > 0 && masses[x - 1, y].mass.exist)
        {
            return true;
        }
        if (x < 31 && masses[x + 1, y].mass.exist)
        {
            return true;
        }
        if (y > 0 && masses[x, y - 1].mass.exist)
        {
            return true;
        }
        if (y < 31 && masses[x, y + 1].mass.exist)
        {
            return true;
        }
        return false;
    }

    // アイテムがあるマスのリスト
    private List<Mass> _itemMasses = new List<Mass>();
    // トラップがあるマスのリスト
    private List<Mass> _trapMasses = new List<Mass>();
    // ボスがいるマス
    private Mass _bossMass = null;
    // シンボルユニットのリスト
    private List<IUnit> _unitSymbols = new List<IUnit>();
}
