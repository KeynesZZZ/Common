using System.Collections.Generic;
using BlockBlast.Core;
using UnityEngine;

namespace BlockBlast.Managers
{
    /// <summary>
    /// 棋盘管理器 - 管理棋盘状态、方块放置和消除逻辑
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        public const int BOARD_SIZE = 8;
        public const int TOTAL_CELLS = BOARD_SIZE * BOARD_SIZE;

        private byte[] board = new byte[TOTAL_CELLS];
        private List<int> tempRowBuffer = new List<int>(BOARD_SIZE);
        private List<int> tempColBuffer = new List<int>(BOARD_SIZE);

        /// <summary>
        /// 当方块放置时触发
        /// </summary>
        public System.Action<int, int> OnCellPlaced;

        /// <summary>
        /// 当行或列消除时触发
        /// </summary>
        public System.Action<List<int>, List<int>> OnLinesEliminated;

        /// <summary>
        /// 清空棋盘
        /// </summary>
        public void ClearBoard()
        {
            System.Array.Clear(board, 0, board.Length);
        }

        /// <summary>
        /// 检测方块是否可以放置在指定位置
        /// </summary>
        public bool CanPlaceBlock(BlockShape block, int x, int y)
        {
            for (int by = 0; by < block.height; by++)
            {
                for (int bx = 0; bx < block.width; bx++)
                {
                    if (!block.IsCellOccupied(bx, by)) continue;

                    int boardX = x + bx;
                    int boardY = y + by;

                    if (boardX < 0 || boardX >= BOARD_SIZE || boardY < 0 || boardY >= BOARD_SIZE)
                        return false;

                    int boardIndex = boardY * BOARD_SIZE + boardX;
                    if (board[boardIndex] == 1)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 在指定位置放置方块
        /// </summary>
        public void PlaceBlock(BlockShape block, int x, int y)
        {
            for (int by = 0; by < block.height; by++)
            {
                for (int bx = 0; bx < block.width; bx++)
                {
                    if (!block.IsCellOccupied(bx, by)) continue;

                    int boardX = x + bx;
                    int boardY = y + by;
                    int boardIndex = boardY * BOARD_SIZE + boardX;

                    board[boardIndex] = 1;
                    OnCellPlaced?.Invoke(boardX, boardY);
                }
            }
        }

        /// <summary>
        /// 检测是否有完整的行或列可以消除
        /// </summary>
        public EliminationResult CheckElimination()
        {
            tempRowBuffer.Clear();
            tempColBuffer.Clear();

            // 检测完整的行
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                bool isRowFull = true;
                for (int x = 0; x < BOARD_SIZE; x++)
                {
                    if (board[y * BOARD_SIZE + x] == 0)
                    {
                        isRowFull = false;
                        break;
                    }
                }
                if (isRowFull) tempRowBuffer.Add(y);
            }

            // 检测完整的列
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                bool isColFull = true;
                for (int y = 0; y < BOARD_SIZE; y++)
                {
                    if (board[y * BOARD_SIZE + x] == 0)
                    {
                        isColFull = false;
                        break;
                    }
                }
                if (isColFull) tempColBuffer.Add(x);
            }

            var result = new EliminationResult
            {
                rows = new List<int>(tempRowBuffer),
                columns = new List<int>(tempColBuffer)
            };

            return result;
        }

        /// <summary>
        /// 消除指定的行和列
        /// </summary>
        public void Eliminate(EliminationResult result)
        {
            foreach (int row in result.rows)
            {
                for (int x = 0; x < BOARD_SIZE; x++)
                {
                    board[row * BOARD_SIZE + x] = 0;
                }
            }

            foreach (int col in result.columns)
            {
                for (int y = 0; y < BOARD_SIZE; y++)
                {
                    board[y * BOARD_SIZE + col] = 0;
                }
            }

            OnLinesEliminated?.Invoke(result.rows, result.columns);
        }

        /// <summary>
        /// 检测游戏是否结束（所有可用方块都无法放置）
        /// </summary>
        public bool IsGameOver(BlockShape[] availableBlocks)
        {
            foreach (var block in availableBlocks)
            {
                if (block.id == 0) continue;
                if (CanPlaceBlockAnywhere(block))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 检测方块是否可以放置在棋盘的任何位置
        /// </summary>
        private bool CanPlaceBlockAnywhere(BlockShape block)
        {
            int maxY = BOARD_SIZE - block.height + 1;
            int maxX = BOARD_SIZE - block.width + 1;

            for (int y = 0; y < maxY; y++)
            {
                for (int x = 0; x < maxX; x++)
                {
                    if (CanPlaceBlock(block, x, y))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取棋盘空格率
        /// </summary>
        public float GetEmptyRate()
        {
            int emptyCount = 0;
            for (int i = 0; i < board.Length; i++)
            {
                if (board[i] == 0) emptyCount++;
            }
            return (float)emptyCount / TOTAL_CELLS;
        }

        /// <summary>
        /// 检测指定位置是否被占用
        /// </summary>
        public bool IsCellOccupied(int x, int y)
        {
            if (x < 0 || x >= BOARD_SIZE || y < 0 || y >= BOARD_SIZE) return false;
            return board[y * BOARD_SIZE + x] == 1;
        }

        /// <summary>
        /// 获取棋盘状态的副本
        /// </summary>
        public byte[] GetBoardState()
        {
            return (byte[])board.Clone();
        }
    }

    /// <summary>
    /// 消除结果结构
    /// </summary>
    public class EliminationResult
    {
        public List<int> rows = new List<int>();
        public List<int> columns = new List<int>();

        public int TotalLines => rows.Count + columns.Count;
        public bool HasElimination => TotalLines > 0;
    }
}
