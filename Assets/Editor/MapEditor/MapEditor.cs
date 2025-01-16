using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor.UIElements;
using static MapData;

public partial class MapEditorWindow : EditorWindow
{
    private float _cellSize = 40.0f;
    private float _cellScale = 0.9f;
    private float _wallSize = 4.0f;
    private Vector2 _gridOffset = new Vector2(128.0f, 16.0f);
    private bool _isSettingFold = false;

    // 選択モード
    internal enum SelectionMode
    {
        Floor,
        Wall,
        Edit,
    }

    // 選択モードの文字列の配列
    private string[] _selectionModeStrings = new string[]
    {
        "床配置",
        "壁配置",
        "編集",
    };
    private SelectionMode _currentMode = SelectionMode.Floor;

    private Vector2Int _currentMass = new Vector2Int(-1, -1);
    private Vector2Int _currentHorizontalWall = new Vector2Int(-1, -1);
    private Vector2Int _currentVerticalWall = new Vector2Int(-1, -1);

    private Vector2 _selectedCell = new Vector2(-1, -1);
    private Vector2Int _selectedHorizontalWall = new Vector2Int(-1, -1);
    private Vector2Int _selectedVerticalWall = new Vector2Int(-1, -1);

    private Vector2Int _editMass = new Vector2Int(-1, -1);

    private MapData _mapData = null;

    // 実行するコマンドキュー
    private Queue<IMapEditorCommand> _commandQueue = new Queue<IMapEditorCommand>();
    // コマンドの実行履歴
    private readonly Stack<IMapEditorCommand> _commandHistory = new Stack<IMapEditorCommand>();
    // コマンドの取り消し履歴
    private readonly Stack<IMapEditorCommand> _undoHistory = new Stack<IMapEditorCommand>();



    [MenuItem("Window/Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<MapEditorWindow>("Map Editor");
    }

    private void CreateGUI()
    {
        Initialize();

        var toolbar1 = new Toolbar();
        rootVisualElement.Add(toolbar1);

        // Menu
        var menu = new ToolbarMenu();
        menu.text = "File";
        menu.menu.AppendAction("New", action => CreateNewMapData());
        menu.menu.AppendAction("Open", action => OpenMapData());
        menu.menu.AppendSeparator();
        menu.menu.AppendAction("Save", action => SaveMapData());
        toolbar1.Add(menu);

        // Button
        {
            var button = new ToolbarButton();
            button.text = "New";
            button.clicked += () => CreateNewMapData();
            toolbar1.Add(button);
        }

        {
            var button = new ToolbarButton();
            button.text = "Open";
            button.clicked += () => OpenMapData();
            toolbar1.Add(button);
        }

        {
            var button = new ToolbarButton();
            button.text = "Save";
            button.clicked += () => SaveMapData();
            toolbar1.Add(button);
        }

        // Spacer
        toolbar1.Add(new ToolbarSpacer());

        {
            var button = new ToolbarButton();
            button.text = "Undo";
            button.clicked += () => Undo();
            toolbar1.Add(button);
        }
        {
            var button = new ToolbarButton();
            button.text = "Redo";
            button.clicked += () => Redo();
            toolbar1.Add(button);
        }
        {
            var button = new ToolbarButton();
            button.text = "History";
            button.clicked += () => CommandHistoryWindow.ShowWindow(_commandHistory, _undoHistory);
            toolbar1.Add(button);
        }


        // PopupSearchField
        //var popupSearchField = new ToolbarPopupSearchField();
        //popupSearchField.RegisterValueChangedCallback(evt => Debug.Log($"PopupSearchField: {evt.newValue}"));
        //popupSearchField.menu.AppendAction("First", action => Debug.Log("First"));
        //popupSearchField.menu.AppendAction("Second", action => Debug.Log("Second"));
        //popupSearchField.menu.AppendAction("Third", action => Debug.Log("Third"));
        //toolbar1.Add(popupSearchField);
    }

    private void OnGUI()
    {
        OnGUIUpdate();
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        Debug.Log("OnDisable");

        // マスの解放
        _mapData = null;
    }

    // 初期化関数
    private void Initialize()
    {
        _mapData = new MapData();
    }

    // Undo処理
    private void Undo()
    {
        if (_commandHistory.Count > 0)
        {
            var command = _commandHistory.Pop();
            command.Undo();
            _undoHistory.Push(command);

            CommandHistoryWindow.UpdateHistory();
        }
    }
    // Redo処理
    private void Redo()
    {
        if (_undoHistory.Count > 0)
        {
            var command = _undoHistory.Pop();
            command.Execute();
            _commandHistory.Push(command);

            CommandHistoryWindow.UpdateHistory();
        }
    }



    private void OnGUIUpdate()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            // 右ペイン: グリッド描画        
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.7f));
            GUILayout.Label("Map Editor", EditorStyles.boldLabel);

            // モード切替パネル
            GUILayout.Label("Mode Selection", EditorStyles.boldLabel);
            _currentMode = (SelectionMode)GUILayout.Toolbar((int)_currentMode, _selectionModeStrings);

            // `GUILayout.Label` の表示位置とサイズを取得
            Rect lastRect = GUILayoutUtility.GetLastRect();
            _gridOffset = new Vector2(lastRect.x, lastRect.y + lastRect.height * 2.0f);

            var mousePos = Event.current.mousePosition;
            UpdateCurrent(mousePos);

            DrawGrid();

            EditorGUILayout.EndVertical();

            // 左ペイン: 設定変更
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.3f));
            _isSettingFold = EditorGUILayout.Foldout(_isSettingFold, "Settings");
            if (_isSettingFold)
            {
                //gridSize = EditorGUILayout.IntSlider("Grid Size", gridSize, 1, 100);
                _cellSize = EditorGUILayout.FloatField("Mass Size", _cellSize);
                _cellScale = EditorGUILayout.FloatField("Mass Scale", _cellScale);
                _wallSize = EditorGUILayout.FloatField("Wall Size", _wallSize);
                _gridOffset = EditorGUILayout.Vector2Field("Grid Offset", _gridOffset);

                // 現在のウィンドウサイズを表示
                GUILayout.Label($"Window Size: {position.width} x {position.height}");
            }

            // マップ名入力
            if (_mapData != null)
            {
                _mapData.mapName = EditorGUILayout.TextField("Map Name", _mapData.mapName);
            }

            DrawInspector();

            EditorGUILayout.EndVertical();
        }

        // マウスを左クリックしているとき
        if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && Event.current.button == 0)
        {
            OnMapLeftClick();
        }

        // マウスを右クリックしているとき
        if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && Event.current.button == 1)
        {
            OnMapRightClick();
        }
    }

    private void OnEditorUpdate()
    {
        if (focusedWindow == this)
        {
            Repaint();


            // コマンドの実行
            if (_commandQueue.Count > 0)
            {
                while (_commandQueue.Count > 0)
                {
                    var command = _commandQueue.Dequeue();
                    if (command != null)
                    {
                        command.Execute();
                        _commandHistory.Push(command);
                        _undoHistory.Clear();
                    }
                }
                // コマンド履歴ウインドウの更新
                CommandHistoryWindow.UpdateHistory();
            }
        }
    }

    private void DrawGrid()
    {
        Vector3 offset3 = new Vector3(_gridOffset.x, _gridOffset.y, 0.0f);

        GUIStyle style = new GUIStyle();
        style.fontSize = 8;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.white;

        Vector2Int mapIndex = Vector2Int.zero;
        for (mapIndex.y = 0; mapIndex.y < MapData.MassCount.y; mapIndex.y++)
        {
            for (mapIndex.x = 0; mapIndex.x < MapData.MassCount.x; mapIndex.x++)
            {
                Vector3 start = new Vector3(mapIndex.x * _cellSize, mapIndex.y * _cellSize, 0);
                start += offset3;
                DrawGrid(mapIndex.x, mapIndex.y, _currentMass == mapIndex, _mapData.GetMass(mapIndex));
                Handles.Label(start, $"({mapIndex})", style);
                var mass = GetMass(mapIndex);
            }
        }

        // 水平壁を描画
        Vector2Int horizontalWallIndex = Vector2Int.zero;
        for (horizontalWallIndex.y = 0; horizontalWallIndex.y < MapData.HorizontalWallCount.y; horizontalWallIndex.y++)
        {
            for (horizontalWallIndex.x = 0; horizontalWallIndex.x < MapData.HorizontalWallCount.x; horizontalWallIndex.x++)
            {
                DrawHorizontalWall(horizontalWallIndex.x, horizontalWallIndex.y, _currentHorizontalWall == horizontalWallIndex, IsExistHorizontalWall(horizontalWallIndex));
            }
        }

        // 垂直壁を描画
        Vector2Int verticalWallIndex = Vector2Int.zero;
        for (verticalWallIndex.y = 0; verticalWallIndex.y < MapData.VerticalWallCount.y; verticalWallIndex.y++)
        {
            for (verticalWallIndex.x = 0; verticalWallIndex.x < MapData.VerticalWallCount.x; verticalWallIndex.x++)
            {
                DrawVerticalWall(verticalWallIndex.x, verticalWallIndex.y, _currentVerticalWall == verticalWallIndex, IsExistVerticalWall(verticalWallIndex));
            }
        }

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    }

    private void DrawGrid(int x, int y, bool overlay, MapData.Mass mass)
    {
        Vector2 pos = new Vector2(x * _cellSize, y * _cellSize);
        pos += _gridOffset;
        Vector2 wallOffset = new Vector2(_wallSize, _wallSize);
        pos += wallOffset * 0.5f;
        Vector2 cellSizeVec = new Vector2(_cellSize, _cellSize);
        cellSizeVec -= wallOffset;
        Rect rect = new Rect(pos, cellSizeVec * _cellScale);
        Handles.DrawSolidRectangleWithOutline(
            rect,
            GetColor(mass),
            overlay ? Color.blue : Color.gray
            );
    }

    private Color GetColor(MapData.Mass mass)
    {
        if (!mass.exist)
        {
            return Color.black;
        }

        switch (mass.type)
        {
            case MapData.Type.Floor:
                return Color.white;
            case MapData.Type.Door:
                return Color.yellow;
            case MapData.Type.Stairs:
                return Color.yellow;
            case MapData.Type.Item:
                return Color.green;
            case MapData.Type.Box:
                return Color.cyan;
            case MapData.Type.Trap:
                return Color.magenta;
            case MapData.Type.SymbolEnemy:
                return Color.cyan;
            case MapData.Type.Boss:
                return Color.red;
            case MapData.Type.Start:
                return Color.blue;
        }

        return Color.black;
    }

    // 水平壁を描画
    private void DrawHorizontalWall(int x, int y, bool overlay, bool exist)
    {
        Vector2 pos = new Vector2(x * _cellSize, y * _cellSize);
        pos += _gridOffset;
        pos -= new Vector2(0.0f, _wallSize * 0.5f);
        Rect rect = new Rect(pos, new Vector2(_cellSize, _wallSize * 0.8f));
        Handles.DrawSolidRectangleWithOutline(
            rect,
            exist ? Color.white : Color.black,
            overlay ? Color.red : Color.gray
            );
    }

    // 垂直壁を描画
    private void DrawVerticalWall(int x, int y, bool overlay, bool exist)
    {
        Vector2 pos = new Vector2(x * _cellSize, y * _cellSize);
        pos += _gridOffset;
        pos -= new Vector2(_wallSize * 0.5f, 0.0f);
        Rect rect = new Rect(pos, new Vector2(_wallSize * 0.8f, _cellSize));
        Handles.DrawSolidRectangleWithOutline(
            rect,
            exist ? Color.white : Color.black,
            overlay ? Color.red : Color.gray
            );
    }

    // マウス位置にあるマスと壁を取得
    private void UpdateCurrent(Vector2 mousePos)
    {
        Vector2 hitPoint = mousePos;
        hitPoint -= _gridOffset;
        float xPos = hitPoint.x / _cellSize;
        float yPos = hitPoint.y / _cellSize;
        float xFrac = xPos - Mathf.Floor(xPos);
        float yFrac = yPos - Mathf.Floor(yPos);

        EditorGUILayout.LabelField($"Mouse Pos: {mousePos.x}, {mousePos.y}");
        EditorGUILayout.LabelField($"Hit Point: {hitPoint.x}, {hitPoint.y}");
        EditorGUILayout.LabelField($"Grid Pos: {xPos}, {yPos}");
        EditorGUILayout.LabelField($"Frac: {xFrac}, {yFrac}");

        int x = Mathf.FloorToInt(xPos);
        int y = Mathf.FloorToInt(yPos);

        // 壁の半分のサイズ
        float wallHalfSize = _wallSize * 0.5f;

        // 一度リセット
        _currentMass = new Vector2Int(-1, -1);
        _currentHorizontalWall = new Vector2Int(-1, -1);
        _currentVerticalWall = new Vector2Int(-1, -1);

        // 範囲外だったら終了
        if (x < -wallHalfSize || x >= MapData.MassCount.x + _wallSize || y < -wallHalfSize || y >= MapData.MassCount.y + _wallSize)
        {
            return;
        }

        if (_currentMode == SelectionMode.Wall)
        {
            // 水平線の判定
            if (yFrac < 0.2f || yFrac > 0.8f)
            {
                var tempHorizontalWall = new Vector2Int(Mathf.FloorToInt(xPos), Mathf.RoundToInt(yPos));
                if(IsValidHorizontalWallIndex(tempHorizontalWall))
                {
                    _currentHorizontalWall = tempHorizontalWall;
                }
            }
            // 垂直線の判定
            else if (xFrac < 0.2f || xFrac > 0.8f)
            {
                var tempVerticalWall = new Vector2Int(Mathf.RoundToInt(xPos), Mathf.FloorToInt(yPos));
                if (IsValidVerticalWallIndex(tempVerticalWall))
                {
                    _currentVerticalWall = tempVerticalWall;
                }
            }
        }
        else
        {
            var tempMass = new Vector2Int(Mathf.FloorToInt(xPos), Mathf.FloorToInt(yPos));
            if (IsValidMapIndex(tempMass))
            {
                _currentMass = tempMass;
            }
        }
    }

    // マップ上で左クリックしたときの動作
    private void OnMapLeftClick()
    {
        // 現在の選択モードによって処理を分岐
        switch (_currentMode)
        {
            case SelectionMode.Floor:
                CellCommand(true);
                break;
            case SelectionMode.Wall:
                WallCommand(true);
                break;
            case SelectionMode.Edit:
                if (MapData.IsValidMapIndex(_currentMass))
                {
                    _editMass = _currentMass;
                }
                break;
        }
    }

    // マップ上で右クリックしたときの動作
    private void OnMapRightClick()
    {
        // 現在の選択モードによって処理を分岐
        switch (_currentMode)
        {
            case SelectionMode.Floor:
                CellCommand(false);
                break;
            case SelectionMode.Wall:
                WallCommand(false);
                break;
            case SelectionMode.Edit:
                _editMass = new Vector2Int(-1, -1);
                break;
        }
    }

    // 壁用のコマンドを作成
    private void WallCommand(bool isPlace)
    {
        // _currentHorizontalWallが有効な場合
        if (IsValidHorizontalWallIndex(_currentHorizontalWall))
        {
            // 水平壁が存在しないときは設置コマンド
            if (isPlace && !IsExistHorizontalWall(_currentHorizontalWall))
            {
                _commandQueue.Enqueue(new ChangeWallCommand(this, _currentHorizontalWall, -Vector2Int.one, true));
            }
            // 水平壁が存在するときは削除コマンド
            else if (!isPlace && IsExistHorizontalWall(_currentHorizontalWall))
            {
                _commandQueue.Enqueue(new ChangeWallCommand(this, _currentHorizontalWall, -Vector2Int.one, false));
            }
        }
        // _currentVerticalWallが有効な場合
        if (IsValidVerticalWallIndex(_currentVerticalWall))
        {
            // 垂直壁が存在しないときは設置コマンド
            if (isPlace && !IsExistVerticalWall(_currentVerticalWall))
            {
                _commandQueue.Enqueue(new ChangeWallCommand(this, -Vector2Int.one, _currentVerticalWall, true));
            }
            // 垂直壁が存在するときは削除コマンド
            else if (!isPlace && IsExistVerticalWall(_currentVerticalWall))
            {
                _commandQueue.Enqueue(new ChangeWallCommand(this, -Vector2Int.one, _currentVerticalWall, false));
            }
        }
    }

    // マス用のコマンドを作成
    private void CellCommand(bool isPlace)
    {
        // _currentCellが無効な場合
        if (!IsValidMapIndex(_currentMass))
        {
            return;
        }


        // マスが存在しないときは設置コマンド
        if (isPlace && !IsExistMass(_currentMass))
        {
            _commandQueue.Enqueue(new PlaceCellCommand(this, _currentMass, true));
        }
        // マスが存在するときは削除コマンド
        else if (!isPlace && IsExistMass(_currentMass))
        {
            _commandQueue.Enqueue(new PlaceCellCommand(this, _currentMass, false));
        }
    }


    // ウインドウ上の座標からマップインデックスを取得
    private Vector2Int GetMapIndex(Vector2 pos)
    {
        pos -= _gridOffset;
        float xPos = pos.x / _cellSize;
        float yPos = pos.y / _cellSize;
        return new Vector2Int(Mathf.FloorToInt(xPos), Mathf.FloorToInt(yPos));
    }

    // マップインデックスからマップ上の座標を取得
    private Vector2 GetMapPosition(Vector2Int index)
    {
        return new Vector2(index.x * _cellSize, index.y * _cellSize);
    }

    // 有効なマップインデックスかどうか
    private bool IsValidMapIndex(Vector2Int index)
    {
        return MapData.IsValidMapIndex(index);
    }

    // 有効な水平壁のインデックスかどうか
    private bool IsValidHorizontalWallIndex(Vector2Int index)
    {
        return MapData.IsValidHorizontalWallIndex(index);
    }
    // 有効な垂直壁のインデックスかどうか
    private bool IsValidVerticalWallIndex(Vector2Int index)
    {
        return MapData.IsValidVerticalWallIndex(index);
    }

    // マスを取得する
    private MapData.Mass GetMass(Vector2Int index)
    {
        return _mapData.GetMass(index);
    }

    private MapData.Mass GetMass(int x, int y)
    {
        return GetMass(new Vector2Int(x, y));
    }

    private bool TryGetMass(Vector2Int index, out MapData.Mass mass)
    {
        return _mapData.TryGetMass(index, out mass);
    }

    // マスの複製
    private MapData.Mass CloneMass(Vector2Int index)
    {
        if (TryGetMass(index, out var mass))
        {
            return mass.Clone();
        }
        return null;
    }
    // マスの上書き
    private void OverwriteMass(Vector2Int index, MapData.Mass mass)
    {
        if (TryGetMass(index, out var targetMass))
        {
            targetMass.Overwrite(mass);
        }
    }

    // x,yのマスにが存在するかどうか
    private bool IsExistMass(Vector2Int index)
    {
        if (TryGetMass(index, out var mass))
        {
            return mass.exist;
        }
        return false;
    }
    // x,yのマスのフラグを設定する
    private void SetMassExist(Vector2Int index, bool value)
    {
        if (TryGetMass(index, out var mass))
        {
            mass.exist = value;
        }
    }

    // 開始マスを設定
    private void SetStartMass(Vector2Int mapIndex)
    {
        if(TryGetMass(_mapData.startMass, out var oldStartMass))
        {
            oldStartMass.start = false;
            oldStartMass.type = MapData.Type.Floor;
        }

        if (TryGetMass(mapIndex, out var mass))
        {
            mass.start = true;
            mass.type = MapData.Type.Start;
        }
        _mapData.startMass = mapIndex;
    }
    // 開始マスを取得
    private Vector2Int GetStartMassIndex()
    {
        return _mapData.startMass;
    }
    // 開始マスが存在するかどうか
    private bool IsExistStartMass(Vector2Int mapIndex)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            return mass.start;
        }
        return false;
    }
    // 開始マスを取得
    private MapData.Mass GetStartMass()
    {
        if (TryGetMass(GetStartMassIndex(), out var mass))
        {
            return mass;
        }
        return null;
    }
    // 開始マスの取得を試みる
    private bool TryGetStartMass(out MapData.Mass mass)
    {
        return TryGetMass(GetStartMassIndex(), out mass);
    }

    // 隠しマスを設定
    private void SetHiddenMass(Vector2Int mapIndex, bool isHidden)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            mass.hidden = isHidden;
        }
    }
    // 隠しマスかを取得
    private bool IsHiddenMass(Vector2Int mapIndex)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            return mass.hidden;
        }
        return false;
    }

    // ボスマスを設定
    private void SetBossMass(Vector2Int mapIndex)
    {
        if (TryGetMass(_mapData.bossMass, out var oldBossMass))
        {
            oldBossMass.isBoss = false;
        }
        if (TryGetMass(mapIndex, out var mass))
        {
            mass.isBoss = true;
        }
        _mapData.bossMass = mapIndex;
    }
    // ボスマスを取得
    private Vector2Int GetBossMassIndex()
    {
        return _mapData.bossMass;
    }
    // ボスマスが存在するかどうか
    private bool IsExistBossMass(Vector2Int mapIndex)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            return mass.isBoss;
        }
        return false;
    }
    // ボスマスを取得
    private MapData.Mass GetBossMass()
    {
        if (TryGetMass(_mapData.bossMass, out var mass))
        {
            return mass;
        }
        return null;
    }
    // ボスマスの取得を試みる
    private bool TryGetBossMass(out MapData.Mass mass)
    {
        return TryGetMass(_mapData.bossMass, out mass);
    }

    /// <summary>
    /// indexの水平壁が存在するかどうか
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private bool IsExistHorizontalWall(Vector2Int index)
    {
        return _mapData.GetHorizontalWall(index);
    }
    // x,yの水平壁のフラグを設定する
    private void SetHorizontalWall(Vector2Int index, bool value)
    {
        _mapData.SetHorizontalWall(index, value);
    }
    // x,yの垂直壁が存在するかどうか
    private bool IsExistVerticalWall(Vector2Int index)
    {
        return _mapData.GetVerticalWall(index);
    }

    // x,yの垂直壁のフラグを設定する
    private void SetVerticalWall(Vector2Int index, bool value)
    {
        _mapData?.SetVerticalWall(index, value);
    }

    // アイテムグループ設定
    private void SetItemGroup(Vector2Int mapIndex, int itemGroup)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            mass.itemGroup = itemGroup;
        }
    }

    private int GetItemGroup(Vector2Int mapIndex)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            return mass.itemGroup;
        }
        return -1;
    }

    // 宝箱を設定
    private void SetBox(Vector2Int mapIndex, int boxId)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            mass.boxId = boxId;
        }
    }

    // 宝箱を取得
    private int GetBox(Vector2Int mapIndex)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            return mass.boxId;
        }
        return -1;
    }

    // ボスマスを設定
    private void SetBoss(Vector2Int mapIndex, bool isBoss)
    {
        if (isBoss &&  TryGetMass(_mapData.bossMass, out var oldMass))
        {
            oldMass.isBoss = false;
            oldMass.type = MapData.Type.Floor;
        }

        if (TryGetMass(mapIndex, out var mass))
        {
            mass.isBoss = isBoss;
            if(isBoss)
            {
                mass.type = MapData.Type.Boss;
                _mapData.bossMass = mapIndex;
            }
            else
            {
                _mapData.bossMass = new Vector2Int(-1, -1);
                mass.type = MapData.Type.Floor;
            }
        }
    }

    // ボスマスを取得
    private bool GetBoss(Vector2Int mapIndex)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            return mass.isBoss;
        }
        return false;
    }

    // シンボルエネミーを設定
    private void SetSimbolEnemy(Vector2Int mapIndex, int enemyId)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            mass.symbolEnemyId = enemyId;
        }
    }

    // シンボルエネミーを取得
    private int GetSimbolEnemy(Vector2Int mapIndex)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            return mass.symbolEnemyId;
        }
        return -1;
    }

    // トラップを設定
    private void SetTrap(Vector2Int mapIndex, int trapId)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            mass.trapId = trapId;
        }
    }

    // トラップを取得
    private int GetTrap(Vector2Int mapIndex)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            return mass.trapId;
        }
        return -1;
    }

    // 階段マスを設定
    private void SetStep(Vector2Int mapIndex, bool isStep)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            mass.isStairs = isStep;
        }
    }

    // 階段マスを取得
    private bool GetStep(Vector2Int mapIndex)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            return mass.isStairs;
        }
        return false;
    }

    // ドアを設定
    private void SetDoor(Vector2Int mapIndex, int doorId)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            mass.doorId = doorId;
        }
    }

    // ドアを取得
    private int GetDoor(Vector2Int mapIndex)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            return mass.doorId;
        }
        return -1;
    }

    // マスの種類を設定
    private void SetMassType(Vector2Int mapIndex, MapData.Type type)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            mass.type = type;
        }
    }

    // マスの種類を取得
    private MapData.Type GetMassType(Vector2Int mapIndex)
    {
        if (TryGetMass(mapIndex, out var mass))
        {
            return mass.type;
        }
        return MapData.Type.Floor;
    }
}
