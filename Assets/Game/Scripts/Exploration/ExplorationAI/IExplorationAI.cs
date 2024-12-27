using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public interface IExplorationAI
{
    // 経路探査を行う
    public UniTask SearchRoute(Vector2Int start, Vector2Int goal, CancellationToken cancellationToken = default);

    // 次の移動方向を取得する
    Vector2Int GetNextDirection();

    // 移動先の座標を取得する
    Vector2Int GetNextPosition();

    // 次の移動先に進む
    void Next();
}
