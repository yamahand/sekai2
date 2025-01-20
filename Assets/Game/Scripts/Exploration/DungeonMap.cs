using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public class DungeonMap : MonoBehaviour
{
    public struct Desc
    {
        public int layer;
        public int index;
    }

    public RenderTexture renderTexture { get { return _renderTexture; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var unit in _units)
        {
            unit.UpdateUnit(Time.deltaTime, _map);
        }
    }

    public async UniTask Setup(Desc desc)
    {
        Debug.Log("DungeonMap.Setup");
        gameObject.layer = desc.layer;
        // レンダーターゲットテクスチャを作成する
        _renderTexture = new RenderTexture(512, 512, 24);
        _renderTexture.name = "DungeonMap_" + desc.index.ToString();
        _camera.gameObject.name = "DungeonMapCamera_" + desc.index.ToString();
        _camera.cullingMask = 1 << gameObject.layer;
        _camera.targetTexture = _renderTexture;

        // マップデータを取得して、セットアップする
        ExplorationMap.SetupParam setupParam = new ExplorationMap.SetupParam();
        // desc.indexをtest_map01のような文字列に変換して、マップ名に設定する
        setupParam.dugeonName = "test_map" + (desc.index + 1).ToString("D2");
        _map.Setup(setupParam);

        var taskMap = _CreateMap();
        var taskAdventure = _CreateUnitAdventure();

        await UniTask.WhenAll(taskMap, taskAdventure);
    }

    private async UniTask _CreateMap()
    {
        // Addressablesからマップチップをロードする
        var mapChip64 = await Addressables.LoadAssetAsync<GameObject>("DungeonMaps/MapChip64.prefab").Task;
        if (mapChip64 == null)
        {
            Debug.LogError("Failed to load MapChip64 prefab.");
            return;
        }
        mapChip64.layer = gameObject.layer;

        // Addressablesからwallchip64をロードする
        var wallChip64 = await Addressables.LoadAssetAsync<GameObject>("DungeonMaps/WallChip64.prefab").Task;
        if (wallChip64 == null)
        {
            Debug.LogError("Failed to load WallChip64 prefab.");
            return;
        }
        wallChip64.layer = gameObject.layer;

        // 水平壁と垂直壁のオブジェクトを保持しておく配列
        GameObject[,] horizontalWalls = new GameObject[MapData.HorizontalWallCount.x, MapData.HorizontalWallCount.y];
        GameObject[,] verticalWalls = new GameObject[MapData.VerticalWallCount.x, MapData.VerticalWallCount.y];

        // 水平壁のオブジェクトを取得する
        GameObject GetHorizontalWallObject(Vector2Int index)
        {
            if (horizontalWalls.IsValidIndex(index.x, index.y))
            {
                if (horizontalWalls[index.x, index.y] == null)
                {
                    horizontalWalls[index.x, index.y] = Instantiate(wallChip64, _mapChipObject.transform);
                    horizontalWalls[index.x, index.y].transform.rotation = Quaternion.Euler(0, 0, 90);
                }

                return horizontalWalls[index.x, index.y];
            }
            return null;
        }

        // 垂直壁のオブジェクトを取得する
        GameObject GetVerticalWallObject(Vector2Int index)
        {
            if (verticalWalls.IsValidIndex(index.x, index.y))
            {
                if (verticalWalls[index.x, index.y] == null)
                {
                    verticalWalls[index.x, index.y] = Instantiate(wallChip64, _mapChipObject.transform);
                }
                return verticalWalls[index.x, index.y];
            }
            return null;
        }

        // マスの存在を取得して、マスのオブジェクトを生成する
        float massBeginPosX = -MapDef.MapWidth / 2 + MapDef.MassSize / 2;   // Xは左端から右端へ
        float massBeginPosY = MapDef.MapHeight / 2 - MapDef.MassSize / 2;   // Yは上端から下端へ
        float massPosX = massBeginPosX;
        for (int x = 0; x < MapDef.MassMaxX; x++)
        {
            float massPosY = massBeginPosY;
            for (int y = 0; y < MapDef.MassMaxY; y++)
            {
                var mapIndex = new Vector2Int(x, y);
                if (_map.GetMassExists(mapIndex))
                {
                    GameObject massObject = Instantiate(mapChip64, _mapChipObject.transform);
                    massObject.name = $"Mass_{x}_{y}";
                    massObject.transform.localPosition = new Vector3(massPosX, massPosY, 0);
                    var mass = _map.GetMass(mapIndex);
                    mass.SetGameObject(massObject);

                    if(_map.IsHorizontalWall(mapIndex, MapDef.Direction.Up))
                    {
                        var wallIndex = MapUtil.GetWallIndex(mapIndex, MapDef.Direction.Up);
                        // 水平壁を生成する
                        GameObject wallObject = GetHorizontalWallObject(wallIndex);
                        wallObject.name = $"H_Wall_{wallIndex.x}_{wallIndex.y}";
                        wallObject.transform.localPosition = new Vector3(massPosX, massPosY + MapDef.MassSize / 2, 0);
                        horizontalWalls.SetT(wallIndex, wallObject);
                        mass.SetWall(MapDef.Direction.Up, wallObject);
                    }

                    if (_map.IsHorizontalWall(mapIndex, MapDef.Direction.Down))
                    {
                        var wallIndex = MapUtil.GetWallIndex(mapIndex, MapDef.Direction.Down);
                        // 水平壁を生成する
                        GameObject wallObject = GetHorizontalWallObject(wallIndex);
                        wallObject.name = $"H_Wall_{wallIndex.x}_{wallIndex.y}";
                        wallObject.transform.localPosition = new Vector3(massPosX, massPosY - MapDef.MassSize / 2, 0);
                        horizontalWalls.SetT(wallIndex, wallObject);
                        mass.SetWall(MapDef.Direction.Down, wallObject);
                    }

                    if (_map.IsVerticalWall(mapIndex, MapDef.Direction.Left))
                    {
                        var wallIndex = MapUtil.GetWallIndex(mapIndex, MapDef.Direction.Left);
                        // 垂直壁を生成する
                        GameObject wallObject = GetVerticalWallObject(wallIndex);
                        wallObject.name = $"V_Wall_{wallIndex.x}_{wallIndex.y}";
                        wallObject.transform.localPosition = new Vector3(massPosX - MapDef.MassSize / 2, massPosY, 0);
                        verticalWalls.SetT(wallIndex, wallObject);
                        mass.SetWall(MapDef.Direction.Left, wallObject);
                    }

                    if (_map.IsVerticalWall(mapIndex, MapDef.Direction.Right))
                    {
                        var wallIndex = MapUtil.GetWallIndex(mapIndex, MapDef.Direction.Right);
                        // 垂直壁を生成する
                        GameObject wallObject = GetVerticalWallObject(wallIndex);
                        wallObject.name = $"V_Wall_{wallIndex.x}_{wallIndex.y}";
                        wallObject.transform.localPosition = new Vector3(massPosX + MapDef.MassSize / 2, massPosY, 0);
                        verticalWalls.SetT(wallIndex, wallObject);
                        mass.SetWall(MapDef.Direction.Right, wallObject);
                    }
                }
                massPosY -= MapDef.MassSize;
            }
            massPosX += MapDef.MassSize;
        }
    }

    // UnitAdventureを生成する
    async UniTask _CreateUnitAdventure()
    {
        // ユニットのオブジェクトを生成する
        var op = await Addressables.LoadAssetAsync<GameObject>("DungeonMaps/UnitAdventure.prefab").Task;
        if (op == null)
        {
            Debug.LogError("Failed to load UnitAdventure prefab.");
            return;
        }

        GameObject unitObject = Instantiate(op, transform);
        unitObject.layer = gameObject.layer;
        unitObject.name = "UnitAdventure";
        unitObject.transform.localPosition = new Vector3(0, 0, 0);
        IUnit unit = unitObject.AddComponent<UnitAdventurer>();
        _units.Add(unit);
        unit.Setup(UnitType.Adventurer, "冒険者", new Vector2Int(0, 0), _map);
        _unitAdventure = unitObject;
    }

    // カメラコンポーネント
    [SerializeField] Camera _camera = null;
    // レンダーテクスチャ
    RenderTexture _renderTexture = null;

    // マップチップをぶら下げる親オブジェクト
    [SerializeField]GameObject _mapChipObject = null;

    ExplorationMap _map = new ExplorationMap();
    GameObject _unitAdventure = null;

    // ユニット
    private List<IUnit> _units = new List<IUnit>();
}
