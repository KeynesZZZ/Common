using System;
using SimpleBoard.Core;

namespace SimpleBoard.Games.Bingo
{
    /// <summary>
    /// Bingo 格子状态类 - 管理格子的数字、标记状态等
    /// </summary>
    public class BingoSlotState
    {
        /// <summary>格子数字</summary>
        public int Number { get; private set; }
        
        /// <summary>是否已标记</summary>
        public bool IsMarked { get; private set; }
        
        /// <summary>是否为自由空间</summary>
        public bool IsFreeSpace { get; private set; }
        
        /// <summary>所在列</summary>
        public BingoColumn Column { get; private set; }
        
        /// <summary>网格位置</summary>
        public GridPosition GridPosition { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="number">格子数字</param>
        /// <param name="column">所在列</param>
        /// <param name="isFreeSpace">是否为自由空间</param>
        public BingoSlotState(int number, BingoColumn column, bool isFreeSpace = false)
        {
            Number = number;
            Column = column;
            IsFreeSpace = isFreeSpace;
            IsMarked = false;
            GridPosition = GridPosition.Zero;
        }

        /// <summary>
        /// 标记格子
        /// </summary>
        public void Mark()
        {
            if (!IsFreeSpace)
            {
                IsMarked = true;
            }
        }

        /// <summary>
        /// 取消标记
        /// </summary>
        public void Unmark()
        {
            IsMarked = false;
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset()
        {
            IsMarked = false;
        }

        /// <summary>
        /// 是否可以标记
        /// </summary>
        public bool CanMark()
        {
            return !IsMarked && !IsFreeSpace;
        }

        /// <summary>
        /// 获取显示文本
        /// </summary>
        public string GetDisplayText()
        {
            return IsFreeSpace ? "FREE" : Number.ToString();
        }

        /// <summary>
        /// 检查数字是否匹配
        /// </summary>
        public bool IsMatch(int number)
        {
            return Number == number;
        }

        /// <summary>
        /// 转换为字符串
        /// </summary>
        public override string ToString()
        {
            return $"BingoSlot[{Number}, {Column}, Marked:{IsMarked}, Free:{IsFreeSpace}]";
        }
    }

    /// <summary>
    /// Bingo 列枚举
    /// </summary>
    public enum BingoColumn
    {
        B = 0,  // 1-15
        I = 1,  // 16-30
        N = 2,  // 31-45
        G = 3,  // 46-60
        O = 4   // 61-75
    }
}