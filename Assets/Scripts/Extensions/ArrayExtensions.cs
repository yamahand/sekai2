using UnityEngine;

public static class ArrayExtensions
{
    // 配列の拡張メソッド Invalid を定義
    public static bool Invalid<T>(this T[] array, int index)
    {
        return index < 0 || index >= array.Length;
    }

    public static bool IsValid<T>(this T[] array, int index)
    {
        return !array.Invalid(index);
    }

    // 2次元配列の拡張メソッド
    public static bool Invalid<T>(this T[,] array, int x, int y)
    {
        return x < 0 || x >= array.GetLength(1) || y < 0 || y >= array.GetLength(0);
    }

    public static bool IsValid<T>(this T[,] array, int x, int y)
    {
        return !array.Invalid(x, y);
    }

    public static bool Invalid<T>(this T[,] array, Vector2Int index)
    {
        return array.Invalid(index.x, index.y);
    }

    public static bool IsValid<T>(this T[,] array, Vector2Int index)
    {
        return !array.Invalid(index);
    }

    public static T GetT<T>(this T[,] array, Vector2Int index)
    {
        return array[index.y, index.x];
    }

    public static void SetT<T>(this T[,] array, Vector2Int index, T value)
    {
        array[index.y, index.x] = value;
    }

    public static bool TryGetT<T>(this T[,] array, Vector2Int index, out T value, T d = default)
    {
        if (array.Invalid(index))
        {
            value = d;
            return false;
        }
        value = array.GetT(index);
        return true;
    }

    // 3次元配列の拡張メソッド
    public static bool Invalid<T>(this T[,,] array, int x, int y, int z)
    {
        return x < 0 || x >= array.GetLength(2) || y < 0 || y >= array.GetLength(1) || z < 0 || z >= array.GetLength(0);
    }

    public static bool IsValid<T>(this T[,,] array, int x, int y, int z)
    {
        return !array.Invalid(x, y, z);
    }

    public static bool Invalid<T>(this T[,,] array, Vector3Int index)
    {
        return array.Invalid(index.x, index.y, index.z);
    }

    public static bool IsValid<T>(this T[,,] array, Vector3Int index)
    {
        return !array.Invalid(index);
    }

    public static T GetT<T>(this T[,,] array, Vector3Int index)
    {
        return array[index.z, index.y, index.x];
    }

    public static void SetT<T>(this T[,,] array, Vector3Int index, T value)
    {
        array[index.z, index.y, index.x] = value;
    }

    public static bool TryGetT<T>(this T[,,] array, Vector3Int index, out T value)
    {
        if (array.Invalid(index))
        {
            value = default;
            return false;
        }
        value = array.GetT(index);
        return true;
    }

    // 配列のクリア
    public static void Clear<T>(this T[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = default;
        }
    }

    public static void Clear<T>(this T[,] array)
    {
        for (int y = 0; y < array.GetLength(0); y++)
        {
            for (int x = 0; x < array.GetLength(1); x++)
            {
                array[y, x] = default;
            }
        }
    }

    public static void Clear<T>(this T[,,] array)
    {
        for (int z = 0; z < array.GetLength(0); z++)
        {
            for (int y = 0; y < array.GetLength(1); y++)
            {
                for (int x = 0; x < array.GetLength(2); x++)
                {
                    array[z, y, x] = default;
                }
            }
        }
    }

    // 配列のコピー
    public static void CopyTo<T>(this T[] source, T[] destination)
    {
        for (int i = 0; i < source.Length && i < destination.Length; i++)
        {
            destination[i] = source[i];
        }
    }

    public static void CopyTo<T>(this T[,] source, T[,] destination)
    {
        for (int y = 0; y < source.GetLength(0) && y < destination.GetLength(0); y++)
        {
            for (int x = 0; x < source.GetLength(1) && x < destination.GetLength(1); x++)
            {
                destination[y, x] = source[y, x];
            }
        }
    }

    public static void CopyTo<T>(this T[,,] source, T[,,] destination)
    {
        for (int z = 0; z < source.GetLength(0) && z < destination.GetLength(0); z++)
        {
            for (int y = 0; y < source.GetLength(1) && y < destination.GetLength(1); y++)
            {
                for (int x = 0; x < source.GetLength(2) && x < destination.GetLength(2); x++)
                {
                    destination[z, y, x] = source[z, y, x];
                }
            }
        }
    }

    // 配列の初期化
    public static void Initialize<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }

    public static void Initialize<T>(this T[,] array, T value)
    {
        for (int y = 0; y < array.GetLength(0); y++)
        {
            for (int x = 0; x < array.GetLength(1); x++)
            {
                array[y, x] = value;
            }
        }
    }

    public static void Initialize<T>(this T[,,] array, T value)
    {
        for (int z = 0; z < array.GetLength(0); z++)
        {
            for (int y = 0; y < array.GetLength(1); y++)
            {
                for (int x = 0; x < array.GetLength(2); x++)
                {
                    array[z, y, x] = value;
                }
            }
        }
    }
}
