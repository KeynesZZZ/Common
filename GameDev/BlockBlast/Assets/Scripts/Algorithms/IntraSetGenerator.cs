using System.Collections.Generic;
using BlockBlast.Core;

namespace BlockBlast.Algorithms
{
    /// <summary>
    /// 三件套内部关联生成器 - 生成具有空间互斥或协同特性的方块组合
    /// </summary>
    public class IntraSetGenerator
    {
        private const int Size = 8;

        /// <summary>
        /// 生成一组关联的方块
        /// </summary>
        public List<BlockShape> GenerateLinkedSet(byte[] currentBoard, List<BlockShape> shapeTemplates, bool aggressiveMode)
        {
            var resultSet = new List<BlockShape>();
            byte[] virtualBoard = (byte[])currentBoard.Clone();

            for (int i = 0; i < 3; i++)
            {
                // 1. 扫描当前虚拟棋盘的所有可用位置
                var possibleMoves = ScanAllMoves(virtualBoard, shapeTemplates);

                // 2. 挑选一个"种子方块"
                BlockShape selectedBlock;
                if (aggressiveMode)
                {
                    // 处决模式：选一个体型巨大，且放下去后会显著减少其他方块摆放可能性的块
                    selectedBlock = PickBlockThatLimitsFutureMoves(possibleMoves, shapeTemplates);
                }
                else
                {
                    // 仁慈模式：选一个能产生消除，为后面两块腾位置的块
                    selectedBlock = PickBlockThatClearsSpace(possibleMoves, shapeTemplates);
                }

                resultSet.Add(selectedBlock);

                // 3. 关键：更新虚拟棋盘，后续方块的生成必须基于前一个方块放下的状态
                SimulatePlacementAndClear(virtualBoard, selectedBlock);
            }

            return resultSet;
        }

        /// <summary>
        /// 扫描所有可能的移动位置
        /// </summary>
        private List<Move> ScanAllMoves(byte[] board, List<BlockShape> shapeTemplates)
        {
            var moves = new List<Move>();

            foreach (var shape in shapeTemplates)
            {
                for (int y = 0; y < Size; y++)
                {
                    for (int x = 0; x < Size; x++)
                    {
                        if (CanPlace(board, shape, x, y))
                        {
                            moves.Add(new Move { shape = shape, x = x, y = y });
                        }
                    }
                }
            }

            return moves;
        }

        /// <summary>
        /// 检测方块是否可以放置
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

                    if (targetR >= Size || targetC >= Size || board[targetR * Size + targetC] == 1)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 选择会限制未来移动的方块（处决模式）
        /// </summary>
        private BlockShape PickBlockThatLimitsFutureMoves(List<Move> moves, List<BlockShape> shapeTemplates)
        {
            var shapeFragmentation = new Dictionary<BlockShape, int>();

            foreach (var shape in shapeTemplates)
            {
                int maxFragments = 0;
                var shapeMoves = moves.FindAll(m => m.shape.id == shape.id);

                foreach (var move in shapeMoves)
                {
                    byte[] testBoard = new byte[Size * Size];
                    int fragments = CalculateFragmentationAfterPlacement(testBoard, shape, move.x, move.y);
                    if (fragments > maxFragments)
                        maxFragments = fragments;
                }

                shapeFragmentation[shape] = maxFragments;
            }

            BlockShape worstShape = null;
            int worstFragments = -1;

            foreach (var kvp in shapeFragmentation)
            {
                if (kvp.Value > worstFragments)
                {
                    worstFragments = kvp.Value;
                    worstShape = kvp.Key;
                }
            }

            return worstShape ?? shapeTemplates[0];
        }

        /// <summary>
        /// 选择能清除空间的方块（仁慈模式）
        /// </summary>
        private BlockShape PickBlockThatClearsSpace(List<Move> moves, List<BlockShape> shapeTemplates)
        {
            var shapeLines = new Dictionary<BlockShape, int>();

            foreach (var shape in shapeTemplates)
            {
                int maxLines = 0;
                var shapeMoves = moves.FindAll(m => m.shape.id == shape.id);

                foreach (var move in shapeMoves)
                {
                    int lines = CalculateLinesAfterPlacement(shape, move.x, move.y);
                    if (lines > maxLines)
                        maxLines = lines;
                }

                shapeLines[shape] = maxLines;
            }

            BlockShape bestShape = null;
            int bestLines = -1;

            foreach (var kvp in shapeLines)
            {
                if (kvp.Value > bestLines)
                {
                    bestLines = kvp.Value;
                    bestShape = kvp.Key;
                }
            }

            return bestShape ?? shapeTemplates[0];
        }

        /// <summary>
        /// 模拟放置并清除行
        /// </summary>
        private void SimulatePlacementAndClear(byte[] board, BlockShape block)
        {
            var pos = FindBestPlacement(board, block);
            if (pos.HasValue)
            {
                for (int by = 0; by < block.height; by++)
                {
                    for (int bx = 0; bx < block.width; bx++)
                    {
                        if (block.IsCellOccupied(bx, by))
                            board[(pos.Value.y + by) * Size + (pos.Value.x + bx)] = 1;
                    }
                }

                MockClearLines(board);
            }
        }

        /// <summary>
        /// 寻找最佳放置位置
        /// </summary>
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

        /// <summary>
        /// 计算放置后的碎片化程度
        /// </summary>
        private int CalculateFragmentationAfterPlacement(byte[] board, BlockShape block, int x, int y)
        {
            byte[] testBoard = (byte[])board.Clone();

            for (int by = 0; by < block.height; by++)
            {
                for (int bx = 0; bx < block.width; bx++)
                {
                    if (block.IsCellOccupied(bx, by))
                        testBoard[(y + by) * Size + (x + bx)] = 1;
                }
            }

            return CalculateFragmentation(testBoard);
        }

        /// <summary>
        /// 计算碎片化程度（洪水填充算法）
        /// </summary>
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

        /// <summary>
        /// 洪水填充算法
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
        /// 计算放置后能消除的行数（简化版）
        /// </summary>
        private int CalculateLinesAfterPlacement(BlockShape block, int x, int y)
        {
            return 0;
        }

        /// <summary>
        /// 模拟清除行和列
        /// </summary>
        private void MockClearLines(byte[] board)
        {
            for (int i = 0; i < Size; i++)
            {
                bool rowFull = true;
                bool colFull = true;

                for (int j = 0; j < Size; j++)
                {
                    if (board[i * Size + j] == 0) rowFull = false;
                    if (board[j * Size + i] == 0) colFull = false;
                }

                if (rowFull)
                {
                    for (int j = 0; j < Size; j++)
                        board[i * Size + j] = 0;
                }

                if (colFull)
                {
                    for (int j = 0; j < Size; j++)
                        board[j * Size + i] = 0;
                }
            }
        }
    }

    /// <summary>
    /// 移动位置结构
    /// </summary>
    public struct Move
    {
        public BlockShape shape;
        public int x;
        public int y;
    }
}
