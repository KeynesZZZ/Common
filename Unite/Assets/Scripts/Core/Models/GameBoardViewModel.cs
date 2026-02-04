using UnityEngine;
using Cysharp.Threading.Tasks;
using BingoGame.Core.Models;

namespace BingoGame.Core.Models
{
    /// <summary>
    /// 游戏棋盘视图模型抽象基类
    /// </summary>
    public abstract class GameBoardViewModel
    {
        /// <summary>
        /// 棋盘模型
        /// </summary>
        protected GameBoard model;

        /// <summary>
        /// 初始化视图模型
        /// </summary>
        /// <param name="board">棋盘模型</param>
        public GameBoardViewModel(GameBoard board)
        {
            model = board;
        }

        /// <summary>
        /// 初始化视图
        /// </summary>
        public abstract UniTask InitializeAsync();

        /// <summary>
        /// 交互指定位置的单元格
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        public abstract UniTask InteractCellAsync(int row, int col);

        /// <summary>
        /// 高亮指定位置的单元格
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        public abstract UniTask HighlightCellAsync(int row, int col);

        /// <summary>
        /// 重置棋盘视图
        /// </summary>
        public abstract UniTask ResetBoardAsync();
    }
}
