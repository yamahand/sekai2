using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using LitMotion;
using LitMotion.Extensions;
using LitMotion.Editor;

public class MapEditorUI : EditorWindow
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

    internal struct BoxData
    {
        public BoxType type;
        public Vector2 index;
    }

    private enum EditMode
    {
        Paint,
        Edit,
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


    private LitMotion.MotionHandle motionHandle { get; set; }
    private EditMode editMode { get; set; } = EditMode.Paint;

    private PaintMode paintMode { get; set; } = PaintMode.None;


    // マップデータ
    private MapData mapData { get; set; } = null;

    private Label[,] _boxes = null;

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // rootからMassPanelを取得
        var massPanel = root.Q<VisualElement>("MassPanel");

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
            root.Add(row);
            for (int x = 0; x < massCountX; x++)
            {
                // xは偶数?
                bool isEvenX = x % 2 == 0;
                // yは偶数?
                bool isEvenY = y % 2 == 0;
                // インデックス
                Vector2 index = new Vector2(x, y);

                // x,yともに偶数の時
                var box = new Label();
                box.RegisterCallback<MouseOverEvent>(OnMouseOver);
                box.RegisterCallback<MouseOutEvent>(OnMouseOut);
                box.RegisterCallback<MouseDownEvent>(OnMouseClick);
                box.RegisterCallback<MouseUpEvent>(OnMouseUp);
                box.name = $"Box_{x}_{y}";

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
                    box.style.width = verticalWallThickness;
                    box.style.height = horizontalWallThickness;
                    box.style.backgroundColor = Color.black;
                    box.userData = new BoxData { type = BoxType.none, index = index };
                }
                else if (isEvenX)
                {
                    box.style.width = verticalWallThickness;
                    box.style.height = massSize;
                    box.style.backgroundColor = Color.red;
                    box.userData = new BoxData { type = BoxType.vertical_wall, index = index };
                }
                else if (isEvenY)
                {
                    box.style.width = massSize;
                    box.style.height = horizontalWallThickness;
                    box.style.backgroundColor = Color.blue;
                    box.userData = new BoxData { type = BoxType.horizontal_wall, index = index };
                }
                else
                {
                    box.style.width = massSize;
                    box.style.height = massSize;
                    box.style.backgroundColor = Color.black;
                    box.userData = new BoxData { type = BoxType.floor, index = index };
                }
            }
        }
    }

    // マウスオーバーイベント
    private void OnMouseOver(MouseOverEvent evt)
    {
        var box = evt.target as VisualElement;
        if (box != null)
        {
            var boxData = box.userData as BoxData?;
            if (boxData.HasValue)
            {
                if(paintMode == PaintMode.Floor && boxData.Value.type == BoxType.floor)
                {
                    box.style.backgroundColor = Color.white;
                }
                else if (paintMode == PaintMode.Wall)
                {
                    if (boxData.Value.type == BoxType.horizontal_wall)
                    {
                        box.style.backgroundColor = Color.white;
                    }
                    else if (boxData.Value.type == BoxType.vertical_wall)
                    {
                        box.style.backgroundColor = Color.white;
                    }
                }

                Debug.Log($"マウスオーバー {evt.mousePosition} {evt.pressedButtons} {boxData.Value.type}");
                motionHandle = LMotion.Create(box.style.backgroundColor.value.a, 0.0f, 0.4f)
                    .WithLoops(-1, LoopType.Yoyo)
                    .Bind(x => box.style.backgroundColor = new Color(box.style.backgroundColor.value.r, box.style.backgroundColor.value.g, box.style.backgroundColor.value.b, x));
            }
        }
    }

    // マウスアウトイベント
    private void OnMouseOut(MouseOutEvent evt) {
        var box = evt.target as VisualElement;
        if (box != null)
        {
            var boxData = box.userData as BoxData?;
            if (boxData.HasValue)
            {
                Debug.Log($"マウスアウト {evt.mousePosition} {evt.pressedButtons} {boxData.Value.type}");
                if(motionHandle.IsActive())
                {
                    motionHandle.Cancel();
                    Color color = box.style.backgroundColor.value;
                    color.a = 1.0f;
                    box.style.backgroundColor = color;
                }
            }
        }
    }

    // マウスクリックイベント
    private void OnMouseClick(MouseDownEvent evt)
    {
        var box = evt.target as VisualElement;
        if (box != null)
        {
            var boxData = box.userData as BoxData?;
            if (boxData.HasValue)
            {
                switch (boxData.Value.type)
                {
                    case BoxType.floor:
                        paintMode = PaintMode.Floor;
                        break;
                    case BoxType.horizontal_wall:
                        paintMode = PaintMode.Wall;
                        break;
                    case BoxType.vertical_wall:
                        paintMode = PaintMode.Wall;
                        break;
                    default:
                        paintMode = PaintMode.None;
                        break;
                }
                Debug.Log($"マウスクリック {evt.mousePosition} {evt.pressedButtons} {boxData.Value.type}");
            }
        }
    }

    // マウスアップイベント
    private void OnMouseUp(MouseUpEvent evt)
    {
        var box = evt.target as VisualElement;
        if (box != null)
        {
            var boxData = box.userData as BoxData?;
            if (boxData.HasValue)
            {
                Debug.Log($"マウスアップ {evt.mousePosition} {evt.pressedButtons} {boxData.Value.type}");
            }
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
}
