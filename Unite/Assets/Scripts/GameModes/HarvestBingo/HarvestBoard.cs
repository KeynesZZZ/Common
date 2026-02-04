using UnityEngine;
using BingoGame.Core;
using BingoGame.Core.Models;
using System.Linq;
using System.Collections.Generic;

namespace BingoGame.GameModes.HarvestBingo
{
    /// <summary>
    /// 割草Bingo棋盘
    /// </summary>
    public class HarvestBoard : GameBoard
    {
        /// <summary>
        /// 草地单元格数组
        /// </summary>
        protected GrassCell[,] grassCells;

        /// <summary>
        /// 各颜色小车数量
        /// </summary>
        private Dictionary<GrassColor, int> carCounts;

        /// <summary>
        /// 各颜色钥匙数量
        /// </summary>
        private Dictionary<GrassColor, int> keyCounts;

        /// <summary>
        /// 构造函数
        /// </summary>
        public HarvestBoard()
        {
            grassCells = new GrassCell[BoardSize, BoardSize];
            cells = grassCells;
            carCounts = new Dictionary<GrassColor, int>();
            keyCounts = new Dictionary<GrassColor, int>();

            foreach (GrassColor color in System.Enum.GetValues(typeof(GrassColor)))
            {
                carCounts[color] = 0;
                keyCounts[color] = 0;
            }

            InitializeBoard();
        }

        /// <summary>
        /// 初始化棋盘
        /// </summary>
        private void InitializeBoard()
        {
            var random = new System.Random();

            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    grassCells[row, col] = new GrassCell
                    {
                        Position = new Vector2Int(row, col),
                        Color = (GrassColor)random.Next(0, 5),
                        IsHarvested = false,
                        GrowthProgress = 1f,
                        HasCar = false,
                        HasKey = false
                    };
                }
            }
        }

        /// <summary>
        /// 获取指定位置的单元格
        /// </summary>
        public override GameCell GetCell(int row, int col)
        {
            if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
            {
                Debug.LogWarning($"无效的单元格位置: ({row}, {col})");
                return null;
            }
            return grassCells[row, col];
        }

        /// <summary>
        /// 交互指定位置的单元格（收割）
        /// </summary>
        public override void InteractCell(int row, int col)
        {
            if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
            {
                Debug.LogWarning($"无效的单元格位置: ({row}, {col})");
                return;
            }

            var cell = grassCells[row, col];

            if (!cell.IsHarvested)
            {
                if (cell.HasCar)
                {
                    cell.IsHarvested = true;
                    cell.HasCar = false;
                    carCounts[cell.Color]--;
                    Debug.Log($"收割单元格 ({row}, {col}), 颜色: {cell.Color}, 使用小车");
                }
                else if (cell.HasKey)
                {
                    HarvestColumn(col);
                    keyCounts[cell.Color]--;
                    Debug.Log($"收割整列 {col}, 颜色: {cell.Color}, 使用钥匙");
                }
                else
                {
                    Debug.LogWarning($"单元格 ({row}, {col}) 无法收割，需要道具");
                }
            }
        }

        /// <summary>
        /// 收割整列
        /// </summary>
        /// <param name="col">列索引</param>
        private void HarvestColumn(int col)
        {
            for (int row = 0; row < BoardSize; row++)
            {
                var cell = grassCells[row, col];
                if (!cell.IsHarvested)
                {
                    cell.IsHarvested = true;
                    Debug.Log($"收割单元格 ({row}, {col})");
                }
            }
        }

        /// <summary>
        /// 检查指定位置的单元格是否已被收割
        /// </summary>
        public override bool IsCellInteracted(int row, int col)
        {
            if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
            {
                return false;
            }
            return grassCells[row, col].IsHarvested;
        }

        /// <summary>
        /// 获取所有单元格
        /// </summary>
        public override GameCell[,] GetAllCells()
        {
            return grassCells;
        }

        /// <summary>
        /// 重置棋盘
        /// </summary>
        public override void Reset()
        {
            foreach (var cell in grassCells)
            {
                cell.IsHarvested = false;
                cell.HasCar = false;
                cell.HasKey = false;
                cell.GrowthProgress = 1f;
            }

            foreach (GrassColor color in System.Enum.GetValues(typeof(GrassColor)))
            {
                carCounts[color] = 0;
                keyCounts[color] = 0;
            }

            Debug.Log("割草棋盘已重置");
        }

        /// <summary>
        /// 检查是否所有单元格都已完成
        /// </summary>
        public override bool IsAllCompleted()
        {
            return grassCells.Cast<GrassCell>().All(c => c.IsHarvested);
        }

        /// <summary>
        /// 添加小车（汽油道具）
        /// </summary>
        /// <param name="color">颜色</param>
        public void AddCar(GrassColor color)
        {
            carCounts[color]++;
            UpdateCarStatus(color);
            Debug.Log($"添加 {color} 小车，当前数量: {carCounts[color]}");
        }

        /// <summary>
        /// 添加钥匙（钥匙道具）
        /// </summary>
        /// <param name="color">颜色</param>
        public void AddKey(GrassColor color)
        {
            keyCounts[color]++;
            UpdateKeyStatus(color);
            Debug.Log($"添加 {color} 钥匙，当前数量: {keyCounts[color]}");
        }

        /// <summary>
        /// 更新小车状态
        /// </summary>
        /// <param name="color">颜色</param>
        private void UpdateCarStatus(GrassColor color)
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    var cell = grassCells[row, col];
                    if (cell.Color == color && !cell.IsHarvested)
                    {
                        cell.HasCar = carCounts[color] > 0;
                    }
                }
            }
        }

        /// <summary>
        /// 更新钥匙状态
        /// </summary>
        /// <param name="color">颜色</param>
        private void UpdateKeyStatus(GrassColor color)
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    var cell = grassCells[row, col];
                    if (cell.Color == color && !cell.IsHarvested)
                    {
                        cell.HasKey = keyCounts[color] > 0;
                    }
                }
            }
        }

        /// <summary>
        /// 检查是否有指定颜色的小车
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>是否有小车</returns>
        public bool HasCar(GrassColor color)
        {
            return carCounts[color] > 0;
        }

        /// <summary>
        /// 检查是否有指定颜色的钥匙
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>是否有钥匙</returns>
        public bool HasKey(GrassColor color)
        {
            return keyCounts[color] > 0;
        }

        /// <summary>
        /// 获取指定颜色的小车数量
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>小车数量</returns>
        public int GetCarCount(GrassColor color)
        {
            return carCounts[color];
        }

        /// <summary>
        /// 获取指定颜色的钥匙数量
        /// </summary>
        /// <param name="color">颜色</param>
        /// <returns>钥匙数量</returns>
        public int GetKeyCount(GrassColor color)
        {
            return keyCounts[color];
        }
    }
}
