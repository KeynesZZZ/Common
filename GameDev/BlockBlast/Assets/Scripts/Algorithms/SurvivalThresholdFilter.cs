using System.Collections.Generic;
using System.Linq;
using BlockBlast.Core;

namespace BlockBlast.Algorithms
{
    /// <summary>
    /// 生存阈值过滤器 - 根据玩家状态调节游戏难度
    /// </summary>
    public class SurvivalThresholdFilter
    {
        private const int TotalCells = 64;

        public enum DifficultyMode { Mercy, Neutral, Execution } // 仁慈、中立、处决模式

        /// <summary>
        /// 根据当前棋盘状态决定发牌策略
        /// </summary>
        public List<BlockShape> FilterCandidateSets(List<List<BlockShape>> potentialSets, byte[] board, int score)
        {
            int emptyCells = CountEmptyCells(board);
            float fillRate = (TotalCells - emptyCells) / (float)TotalCells;

            // 1. 确定当前的难度模式
            DifficultyMode mode = DetermineMode(fillRate, score);

            // 2. 对所有候选方案进行生存评估
            var shadowSim = new ShadowSimulator();
            var scoredSets = potentialSets.Select(set => new
            {
                Blocks = set,
                Result = shadowSim.EvaluateSet(board, set)
            }).ToList();

            // 3. 过滤逻辑
            switch (mode)
            {
                case DifficultyMode.Mercy:
                    // 仁慈模式：必须选一个能放下且碎片化最低的方案
                    return scoredSets.Where(s => s.Result.IsViable)
                                     .OrderBy(s => s.Result.Fragments)
                                     .First().Blocks;

                case DifficultyMode.Execution:
                    // 处决模式：优先选"不致死但最难受"的方案
                    // 比如：碎片化极高，且潜在消除行数为 0 的组合
                    return scoredSets.Where(s => s.Result.IsViable)
                                     .OrderByDescending(s => s.Result.Fragments)
                                     .ThenBy(s => s.Result.PotentialLines)
                                     .First().Blocks;

                default:
                    // 中立模式：随机选择
                    int randomIndex = new System.Random().Next(scoredSets.Count);
                    return scoredSets[randomIndex].Blocks;
            }
        }

        /// <summary>
        /// 根据填充率和分数确定难度模式
        /// </summary>
        private DifficultyMode DetermineMode(float fillRate, int score)
        {
            // 棋盘快满了，给点简单的块续命（为了留住玩家）
            if (fillRate > 0.85f) return DifficultyMode.Mercy;
            // 高分段且空间局促，开启处决
            if (score > 5000 && fillRate > 0.6f) return DifficultyMode.Execution;
            return DifficultyMode.Neutral;
        }

        /// <summary>
        /// 统计棋盘空格数量
        /// </summary>
        private int CountEmptyCells(byte[] board)
        {
            int emptyCount = 0;
            for (int i = 0; i < board.Length; i++)
            {
                if (board[i] == 0) emptyCount++;
            }
            return emptyCount;
        }
    }
}
