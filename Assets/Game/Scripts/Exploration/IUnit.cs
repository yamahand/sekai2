using UnityEngine;

// ユニットインターフェイス
namespace Game.Scripts.Exploration
{
    public interface IUnit
    {
        // ユニットタイプ
        UnitType unitType { get; }
        // 名前
        string unitName { get; }
        // 位置
        Vector2Int position { get; }
        // セットアップ
        void Setup(UnitType type, string name, Vector2Int position, Map map);
        // 更新
        void UpdateUnit(float deltaTime, Map map);
    }
}
