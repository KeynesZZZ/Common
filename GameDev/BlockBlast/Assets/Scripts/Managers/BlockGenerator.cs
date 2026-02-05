using System.Collections.Generic;
using BlockBlast.Algorithms;
using BlockBlast.Core;
using UnityEngine;

namespace BlockBlast.Managers
{
    /// <summary>
    /// 方块生成器 - 整合所有算法实现智能出牌系统
    /// </summary>
    public class BlockGenerator : MonoBehaviour
    {
        private System.Random random = new System.Random();
        private List<BlockShape> shapeTemplates = new List<BlockShape>();
        private List<float> weightBuffer = new List<float>(12);

        // 核心算法组件
        private ShadowSimulator shadowSimulator;
        private SurvivalThresholdFilter survivalFilter;
        private WeightDistributor weightDistributor;
        private PatternDetector patternDetector;
        private IntraSetGenerator intraSetGenerator;

        // 游戏状态追踪
        private int comboCount = 0;
        private int consecutiveNonEliminations = 0;

        private void Awake()
        {
            InitializeShapes();
            InitializeAlgorithms();
        }

        /// <summary>
        /// 初始化所有方块模板
        /// </summary>
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

        /// <summary>
        /// 初始化所有算法组件
        /// </summary>
        private void InitializeAlgorithms()
        {
            shadowSimulator = new ShadowSimulator();
            survivalFilter = new SurvivalThresholdFilter();
            weightDistributor = new WeightDistributor();
            patternDetector = new PatternDetector();
            intraSetGenerator = new IntraSetGenerator();
        }

        /// <summary>
        /// 生成三个方块（核心方法）
        /// </summary>
        /// <param name="board">当前棋盘状态</param>
        /// <param name="currentScore">当前分数</param>
        /// <param name="hadElimination">上一次是否产生消除</param>
        public BlockShape[] GenerateBlocks(byte[] board, int currentScore, bool hadElimination)
        {
            // 更新连击计数
            UpdateComboCount(hadElimination);

            // 计算空格率
            float emptyRate = GetEmptyRate(board);

            // 获取动态权重分布（根据分数、填充率、连击数）
            var dynamicWeights = weightDistributor.GetDynamicWeights(currentScore, 1f - emptyRate, comboCount);

            // 检测玩家是否在等长条
            bool isWaitingForLongBar = patternDetector.IsWaitingForLongBar(board);

            // 生成多个候选方案供生存阈值过滤选择
            var candidateSets = GenerateCandidateSets(10, dynamicWeights, isWaitingForLongBar);

            // 使用生存阈值过滤选择最佳方案
            var selectedSet = survivalFilter.FilterCandidateSets(candidateSets, board, currentScore);

            return selectedSet.ToArray();
        }

        /// <summary>
        /// 更新连击计数
        /// </summary>
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
                // 连续 3 次未消除，连击数递减
                if (consecutiveNonEliminations > 3)
                {
                    comboCount = Mathf.Max(0, comboCount - 1);
                }
            }
        }

        /// <summary>
        /// 计算棋盘空格率
        /// </summary>
        private float GetEmptyRate(byte[] board)
        {
            int emptyCount = 0;
            for (int i = 0; i < board.Length; i++)
            {
                if (board[i] == 0) emptyCount++;
            }
            return (float)emptyCount / board.Length;
        }

        /// <summary>
        /// 生成多个候选方块组合
        /// </summary>
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

        /// <summary>
        /// 生成单个方块组合（3个方块）
        /// </summary>
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

        /// <summary>
        /// 根据权重生成单个方块
        /// </summary>
        private BlockShape GenerateSingleBlock(Dictionary<WeightDistributor.ShapeGrade, float> dynamicWeights, bool isWaitingForLongBar)
        {
            weightBuffer.Clear();
            float totalWeight = 0;

            foreach (var shape in shapeTemplates)
            {
                // 获取基础权重
                float weight = weightDistributor.GetShapeWeight(shape, dynamicWeights);

                // 应用长条饥饿修正
                weight *= patternDetector.GetLongBarWeightModifier(shape, isWaitingForLongBar);

                weightBuffer.Add(weight);
                totalWeight += weight;
            }

            // 加权随机选择
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

        /// <summary>
        /// 重置连击计数（新游戏开始时调用）
        /// </summary>
        public void ResetCombo()
        {
            comboCount = 0;
            consecutiveNonEliminations = 0;
        }
    }
}
