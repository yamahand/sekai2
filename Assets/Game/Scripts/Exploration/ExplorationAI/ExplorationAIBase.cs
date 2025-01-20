using UnityEngine;

/// <summary>
/// 冒険者の探索AIの基底クラス
/// </summary>
public abstract class ExplorationAIBase
{
    // 目的のenum
    public enum GoalType
    {
        // なし
        None,
        // 未踏破
        Unexplored,
        // アイテム
        Item,
        // ボス
        Boss,
        // シンボルエネミー
        SymbolEnemy,
    }

    public ExplorationAIBase(ExplorationMap map, GoalType type)
    {
        _map = map;
        _goalType = type;
    }

    // 探索開始
    public abstract void StartExploration(Vector2Int startIndex);

    // 探索終了
    public abstract void EndExploration();

    // 次に行く方向が決まったかどうか
    public abstract bool IsDecidedNextDirection();

    // 次に行く方向を取得
    public abstract Vector2Int GetNextDirection();

    // 次に行くマスを取得
    public abstract Vector2Int GetNextPosition();

    // 次のマスに移動
    public abstract void MoveNextPosition();

    private ExplorationMap _map;
    private GoalType _goalType;
    private Vector2Int _position;
    private Vector2Int _nextPosition;
    private Vector2Int _nextDirection;
    private bool _isDecidedNextDirection;


}
