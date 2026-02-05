using System.Collections.Generic;
using BlockBlast.Core;

namespace BlockBlast.Algorithms
{
    /// <summary>
    /// 影子棋盘模拟器 - 在虚拟棋盘上预演方块放置，评估组合质量
    /// </summary>
    public class ShadowSimulator
    {
        private const int Size = 8;

        /// <summary>
        /// 模拟放置方块的结果
        /// </summary>
        public struct SimulationResult
        {
            public bool IsViable;      // 这组方块是否全都能放下
            public int Fragments;     // 放置后产生的碎片空间数量（数值越高，棋盘越乱）
            public int PotentialLines; // 放置后能消除的行数
        }

        /// <summary>
        /// 核心方法：模拟投放一组方块
        /// </summary>
        /// <param name="currentBoard">当前真实棋盘状态</param>
        /// <param name="candidateSet">系统拟定发给玩家的三个方块</param>
        public SimulationResult EvaluateSet(byte[] currentBoard, List<BlockShape> candidateSet)
        {
            // 1. 克隆一个"影子棋盘"，避免影响真实游戏数据
            byte[] shadowBoard = (byte[])currentBoard.Clone();

            bool allPlaced = true;
            int totalCleared = 0;

            // 2. 按顺序模拟投放
            foreach (var block in candidateSet)
            {
                // 寻找影子棋盘上的最佳/可用放置点
                var pos = FindBestPlacement(shadowBoard, block);

                if (pos.HasValue)
                {
                    // 在影子棋盘上模拟"落子"并计算消除
                    totalCleared += ApplyPlacement(shadowBoard, block, pos.Value.x, pos.Value.y);
                }
                else
                {
                    // 只要有一个放不下，这一组就是"死局"候选
                    allPlaced = false;
                    break;
                }
            }

            // 3. 评估棋盘质量：计算连通区域（碎片化程度）
            int fragments = CalculateFragmentation(shadowBoard);

            return new SimulationResult
            {
                IsViable = allPlaced,
                Fragments = fragments,
                PotentialLines = totalCleared
            };
        }

        /// <summary>
        /// 影子棋盘上的碰撞检测
        /// </summary>
        private bool CanPlace(byte[] board, BlockShape block, int row, int col)
        {
            for (int by = 0; by < block.height; by++)
            {
                for (int bx = 0; bx < block.width; bx++)
                {
                    if (!block.IsCellOccupied(bx, by)) continue;

                    int targetR = row + by;
                    int targetC = col + bx;

                    // 越界或重叠
                    if (targetR >= Size || targetC >= Size || board[targetR * Size + targetC] == 1)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 影子棋盘的更新：模拟消行
        /// </summary>
        private int ApplyPlacement(byte[] board, BlockShape block, int row, int col)
        {
            // 放置方块
            for (int by = 0; by < block.height; by++)
            {
                for (int bx = 0; bx < block.width; bx++)
                {
                    if (block.IsCellOccupied(bx, by))
                        board[(row + by) * Size + (col + bx)] = 1;
                }
            }

            // 影子消除逻辑（模拟真实消除）
            return MockClearLines(board);
        }

        /// <summary>
        /// 模拟消除行和列
        /// </summary>
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

        /// <summary>
        /// 计算碎片化程度（使用简单的洪水填充算法 Flood Fill）
        /// </summary>
        private int CalculateFragmentation(byte[] board)
        {
            bool[] visited = new bool[Size * Size];
            int regionCount = 0;

            for (int i = 0; i < Size * Size; i++)
            {
                if (board[i] == 0 && !visited[i])
                {
                    regionCount++; // 发现一个独立的空闲区域
                    FloodFill(board, visited, i);
                }
            }
            return regionCount; // 区域越多，意味着大块方块越难放下
        }

        /// <summary>
        /// 洪水填充算法 - 标记所有连通的空闲区域
        /// </summary>
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

        /// <summary>
        /// 寻找影子棋盘上的最佳放置位置
        /// </summary>
        private (int x, int y)? FindBestPlacement(byte[] board, BlockShape block)
        {
            // 影子算法会遍历 8x8 所有坐标，返回第一个能放下的点
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
