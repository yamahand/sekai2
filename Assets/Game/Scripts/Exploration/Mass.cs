using UnityEngine;

namespace Game.Scripts.Exploration
{
    // マップの1マスを表すクラス
    public class Mass
    {
        // マスが探索済みかのフラグ
        public bool explored { get; set; }
        // マスが隠れているかのフラグ
        public bool hidden { get; private set; }
        //マスに到達できるかのフラグ
        public bool reachable { get; set; }
        // アイテム取得済みかのフラグ
        public bool itemGot { get; set; }
        public MapData.Mass mass { get; private set; }

        // 上下左右のどこに壁があるかのビットフラグ
        public MapDef.Wall wall { get; private set; }

        // コンストラクタ
        public Mass(int x, int y)
        {
            explored = false;
            hidden = true;
            itemGot = false;

            // 壁の初期化
            wall = 0;
            if (x == 0) wall |= MapDef.Wall.Left;
            if (x == MapDef.MassMaxX - 1) wall |= MapDef.Wall.Right;
            if (y == 0) wall |= MapDef.Wall.Up;
            if (y == MapDef.MassMaxY - 1) wall |= MapDef.Wall.Down;
        }

        // MapData.MassDataからマスの情報をセットアップ
        public void Setup(MapData.Mass massData)
        {
            mass = massData;
            hidden = massData.hidden;
        }

        // GameObjectを設定
        public void SetGameObject(GameObject gameObject)
        {
            _gameObject = gameObject;
            if (!explored)
            {
                _gameObject.SetActive(false);
            }
        }

        // 壁の有無を設定
        public void SetWall(MapDef.Direction direction)
        {
            wall |= MapDef.DirectionToWall(direction);
        }

        public void Show()
        {
            if (!explored)
            {
                _gameObject?.SetActive(true);
                explored = true;
            }
        }

        GameObject _gameObject = null;

    }
}
