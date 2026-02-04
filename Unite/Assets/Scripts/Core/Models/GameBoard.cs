using UnityEngine;
using Cysharp.Threading.Tasks;

namespace BingoGame.Core.Models
{
    /// <summary>
    /// 游戏棋盘抽象基类
    /// 所有玩法的棋盘都继承此类
    /// </summary>
    public abstract class GameBoard
    {
        /// <summary>
        /// 棋盘大小（5x5）
        /// </summary>
        public const int BoardSize = 5;

        /// <summary>
        /// 棋盘单元格数组
        /// </summary>
        protected GameCell[,] cells;

        /// <summary>
        /// 获取指定位置的单元格
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        /// <returns>单元格对象</returns>
        public abstract GameCell GetCell(int row, int col);

        /// <summary>
        /// 交互指定位置的单元格
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        public abstract void InteractCell(int row, int col);

        /// <summary>
        /// 检查指定位置的单元格是否已被交互
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        /// <returns>是否已被交互</returns>
        public abstract bool IsCellInteracted(int row, int col);

        /// <summary>
        /// 获取所有单元格
        /// </summary>
        /// <returns>单元格数组</returns>
        public abstract GameCell[,] GetAllCells();

        /// <summary>
        /// 重置棋盘
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// 检查是否所有单元格都已完成
        /// </summary>
        /// <returns>是否全部完成</returns>
        public abstract bool IsAllCompleted();
    }
}
