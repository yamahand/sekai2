using MessagePack;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public partial class MapData
{
    // シンボルエネミーの情報
    [MessagePackObject]
    public class SymbolEnemy
    {
        /// <summary>
        /// 移動タイプ
        /// </summary>
        public enum MoveType
        {
            /// <summary>
            /// 移動しない
            /// </summary>
            None,
            /// <summary>
            /// ランダム
            /// </summary>
            Random,
            /// <summary>
            /// 追跡
            /// </summary>
            Follow,
            /// <summary>
            /// ルート移動
            /// </summary>
            Route,
        }

        // 移動タイプ
        [Key(0)]
        public MoveType moveType { get; set; } = MoveType.None;

        // 移動するマスのインデックスリスト
        [Key(1)]
        public List<Vector2Int> moveMassIndexes { get; set; } = new List<Vector2Int>();
    }
}
