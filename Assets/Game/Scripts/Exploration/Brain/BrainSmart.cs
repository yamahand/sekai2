using UnityEngine;

[System.Serializable]
public class BrainSmart : IBrain
{
    public BrainSmart(ExplorationAIDef.Type type, ExplorationMap map)
    {
        _explorationAI = ExplorationAIUtil.CreateAI(type, map);
        _map = map;
    }

    // 指定座標から一番近い未探索座標を取得
    public Vector2Int GetNearestUnexploredPosition(Vector2Int position)
    {
        var result = _map.FindNearestUnexploredMass(position, false);
        return result.position;
    }

    private IExplorationAI _explorationAI;
    private ExplorationMap _map;
}
