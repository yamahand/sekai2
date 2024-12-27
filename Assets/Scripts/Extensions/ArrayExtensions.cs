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

    public static bool Invalid<T>(this T[,] array, int x, int y)
    {
        return x < 0 || x >= array.GetLength(1) || y < 0 || y >= array.GetLength(0);
    }

    public static bool IsValid<T>(this T[,] array, int x, int y)
    {
        return !array.Invalid(x, y);
    }
}