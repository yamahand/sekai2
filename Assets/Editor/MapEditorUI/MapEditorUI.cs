using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using LitMotion;
using LitMotion.Extensions;
using LitMotion.Editor;
using System.Collections.Generic;

public partial class MapEditorUI : EditorWindow
{
    [MenuItem("Sekai/MapEditorUI")]
    public static void ShowExample()
    {
        MapEditorUI wnd = GetWindow<MapEditorUI>();
        wnd.titleContent = new GUIContent("MapEditorUI");
    }

    internal enum BoxType
    {
        // 床
        floor,
        // 水平壁
        horizontal_wall,
        // 垂直壁
        vertical_wall,
        // なし
        none,
    }

    internal class BoxData
    {
        public BoxType type;
        public Vector2Int index;
    }

    // ペイントモード
    private enum PaintMode
    {
        // 床
        Floor,
        // 壁
        Wall,
        // なし
        None,
    }

    // マスの種類の一文字ラベル配列変数
    public static readonly string[] floarLabel = new string[]
    {
        "",
        "扉",
        "階",
        "罠",
        "物",
        "宝",
        "敵",
        "強",
        "始",
    };

    // マスの種類の一文字ラベルを取得
    public static string GetFloarLabel(int type)
    {
        if (floarLabel.IsValid(type))
        {
            return floarLabel[type];
        }
        return "";
    }

    // マウスオーバーしているマスのモーションハンドル
    private LitMotion.MotionHandle mouceOverMotionHandle { get; set; }

    // 選択しているマスのモーションハンドル
    private LitMotion.MotionHandle selectedMassMotionHandle { get; set; }


    private PaintMode paintMode { get; set; } = PaintMode.None;

    // マップデータ
    private MapData mapData { get; set; } = null;

    private Label[,] _boxes = null;

    // 現在選択しているマスのインデックス
    private Vector2Int selectedMassIndex { get; set; } = new Vector2Int(-1, -1);
    private BoxData selectedBoxData { get; set; } = null;

    [SerializeField]
    private VisualTreeAsset _uxmlTree;

    // マップ名を指定するテキストフィールド
    private TextField _mapNameTextField;
    // 選択しているマスのインデックスを表示するフィールド
    private Vector2IntField _selectedIndexField;
    // 選択しているマスの種類を選択するドロップダウン
    private DropdownField _typeDropdown;


    // 実行するコマンドキュー
    private Queue<IMapEditorCommand> _commandQueue = new Queue<IMapEditorCommand>();
    // コマンドの実行履歴
    private readonly Stack<IMapEditorCommand> _commandHistory = new Stack<IMapEditorCommand>();
    // コマンドの取り消し履歴
    private readonly Stack<IMapEditorCommand> _undoHistory = new Stack<IMapEditorCommand>();

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        if (_uxmlTree != null)
        {
            root.Add(_uxmlTree.Instantiate());
        }
        else
        {
            // ログを表示して終了
            Debug.LogError("UXMLファイルが読み込まれていません");
            return;
        }

        // rootにキーボードイベントを登録
        root.RegisterCallback<KeyDownEvent>(OnKeyDown);

        // rootのフォーカスを有効にする
        root.focusable = true;
        root.Focus();

        // マップデータ初期化
        mapData = new MapData();

        // ファイル操作のボタンのコールバック登録
        var saveButton = root.Q<Button>("SaveButton");
        saveButton.clicked += SaveMapData;
        var openButton = root.Q<Button>("OpenButton");
        openButton.clicked += OpenMapData;
        var newButton = root.Q<Button>("NewButton");
        newButton.clicked += CreateNewMapData;

        // Undo, Redoボタンのコールバック登録
        var undoButton = root.Q<Button>("UndoButton");
        undoButton.clicked += Undo;
        var redoButton = root.Q<Button>("RedoButton");
        redoButton.clicked += Redo;

        // マップ名を指定するテキストフィールドの取得
        _mapNameTextField = root.Q<TextField>("MapName");
        _mapNameTextField.value = mapData.mapName;
        _mapNameTextField.RegisterValueChangedCallback(evt =>
        {
            mapData.mapName = evt.newValue;
        });

        // rootからEditorPanelを取得
        var editorPanel = root.Q<VisualElement>("EditorPanel");

        var editorSplitView = new TwoPaneSplitView(
           fixedPaneIndex: 0,   // 固定サイズとする箇所のIndex(0, 1...
           fixedPaneStartDimension: 640, // 固定サイズの初期幅 or 高さ
           TwoPaneSplitViewOrientation.Horizontal); // 分割方向
        editorSplitView.style.minHeight = 640;

        editorPanel.Add(editorSplitView);

        var leftPanel = new VisualElement();
        leftPanel.style.minWidth = 640;
        leftPanel.style.backgroundColor = Color.black;
        editorSplitView.Add(leftPanel);

        var massPanel = new VisualElement();
        massPanel.style.minWidth = 320;
        massPanel.style.minHeight = 640;
        leftPanel.Add(massPanel);

        var inspectorPanel = new VisualElement();
        inspectorPanel.style.backgroundColor = Color.black;
        editorSplitView.Add(inspectorPanel);

        _selectedIndexField = new Vector2IntField();
        _selectedIndexField.label = "Selected Index";
        _selectedIndexField.value = selectedMassIndex;
        inspectorPanel.Add(_selectedIndexField);

        // マスの種類を選択するドロップダウンの作成
        var typeDropdown = new DropdownField("マスの種類");
        if (typeDropdown != null)
        {
            inspectorPanel.Add(typeDropdown);
            var typeCount = MapData.TypeNames.Length;
            for (int i = 0; i < typeCount; i++)
            {
                typeDropdown.choices.Add(MapData.TypeNames[i]);
            }
            typeDropdown.RegisterValueChangedCallback(evt =>
            {
                Debug.Log($"TypeDropdown {evt.newValue}");

                if (selectedBoxData != null)
                {
                    if (selectedBoxData.type == BoxType.floor)
                    {
                        _commandQueue.Enqueue(new ChangeMassTypeCommand(this, selectedBoxData, (MapData.Type)_typeDropdown.index));
                    }
                }
            });
            typeDropdown.index = 0;
            _typeDropdown = typeDropdown;
        }

        var autoArrangeButton = new Button(() =>
        {
            AutoArrangeWalls();
        });
        autoArrangeButton.text = "壁の整理";
        inspectorPanel.Add(autoArrangeButton);

        // マスの数
        int massCountX = MapData.MassCount.x + MapData.VerticalWallCount.x;
        int massCountY = MapData.MassCount.y + MapData.HorizontalWallCount.y;

        // 水平壁の厚み
        int horizontalWallThickness = 5;
        // 垂直壁の厚み
        int verticalWallThickness = 5;
        // マスのサイズ
        int massSize = 50;

        _boxes = new Label[massCountY, massCountX];

        for (int y = 0; y < massCountY; y++)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.name = $"Row_{y}";
            massPanel.Add(row);
            for (int x = 0; x < massCountX; x++)
            {
                // xは偶数?
                bool isEvenX = x % 2 == 0;
                // yは偶数?
                bool isEvenY = y % 2 == 0;
                // インデックス
                Vector2Int index = new Vector2Int(x, y);

                var box = new Label();
                box.RegisterCallback<MouseOverEvent>(OnMouseOver);
                box.RegisterCallback<MouseOutEvent>(OnMouseOut);
                box.RegisterCallback<MouseDownEvent>(OnMouseClick);
                box.RegisterCallback<MouseUpEvent>(OnMouseUp);
                box.name = $"Box_{x}_{y}";
                box.style.unityTextAlign = TextAnchor.MiddleCenter;
                box.style.fontSize = 24;
                box.style.color = Color.black;

                int borderThickness = 1;

                box.style.borderBottomColor = Color.gray;
                box.style.borderBottomWidth = borderThickness;
                box.style.borderTopColor = Color.gray;
                box.style.borderTopWidth = borderThickness;
                box.style.borderLeftColor = Color.gray;
                box.style.borderLeftWidth = borderThickness;
                box.style.borderRightColor = Color.gray;
                box.style.borderRightWidth = borderThickness;
                //box.text = $"{x},{y}";
                row.Add(box);
                _boxes[y, x] = box;


                if (isEvenX && isEvenY)
                {
                    // x,yともに偶数の時は空白
                    box.style.width = verticalWallThickness;
                    box.style.height = horizontalWallThickness;
                    box.style.backgroundColor = Color.black;
                    box.userData = new BoxData { type = BoxType.none, index = index };
                }
                else if (isEvenX)
                {
                    // xが偶数の時は垂直壁
                    box.style.width = verticalWallThickness;
                    box.style.height = massSize;
                    box.style.backgroundColor = Color.black;
                    box.userData = new BoxData { type = BoxType.vertical_wall, index = index };
                }
                else if (isEvenY)
                {
                    // yが偶数の時は水平壁
                    box.style.width = massSize;
                    box.style.height = horizontalWallThickness;
                    box.style.backgroundColor = Color.black;
                    box.userData = new BoxData { type = BoxType.horizontal_wall, index = index };
                }
                else
                {
                    // それ以外は床
                    box.style.width = massSize;
                    box.style.height = massSize;
                    box.style.backgroundColor = Color.black;
                    box.userData = new BoxData { type = BoxType.floor, index = index };
                    box.text = "";
                }
            }
        }
    }

    // 更新処理
    private void Update()
    {
        // 選択しているマスの情報を表示
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
        }
    }

    // Undo処理
    private void Undo()
    {
        Debug.Log("Undo");
        if (_commandHistory.Count > 0)
        {
            var command = _commandHistory.Pop();
            command.Undo();
            _undoHistory.Push(command);
        }
    }
    // Redo処理
    private void Redo()
    {
        Debug.Log("Redo");
        if (_undoHistory.Count > 0)
        {
            var command = _undoHistory.Pop();
            command.Execute();
            _commandHistory.Push(command);
        }
    }

    // EventBaseからBoxDataの取得を試みる
    private bool TryGetBoxData(EventBase evt, out BoxData boxData)
    {
        boxData = null;
        if (evt.target is VisualElement box)
        {
            if (box.userData is BoxData data)
            {
                boxData = data;
                return true;
            }
        }
        return false;
    }

    // マウスオーバーイベント
    private void OnMouseOver(MouseOverEvent evt)
    {
        if (TryGetBoxData(evt, out var boxData))
        {
            // ペイントモードとboxData.typeが一致している場合
            if ((paintMode == PaintMode.Floor && boxData.type == BoxType.floor) || (paintMode == PaintMode.Wall && (boxData.type == BoxType.horizontal_wall || boxData.type == BoxType.vertical_wall)))
            {
                // 左クリックかどうか
                bool isLeftClick = evt.pressedButtons == 1;
                PlaceCommand(boxData, isLeftClick);
            }

            mouceOverMotionHandle = LMotion.Create(Color.gray, GetMassIndex(boxData) == selectedMassIndex ? Color.yellow : Color.white, 0.4f)
                .WithLoops(-1, LoopType.Yoyo)
                .Bind(x => ChangeBorder(boxData.index, x));
        }
    }

    // マウスアウトイベント
    private void OnMouseOut(MouseOutEvent evt)
    {
        if (TryGetBoxData(evt, out var boxData))
        {
            if (mouceOverMotionHandle.IsActive())
            {
                mouceOverMotionHandle.Cancel();
                ChangeBorder(boxData.index, Color.gray);
            }
        }
    }

    // マウスクリックイベント
    private void OnMouseClick(MouseDownEvent evt)
    {
        if (evt.target is VisualElement box)
        {
            if (box.userData is BoxData boxData)
            {
                switch (boxData.type)
                {
                    case BoxType.floor:
                        paintMode = PaintMode.Floor;
                        selectedMassIndex = GetMassIndex(boxData);
                        _selectedIndexField.value = selectedMassIndex;
                        ShowSelectedMassInfo();
                        break;
                    case BoxType.horizontal_wall:
                    case BoxType.vertical_wall:
                        paintMode = PaintMode.Wall;
                        break;
                    default:
                        paintMode = PaintMode.None;
                        break;
                }
                Debug.Log($"マウスクリック {evt.mousePosition} {evt.pressedButtons} {boxData.type}");
                if (paintMode != PaintMode.None)
                {
                    PlaceCommand(boxData, evt.pressedButtons == 1);
                }
            }
        }
    }

    // マウスアップイベント
    private void OnMouseUp(MouseUpEvent evt)
    {
        if(evt.button == 0 && TryGetBoxData(evt, out var boxData))
        {
            if (selectedMassMotionHandle.IsActive())
            {
                selectedMassMotionHandle.Cancel();
                if (selectedBoxData != null)
                {
                    var prevSelectedBoxIndex = selectedBoxData.index;
                    ChangeBoxColor(prevSelectedBoxIndex, Color.white);
                }
            }

            selectedBoxData = boxData;

            // マスが選択されたらマスの色を変更する
            selectedMassMotionHandle = LMotion.Create(Color.white, Color.grey, 0.4f)
                .WithLoops(-1, LoopType.Yoyo)
                .Bind(x => ChangeBoxColor(boxData.index, x));
        }
        paintMode = PaintMode.None;
    }

    private void OnDragEnter(MouseOverEvent evt)
    {
        var droppable = evt.target as VisualElement;
        if (droppable != null)
        {
            var userData = droppable.userData as Vector2Int?;
            if (userData.HasValue)
            {
                Debug.Log($"ドラッグされたものが入ってきた時の処理 {evt.mousePosition} {evt.pressedButtons} {evt.target} userData: {userData.Value}");
            }
        }

        Debug.Log($"ドラッグされたものが入ってきた時の処理 {evt.mousePosition} {evt.pressedButtons} {evt.target}");
    }

    private void OnDragLeave(DragLeaveEvent evt)
    {
        Debug.Log("ドラッグされたものが出ていった時の処理");
    }

    private void OnDragUpdate(DragUpdatedEvent evt)
    {
        // ドラッグ中の処理
        DragAndDrop.visualMode = DragAndDropVisualMode.Copy; // ドラッグ中はカーソルのアイコンを変える
    }

    private void OnDragPerform(DragPerformEvent evt)
    {
        Debug.Log("ドロップされた時の処理");

        // ドロップを受け入れる
        DragAndDrop.AcceptDrag();

        // ProjectウィンドウからアセットをD&Dしたり、Unity外からファイルをD&Dしたりしたらpathsにファイルパスが入ってくる
        foreach (var path in DragAndDrop.paths)
            Debug.Log(path);

        // シーンからオブジェクトをD&Dしたり、ProjectウィンドウからアセットをD&DしたらobjectReferencesにオブジェクトが入ってくる
        foreach (var obj in DragAndDrop.objectReferences)
            Debug.Log(obj);

        // DragAndDrop.SetGenericDataで設定したデータを取得する
        var genericData = DragAndDrop.GetGenericData("test");
        Debug.Log(genericData);
    }

    private void OnDragExited(DragExitedEvent evt)
    {
        Debug.Log("ドラッグ&ドロップの処理が終了した時の処理");
    }

    private void OnTypeToggleChange(ClickEvent evt)
    {
        var toggle = evt.target as Button;
        //if (toggle != null)
        {
            var type = (MapData.Type)toggle.userData;
            Debug.Log($"TypeToggleChange {type}");
        }
    }

    void OnKeyDown(KeyDownEvent evt)
    {
        if (evt.actionKey && evt.keyCode == KeyCode.Z)
        {
            Undo();
            evt.StopImmediatePropagation();
        }
        else if (evt.actionKey && evt.keyCode == KeyCode.Y)
        {
            Redo();
            evt.StopImmediatePropagation();
        }
    }    

    // ボックスからマスを取得
    private MapData.Mass GetMass(VisualElement box)
    {
        if (box != null && box.userData is BoxData boxData)
        {
            return GetMass(GetMassIndex(boxData.index));
        }
        return null;
    }

    // マスのインデックスからマスを取得
    private MapData.Mass GetMass(Vector2Int massIndex)
    {
        if (mapData != null)
        {
            return mapData.GetMass(massIndex);
        }
        return null;
    }

    // マスの取得を試みる
    private bool TryGetMass(Vector2Int massIndex, out MapData.Mass mass)
    {
        mass = GetMass(massIndex);
        return mass != null;
    }

    // Boxからマスのインデックスを取得
    private Vector2Int GetMassIndex(VisualElement box)
    {
        if (box.userData is BoxData boxData)
        {
            return GetMassIndex(boxData);
        }
        return new Vector2Int(-1, -1);
    }

    // BoxData.indexからマスのインデックスを取得
    private Vector2Int GetMassIndex(BoxData boxData)
    {
        return GetMassIndex(boxData.index);
    }

    private Vector2Int GetMassIndex(Vector2Int index)
    {
        return new Vector2Int(index.x / 2, index.y / 2);
    }

    // BoxData.indexから壁のインデックスを取得
    private Vector2Int GetWallIndex(BoxData boxData)
    {
        return GetWallIndex(boxData.index);
    }

    // indexから壁のインデックスを取得
    private Vector2Int GetWallIndex(Vector2Int index)
    {
        // 水平、垂直どっちも同じ計算で求められる
        return new Vector2Int(index.x / 2, index.y / 2);
    }

    // Boxの変更すべき色を取得
    private Color GetBoxColor(BoxData boxData)
    {
        if (boxData == null)
        {
            return Color.black;
        }

        switch (boxData.type)
        {
            case BoxType.floor:
                if(TryGetMass(GetMassIndex(boxData), out var mass))
                {
                    if(!mass.exist)
                    {
                        return Color.black;
                    }

                    if (IsExistFloorBoxAround(boxData))
                    {
                        return Color.red;
                    }

                    return Color.white;
                }
                return Color.black;
            case BoxType.horizontal_wall:
            case BoxType.vertical_wall:
                return Color.black;
            default:
                return Color.black;
        }
    }

    // Boxの色を変更する
    private void ChangeBoxColor(Vector2Int index, Color color)
    {
        if (_boxes.TryGetT(index, out var box))
        {
            box.style.backgroundColor = color;
        }
    }

    // Boxの色を変更する
    private void ChangeBoxColor(VisualElement box, Color color)
    {
        if (box != null)
        {
            box.style.backgroundColor = color;
        }
    }

    // Boxのラベルを変更する
    private void ChangeBoxLabel(Vector2Int index, string label)
    {
        if (_boxes.TryGetT(index, out var box))
        {
            box.text = label;
        }
    }

    // マウスオーバーしているマスのボーダー色を変更する
    private void ChangeBorder(Vector2Int index, Color color)
    {
        if (_boxes.TryGetT(index, out var box))
        {
            ChangeBorder(box, color);
        }
    }

    private void ChangeBorder(VisualElement box, Color color)
    {
        if (box == null)
        {
            return;
        }

        box.style.borderBottomColor = color;
        box.style.borderTopColor = color;
        box.style.borderLeftColor = color;
        box.style.borderRightColor = color;
    }

    // 選択しているマスの情報を取得
    private MapData.Mass GetSelectedMass()
    {
        if (mapData != null)
        {
            return mapData.GetMass(selectedMassIndex);
        }
        return null;
    }

    // 選択しているマスの情報の取得を試みる
    private bool TryGetSelectedMass(out MapData.Mass mass)
    {
        mass = GetSelectedMass();
        return mass != null;
    }

    // 選択しているマスの情報を表示
    private void ShowSelectedMassInfo()
    {
        if (TryGetSelectedMass(out var mass))
        {
            // UIに表示
            _typeDropdown.index = (int)mass.type;
        }
    }

    // 上下左右のどれかに床が配置されているか
    private bool IsExistFloorBoxAround(BoxData boxData)
    {
        var index = GetMassIndex(boxData);
        if (mapData != null)
        {
            // 上下左右のマスを取得
            var upIndex = new Vector2Int(index.x, index.y - 1);
            var downIndex = new Vector2Int(index.x, index.y + 1);
            var leftIndex = new Vector2Int(index.x - 1, index.y);
            var rightIndex = new Vector2Int(index.x + 1, index.y);
            // 上下左右のマスの床が存在するか
            return mapData.GetMass(upIndex)?.exist == true || mapData.GetMass(downIndex)?.exist == true || mapData.GetMass(leftIndex)?.exist == true || mapData.GetMass(rightIndex)?.exist == true;
        }
        return false;
    }

    // マス、壁の配置コマンドをキューに積む
    private void PlaceCommand(BoxData boxData, bool isPlace)
    {
        if (mapData != null && boxData != null)
        {
            switch (boxData.type)
            {
                case BoxType.floor:
                    _commandQueue.Enqueue(new PlaceMassCommand(this, boxData, isPlace));
                    break;
                case BoxType.horizontal_wall:
                    _commandQueue.Enqueue(new HorizontalWallCommand(this, boxData, isPlace));
                    break;
                case BoxType.vertical_wall:
                    _commandQueue.Enqueue(new VerticalWallCommand(this, boxData, isPlace));
                    break;
            }
        }
    }

    // 壁の整理
    private void AutoArrangeWalls()
    {
        if (mapData != null)
        {
            var autoArrangeWallsCommand = new AutoArrangeWallsCommand(this);
            for (int y = 0; y < _boxes.GetLength(0); y++)
            {
                for (int x = 0; x < _boxes.GetLength(1); x++)
                {
                    Vector2Int boxIndex = new Vector2Int(x, y);

                    // 各要素にアクセス
                    var box = _boxes.GetT(boxIndex);
                    BoxData boxData = box.userData as BoxData;

                    // 上下左右のBoxの取得
                    _boxes.TryGetT(new Vector2Int(x, y - 1), out var upBox);
                    _boxes.TryGetT(new Vector2Int(x, y + 1), out var downBox);
                    _boxes.TryGetT(new Vector2Int(x - 1, y), out var leftBox);
                    _boxes.TryGetT(new Vector2Int(x + 1, y), out var rightBox);

                    // 壁ではなかったら次へ
                    if (boxData == null || (boxData.type != BoxType.horizontal_wall && boxData.type != BoxType.vertical_wall))
                    {
                        continue;
                    }

                    var wallIndex = GetWallIndex(boxData);

                    // 水平壁の整理
                    if (boxData.type == BoxType.horizontal_wall)
                    {
                        // 水平壁の情報を取得
                        var horizontalWall = mapData.GetHorizontalWall(wallIndex);

                        // 上のマスを取得
                        MapData.Mass upMass = GetMass(upBox);

                        // 下のマスを取得
                        MapData.Mass downMass = GetMass(downBox);

                        if (horizontalWall)
                        {
                            // 上下のマスが存在しない場合
                            if ((upMass == null || !upMass.exist) && (downMass == null || !downMass.exist))
                            {
                                autoArrangeWallsCommand.AddHorizontalWall(wallIndex, false);
                            }
                        }
                        else
                        {
                            // 上下のどちらかだけ存在する場合
                            if ((upMass == null || !upMass.exist) != (downMass == null || !downMass.exist))
                            {
                                autoArrangeWallsCommand.AddHorizontalWall(wallIndex, true);
                            }
                        }
                    }

                    // 垂直壁の整理
                    if (boxData.type == BoxType.vertical_wall)
                    {
                        // 垂直壁の情報を取得
                        var verticalWall = mapData.GetVerticalWall(wallIndex);
                        // 左のマスを取得
                        MapData.Mass leftMass = GetMass(leftBox);
                        // 右のマスを取得
                        MapData.Mass rightMass = GetMass(rightBox);
                        if (verticalWall)
                        {
                            // 左右のマスが存在しない場合
                            if ((leftMass == null || !leftMass.exist) && (rightMass == null || !rightMass.exist))
                            {
                                autoArrangeWallsCommand.AddVerticalWall(wallIndex, false);
                            }
                        }
                        else
                        {
                            // 左右のどちらかだけ存在する場合
                            if ((leftMass == null || !leftMass.exist) != (rightMass == null || !rightMass.exist))
                            {
                                autoArrangeWallsCommand.AddVerticalWall(wallIndex, true);
                            }
                        }
                    }
                }
            }
            _commandQueue.Enqueue(autoArrangeWallsCommand);
        }
    }

    private void UpdateBox(Vector2Int index)
    {
        if (_boxes.TryGetT(index, out var box))
        {
            if (box.userData is BoxData boxData)
            {
                UpdateBox(boxData);
            }
        }
    }

    // マス更新
    private void UpdateBox(BoxData boxData)
    {
        if (boxData == null)
        {
            return;
        }

        if (_boxes.TryGetT(boxData.index, out var box))
        {
            // boxData.typeで処理を分ける
            switch (boxData.type)
            {
                case BoxType.floor:
                    if (mapData.TryGetMass(GetMassIndex(boxData), out var mass))
                    {
                        // 周囲に床があるか
                        bool isExistFloorAround = IsExistFloorBoxAround(boxData);

                        ChangeBoxColor(boxData.index, mass.exist ? isExistFloorAround ? Color.white : Color.red : Color.black);
                        ChangeBoxLabel(boxData.index, GetFloarLabel((int)mass.type));
                    }
                    break;
                case BoxType.horizontal_wall:
                    if (mapData.GetHorizontalWall(GetWallIndex(boxData)))
                    {
                        ChangeBoxColor(boxData.index, Color.red);
                    }
                    else
                    {
                        ChangeBoxColor(boxData.index, Color.black);
                    }
                    break;
                case BoxType.vertical_wall:
                    if (mapData.GetVerticalWall(GetWallIndex(boxData)))
                    {
                        ChangeBoxColor(boxData.index, Color.red);
                    }
                    else
                    {
                        ChangeBoxColor(boxData.index, Color.black);
                    }
                    break;
            }
        }
    }

    // 指定したボックスとその周囲のボックスの更新
    private void UpdateAroundBoxes(BoxData boxData)
    {
        if (boxData == null)
        {
            return;
        }
        // 上下左右のボックスの更新
        UpdateBox(new Vector2Int(boxData.index.x, boxData.index.y - 2));
        UpdateBox(new Vector2Int(boxData.index.x, boxData.index.y + 2));
        UpdateBox(new Vector2Int(boxData.index.x - 2, boxData.index.y));
        UpdateBox(new Vector2Int(boxData.index.x + 2, boxData.index.y));
    }

    // 全てのBoxの更新
    private void UpdateAllBoxes()
    {
        for (int y = 0; y < _boxes.GetLength(0); y++)
        {
            for (int x = 0; x < _boxes.GetLength(1); x++)
            {
                if (_boxes.TryGetT(new Vector2Int(x, y), out var box))
                {
                    if (box.userData is BoxData boxData)
                    {
                        UpdateBox(boxData);
                    }
                }
            }
        }
    }
}
