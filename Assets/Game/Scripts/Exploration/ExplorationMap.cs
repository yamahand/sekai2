using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.AddressableAssets;
using MessagePack;

namespace Game.Scripts.Exploration
{
    // 32*32のマップを表すクラス
    public class Map
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

        public Mass[,] masses { get; private set; }
        private MapData _mapData;

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

        // コンストラクタ
        public Map()
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
            var mapData = GameManager.Instance.GetMapData(param.dugeonName);
            for (int y = 0; y < MapDef.MassMaxY; y++)
            {
                for (int x = 0; x < MapDef.MassMaxX; x++)
                {
                    Debug.Log($"Mass Setup: ({x}, {y})");
                    masses[x, y].Setup(mapData.GetMass(new Vector2Int(x, y)));
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
            startPosition = mapData.startMass;
            SetReachableAllMass(startPosition);
        }

        // マスの存在を取得するメソッド
        public bool GetMassExists(int x, int y)
        {
            // 範囲チェック
            if (x < 0 || x >= MapDef.MassMaxX || y < 0 || y >= MapDef.MassMaxY)
            {
                return false;
            }

            return masses[x, y].mass.exist;
        }

        // マスを取得するメソッド
        public Mass GetMass(int x, int y)
        {
            return masses[x, y];
        }

        // マップインデックスでマスの存在を取得するメソッド
        public bool GetMassExists(Vector2Int mapIndex)
        {
            return masses[mapIndex.x, mapIndex.y].mass.exist;
        }

        // マップインデックスでマスを取得するメソッド
        public Mass GetMass(Vector2Int mapIndex)
        {
            return masses[mapIndex.x, mapIndex.y];
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
            Queue<Game.Scripts.Exploration.Mass> queue = new Queue<Game.Scripts.Exploration.Mass>();
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
}

