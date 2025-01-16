using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class MapEditorUI
{
    /// <summary>
    /// コマンドのインターフェイス
    /// </summary>
    internal interface IMapEditorCommand
    {
        void Execute();
        void Undo();
    }

    internal abstract class CommandBase : IMapEditorCommand
    {
        public void Execute()
        {
            Debug.Log("Execute");
            OnPreExecute();
            OnExecute();
            // boxDataがnullの場合は全てのボックスを更新する
            if (boxData == null)
            {
                mapEditor.UpdateAllBoxes();
            }
            else
            {
                mapEditor.UpdateBox(boxData);

                // boxDataがfloorの場合は周囲の壁も更新する
                if (boxData.type == BoxType.floor)
                {
                    mapEditor.UpdateAroundBoxes(boxData);
                }
            }
        }
        public void Undo()
        {
            Debug.Log("Undo");
            OnUndo();
            // boxDataがnullの場合は全てのボックスを更新する
            if (boxData == null)
            {
                mapEditor.UpdateAllBoxes();
            }
            else
            {
                mapEditor.UpdateBox(boxData);

                // boxDataがfloorの場合は周囲の壁も更新する
                if (boxData.type == BoxType.floor)
                {
                    mapEditor.UpdateAroundBoxes(boxData);
                }
            }
        }

        // OnExecuteの前に実行される処理
        protected abstract void OnPreExecute();

        protected abstract void OnExecute();
        protected abstract void OnUndo();

        protected MapEditorUI mapEditor { get { return _mapEditorUI; } }
        protected BoxData boxData { get { return _boxData; } }

        // マップエディタUI
        private MapEditorUI _mapEditorUI;
        // BoxData
        private BoxData _boxData;

        // コンストラクタ
        protected CommandBase(MapEditorUI mapEditorUI, BoxData boxData)
        {
            _mapEditorUI = mapEditorUI;
            _boxData = boxData;
        }
    }

    internal abstract class MassCommand : CommandBase
    {
        // 古いマス
        protected MapData.Mass _oldMass;

        protected Vector2Int massIndex { get { return mapEditor.GetMassIndex(boxData); } }

        protected MassCommand(MapEditorUI mapEditorUI, BoxData boxData) : base(mapEditorUI, boxData)
        {
        }

        protected override void OnPreExecute()
        {
            if (mapEditor.mapData.TryGetMass(massIndex, out var mass))
            {
                _oldMass = mass.Clone();
            }
        }
    }

    // 壁用のコマンドのベースクラス
    internal abstract class WallCommand : CommandBase
    {
        // 古い壁の状態
        protected bool _oldWall;
        // インデックス
        protected Vector2Int index { get { return mapEditor.GetWallIndex(boxData); } }

        protected WallCommand(MapEditorUI mapEditorUI, BoxData boxData) : base(mapEditorUI, boxData)
        {
        }

        protected abstract bool GetWall();

        protected override void OnPreExecute()
        {
            _oldWall = GetWall();
        }
    }

    // 水平壁用のコマンド
    internal class HorizontalWallCommand : WallCommand
    {
        private bool _wall;
        public HorizontalWallCommand(MapEditorUI mapEditorUI, BoxData boxData, bool wall) : base(mapEditorUI, boxData)
        {
            _wall = wall;
        }
        protected override void OnExecute()
        {
            mapEditor.mapData.SetHorizontalWall(index, _wall);
        }
        protected override void OnUndo()
        {
            mapEditor.mapData.SetHorizontalWall(index, _oldWall);
        }

        protected override bool GetWall()
        {
            return mapEditor.mapData.GetHorizontalWall(index);
        }
    }

    // 垂直壁用のコマンド
    internal class VerticalWallCommand : WallCommand
    {
        private bool _wall;
        public VerticalWallCommand(MapEditorUI mapEditorUI, BoxData boxData, bool wall) : base(mapEditorUI, boxData)
        {
            _wall = wall;
        }
        protected override void OnExecute()
        {
            mapEditor.mapData.SetVerticalWall(index, _wall);
        }
        protected override void OnUndo()
        {
            mapEditor.mapData.SetVerticalWall(index, _oldWall);
        }
        protected override bool GetWall()
        {
            return mapEditor.mapData.GetVerticalWall(index);
        }
    }

    // 壁を整理するコマンド
    internal class ClearWallCommand : CommandBase
    {
        private bool[] _oldHorizontalWalls;
        private bool[] _oldVerticalWalls;
        public ClearWallCommand(MapEditorUI mapEditorUI) : base(mapEditorUI, null)
        {
        }
        protected override void OnPreExecute()
        {
            _oldHorizontalWalls = new bool[MapData.HorizontalWallCount.y];
            for (int y = 0; y < MapData.HorizontalWallCount.y; y++)
            {
                _oldHorizontalWalls[y] = mapEditor.mapData.GetHorizontalWall(new Vector2Int(0, y));
            }
            _oldVerticalWalls = new bool[MapData.VerticalWallCount.x];
            for (int x = 0; x < MapData.VerticalWallCount.x; x++)
            {
                _oldVerticalWalls[x] = mapEditor.mapData.GetVerticalWall(new Vector2Int(x, 0));
            }
        }
        protected override void OnExecute()
        {
            for (int y = 0; y < MapData.HorizontalWallCount.y; y++)
            {
                mapEditor.mapData.SetHorizontalWall(new Vector2Int(0, y), false);
            }
            for (int x = 0; x < MapData.VerticalWallCount.x; x++)
            {
                mapEditor.mapData.SetVerticalWall(new Vector2Int(x, 0), false);
            }
        }
        protected override void OnUndo()
        {
            for (int y = 0; y < MapData.HorizontalWallCount.y; y++)
            {
                mapEditor.mapData.SetHorizontalWall(new Vector2Int(0, y), _oldHorizontalWalls[y]);
            }
            for (int x = 0; x < MapData.VerticalWallCount.x; x++)
            {
                mapEditor.mapData.SetVerticalWall(new Vector2Int(x, 0), _oldVerticalWalls[x]);
            }
        }
    }

    // 壁を自動配置するコマンド
    internal class AutoArrangeWallsCommand : CommandBase
    {
        internal struct WallData
        {
            public Vector2Int index;
            public bool wall;
        }

        private List<WallData> _horizontalWallList = new List<WallData>();
        private List<WallData> _verticalWallList = new List<WallData>();

        private List<WallData> _oldHorizontalWallList = new List<WallData>();
        private List<WallData> _oldVerticalWallList = new List<WallData>();

        public void AddHorizontalWall(Vector2Int index, bool wall)
        {
            _horizontalWallList.Add(new WallData { index = index, wall = wall });
        }
        public void AddVerticalWall(Vector2Int index, bool wall)
        {
            _verticalWallList.Add(new WallData { index = index, wall = wall });
        }

        public AutoArrangeWallsCommand(MapEditorUI mapEditorUI) : base(mapEditorUI, null)
        {
        }
        protected override void OnPreExecute()
        {
            _oldHorizontalWallList.Clear();
            foreach (var wallData in _horizontalWallList)
            {
                _oldHorizontalWallList.Add(new WallData { index = wallData.index, wall = mapEditor.mapData.GetHorizontalWall(wallData.index) });
            }
            _oldVerticalWallList.Clear();
            foreach (var wallData in _verticalWallList)
            {
                _oldVerticalWallList.Add(new WallData { index = wallData.index, wall = mapEditor.mapData.GetVerticalWall(wallData.index) });
            }
        }
        
        protected override void OnExecute()
        {
            foreach (var wallData in _horizontalWallList)
            {
                mapEditor.mapData.SetHorizontalWall(wallData.index, wallData.wall);
            }
            foreach (var wallData in _verticalWallList)
            {
                mapEditor.mapData.SetVerticalWall(wallData.index, wallData.wall);
            }
        }
        
        protected override void OnUndo()
        {
            foreach (var wallData in _oldHorizontalWallList)
            {
                mapEditor.mapData.SetHorizontalWall(wallData.index, wallData.wall);
            }
            foreach (var wallData in _oldVerticalWallList)
            {
                mapEditor.mapData.SetVerticalWall(wallData.index, wallData.wall);
            }
        }
    }

    // マスの配置、削除コマンド
    internal class PlaceMassCommand : MassCommand
    {
        // マスを配置するかのフラグ
        private bool _place;
        private bool _oldPlace;

        public PlaceMassCommand(MapEditorUI mapEditorUI, BoxData boxData, bool place) : base(mapEditorUI, boxData)
        {
            _place = place;
        }
        protected override void OnExecute()
        {
            if (mapEditor.mapData.TryGetMass(massIndex, out var mass))
            {
                _oldPlace = mass.exist;
                mass.exist = _place;
            }
        }
        protected override void OnUndo()
        {
            if (mapEditor.mapData.TryGetMass(massIndex, out var mass))
            {
                mass.exist = _oldPlace;
            }
        }
    }

    // マスの種類を変更するコマンド
    internal class ChangeMassTypeCommand : MassCommand
    {
        private MapData.MassType _type;
        private MapData.MassType _oldType;
        public ChangeMassTypeCommand(MapEditorUI mapEditorUI, BoxData boxData, MapData.MassType type) : base(mapEditorUI, boxData)
        {
            _type = type;
        }
        protected override void OnExecute()
        {
            if (mapEditor.mapData.TryGetMass(massIndex, out var mass))
            {
                _oldType = mass.type;
                mass.type = _type;
                Debug.Log($"ChangeMassTypeCommand OnExecute {_oldType} -> {_type}");
            }
        }
        protected override void OnUndo()
        {
            if (mapEditor.mapData.TryGetMass(massIndex, out var mass))
            {
                mass.type = _oldType;
            }
        }
    }
}
