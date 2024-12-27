using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

/// <summary>
/// A*アルゴリズムを使用して経路探索を行うクラス
/// </summary>
public class AStar : IExplorationAI
{
    /// <summary>
    /// ノードを表す内部クラス
    /// </summary>
    class Node : IComparable<Node>
    {
        public Vector2Int position; // ノードの位置
        public int cost; // スタートからこのノードまでのコスト
        public int heuristic; // ゴールまでの推定コスト（ヒューリスティック）
        public int totalCost; // 合計コスト（cost + heuristic）
        public Node parent; // 親ノード

        /// <summary>
        /// ノードのコンストラクタ
        /// </summary>
        /// <param name="position">ノードの位置</param>
        /// <param name="cost">スタートからこのノードまでのコスト</param>
        /// <param name="heuristic">ゴールまでの推定コスト</param>
        /// <param name="parent">親ノード</param>
        public Node(Vector2Int position, int cost, int heuristic, Node parent = null)
        {
            this.position = position;
            this.cost = cost;
            this.heuristic = heuristic;
            this.totalCost = cost + heuristic;
            this.parent = parent;
        }

        /// <summary>
        /// ノードの比較を行う
        /// </summary>
        /// <param name="other">比較対象のノード</param>
        /// <returns>比較結果</returns>
        public int CompareTo(Node other)
        {
            return this.totalCost.CompareTo(other.totalCost);
        }
    }

    private Game.Scripts.Exploration.Map _map; // マップ
    private Vector2Int _start; // スタート位置
    private Vector2Int _goal; // ゴール位置
    private PriorityQueue<Node> _openList; // オープンリスト
    private Node[] _closeList; // クローズリスト
    private int _closeListCount; // クローズリストの要素数

    // 探索した結果を保存する配列
    private Node[] _route;
    private int _routeCount; // 経路の要素数
    private int _currentRouteIndex; // 現在の経路インデックス

    /// <summary>
    /// AStarクラスのコンストラクタ
    /// </summary>
    /// <param name="map">探索対象のマップ</param>
    public AStar(Game.Scripts.Exploration.Map map)
    {
        _map = map;
        _openList = new PriorityQueue<Node>(_map.mapSizeX * _map.mapSizeY);
        _closeList = new Node[_map.mapSizeX * _map.mapSizeY];
    }

    /// <summary>
    /// 経路探索を行う
    /// </summary>
    /// <param name="start">スタート位置</param>
    /// <param name="goal">ゴール位置</param>
    /// <param name="cancellationToken">キャンセル用トークン</param>
    public async UniTask SearchRoute(Vector2Int start, Vector2Int goal, CancellationToken cancellationToken = default)
    {
        _start = start;
        _goal = goal;
        _openList = new PriorityQueue<Node>(_map.mapSizeX * _map.mapSizeY);
        _closeListCount = 0;
        _routeCount = 0;
        _route = null;
        _currentRouteIndex = 0;
        // スタート地点をオープンリストに追加
        _openList.Enqueue(new Node(start, 0, _CalcHeuristic(start, goal)));
        while (_openList.Count > 0)
        {
            // キャンセルが要求されたかどうかをチェック
            if (cancellationToken.IsCancellationRequested)
            {
                Debug.Log("SearchRoute was cancelled.");
                return;
            }

            // オープンリストから最小コストのノードを取得
            Node current = _openList.Dequeue();
            // ゴールに到達したら終了
            if (current.position == goal)
            {
                // 経路を保存
                _routeCount = current.cost + 1;
                _route = new Node[_routeCount];
                // 経路を逆順に保存
                Node node = current;
                for (int i = current.cost; i >= 0; i--)
                {
                    _route[i] = node;
                    node = node.parent;
                }

                break;
            }
            // クローズリストに追加
            _closeList[_closeListCount++] = current;
            // 上下左右のノードをオープンリストに追加
            _AddNode(current.position, MapDef.Direction.Up, _map, current);
            _AddNode(current.position, MapDef.Direction.Down, _map, current);
            _AddNode(current.position, MapDef.Direction.Left, _map, current);
            _AddNode(current.position, MapDef.Direction.Right, _map, current);

            await UniTask.Yield();
        }
    }

    /// <summary>
    /// ゴールまでの経路が存在するかどうかを返す
    /// </summary>
    /// <returns>経路が存在する場合はtrue、それ以外の場合はfalse</returns>
    public bool CanReachGoal()
    {
        return _routeCount > 0;
    }

    /// <summary>
    /// 現在の位置がゴールかどうかを返す
    /// </summary>
    /// <returns>ゴールの場合はtrue、それ以外の場合はfalse</returns>
    public bool IsGoal()
    {
        return _currentRouteIndex == _routeCount - 1;
    }

    /// <summary>
    /// 次の移動方向を取得する
    /// </summary>
    /// <returns>次の移動方向</returns>
    public Vector2Int GetNextDirection()
    {
        if (IsGoal())
        {
            return Vector2Int.zero;
        }
        return _route[_currentRouteIndex].position - _route[_currentRouteIndex + 1].position;
    }

    /// <summary>
    /// 次の移動先の座標を取得する
    /// </summary>
    /// <returns>次の移動先の座標</returns>
    public Vector2Int GetNextPosition()
    {
        if (IsGoal())
        {
            return Vector2Int.zero;
        }
        return _route[_currentRouteIndex + 1].position;
    }

    /// <summary>
    /// 現在の経路インデックスを次へ進める
    /// </summary>
    public void Next()
    {
        if (IsGoal())
        {
            return;
        }
        _currentRouteIndex++;
    }

    /// <summary>
    /// スタートからゴールまでの次の移動先の座標を取得する
    /// </summary>
    /// <param name="start">スタート位置</param>
    /// <param name="goal">ゴール位置</param>
    /// <param name="cancellationToken">キャンセル用トークン</param>
    /// <returns>次の移動先の座標</returns>
    public async UniTask<Vector2Int> GetNextPosition(Vector2Int start, Vector2Int goal, CancellationToken cancellationToken = default)
    {
        await SearchRoute(start, goal, cancellationToken);
        return GetNextPosition();
    }

    /// <summary>
    /// 指定された方向に隣接するノードをオープンリストに追加する
    /// </summary>
    /// <param name="current">現在の位置</param>
    /// <param name="direction">移動する方向</param>
    /// <param name="map">マップ</param>
    /// <param name="parent">親ノード</param>
    private void _AddNode(Vector2Int current, MapDef.Direction direction, Game.Scripts.Exploration.Map map, Node parent)
    {
        var next = current + MapUtil.DirectionToVector2Int(direction);
        if (!map.CanMove(current, direction))
        {
            return;
        }
        if (_IsInList(_closeList, _closeListCount, next))
        {
            return;
        }
        _openList.Enqueue(new Node(next, parent.cost + 1, _CalcHeuristic(next, _goal), parent));
    }

    /// <summary>
    /// 指定された位置がリストに含まれているかどうかを確認する
    /// </summary>
    /// <param name="list">ノードのリスト</param>
    /// <param name="count">リストの要素数</param>
    /// <param name="position">確認する位置</param>
    /// <returns>リストに含まれている場合はtrue、それ以外の場合はfalse</returns>
    private bool _IsInList(Node[] list, int count, Vector2Int position)
    {
        for (int i = 0; i < count; i++)
        {
            if (list[i].position == position)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 指定された位置からゴールまでの推定コスト（ヒューリスティック）を計算する
    /// </summary>
    /// <param name="position">現在の位置</param>
    /// <param name="goal">ゴール位置</param>
    /// <returns>推定コスト</returns>
    private int _CalcHeuristic(Vector2Int position, Vector2Int goal)
    {
        return Mathf.Abs(position.x - goal.x) + Mathf.Abs(position.y - goal.y);
    }

    /// <summary>
    /// デバッグ用にオープンリストとクローズリストのノードを描画する
    /// </summary>
    public void DrawDebug()
    {
        for (int i = 0; i < _openList.Count; i++)
        {
            Debug.DrawLine(new Vector3(_openList[i].position.x, _openList[i].position.y, 0), new Vector3(_openList[i].position.x + 1, _openList[i].position.y + 1, 0), Color.red);
            Debug.DrawLine(new Vector3(_openList[i].position.x + 1, _openList[i].position.y, 0), new Vector3(_openList[i].position.x, _openList[i].position.y + 1, 0), Color.red);
        }
        for (int i = 0; i < _closeListCount; i++)
        {
            Debug.DrawLine(new Vector3(_closeList[i].position.x, _closeList[i].position.y, 0), new Vector3(_closeList[i].position.x + 1, _closeList[i].position.y + 1, 0), Color.blue);
            Debug.DrawLine(new Vector3(_closeList[i].position.x + 1, _closeList[i].position.y, 0), new Vector3(_closeList[i].position.x, _closeList[i].position.y + 1, 0), Color.blue);
        }
    }

    /// <summary>
    /// デバッグ用に経路を描画する
    /// </summary>
    public void DrawRoute()
    {
        for (int i = 0; i < _closeListCount - 1; i++)
        {
            Debug.DrawLine(new Vector3(_closeList[i].position.x + 0.5f, _closeList[i].position.y + 0.5f, 0), new Vector3(_closeList[i + 1].position.x + 0.5f, _closeList[i + 1].position.y + 0.5f, 0), Color.green);
        }
    }

    /// <summary>
    /// スタートからゴールまでの経路を描画する
    /// </summary>
    /// <param name="start">スタート位置</param>
    /// <param name="goal">ゴール位置</param>
    /// <param name="cancellationToken">キャンセル用トークン</param>
    public async UniTask DrawRoute(Vector2Int start, Vector2Int goal, CancellationToken cancellationToken = default)
    {
        await SearchRoute(start, goal, cancellationToken);
        DrawRoute();
    }
}
