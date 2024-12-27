using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEditor;
using UnityEngine;

public partial class MapEditorWindow
{
    /// <summary>
    /// コマンドのインターフェイス
    /// </summary>
    internal interface IMapEditorCommand
    {
        void Execute();
        void Undo();
        SelectionMode selectionMode { get; }
    }

    /// <summary>
    /// コマンドのベースクラス
    /// </summary>
    private abstract class MapEditorCommandBase : IMapEditorCommand
    {
        public abstract void Execute();
        public abstract void Undo();
        public abstract SelectionMode selectionMode { get; }

        // コンストラクタ
        protected MapEditorCommandBase(MapEditorWindow window, Vector2Int mapIndex)
        {
            _window = window;
            _mapIndex = mapIndex;
        }

        protected MapEditorWindow _window;
        protected Vector2Int _mapIndex;
    }

    // マスが存在するかを変更するコマンド
    private class PlaceCellCommand : MapEditorCommandBase
    {
        private bool _isPlace;
        private MapData.Mass _massBefore;
        public PlaceCellCommand(MapEditorWindow window, Vector2Int mapIndex, bool isPlace) : base(window, mapIndex)
        {
            _isPlace = isPlace;
        }

        public override SelectionMode selectionMode { get { return SelectionMode.Floor; } }

        public override void Execute()
        {
            _massBefore = _window.CloneMass(_mapIndex);
            _window.SetMassExist(_mapIndex, _isPlace);
        }

        public override void Undo()
        {
            _window.OverwriteMass(_mapIndex, _massBefore);
        }
    }

    // マスの隠し属性を変更するコマンド
    private class ChangeHiddenCommand : MapEditorCommandBase
    {
        private bool _isHidden;
        private bool _isHiddenBefore;
        public ChangeHiddenCommand(MapEditorWindow window, Vector2Int mapIndex, bool isHidden) : base(window, mapIndex)
        {
            _isHidden = isHidden;
        }
        public override SelectionMode selectionMode { get { return SelectionMode.Floor; } }
        public override void Execute()
        {
            _isHiddenBefore = _window.IsHiddenMass(_mapIndex);
            _window.SetHiddenMass(_mapIndex, _isHidden);
        }
        public override void Undo()
        {
            _window.SetHiddenMass(_mapIndex, _isHiddenBefore);
        }
    }

    // 開始マスを変更するコマンド
    private class ChangeStartMassCommand : MapEditorCommandBase
    {
        private Vector2Int _prevStartMassIndex;
        private MapData.Mass _selectMassBefore;

        public ChangeStartMassCommand(MapEditorWindow window, Vector2Int mapIndex) : base(window, mapIndex)
        {
        }
        public override SelectionMode selectionMode { get { return SelectionMode.Edit; } }
        public override void Execute()
        {
            _prevStartMassIndex = _window.GetStartMassIndex();
            _selectMassBefore = _window.CloneMass(_mapIndex);
            _window.SetStartMass(_mapIndex);
        }
        public override void Undo()
        {
            _window.SetStartMass(_prevStartMassIndex);
            _window.OverwriteMass(_mapIndex, _selectMassBefore);
        }
    }

    // 壁の存在を変更するコマンド
    private class ChangeWallCommand : MapEditorCommandBase
    {
        private readonly Vector2Int _horizontalWall;
        private readonly Vector2Int _verticalWall;
        private readonly bool _isPlace;
        private bool _isHorizontalWallBefore;
        private bool _isVerticalWallBefore;


        public override SelectionMode selectionMode { get { return SelectionMode.Wall; } }
        public ChangeWallCommand(MapEditorWindow window, Vector2Int horizontalIndex, Vector2Int verticalIndex, bool isWall) : base(window, -Vector2Int.one)
        {
            _horizontalWall = horizontalIndex;
            _verticalWall = verticalIndex;
            _isPlace = isWall;
        }
        public override void Execute()
        {
            _isHorizontalWallBefore = _window.IsExistHorizontalWall(_horizontalWall);
            _isVerticalWallBefore = _window.IsExistVerticalWall(_verticalWall);
            _window.SetHorizontalWall(_horizontalWall, _isPlace);
            _window.SetVerticalWall(_verticalWall, _isPlace);
        }
        public override void Undo()
        {
            _window.SetHorizontalWall(_horizontalWall, _isHorizontalWallBefore);
            _window.SetVerticalWall(_verticalWall, _isVerticalWallBefore);
        }
    }

    // 水平壁、垂直壁のインデックスの配列を受け取りまとめて壁を設置するコマンド
    private class ChangeWallListCommand : MapEditorCommandBase
    {
        private readonly List<Vector2Int> _horizontalWallList;
        private readonly List<Vector2Int> _verticalWallList;
        private readonly bool _isPlace;
        private List<bool> _isHorizontalWallBefore;
        private List<bool> _isVerticalWallBefore;
        public override SelectionMode selectionMode { get { return SelectionMode.Wall; } }
        public ChangeWallListCommand(MapEditorWindow window, List<Vector2Int> horizontalWallList, List<Vector2Int> verticalWallList, bool isPlace) : base(window, -Vector2Int.one)
        {
            _horizontalWallList = horizontalWallList;
            _verticalWallList = verticalWallList;
            _isPlace = isPlace;
        }
        public override void Execute()
        {
            _isHorizontalWallBefore = new List<bool>();
            _isVerticalWallBefore = new List<bool>();
            foreach (var index in _horizontalWallList)
            {
                _isHorizontalWallBefore.Add(_window.IsExistHorizontalWall(index));
                _window.SetHorizontalWall(index, _isPlace);
            }
            foreach (var index in _verticalWallList)
            {
                _isVerticalWallBefore.Add(_window.IsExistVerticalWall(index));
                _window.SetVerticalWall(index, _isPlace);
            }
        }
        public override void Undo()
        {
            for (int i = 0; i < _horizontalWallList.Count; i++)
            {
                _window.SetHorizontalWall(_horizontalWallList[i], _isHorizontalWallBefore[i]);
            }
            for (int i = 0; i < _verticalWallList.Count; i++)
            {
                _window.SetVerticalWall(_verticalWallList[i], _isVerticalWallBefore[i]);
            }
        }
    }

    // アイテムを設置、削除するコマンド
    private class PlaceItemCommand : MapEditorCommandBase
    {
        // 
        private readonly int _itemGroup;
        private int _isItemBefore;

        public override SelectionMode selectionMode { get { return SelectionMode.Edit; } }
        public PlaceItemCommand(MapEditorWindow window, Vector2Int mapIndex, int itemGroup) : base(window, mapIndex)
        {
            _itemGroup = itemGroup;
        }
        public override void Execute()
        {
            _isItemBefore = _window.GetItemGroup(_mapIndex);
        }
        public override void Undo()
        {
            _window.SetItemGroup(_mapIndex, _isItemBefore);
        }
    }

    // ボックスを設置、削除するコマンド
    private class PlaceBoxCommand : MapEditorCommandBase
    {
        private readonly int _boxId;
        private int _boxIdBefore;

        public override SelectionMode selectionMode { get { return SelectionMode.Edit; } }
        public PlaceBoxCommand(MapEditorWindow window, Vector2Int mapIndex, int boxId) : base(window, mapIndex)
        {
            _boxId = boxId;
        }
        public override void Execute()
        {
            _boxIdBefore = _window.GetBox(_mapIndex);
            _window.SetBox(_mapIndex, _boxId);
        }
        public override void Undo()
        {
            _window.SetBox(_mapIndex, _boxIdBefore);
        }
    }

    // ボスマスを設定、削除するコマンド
    private class PlaceBossCommand : MapEditorCommandBase
    {
        private readonly bool _isBoss;
        private bool _isBossBefore;

        public override SelectionMode selectionMode { get { return SelectionMode.Edit; } }
        public PlaceBossCommand(MapEditorWindow window, Vector2Int mapIndex, bool isBoss) : base(window, mapIndex)
        {
            _isBoss = isBoss;
        }
        public override void Execute()
        {
            _isBossBefore = _window.GetBoss(_mapIndex);
            _window.SetBoss(_mapIndex, _isBoss);
        }
        public override void Undo()
        {
            _window.SetBoss(_mapIndex, _isBossBefore);
        }
    }

    // シンボルエネミーを設置、削除するコマンド
    private class PlaceSymbolEnemyCommand : MapEditorCommandBase
    {
        private readonly int _enemyId;
        private int _enemyIdBefore;

        public override SelectionMode selectionMode { get { return SelectionMode.Edit; } }
        public PlaceSymbolEnemyCommand(MapEditorWindow window, Vector2Int mapIndex, int enemyId) : base(window, mapIndex)
        {
            _enemyId = enemyId;
        }
        public override void Execute()
        {
            _enemyIdBefore = _window.GetSimbolEnemy(_mapIndex);
            _window.SetSimbolEnemy(_mapIndex, _enemyId);
        }
        public override void Undo()
        {
            _window.SetSimbolEnemy(_mapIndex, _enemyIdBefore);
        }
    }

    // トラップを設置、削除するコマンド
    private class PlaceTrapCommand : MapEditorCommandBase
    {
        private readonly int _trapId;
        private int _trapIdBefore;

        public override SelectionMode selectionMode { get { return SelectionMode.Edit; } }
        public PlaceTrapCommand(MapEditorWindow window, Vector2Int mapIndex, int trapId) : base(window, mapIndex)
        {
            _trapId = trapId;
        }
        public override void Execute()
        {
            _trapIdBefore = _window.GetTrap(_mapIndex);
            _window.SetTrap(_mapIndex, _trapId);
        }
        public override void Undo()
        {
            _window.SetTrap(_mapIndex, _trapIdBefore);
        }
    }

    // 階段マスを設定、削除するコマンド
    private class PlaceStepCommand : MapEditorCommandBase
    {
        private readonly bool _isStep;
        private bool _isStepBefore;

        public override SelectionMode selectionMode { get { return SelectionMode.Edit; } }
        public PlaceStepCommand(MapEditorWindow window, Vector2Int mapIndex, bool isStep) : base(window, mapIndex)
        {
            _isStep = isStep;
        }
        public override void Execute()
        {
            _isStepBefore = _window.GetStep(_mapIndex);
            _window.SetStep(_mapIndex, _isStep);
        }
        public override void Undo()
        {
            _window.SetStep(_mapIndex, _isStepBefore);
        }
    }

    // ドアを設置、削除するコマンド
    private class PlaceDoorCommand : MapEditorCommandBase
    {
        private readonly int _doorId;
        private int _doorIdBefore;

        public override SelectionMode selectionMode { get { return SelectionMode.Edit; } }
        public PlaceDoorCommand(MapEditorWindow window, Vector2Int mapIndex, int doorId) : base(window, mapIndex)
        {
            _doorId = doorId;
        }
        public override void Execute()
        {
            _doorIdBefore = _window.GetDoor(_mapIndex);
            _window.SetDoor(_mapIndex, _doorId);
        }
        public override void Undo()
        {
            _window.SetDoor(_mapIndex, _doorIdBefore);
        }
    }

    // マスの種類を変更するコマンド
    private class ChangeMassTypeCommand : MapEditorCommandBase
    {
        public override SelectionMode selectionMode { get { return SelectionMode.Edit; } }
        private MapData.Mass.Type _type;
        private MapData.Mass.Type _typeBefore;
        private Vector2Int _prevMapIndex;

        // コンストラクタ
        public ChangeMassTypeCommand(MapEditorWindow window, Vector2Int mapIndex, MapData.Mass.Type type) : base(window, mapIndex)
        {
            _type = type;
        }
        public override void Execute()
        {
            if(_type == MapData.Mass.Type.Start)
            {
                _prevMapIndex = _window.GetStartMassIndex();
                _window.SetStartMass(_mapIndex);
            }
            else if (_type == MapData.Mass.Type.Boss)
            {
                _prevMapIndex = _window.GetBossMassIndex();
                _window.SetBoss(_mapIndex, true);
            }

            _typeBefore = _window.GetMassType(_mapIndex);
            _window.SetMassType(_mapIndex, _type);
        }

        public override void Undo()
        {
            _window.SetMassType(_mapIndex, _typeBefore);
            if (_type == MapData.Mass.Type.Start)
            {
                _window.SetStartMass(_prevMapIndex);
            }
            else if (_type == MapData.Mass.Type.Boss)
            {
                _window.SetBoss(_prevMapIndex, true);
            }
        }

    }


    internal class CommandHistoryWindow : EditorWindow
    {
        private Stack<IMapEditorCommand> _commandHistory;
        private Stack<IMapEditorCommand> _undoHistory;
        private static CommandHistoryWindow _window;

        internal static void ShowWindow(Stack<IMapEditorCommand> commandHistory, Stack<IMapEditorCommand> undoHistory)
        {
            _window = GetWindow<CommandHistoryWindow>("Command History");
            _window._commandHistory = commandHistory;
            _window._undoHistory = undoHistory;
        }

        internal static void UpdateHistory()
        {
            if (_window != null)
            {
                _window.Repaint();
            }
        }

        // 閉じるボタンを押した時に呼ばれる
        private void OnClose()
        {
            _window = null;
        }

        private void OnGUI()
        {
            GUILayout.Label("Command History", EditorStyles.boldLabel);

            if (_commandHistory != null)
            {
                GUILayout.Label("Executed Commands:");
                foreach (var command in _commandHistory)
                {
                    GUILayout.Label(command.ToString());
                }
            }

            if (_undoHistory != null)
            {
                GUILayout.Label("Undone Commands:");
                foreach (var command in _undoHistory)
                {
                    GUILayout.Label(command.ToString());
                }
            }
        }
    }

}
