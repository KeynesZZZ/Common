using System.Collections.Generic;
using BlockBlast.Algorithms;
using BlockBlast.Core;
using UnityEngine;

namespace BlockBlast.Managers
{
    public class BlockGenerator : MonoBehaviour
    {
        private System.Random random = new System.Random();
        private List<BlockShape> shapeTemplates = new List<BlockShape>();
        private List<float> weightBuffer = new List<float>(12);

        private ShadowSimulator shadowSimulator;
        private SurvivalThresholdFilter survivalFilter;
        private WeightDistributor weightDistributor;
        private PatternDetector patternDetector;
        private IntraSetGenerator intraSetGenerator;

        private int comboCount = 0;
        private int consecutiveNonEliminations = 0;

        private void Awake()
        {
            InitializeShapes();
            InitializeAlgorithms();
        }

        private void InitializeShapes()
        {
            shapeTemplates.Add(BlockShape.CreateSingle());
            shapeTemplates.Add(BlockShape.CreateDoubleHorizontal());
            shapeTemplates.Add(BlockShape.CreateDoubleVertical());
            shapeTemplates.Add(BlockShape.CreateTripleHorizontal());
            shapeTemplates.Add(BlockShape.CreateTripleVertical());
            shapeTemplates.Add(BlockShape.CreateSquare());
            shapeTemplates.Add(BlockShape.CreateLShape());
            shapeTemplates.Add(BlockShape.CreateTShape());
            shapeTemplates.Add(BlockShape.CreateLongHorizontal());
            shapeTemplates.Add(BlockShape.CreateLongVertical());
            shapeTemplates.Add(BlockShape.CreateFiveHorizontal());
            shapeTemplates.Add(BlockShape.CreateFiveVertical());
        }

        private void InitializeAlgorithms()
        {
            shadowSimulator = new ShadowSimulator();
            survivalFilter = new SurvivalThresholdFilter();
            weightDistributor = new WeightDistributor();
            patternDetector = new PatternDetector();
            intraSetGenerator = new IntraSetGenerator();
        }

        public BlockShape[] GenerateBlocks(byte[] board, int currentScore, bool hadElimination)
        {
            UpdateComboCount(hadElimination);

            float emptyRate = GetEmptyRate(board);
            var dynamicWeights = weightDistributor.GetDynamicWeights(currentScore, 1f - emptyRate, comboCount);
            bool isWaitingForLongBar = patternDetector.IsWaitingForLongBar(board);

            var candidateSets = GenerateCandidateSets(10, dynamicWeights, isWaitingForLongBar);

            var selectedSet = survivalFilter.FilterCandidateSets(candidateSets, board, currentScore);

            return selectedSet.ToArray();
        }

        private void UpdateComboCount(bool hadElimination)
        {
            if (hadElimination)
            {
                comboCount++;
                consecutiveNonEliminations = 0;
            }
            else
            {
                consecutiveNonEliminations++;
                if (consecutiveNonEliminations > 3)
                {
                    comboCount = Mathf.Max(0, comboCount - 1);
                }
            }
        }

        private float GetEmptyRate(byte[] board)
        {
            int emptyCount = 0;
            for (int i = 0; i < board.Length; i++)
            {
                if (board[i] == 0) emptyCount++;
            }
            return (float)emptyCount / board.Length;
        }

        private List<List<BlockShape>> GenerateCandidateSets(int count, Dictionary<WeightDistributor.ShapeGrade, float> dynamicWeights, bool isWaitingForLongBar)
        {
            var candidateSets = new List<List<BlockShape>>();

            for (int i = 0; i < count; i++)
            {
                var blockSet = GenerateSingleSet(dynamicWeights, isWaitingForLongBar);
                candidateSets.Add(blockSet);
            }

            return candidateSets;
        }

        private List<BlockShape> GenerateSingleSet(Dictionary<WeightDistributor.ShapeGrade, float> dynamicWeights, bool isWaitingForLongBar)
        {
            var blockSet = new List<BlockShape>();

            for (int i = 0; i < 3; i++)
            {
                var block = GenerateSingleBlock(dynamicWeights, isWaitingForLongBar);
                blockSet.Add(block);
            }

            return blockSet;
        }

        private BlockShape GenerateSingleBlock(Dictionary<WeightDistributor.ShapeGrade, float> dynamicWeights, bool isWaitingForLongBar)
        {
            weightBuffer.Clear();
            float totalWeight = 0;

            foreach (var shape in shapeTemplates)
            {
                float weight = weightDistributor.GetShapeWeight(shape, dynamicWeights);
                weight *= patternDetector.GetLongBarWeightModifier(shape, isWaitingForLongBar);

                weightBuffer.Add(weight);
                totalWeight += weight;
            }

            float randomValue = (float)(random.NextDouble() * totalWeight);
            float cumulative = 0;

            for (int i = 0; i < weightBuffer.Count; i++)
            {
                cumulative += weightBuffer[i];
                if (randomValue <= cumulative)
                    return shapeTemplates[i];
            }

            return shapeTemplates[0];
        }

        public void ResetCombo()
        {
            comboCount = 0;
            consecutiveNonEliminations = 0;
        }
    }
}
