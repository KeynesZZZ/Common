using System.Collections.Generic;
using BlockBlast.Core;
using UnityEngine;

namespace BlockBlast.Managers
{
    public class BoardManager : MonoBehaviour
    {
        public const int BOARD_SIZE = 8;
        public const int TOTAL_CELLS = BOARD_SIZE * BOARD_SIZE;

        private byte[] board = new byte[TOTAL_CELLS];
        private List<int> tempRowBuffer = new List<int>(BOARD_SIZE);
        private List<int> tempColBuffer = new List<int>(BOARD_SIZE);

        public System.Action<int, int> OnCellPlaced;
        public System.Action<List<int>, List<int>> OnLinesEliminated;

        public void ClearBoard()
        {
            System.Array.Clear(board, 0, board.Length);
        }

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

        public EliminationResult CheckElimination()
        {
            tempRowBuffer.Clear();
            tempColBuffer.Clear();

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

        public float GetEmptyRate()
        {
            int emptyCount = 0;
            for (int i = 0; i < board.Length; i++)
            {
                if (board[i] == 0) emptyCount++;
            }
            return (float)emptyCount / TOTAL_CELLS;
        }

        public bool IsCellOccupied(int x, int y)
        {
            if (x < 0 || x >= BOARD_SIZE || y < 0 || y >= BOARD_SIZE) return false;
            return board[y * BOARD_SIZE + x] == 1;
        }

        public byte[] GetBoardState()
        {
            return (byte[])board.Clone();
        }
    }

    public class EliminationResult
    {
        public List<int> rows = new List<int>();
        public List<int> columns = new List<int>();

        public int TotalLines => rows.Count + columns.Count;
        public bool HasElimination => TotalLines > 0;
    }
}
