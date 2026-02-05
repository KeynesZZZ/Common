using System.Collections.Generic;
using System.Linq;
using BlockBlast.Core;

namespace BlockBlast.Algorithms
{
    public class SurvivalThresholdFilter
    {
        private const int TotalCells = 64;

        public enum DifficultyMode { Mercy, Neutral, Execution }

        public List<BlockShape> FilterCandidateSets(List<List<BlockShape>> potentialSets, byte[] board, int score)
        {
            int emptyCells = CountEmptyCells(board);
            float fillRate = (TotalCells - emptyCells) / (float)TotalCells;

            DifficultyMode mode = DetermineMode(fillRate, score);

            var shadowSim = new ShadowSimulator();
            var scoredSets = potentialSets.Select(set => new
            {
                Blocks = set,
                Result = shadowSim.EvaluateSet(board, set)
            }).ToList();

            switch (mode)
            {
                case DifficultyMode.Mercy:
                    return scoredSets.Where(s => s.Result.IsViable)
                                     .OrderBy(s => s.Result.Fragments)
                                     .First().Blocks;

                case DifficultyMode.Execution:
                    return scoredSets.Where(s => s.Result.IsViable)
                                     .OrderByDescending(s => s.Result.Fragments)
                                     .ThenBy(s => s.Result.PotentialLines)
                                     .First().Blocks;

                default:
                    int randomIndex = new System.Random().Next(scoredSets.Count);
                    return scoredSets[randomIndex].Blocks;
            }
        }

        private DifficultyMode DetermineMode(float fillRate, int score)
        {
            if (fillRate > 0.85f) return DifficultyMode.Mercy;
            if (score > 5000 && fillRate > 0.6f) return DifficultyMode.Execution;
            return DifficultyMode.Neutral;
        }

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
