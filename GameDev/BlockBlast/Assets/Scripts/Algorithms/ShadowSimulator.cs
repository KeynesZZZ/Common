using System.Collections.Generic;
using BlockBlast.Core;

namespace BlockBlast.Algorithms
{
    public class ShadowSimulator
    {
        private const int Size = 8;

        public struct SimulationResult
        {
            public bool IsViable;
            public int Fragments;
            public int PotentialLines;
        }

        public SimulationResult EvaluateSet(byte[] currentBoard, List<BlockShape> candidateSet)
        {
            byte[] shadowBoard = (byte[])currentBoard.Clone();

            bool allPlaced = true;
            int totalCleared = 0;

            foreach (var block in candidateSet)
            {
                var pos = FindBestPlacement(shadowBoard, block);

                if (pos.HasValue)
                {
                    totalCleared += ApplyPlacement(shadowBoard, block, pos.Value.x, pos.Value.y);
                }
                else
                {
                    allPlaced = false;
                    break;
                }
            }

            int fragments = CalculateFragmentation(shadowBoard);

            return new SimulationResult
            {
                IsViable = allPlaced,
                Fragments = fragments,
                PotentialLines = totalCleared
            };
        }

        private bool CanPlace(byte[] board, BlockShape block, int row, int col)
        {
            for (int by = 0; by < block.height; by++)
            {
                for (int bx = 0; bx < block.width; bx++)
                {
                    if (!block.IsCellOccupied(bx, by)) continue;

                    int targetR = row + by;
                    int targetC = col + bx;

                    if (targetR >= Size || targetC >= Size || board[targetR * Size + targetC] == 1)
                        return false;
                }
            }
            return true;
        }

        private int ApplyPlacement(byte[] board, BlockShape block, int row, int col)
        {
            for (int by = 0; by < block.height; by++)
            {
                for (int bx = 0; bx < block.width; bx++)
                {
                    if (block.IsCellOccupied(bx, by))
                        board[(row + by) * Size + (col + bx)] = 1;
                }
            }

            return MockClearLines(board);
        }

        private int MockClearLines(byte[] board)
        {
            int clearedLines = 0;

            for (int i = 0; i < Size; i++)
            {
                bool rowFull = true;
                bool colFull = true;

                for (int j = 0; j < Size; j++)
                {
                    if (board[i * Size + j] == 0) rowFull = false;
                    if (board[j * Size + i] == 0) colFull = false;
                }

                if (rowFull) clearedLines++;
                if (colFull) clearedLines++;
            }

            return clearedLines;
        }

        private int CalculateFragmentation(byte[] board)
        {
            bool[] visited = new bool[Size * Size];
            int regionCount = 0;

            for (int i = 0; i < Size * Size; i++)
            {
                if (board[i] == 0 && !visited[i])
                {
                    regionCount++;
                    FloodFill(board, visited, i);
                }
            }
            return regionCount;
        }

        private void FloodFill(byte[] board, bool[] visited, int index)
        {
            int row = index / Size;
            int col = index % Size;

            if (row < 0 || row >= Size || col < 0 || col >= Size) return;
            if (visited[index]) return;
            if (board[index] == 1) return;

            visited[index] = true;

            FloodFill(board, visited, index - 1);
            FloodFill(board, visited, index + 1);
            FloodFill(board, visited, index - Size);
            FloodFill(board, visited, index + Size);
        }

        private (int x, int y)? FindBestPlacement(byte[] board, BlockShape block)
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (CanPlace(board, block, i, j))
                        return (i, j);
                }
            }
            return null;
        }
    }
}
