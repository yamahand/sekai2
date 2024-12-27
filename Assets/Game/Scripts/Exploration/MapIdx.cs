using UnityEngine;

namespace Game.Scripts.Exploration
{
    // int値x,yを持つクラス
    public class MapIdx
    {
        public int X { get; set; }
        public int Y { get; set; }

        public const int maxValue = 32;

        public MapIdx(int x, int y)
        {
            X = x;
            Y = y;
        }

        // 2つのMapIdxが等しいかどうかを判定するメソッド
        public override bool Equals(object obj)
        {
            if (obj is MapIdx other)
            {
                return X == other.X && Y == other.Y;
            }
            return false;
        }

        // ハッシュコードを生成するメソッド
        public override int GetHashCode()
        {
            return (X, Y).GetHashCode();
        }

        // 文字列表現を返すメソッド
        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        // 加算メソッド
        public MapIdx Add(int dx, int dy)
        {
            return new MapIdx(X + dx, Y + dy);
        }

        // 減算メソッド
        public MapIdx Subtract(int dx, int dy)
        {
            return new MapIdx(X - dx, Y - dy);
        }

        // 0以上かつ上限値未満かを判定するメソッド
        public bool IsValid()
        {
            return X >= 0 && X < maxValue && Y >= 0 && Y < maxValue;
        }

        // == 演算子のオーバーロード
        public static bool operator ==(MapIdx lhs, MapIdx rhs)
        {
            if (ReferenceEquals(lhs, null))
            {
                return ReferenceEquals(rhs, null);
            }
            return lhs.Equals(rhs);
        }

        // != 演算子のオーバーロード
        public static bool operator !=(MapIdx lhs, MapIdx rhs)
        {
            return !(lhs == rhs);
        }

        // + 演算子のオーバーロード
        public static MapIdx operator +(MapIdx lhs, MapIdx rhs)
        {
            return new MapIdx(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        // - 演算子のオーバーロード
        public static MapIdx operator -(MapIdx lhs, MapIdx rhs)
        {
            return new MapIdx(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }
    }
}


