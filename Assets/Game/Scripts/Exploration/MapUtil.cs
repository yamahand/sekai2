using UnityEngine;

public class MapUtil
{
    // 方向をVector2Intに変換するメソッド
    public static Vector2Int DirectionToVector2Int(MapDef.Direction direction)
    {
        switch (direction)
        {
            case MapDef.Direction.Up:
                return new Vector2Int(0, -1);
            case MapDef.Direction.Down:
                return new Vector2Int(0, 1);
            case MapDef.Direction.Left:
                return new Vector2Int(-1, 0);
            case MapDef.Direction.Right:
                return new Vector2Int(1, 0);
        }
        return Vector2Int.zero;
    }

    // Vector2Intを方向に変換するメソッド
    public static MapDef.Direction Vector2IntToDirection(Vector2Int vector)
    {
        if (vector == new Vector2Int(0, 1))
        {
            return MapDef.Direction.Up;
        }
        if (vector == new Vector2Int(0, -1))
        {
            return MapDef.Direction.Down;
        }
        if (vector == new Vector2Int(-1, 0))
        {
            return MapDef.Direction.Left;
        }
        if (vector == new Vector2Int(1, 0))
        {
            return MapDef.Direction.Right;
        }
        return MapDef.Direction.Up;
    }

    // 方向から壁のビットフラグを取得するメソッド
    public static MapDef.Wall DirectionToWallBit(MapDef.Direction direction)
    {
        return (MapDef.Wall)(1 << (int)direction);
    }

    // マップインデックスからワールド座標を取得するメソッド
    public static Vector3 MapIndexToWorldPosition(Vector2Int mapIndex)
    {
        float x = -MapDef.MapWidth / 2 + MapDef.MassSize / 2 + mapIndex.x * MapDef.MassSize;
        float y = MapDef.MapHeight / 2 - MapDef.MassSize / 2 - mapIndex.y * MapDef.MassSize;
        return new Vector3(x, y, 0);
    }

    // ワールド座標からマップインデックスを取得するメソッド
    public static Vector2Int WorldPositionToMapIndex(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x + MapDef.MapWidth / 2 - MapDef.MassSize / 2) / MapDef.MassSize);
        int y = Mathf.FloorToInt((-worldPosition.y + MapDef.MapHeight / 2 - MapDef.MassSize / 2) / MapDef.MassSize);
        return new Vector2Int(x, y);
    }

    // マップインデックスと方向から壁のインデックスを取得する
    static public Vector2Int GetWallIndex(Vector2Int mapIndex, MapDef.Direction direction)
    {
        // 方向によって壁のインデックスを取得
        switch (direction)
        {
            case MapDef.Direction.Up:
                return new Vector2Int(mapIndex.x, mapIndex.y);
            case MapDef.Direction.Down:
                return new Vector2Int(mapIndex.x, mapIndex.y + 1);
            case MapDef.Direction.Left:
                return new Vector2Int(mapIndex.x, mapIndex.y);
            case MapDef.Direction.Right:
                return new Vector2Int(mapIndex.x + 1, mapIndex.y);
        }
        return new Vector2Int(-1, -1);
    }
}
