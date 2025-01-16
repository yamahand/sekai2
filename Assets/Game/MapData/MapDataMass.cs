using UnityEngine;
using MessagePack;

public partial class MapData
{
    // 1マスの情報
    [MessagePackObject]
    public class Mass
    {
        // マップインデックス
        [Key(0)]
        public Vector2Int mapIndex { get; set; } = new Vector2Int(-1, -1);
        // マスが存在するかどうか
        [Key(1)]
        public bool exist { get; set; } = false;
        // 開始マスかどうか
        [Key(2)]
        public bool start { get; set; } = false;
        // 隠しマスかどうか
        [Key(3)]
        public bool hidden { get; set; } = false;
        // アイテムグループ
        [Key(4)]
        public int itemGroup { get; set; } = -1;
        // ボックスID
        [Key(5)]
        public int boxId { get; set; } = -1;
        // ボスマスかどうか
        [Key(6)]
        public bool isBoss { get; set; } = false;
        // シンボルエネミーID
        [Key(7)]
        public int symbolEnemyId { get; set; } = -1;
        // トラップID
        [Key(8)]
        public int trapId { get; set; } = -1;
        // 階段マスかどうか
        [Key(9)]
        public bool isStairs { get; set; } = false;
        // ドアID
        [Key(10)]
        public int doorId { get; set; } = -1;
        [Key(11)]
        public MassType type { get; set; } = MassType.Floor;


        // リセット
        public void Reset()
        {
            mapIndex = new Vector2Int(-1, -1);
            exist = false;
            start = false;
            hidden = false;
            itemGroup = -1;
            boxId = -1;
            isBoss = false;
            symbolEnemyId = -1;
            trapId = -1;
            isStairs = false;
            doorId = -1;
            type = MassType.Floor;
        }


        // 複製
        public Mass Clone()
        {
            return new Mass()
            {
                mapIndex = mapIndex,
                exist = exist,
                start = start,
                hidden = hidden,
                itemGroup = itemGroup,
                boxId = boxId,
                isBoss = isBoss,
                symbolEnemyId = symbolEnemyId,
                trapId = trapId,
                isStairs = isStairs,
                doorId = doorId,
                type = type
            };
        }

        // 上書き
        public void Overwrite(Mass mass)
        {
            if (mass == null)
            {
                return;
            }
            mapIndex = mass.mapIndex;
            exist = mass.exist;
            start = mass.start;
            hidden = mass.hidden;
            itemGroup = mass.itemGroup;
            boxId = mass.boxId;
            isBoss = mass.isBoss;
            symbolEnemyId = mass.symbolEnemyId;
            trapId = mass.trapId;
            isStairs = mass.isStairs;
            doorId = mass.doorId;
            type = mass.type;
        }
    }
}

