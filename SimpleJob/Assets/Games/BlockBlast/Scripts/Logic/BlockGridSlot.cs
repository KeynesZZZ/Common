using System.Runtime.CompilerServices;
using SimpleBoard;
using SimpleBoard.Core;
using SimpleBoard.Data;
using UnityEngine;

namespace BlockBlast
{
    /// <summary>
    /// Block Blast 方块槽位实现
    /// 继承 UnityGridSlot 实现 IBlockGridSlot 接口
    /// </summary>
    public class BlockGridSlot : UnityGridSlot, IBlockGridSlot
    {
        private int _blockType;
        private Color _blockColor;

        /// <summary>
        /// 方块类型标识
        /// </summary>
        public int BlockType => _blockType;

        /// <summary>
        /// 方块颜色
        /// </summary>
        public Color BlockColor => _blockColor;

        /// <summary>
        /// 是否包含方块
        /// </summary>
        public bool HasBlock => _blockType != 0;

        public BlockGridSlot(IGridSlotState state, GridPosition gridPosition) 
            : base(state, gridPosition)
        {
            _blockType = 0;
            _blockColor = Color.clear;
        }

        /// <summary>
        /// 设置方块
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlock(int blockType, Color color)
        {
            if (!CanContainItem)
            {
                throw new System.InvalidOperationException("Cannot set block on a slot that cannot contain items.");
            }

            _blockType = blockType;
            _blockColor = color;
        }

        /// <summary>
        /// 移除方块
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveBlock()
        {
            if (!CanContainItem)
            {
                throw new System.InvalidOperationException("Cannot remove block from a slot that cannot contain items.");
            }

            _blockType = 0;
            _blockColor = Color.clear;
        }

        /// <summary>
        /// 清除槽位（包括方块数据）
        /// </summary>
        public new void Clear()
        {
            base.Clear();
            _blockType = 0;
            _blockColor = Color.clear;
        }
    }
}
