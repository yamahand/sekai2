using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static MapData;

public partial class MapEditorWindow
{
    private void DrawInspector()
    {
        switch (_currentMode)
        {
            case SelectionMode.Floor:
                DrawMassInspector();
                break;
            case SelectionMode.Wall:
                DrawWallInspector();
                break;
            case SelectionMode.Edit:
                DrawEditInspector();
                break;
        }
    }

    private void DrawMassInspector()
    {
        GUILayout.Label($"Current Mass: {_currentMass.x}, {_currentMass.y}");
        // マスの情報を表示
        if (TryGetMass(_currentMass, out var currentMass))
        {
            GUILayout.Label($"Map Index: {currentMass.mapIndex}");
            GUILayout.Label($"Exists: {currentMass.exist}");
            GUILayout.Label($"Hidden: {currentMass.hidden}");
            GUILayout.Label($"Item Group: {currentMass.itemGroup}");
            GUILayout.Label($"Box ID: {currentMass.boxId}");
            GUILayout.Label($"Trap ID: {currentMass.trapId}");
            GUILayout.Label($"Symbol Enemy ID: {currentMass.symbolEnemyId}");
            GUILayout.Label($"Is Boss: {currentMass.isBoss}");
            GUILayout.Label($"Is Step: {currentMass.isStairs}");
            GUILayout.Label($"Door ID: {currentMass.doorId}");
        }
        else
        {
            GUILayout.Label("Map Index: -");
            GUILayout.Label("Exists: -");
            GUILayout.Label("Hidden: -");
            GUILayout.Label("Item Group: -");
            GUILayout.Label("Box ID: -");
            GUILayout.Label("Trap ID: -");
            GUILayout.Label("Symbol Enemy ID: -");
            GUILayout.Label("Is Boss: -");
            GUILayout.Label("Is Step: -");
            GUILayout.Label("Door ID: -");
        }
    }


    private void DrawWallInspector()
    {
        GUILayout.Label($"Current Horizontal Wall: {_currentHorizontalWall.x}, {_currentHorizontalWall.y}");
        GUILayout.Label($"Current Vertical Wall: {_currentVerticalWall.x}, {_currentVerticalWall.y}");

        // 壁を自動配置するボタン
        if (GUILayout.Button("Auto Set Wall"))
        {
            // 配置する水平壁のインデックスのリスト
            List<Vector2Int> horizontalWallIndexList = new List<Vector2Int>();
            // 配置する垂直壁のインデックスのリスト
            List<Vector2Int> verticalWallIndexList = new List<Vector2Int>();

            for (int y = 0; y < MapData.MassCount.y; y++)
            {
                for (int x = 0; x < MapData.MassCount.x; x++)
                {
                    // 上下の水平壁のインデックス
                    Vector2Int upperHorizontalWallIndex = new Vector2Int(-1, -1);
                    Vector2Int lowerHorizontalWallIndex = new Vector2Int(-1, -1);
                    // 左右の垂直壁のインデックス
                    Vector2Int leftVerticalWallIndex = new Vector2Int(-1, -1);
                    Vector2Int rightVerticalWallIndex = new Vector2Int(-1, -1);

                    MapData.Mass mass = GetMass(x, y);
                    if (mass == null || !mass.exist) continue;
                    // 上のマスを取得
                    MapData.Mass upMass = GetMass(x, y - 1);
                    // マスがnullか存在しない場合
                    if (upMass == null || !upMass.exist)
                    {
                        // 上のマスに水平壁を配置
                        upperHorizontalWallIndex = new Vector2Int(x, y);
                    }
                    // 下のマスを取得
                    MapData.Mass downMass = GetMass(x, y + 1);
                    // マスがnullか存在しない場合
                    if (downMass == null || !downMass.exist)
                    {
                        // 下のマスに水平壁を配置
                        lowerHorizontalWallIndex = new Vector2Int(x, y + 1);
                    }
                    // 左のマスを取得
                    MapData.Mass leftMass = GetMass(x - 1, y);
                    // マスがnullか存在しない場合
                    if (leftMass == null || !leftMass.exist)
                    {
                        // 左のマスに垂直壁を配置
                        leftVerticalWallIndex = new Vector2Int(x, y);
                    }
                    // 右のマスを取得
                    MapData.Mass rightMass = GetMass(x + 1, y);
                    // マスがnullか存在しない場合
                    if (rightMass == null || !rightMass.exist)
                    {
                        // 右のマスに垂直壁を配置
                        rightVerticalWallIndex = new Vector2Int(x + 1, y);
                    }

                    // 壁が配置されていない場合はリストに追加
                    if (upperHorizontalWallIndex.x != -1 && !IsExistHorizontalWall(upperHorizontalWallIndex) && !horizontalWallIndexList.Contains(upperHorizontalWallIndex))
                    {
                        horizontalWallIndexList.Add(upperHorizontalWallIndex);
                    }
                    if (lowerHorizontalWallIndex.x != -1 && !IsExistHorizontalWall(lowerHorizontalWallIndex) && !horizontalWallIndexList.Contains(lowerHorizontalWallIndex))
                    {
                        horizontalWallIndexList.Add(lowerHorizontalWallIndex);
                    }
                    if (leftVerticalWallIndex.x != -1 && !IsExistVerticalWall(leftVerticalWallIndex) && !verticalWallIndexList.Contains(leftVerticalWallIndex))
                    {
                        verticalWallIndexList.Add(leftVerticalWallIndex);
                    }
                    if (rightVerticalWallIndex.x != -1 && !IsExistVerticalWall(rightVerticalWallIndex) && !verticalWallIndexList.Contains(rightVerticalWallIndex))
                    {
                        verticalWallIndexList.Add(rightVerticalWallIndex);
                    }
                }
            }

            // まとめて壁を配置
            _commandQueue.Enqueue(new ChangeWallListCommand(this, horizontalWallIndexList, verticalWallIndexList, true));

            // 配置されている壁の周りのマスをチェックして、マスがない時は壁を削除
            // 削除する水平壁のインデックスのリスト
            List<Vector2Int> removeHorizontalWallIndexList = new List<Vector2Int>();
            // 削除する垂直壁のインデックスのリスト
            List<Vector2Int> removeVerticalWallIndexList = new List<Vector2Int>();
            // 水平壁のチェック
            for (int y = 0; y < MapData.HorizontalWallCount.y; y++)
            {
                for (int x = 0; x < MapData.HorizontalWallCount.x; x++)
                {
                    // 水平壁のインデックス
                    Vector2Int horizontalWallIndex = new Vector2Int(x, y);
                    // 水平壁が存在するとき
                    if (IsExistHorizontalWall(horizontalWallIndex))
                    {
                        // 上のマスを取得
                        MapData.Mass upMass = GetMass(x, y - 1);
                        // 下のマスを取得
                        MapData.Mass downMass = GetMass(x, y);
                        // 上下のマスが存在しない場合
                        if ((upMass == null || !upMass.exist) && (downMass == null || !downMass.exist))
                        {
                            // 壁を削除する
                            removeHorizontalWallIndexList.Add(horizontalWallIndex);
                        }
                    }
                }
            }

            // 垂直壁のチェック
            for (int y = 0; y < MapData.VerticalWallCount.y; y++)
            {
                for (int x = 0; x < MapData.VerticalWallCount.x; x++)
                {
                    // 垂直壁のインデックス
                    Vector2Int verticalWallIndex = new Vector2Int(x, y);
                    // 垂直壁が存在するとき
                    if (IsExistVerticalWall(verticalWallIndex))
                    {
                        // 左のマスを取得
                        Mass leftMass = GetMass(verticalWallIndex.x - 1, verticalWallIndex.y);
                        // 右のマスを取得
                        Mass rightMass = GetMass(verticalWallIndex.x, verticalWallIndex.y);
                        // 左右のマスが存在しない場合
                        if ((leftMass == null || !leftMass.exist) && (rightMass == null || !rightMass.exist))
                        {
                            // 壁を削除する
                            removeVerticalWallIndexList.Add(verticalWallIndex);
                        }
                    }
                }
            }


            // まとめて壁を削除
            _commandQueue.Enqueue(new ChangeWallListCommand(this, removeHorizontalWallIndexList, removeVerticalWallIndexList, false));
        }
    }
    private void DrawEditInspector()
    {
        GUILayout.Label($"Current Mass: {_currentMass.x}, {_currentMass.y}");
        GUILayout.Label($"Edit Mass: {_editMass.x}, {_editMass.y}");

        // マスの情報を取得
        if (TryGetMass(_editMass, out var editMass))
        {
            // 横にならべる
            using (new EditorGUILayout.HorizontalScope())
            {

                // マスの種類を表示
                GUILayout.Label($"MassType: {MapData.GetMassTypeName(editMass.type)}");

                // 隠しマスのチェックボックス
                var hidden = GUILayout.Toggle(editMass.hidden, "隠しマス");
                if (hidden != editMass.hidden)
                {
                    _commandQueue.Enqueue(new ChangeHiddenCommand(this, _editMass, hidden));
                }
            }


            var massType = (MapData.MassType)GUILayout.SelectionGrid((int)editMass.type, MapData.massTypeNames, 3);
            if (massType != editMass.type)
            {
                _commandQueue.Enqueue(new ChangeMassTypeCommand(this, _editMass, massType));
            }
        }
        else
        {
            GUILayout.Label("MassType: -");
            GUILayout.Label("隠しマス: -");
        }
    }
}
