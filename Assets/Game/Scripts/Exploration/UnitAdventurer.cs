using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using System.Threading;
using System;
public class UnitAdventurer : MonoBehaviour, IUnit
{
    public UnitType unitType { get; private set; }
    public string unitName { get; private set; }
    public Vector2Int position { get; private set; } = Vector2Int.zero;

    public UnitAdventurer()
    {
    }

    // セットアップ
    public void Setup(UnitType type, string name, Vector2Int _, ExplorationMap map)
    {
        this.unitType = type;
        this.unitName = name;
        this.position = map.startPosition;
        _interval = 1.0f;
        transform.position = MapUtil.MapIndexToWorldPosition(position);

        map.GetMass(position).Show();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    // 更新
    public void UpdateUnit(float deltaTime, ExplorationMap map)
    {
        _currentTimer += deltaTime;
        while (_currentTimer >= _interval)
        {
            _currentTimer = _currentTimer - _interval;
            var direction = ThinkNextDirection(map);
            Move(direction, map, _cancellationTokenSource.Token);
        }
    }

    // 移動
    private async void Move(MapDef.Direction direction, ExplorationMap map, CancellationToken cancellationToken)
    {
        // 念のため移動できるかの判定
        if (map.CanMove(position, direction))
        {
            if (_motionHandle.IsActive())
            {
                _motionHandle.Complete();
            }

            position += MapUtil.DirectionToVector2Int(direction);
            try
            {
                _motionHandle = LMotion.Create(transform.position, MapUtil.MapIndexToWorldPosition(position), _movetime)
                    .WithEase(Ease.InSine)
                    .BindToPosition(transform)
                    .AddTo(gameObject);

                await _motionHandle.ToUniTask(cancellationToken);

                map.GetMass(position).Show();
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合の処理
                Debug.Log("Move operation was canceled.");
                return;
            }
        }
    }

    // 次に移動する方向を考える
    private MapDef.Direction ThinkNextDirection(ExplorationMap map)
    {
        // 上下左右のマスで解放されていないマスを探す
        var directions = new MapDef.Direction[] { MapDef.Direction.Up, MapDef.Direction.Down, MapDef.Direction.Left, MapDef.Direction.Right };
        foreach (var direction in directions)
        {
            if (map.CanMove(position, direction))
            {
                var d = MapUtil.DirectionToVector2Int(direction);
                var nextPosition = position + d;
                if (!map.GetMass(nextPosition).explored)
                {
                    return direction;
                }
            }
        }

        // ランダムに方向を決める
        {
            int index = UnityEngine.Random.Range(0, directions.Length);
            var direction = directions[index];
            while (!map.CanMove(position, direction))
            {
                index = (index + 1) % directions.Length;
                direction = directions[index];
                break;
            }
            return direction;
        }
    }

    [SerializeField] float _currentTimer = 0.0f;
    [SerializeField][Range(0.1f, 1.0f)] float _interval = 0.0f;
    [SerializeField][Range(0.1f, 1.0f)] float _movetime = 0.3f;

    MotionHandle _motionHandle;
    private CancellationTokenSource _cancellationTokenSource;
}
