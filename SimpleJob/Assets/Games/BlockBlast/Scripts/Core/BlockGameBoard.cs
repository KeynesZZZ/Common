using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SimpleBoard;
using SimpleBoard.Core;
using UnityEngine;

namespace BlockBlast
{
    /// <summary>
    /// Block Blast 游戏板
    /// 继承 SimpleBoard.GameBoard 框架基类
    /// </summary>
    public class BlockGameBoard : GameBoard<IBlockGridSlot>, IDisposable
    {
        /// <summary>
        /// 当方块放置时触发
        /// </summary>
        public event Action<GridPosition, BlockData> OnBlockPlaced;
        
        /// <summary>
        /// 当行/列被消除时触发
        /// </summary>
        public event Action<List<int>, List<int>> OnLinesCleared;

        /// <summary>
        /// 初始化游戏板
        /// </summary>
        public void Initialize(int width, int height)
        {
            var slots = new IBlockGridSlot[height, width];
            
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    var position = new GridPosition(row, col);
                    var state = BlockSlotState.Available();
                    slots[row, col] = new BlockGridSlot(state, position);
                }
            }

            SetGridSlots(slots);
        }

        /// <summary>
        /// 检查位置是否为空
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty(GridPosition position)
        {
            if (!IsPositionOnBoard(position))
                return false;
            
            var slot = this[position];
            return !slot.HasBlock;
        }

        /// <summary>
        /// 检查方块是否可以放置在指定位置
        /// </summary>
        public bool CanPlaceBlock(BlockData block, GridPosition origin)
        {
            var positions = block.GetWorldPositions(origin);
            
            foreach (var pos in positions)
            {
                if (!IsPositionOnBoard(pos) || !IsEmpty(pos))
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// 放置方块
        /// </summary>
        public bool PlaceBlock(BlockData block, GridPosition origin)
        {
            if (!CanPlaceBlock(block, origin))
                return false;

            var positions = block.GetWorldPositions(origin);
            int blockType = (int)block.Shape;
            
            foreach (var pos in positions)
            {
                var slot = this[pos];
                slot.SetBlock(blockType, block.Color);
            }

            OnBlockPlaced?.Invoke(origin, block);
            
            // 检查并消除行/列
            CheckAndClearLines();
            
            return true;
        }

        /// <summary>
        /// 检查并消除满行/满列
        /// </summary>
        private void CheckAndClearLines()
        {
            var rowsToClear = new List<int>();
            var colsToClear = new List<int>();

            // 检查行
            for (int row = 0; row < RowCount; row++)
            {
                if (IsRowFull(row))
                    rowsToClear.Add(row);
            }

            // 检查列
            for (int col = 0; col < ColumnCount; col++)
            {
                if (IsColFull(col))
                    colsToClear.Add(col);
            }

            // 执行消除
            if (rowsToClear.Count > 0 || colsToClear.Count > 0)
            {
                ClearLines(rowsToClear, colsToClear);
                OnLinesCleared?.Invoke(rowsToClear, colsToClear);
            }
        }

        /// <summary>
        /// 检查行是否已满
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsRowFull(int row)
        {
            for (int col = 0; col < ColumnCount; col++)
            {
                var position = new GridPosition(row, col);
                if (!this[position].HasBlock)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 检查列是否已满
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsColFull(int col)
        {
            for (int row = 0; row < RowCount; row++)
            {
                var position = new GridPosition(row, col);
                if (!this[position].HasBlock)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 消除指定的行和列
        /// </summary>
        private void ClearLines(List<int> rows, List<int> cols)
        {
            // 消除行
            foreach (var row in rows)
            {
                for (int col = 0; col < ColumnCount; col++)
                {
                    var position = new GridPosition(row, col);
                    this[position].RemoveBlock();
                }
            }

            // 消除列
            foreach (var col in cols)
            {
                for (int row = 0; row < RowCount; row++)
                {
                    var position = new GridPosition(row, col);
                    this[position].RemoveBlock();
                }
            }
        }

        /// <summary>
        /// 获取有效放置位置
        /// </summary>
        public List<GridPosition> GetValidPlacements(BlockData block)
        {
            var validPositions = new List<GridPosition>();
            
            for (int row = 0; row < RowCount; row++)
            {
                for (int col = 0; col < ColumnCount; col++)
                {
                    var pos = new GridPosition(row, col);
                    if (CanPlaceBlock(block, pos))
                        validPositions.Add(pos);
                }
            }
            
            return validPositions;
        }

        /// <summary>
        /// 检查是否可以放置任意方块
        /// </summary>
        public bool CanPlaceAnyBlock(IReadOnlyList<BlockData> blocks)
        {
            foreach (var block in blocks)
            {
                for (int row = 0; row < RowCount; row++)
                {
                    for (int col = 0; col < ColumnCount; col++)
                    {
                        var pos = new GridPosition(row, col);
                        if (CanPlaceBlock(block, pos))
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 清空游戏板
        /// </summary>
        public new void Clear()
        {
            for (int row = 0; row < RowCount; row++)
            {
                for (int col = 0; col < ColumnCount; col++)
                {
                    var position = new GridPosition(row, col);
                    if (this[position] is BlockGridSlot slot)
                    {
                        slot.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public new void Dispose()
        {
            base.Dispose();
            OnBlockPlaced = null;
            OnLinesCleared = null;
        }
    }
}
