using System.Collections.Generic;
using System.Linq;
using BlockBlast.Core;

namespace BlockBlast.Algorithms
{
    /// <summary>
    /// 动态权重分配器 - 根据游戏状态调整方块出现概率
    /// </summary>
    public class WeightDistributor
    {
        public enum ShapeGrade { Basic, Functional, Executioner } // 基础型、功能型、处决型

        /// <summary>
        /// 形状与其基础权重
        /// </summary>
        private Dictionary<ShapeGrade, float> baseWeights = new Dictionary<ShapeGrade, float>
        {
            { ShapeGrade.Basic, 40f },      // 基础块默认 40%
            { ShapeGrade.Functional, 40f }, // 功能块默认 40%
            { ShapeGrade.Executioner, 20f } // 巨型块默认 20%
        };

        /// <summary>
        /// 方块 ID 到形状等级的映射
        /// </summary>
        private Dictionary<int, ShapeGrade> shapeGrades = new Dictionary<int, ShapeGrade>
        {
            { 1, ShapeGrade.Basic },       // 1x1
            { 2, ShapeGrade.Basic },       // 1x2
            { 3, ShapeGrade.Basic },       // 2x1
            { 4, ShapeGrade.Functional }, // 1x3
            { 5, ShapeGrade.Functional }, // 3x1
            { 6, ShapeGrade.Basic },       // 2x2
            { 7, ShapeGrade.Functional }, // L型
            { 8, ShapeGrade.Functional }, // T型
            { 9, ShapeGrade.Functional }, // 1x4
            { 10, ShapeGrade.Functional }, // 4x1
            { 11, ShapeGrade.Executioner }, // 1x5
            { 12, ShapeGrade.Executioner }  // 5x1
        };

        /// <summary>
        /// 根据游戏状态动态获取当前权重分布
        /// </summary>
        public Dictionary<ShapeGrade, float> GetDynamicWeights(int score, float fillRate, int comboCount)
        {
            var dynamicWeights = new Dictionary<ShapeGrade, float>(baseWeights);

            // 核心规则 1：连击惩罚逻辑 (Combo Penalty)
            // 连击越高，系统越倾向于给你"大块"来终结你的连击
            if (comboCount > 10)
            {
                dynamicWeights[ShapeGrade.Executioner] += 15f;
                dynamicWeights[ShapeGrade.Basic] -= 15f;
            }

            // 规则 2：高分收割逻辑 (High Score Harvesting)
            // 分数越高，基础块（小块）的概率越低，让你极难修补碎洞
            if (score > 10000)
            {
                dynamicWeights[ShapeGrade.Basic] *= 0.5f;
                dynamicWeights[ShapeGrade.Executioner] *= 2.0f;
            }

            // 规则 3：绝地求生宽容 (Deathbed Mercy)
            // 棋盘快满时，为了不让玩家立刻挫败，临时提升小块权重
            if (fillRate > 0.9f)
            {
                dynamicWeights[ShapeGrade.Executioner] = 5f;
            }

            return NormalizeWeights(dynamicWeights);
        }

        /// <summary>
        /// 获取指定方块的权重
        /// </summary>
        public float GetShapeWeight(BlockShape shape, Dictionary<ShapeGrade, float> dynamicWeights)
        {
            if (!shapeGrades.ContainsKey(shape.id))
                return 10f;

            ShapeGrade grade = shapeGrades[shape.id];
            return dynamicWeights.ContainsKey(grade) ? dynamicWeights[grade] : 10f;
        }

        /// <summary>
        /// 归一化处理，确保概率总和为 100%
        /// </summary>
        private Dictionary<ShapeGrade, float> NormalizeWeights(Dictionary<ShapeGrade, float> weights)
        {
            float total = weights.Values.Sum();
            return weights.ToDictionary(k => k.Key, v => (v.Value / total) * 100f);
        }
    }
}
