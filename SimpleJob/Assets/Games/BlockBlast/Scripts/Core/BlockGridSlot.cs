using System;
using System.Runtime.CompilerServices;
using SimpleBoard;
using SimpleBoard.Core;
using SimpleBoard.Data;
using SimpleBoard.Interfaces;
using UnityEngine;

namespace BlockBlast
{
    /// <summary>
    /// Block Blast 方块槽位实现
    /// 继承 UnityGridSlot 实现 IBlockGridSlot 接口
    /// </summary>
    public class BlockGridSlot : IBlockGridSlot
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
        {
            _blockType = 0;
            _blockColor = Color.clear;
            State = state;
            GridPosition = gridPosition;
        }

        public long ItemId => Item.UniqueID;

        public int ItemSn => Item.Sn;
        public bool HasItem => Item != null;
        public bool IsMovable => State.IsLocked == false && HasItem;
        public bool CanContainItem => State.CanContainItem;
        public bool CanSetItem => State.CanContainItem && HasItem == false;
        public bool NotAvailable => State.CanContainItem == false || State.IsLocked;
        public IUnityItem Item { get; private set; }
        public IGridSlotState State { get; private set; }
        public GridPosition GridPosition { get; }

        public void SetState(IGridSlotState state)
        {
            State = state;
        }

        public void SetItem(IUnityItem item)
        {
            EnsureItemIsNotNull(item);

            Item = item;
        }

        public void Clear()
        {
            if (State.CanContainItem == false)
            {
                throw new InvalidOperationException("Can not clear an unavailable grid slot.");
            }
            _blockType = 0;
            _blockColor = Color.clear;
            Item = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureItemIsNotNull(IUnityItem item)
        {
            if (item == null)
            {
                throw new NullReferenceException(nameof(item));
            }
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

   
    }
}
