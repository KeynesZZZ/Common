using System.Collections.Generic;
using System.Linq;
using BlockBlast.Core;

namespace BlockBlast.Algorithms
{
    public class WeightDistributor
    {
        public enum ShapeGrade { Basic, Functional, Executioner }

        private Dictionary<ShapeGrade, float> baseWeights = new Dictionary<ShapeGrade, float>
        {
            { ShapeGrade.Basic, 40f },
            { ShapeGrade.Functional, 40f },
            { ShapeGrade.Executioner, 20f }
        };

        private Dictionary<int, ShapeGrade> shapeGrades = new Dictionary<int, ShapeGrade>
        {
            { 1, ShapeGrade.Basic },
            { 2, ShapeGrade.Basic },
            { 3, ShapeGrade.Basic },
            { 4, ShapeGrade.Functional },
            { 5, ShapeGrade.Functional },
            { 6, ShapeGrade.Basic },
            { 7, ShapeGrade.Functional },
            { 8, ShapeGrade.Functional },
            { 9, ShapeGrade.Functional },
            { 10, ShapeGrade.Functional },
            { 11, ShapeGrade.Executioner },
            { 12, ShapeGrade.Executioner }
        };

        public Dictionary<ShapeGrade, float> GetDynamicWeights(int score, float fillRate, int comboCount)
        {
            var dynamicWeights = new Dictionary<ShapeGrade, float>(baseWeights);

            if (comboCount > 10)
            {
                dynamicWeights[ShapeGrade.Executioner] += 15f;
                dynamicWeights[ShapeGrade.Basic] -= 15f;
            }

            if (score > 10000)
            {
                dynamicWeights[ShapeGrade.Basic] *= 0.5f;
                dynamicWeights[ShapeGrade.Executioner] *= 2.0f;
            }

            if (fillRate > 0.9f)
            {
                dynamicWeights[ShapeGrade.Executioner] = 5f;
            }

            return NormalizeWeights(dynamicWeights);
        }

        public float GetShapeWeight(BlockShape shape, Dictionary<ShapeGrade, float> dynamicWeights)
        {
            if (!shapeGrades.ContainsKey(shape.id))
                return 10f;

            ShapeGrade grade = shapeGrades[shape.id];
            return dynamicWeights.ContainsKey(grade) ? dynamicWeights[grade] : 10f;
        }

        private Dictionary<ShapeGrade, float> NormalizeWeights(Dictionary<ShapeGrade, float> weights)
        {
            float total = weights.Values.Sum();
            return weights.ToDictionary(k => k.Key, v => (v.Value / total) * 100f);
        }
    }
}
