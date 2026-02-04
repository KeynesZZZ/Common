using UnityEngine;
using BingoGame.Core.Models;
using System.Linq;

namespace BingoGame.GameModes.ClassicBingo
{
    /// <summary>
    /// 经典数字Bingo棋盘
    /// </summary>
    public class BingoBoard : GameBoard
    {
        /// <summary>
        /// Bingo单元格数组
        /// </summary>
        protected BingoCell[,] bingoCells;

        /// <summary>
        /// 构造函数
        /// </summary>
        public BingoBoard()
        {
            bingoCells = new BingoCell[BoardSize, BoardSize];
            cells = bingoCells;
            InitializeBoard();
        }

        /// <summary>
        /// 初始化棋盘
        /// </summary>
        private void InitializeBoard()
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    bingoCells[row, col] = new BingoCell
                    {
                        Position = new Vector2Int(row, col),
                        Number = 0,
                        IsMarked = false,
                        IsFreeSpace = false
                    };
                }
            }
        }

        /// <summary>
        /// 生成随机数字填充棋盘
        /// </summary>
        /// <param name="minNumber">最小数字</param>
        /// <param name="maxNumber">最大数字</param>
        public void GenerateRandomNumbers(int minNumber = 1, int maxNumber = 75)
        {
            var availableNumbers = new System.Collections.Generic.List<int>();
            for (int i = minNumber; i <= maxNumber; i++)
            {
                availableNumbers.Add(i);
            }

            var random = new System.Random();

            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (row == 2 && col == 2)
                    {
                        bingoCells[row, col].IsFreeSpace = true;
                        bingoCells[row, col].Number = 0;
                        continue;
                    }

                    int index = random.Next(0, availableNumbers.Count);
                    bingoCells[row, col].Number = availableNumbers[index];
                    availableNumbers.RemoveAt(index);
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
            return bingoCells[row, col];
        }

        /// <summary>
        /// 交互指定位置的单元格（标记）
        /// </summary>
        public override void InteractCell(int row, int col)
        {
            if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
            {
                Debug.LogWarning($"无效的单元格位置: ({row}, {col})");
                return;
            }

            if (!bingoCells[row, col].IsMarked)
            {
                bingoCells[row, col].IsMarked = true;
                Debug.Log($"标记单元格 ({row}, {col}), 数字: {bingoCells[row, col].Number}");
            }
        }

        /// <summary>
        /// 检查指定位置的单元格是否已被标记
        /// </summary>
        public override bool IsCellInteracted(int row, int col)
        {
            if (row < 0 || row >= BoardSize || col < 0 || col >= BoardSize)
            {
                return false;
            }
            return bingoCells[row, col].IsMarked;
        }

        /// <summary>
        /// 获取所有单元格
        /// </summary>
        public override GameCell[,] GetAllCells()
        {
            return bingoCells;
        }

        /// <summary>
        /// 重置棋盘
        /// </summary>
        public override void Reset()
        {
            foreach (var cell in bingoCells)
            {
                cell.IsMarked = false;
            }
            Debug.Log("Bingo棋盘已重置");
        }

        /// <summary>
        /// 检查是否所有单元格都已完成
        /// </summary>
        public override bool IsAllCompleted()
        {
            return bingoCells.Cast<BingoCell>().All(c => c.IsMarked || c.IsFreeSpace);
        }

        /// <summary>
        /// 检查是否有指定数字的单元格
        /// </summary>
        /// <param name="number">要查找的数字</param>
        /// <returns>是否包含该数字</returns>
        public bool HasNumber(int number)
        {
            return bingoCells.Cast<BingoCell>().Any(c => c.Number == number);
        }

        /// <summary>
        /// 标记指定数字的单元格
        /// </summary>
        /// <param name="number">要标记的数字</param>
        /// <returns>是否成功标记</returns>
        public bool MarkNumber(int number)
        {
            for (int row = 0; row < BoardSize; row++)
            {
                for (int col = 0; col < BoardSize; col++)
                {
                    if (bingoCells[row, col].Number == number && !bingoCells[row, col].IsMarked)
                    {
                        bingoCells[row, col].IsMarked = true;
                        Debug.Log($"标记数字 {number} 在位置 ({row}, {col})");
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
