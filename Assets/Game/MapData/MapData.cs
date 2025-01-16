using UnityEngine;
using MessagePack;
using System.Collections.Generic;

[MessagePackObject(AllowPrivate = true)]
public partial class MapData
{
    [Key(0)]
    public string mapName { get; set; }
    [Key(1)]
    public Mass[,] masses { get; set; }

    // 開始マスのマップインデックス
    [Key(2)]
    public Vector2Int startMass { get; set; } = DefaultMapIndex;

    // ボスマスのマップインデックス
    [Key(3)]
    public Vector2Int bossMass { get; set; } = DefaultMapIndex;

    // 水平壁が存在するかどうか
    [Key(4)]
    public bool[,] horizontalWalls { get; set; }

    // 垂直壁が存在するかどうか
    [Key(5)]
    public bool[,] verticalWalls { get; set; }

    // シンボルエネミーの情報
    [Key(6)]
    public List<SymbolEnemy> symbolEnemies { get; set; } = new List<SymbolEnemy>();

    // マスの行列数
    public static readonly Vector2Int MassCount = new Vector2Int(16, 16);
    // 水平壁の行列数
    public static readonly Vector2Int HorizontalWallCount = new Vector2Int(16, 17);
    // 垂直壁の行列数
    public static readonly Vector2Int VerticalWallCount = new Vector2Int(17, 16);
    // マップインデックスの初期値
    public static readonly Vector2Int DefaultMapIndex = new Vector2Int(-1, -1);
    // マップデータの保存先
    public static readonly string mapDataPath = "Game/Data/Map/";

    // コンストラクタ
    public MapData()
    {
        Reset();
    }

    /// <summary>
    /// リセット
    /// </summary>
    public void Reset()
    {
        mapName = "";
        masses = new Mass[MassCount.y, MassCount.x];
        for (int y = 0; y < MassCount.y; y++)
        {
            for (int x = 0; x < MassCount.x; x++)
            {
                masses[y, x] = new Mass();
            }
        }
        startMass = DefaultMapIndex;
        bossMass = DefaultMapIndex;
        horizontalWalls = new bool[HorizontalWallCount.y, HorizontalWallCount.x];
        verticalWalls = new bool[VerticalWallCount.y, VerticalWallCount.x];
        symbolEnemies.Clear();
    }

    // Vector2Intでマスのインデックスを指定してマスを取得
    public Mass GetMass(Vector2Int index)
    {
        if (masses != null && IsValidIndex(index, MassCount))
        {
            return masses[index.y, index.x];
        }
        return null;
    }

    // Vector2Intでマスのインデックスを指定してマスを設定
    public void SetMass(Vector2Int index, Mass mass)
    {
        if (masses != null && IsValidIndex(index, MassCount))
        {
            masses[index.y, index.x] = mass;
        }
    }

    // Vector2Intでマスのインデックスを指定してマスの取得を試みる
    public bool TryGetMass(Vector2Int index, out Mass mass)
    {
        mass = GetMass(index);
        return mass != null;
    }

    // Vector2Intで水平壁のインデックスを指定して水平壁の存在を取得
    public bool GetHorizontalWall(Vector2Int index)
    {
        if (horizontalWalls != null && IsValidIndex(index, HorizontalWallCount))
        {
            return horizontalWalls[index.y, index.x];
        }
        return false;
    }

    // Vector2Intで水平壁のインデックスを指定して水平壁の存在を設定
    public void SetHorizontalWall(Vector2Int index, bool isExist)
    {
        if (horizontalWalls != null && IsValidIndex(index, HorizontalWallCount))
        {
            horizontalWalls[index.y, index.x] = isExist;
        }
    }

    // Vector2Intで垂直壁のインデックスを指定して垂直壁の存在を取得
    public bool GetVerticalWall(Vector2Int index)
    {
        if (verticalWalls != null && IsValidIndex(index, VerticalWallCount))
        {
            return verticalWalls[index.y, index.x];
        }
        return false;
    }

    // Vector2Intで垂直壁のインデックスを指定して垂直壁の存在を設定
    public void SetVerticalWall(Vector2Int index, bool isExist)
    {
        if (verticalWalls != null && IsValidIndex(index, VerticalWallCount))
        {
            verticalWalls[index.y, index.x] = isExist;
        }
    }

    // マップインデックスが有効かどうかをチェック
    public static bool IsValidMapIndex(Vector2Int index)
    {
        return IsValidIndex(index, MassCount);
    }

    // 水平壁のインデックスが有効かどうかをチェック
    public static bool IsValidHorizontalWallIndex(Vector2Int index)
    {
        return IsValidIndex(index, HorizontalWallCount);
    }

    // 垂直壁のインデックスが有効かどうかをチェック
    public static bool IsValidVerticalWallIndex(Vector2Int index)
    {
        return IsValidIndex(index, VerticalWallCount);
    }


    // インデックスが有効かどうかをチェック
    private static bool IsValidIndex(Vector2Int index, Vector2Int maxIndex)
    {
        return index.x >= 0 && index.x < maxIndex.x && index.y >= 0 && index.y < maxIndex.y;
    }
}

