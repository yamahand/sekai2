using System;

public partial class MapData
{
    // マスの種類
    public enum MassType
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
    public static string GetMassTypeName(MassType type)
    {
        if (IsValidMassType((int)type))
        {
            return massTypeNames[(int)type];
        }
        return "-";
    }

    // マスの種類名の配列変数
    public static readonly string[] massTypeNames = new string[]
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
    public static bool IsValidMassType(int type)
    {
        return type >= 0 && type < massTypeNames.Length;
    }
}
