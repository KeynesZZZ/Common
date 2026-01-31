using System;
using System.Collections.Generic;
using System.Linq;

namespace BlockBlast.Core
{
    public class BalancedBlockSelector : IBalancedBlockSelector
    {
        private readonly List<BlockShape> _availableShapes;
        private readonly Dictionary<BlockShape, int> _shapeWeights;
        private readonly List<BlockShape> _recentlyUsedShapes;
        private const int MaxRecentShapes = 3;
        private const int MinWeight = 1;
        private const int MaxWeight = 10;

        public BalancedBlockSelector()
        {
            _availableShapes = new List<BlockShape>(BlockShape.AllShapes);
            _shapeWeights = new Dictionary<BlockShape, int>();
            _recentlyUsedShapes = new List<BlockShape>();

            // 初始化权重
            foreach (var shape in _availableShapes)
            {
                _shapeWeights[shape] = 5; // 默认权重
            }
        }

        public List<BlockShape> SelectBlocks(int count = 3)
        {
            var selectedShapes = new List<BlockShape>();

            for (int i = 0; i < count; i++)
            {
                var shape = SelectNextBlockShape();
                selectedShapes.Add(shape);
                UpdateRecentShapes(shape);
                AdjustWeights();
            }

            return selectedShapes;
        }

        private BlockShape SelectNextBlockShape()
        {
            // 计算总权重
            int totalWeight = _shapeWeights.Values.Sum();
            int randomWeight = UnityEngine.Random.Range(0, totalWeight);

            // 根据权重选择形状
            int currentWeight = 0;
            foreach (var shape in _availableShapes)
            {
                currentWeight += _shapeWeights[shape];
                if (randomWeight < currentWeight)
                {
                    return shape;
                }
            }

            // 以防万一，返回第一个形状
            return _availableShapes[0];
        }

        private void UpdateRecentShapes(BlockShape shape)
        {
            _recentlyUsedShapes.Insert(0, shape);
            if (_recentlyUsedShapes.Count > MaxRecentShapes)
            {
                _recentlyUsedShapes.RemoveAt(_recentlyUsedShapes.Count - 1);
            }
        }

        private void AdjustWeights()
        {
            // 降低最近使用过的形状的权重
            foreach (var shape in _recentlyUsedShapes)
            {
                if (_shapeWeights[shape] > MinWeight)
                {
                    _shapeWeights[shape]--;
                }
            }

            // 增加未使用过的形状的权重
            foreach (var shape in _availableShapes)
            {
                if (!_recentlyUsedShapes.Contains(shape) && _shapeWeights[shape] < MaxWeight)
                {
                    _shapeWeights[shape]++;
                }
            }

            // 防止连续生成相同类型的形状
            if (_recentlyUsedShapes.Count >= 2)
            {
                var recentShape = _recentlyUsedShapes[0];
                if (_recentlyUsedShapes.Count(x => x == recentShape) >= 2)
                {
                    // 大幅降低该形状的权重
                    _shapeWeights[recentShape] = MinWeight;
                }
            }

            // 平衡性干预：确保各种形状都有机会出现
            EnsureBalance();
        }

        private void EnsureBalance()
        {
            // 检查是否有形状的权重过低
            foreach (var shape in _availableShapes)
            {
                if (_shapeWeights[shape] == MinWeight && !_recentlyUsedShapes.Contains(shape))
                {
                    // 稍微增加权重，确保它有机会出现
                    _shapeWeights[shape] = MinWeight + 2;
                }
            }

            // 检查是否有形状的权重过高
            foreach (var shape in _availableShapes)
            {
                if (_shapeWeights[shape] == MaxWeight)
                {
                    // 稍微降低权重
                    _shapeWeights[shape] = MaxWeight - 2;
                }
            }
        }

        // 重置权重
        public void ResetWeights()
        {
            foreach (var shape in _availableShapes)
            {
                _shapeWeights[shape] = 5;
            }
            _recentlyUsedShapes.Clear();
        }
    }
}
