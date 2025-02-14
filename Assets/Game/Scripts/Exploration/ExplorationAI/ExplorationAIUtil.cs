﻿using UnityEngine;

public class ExplorationAIUtil
{
    public static IExplorationAI CreateAI(ExplorationAIDef.Type type, ExplorationMap map)
    {
        switch (type)
        {
            case ExplorationAIDef.Type.AStar:
                return new AStar(map);
            default:
                Debug.LogError("Invalid AI type.");
                return null;
        }
    }
}
