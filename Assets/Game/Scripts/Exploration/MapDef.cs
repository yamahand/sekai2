using UnityEngine;

public class MapDef
{
    // マスの横の最大数
    static public int MassMaxX = 16;
    // マスの縦の最大数
    static public int MassMaxY = 16;
    // マスのサイズ
    static public float MassSize = 64.0f;
    // マップの横幅
    static public float MapWidth = MassMaxX * MassSize;
    // マップの縦幅
    static public float MapHeight = MassMaxY * MassSize;

    // 方向
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }

    // 方向の配列
    static public Direction[] Directions = new Direction[]
    {
        Direction.Up,
        Direction.Down,
        Direction.Left,
        Direction.Right,
    };

    // マスの周りに壁があるかどうかを表すビットフラグ
    public enum Wall
    {
        Up = 1 << Direction.Up,
        Down = 1 << Direction.Down,
        Left = 1 << Direction.Left,
        Right = 1 << Direction.Right,
    }

    // 方向から壁のビットフラグを取得する
    static public Wall DirectionToWall(Direction direction)
    {
        // switch文で書くと見づらいので、配列で対応する値を返す
        return new Wall[] { Wall.Up, Wall.Down, Wall.Left, Wall.Right }[(int)direction];
    }
}
