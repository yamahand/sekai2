using UnityEngine;

public static class ArrayExtensions
{
    // 1次元配列のインデックスが有効かどうかをチェック
    public static bool IsValidIndex<T>(this T[] array, int index)
    {
        return index >= 0 && index < array.Length;
    }

    // 2次元配列のインデックスが有効かどうかをチェック
    public static bool IsValidIndex<T>(this T[,] array, int x, int y)
    {
        return x >= 0 && x < array.GetLength(1) && y >= 0 && y < array.GetLength(0);
    }

    // 2次元配列のVector2Intインデックスが有効かどうかをチェック
    public static bool IsValidIndex<T>(this T[,] array, Vector2Int index)
    {
        return array.IsValidIndex(index.x, index.y);
    }

    // 2次元配列からVector2Intインデックスで値を取得
    public static T GetT<T>(this T[,] array, Vector2Int index)
    {
        return array[index.y, index.x];
    }

    // 2次元配列にVector2Intインデックスで値を設定
    public static void SetT<T>(this T[,] array, Vector2Int index, T value)
    {
        array[index.y, index.x] = value;
    }

    // 2次元配列からVector2Intインデックスで値を取得し、失敗した場合はデフォルト値を返す
    public static bool TryGetT<T>(this T[,] array, Vector2Int index, out T value, T d = default)
    {
        if (!array.IsValidIndex(index))
        {
            value = d;
            return false;
        }
        value = array.GetT(index);
        return true;
    }

    // 3次元配列のインデックスが有効かどうかをチェック
    public static bool IsValidIndex<T>(this T[,,] array, int x, int y, int z)
    {
        return x >= 0 && x < array.GetLength(2) && y >= 0 && y < array.GetLength(1) && z >= 0 && z < array.GetLength(0);
    }

    // 3次元配列のVector3Intインデックスが有効かどうかをチェック
    public static bool IsValidIndex<T>(this T[,,] array, Vector3Int index)
    {
        return array.IsValidIndex(index.x, index.y, index.z);
    }

    // 3次元配列からVector3Intインデックスで値を取得
    public static T GetT<T>(this T[,,] array, Vector3Int index)
    {
        return array[index.z, index.y, index.x];
    }

    // 3次元配列にVector3Intインデックスで値を設定
    public static void SetT<T>(this T[,,] array, Vector3Int index, T value)
    {
        array[index.z, index.y, index.x] = value;
    }

    // 3次元配列からVector3Intインデックスで値を取得し、失敗した場合はデフォルト値を返す
    public static bool TryGetT<T>(this T[,,] array, Vector3Int index, out T value)
    {
        if (!array.IsValidIndex(index))
        {
            value = default;
            return false;
        }
        value = array.GetT(index);
        return true;
    }

    // 1次元配列をクリア
    public static void Clear<T>(this T[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = default;
        }
    }

    // 2次元配列をクリア
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

    // 3次元配列をクリア
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

    // 1次元配列をコピー
    public static void CopyTo<T>(this T[] source, T[] destination)
    {
        for (int i = 0; i < source.Length && i < destination.Length; i++)
        {
            destination[i] = source[i];
        }
    }

    // 2次元配列をコピー
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

    // 3次元配列をコピー
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

    // 1次元配列を指定した値で初期化
    public static void Initialize<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }

    // 2次元配列を指定した値で初期化
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

    // 3次元配列を指定した値で初期化
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
