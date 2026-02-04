using UnityEngine;
using BingoGame.Core.Models;

namespace BingoGame.GameModes.ClassicBingo
{
    /// <summary>
    /// 经典数字Bingo单元格
    /// </summary>
    public class BingoCell : GameCell
    {
        /// <summary>
        /// 单元格数字
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// 是否已标记
        /// </summary>
        public bool IsMarked { get; set; }

        /// <summary>
        /// 是否为免费格（中心格）
        /// </summary>
        public bool IsFreeSpace { get; set; }

        /// <summary>
        /// 是否已完成（标记状态）
        /// </summary>
        public override bool IsCompleted
        {
            get => IsMarked;
            set => IsMarked = value;
        }
    }
}
