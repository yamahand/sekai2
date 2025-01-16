using System;

public partial class MapData
{
    // マスの種類
    public enum Type
    {
        Floor,
        Door,
        Stairs,
        Item,
        Box,
        Trap,
        SymbolEnemy,
        Boss,
        Start,
    }

    // マスの種類の名前を取得
    public static string GetTypeName(Type type)
    {
        if (IsValidType((int)type))
        {
            return TypeNames[(int)type];
        }
        return "-";
    }

    // マスの種類名の配列変数
    public static readonly string[] TypeNames = new string[]
    {
        "床",
        "扉",
        "階段",
        "罠",
        "アイテム",
        "宝箱",
        "シンボル",
        "ボス",
        "スタート",
    };

    // 有効なタイプかどうかをチェック
    public static bool IsValidType(int type)
    {
        return type >= 0 && type < TypeNames.Length;
    }
}
