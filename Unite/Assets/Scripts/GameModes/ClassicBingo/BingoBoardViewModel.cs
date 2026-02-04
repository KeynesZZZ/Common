using UnityEngine;
using Cysharp.Threading.Tasks;
using BingoGame.Core;
using BingoGame.Core.Models;
using BingoGame.GameModes.ClassicBingo;

namespace BingoGame.GameModes.ClassicBingo
{
    /// <summary>
    /// Bingo棋盘视图模型
    /// </summary>
    public class BingoBoardViewModel : GameBoardViewModel
    {
        /// <summary>
        /// Bingo棋盘
        /// </summary>
        private BingoBoard bingoBoard;

        /// <summary>
        /// Bingo棋盘视图接口
        /// </summary>
        private IBingoBoardView view;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="board">Bingo棋盘</param>
        /// <param name="view">Bingo棋盘视图</param>
        public BingoBoardViewModel(BingoBoard board, IBingoBoardView view) : base(board)
        {
            this.bingoBoard = board;
            this.view = view;
        }

        /// <summary>
        /// 初始化视图
        /// </summary>
        public override async UniTask InitializeAsync()
        {
            await view.InitializeBoardAsync(bingoBoard);
            Debug.Log("Bingo棋盘视图模型初始化完成");
        }

        /// <summary>
        /// 交互指定位置的单元格
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        public override async UniTask InteractCellAsync(int row, int col)
        {
            bingoBoard.InteractCell(row, col);
            await view.UpdateCellAsync(row, col);
            
            if (AnimationService.Instance != null)
            {
                var cellTransform = view.GetCellTransform(row, col);
                if (cellTransform != null)
                {
                    await AnimationService.Instance.PlayCellMarkAnimationAsync(cellTransform);
                }
            }
        }

        /// <summary>
        /// 高亮指定位置的单元格
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        public override async UniTask HighlightCellAsync(int row, int col)
        {
            await view.HighlightCellAsync(row, col);
        }

        /// <summary>
        /// 重置棋盘视图
        /// </summary>
        public override async UniTask ResetBoardAsync()
        {
            bingoBoard.Reset();
            await view.ResetBoardAsync();
            Debug.Log("Bingo棋盘视图已重置");
        }
    }

    /// <summary>
    /// Bingo棋盘视图接口
    /// </summary>
    public interface IBingoBoardView
    {
        /// <summary>
        /// 初始化棋盘视图
        /// </summary>
        /// <param name="board">Bingo棋盘</param>
        UniTask InitializeBoardAsync(BingoBoard board);

        /// <summary>
        /// 更新指定位置的单元格
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        UniTask UpdateCellAsync(int row, int col);

        /// <summary>
        /// 高亮指定位置的单元格
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        UniTask HighlightCellAsync(int row, int col);

        /// <summary>
        /// 重置棋盘视图
        /// </summary>
        UniTask ResetBoardAsync();

        /// <summary>
        /// 获取指定位置的单元格Transform
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        /// <returns>单元格Transform</returns>
        Transform GetCellTransform(int row, int col);
    }
}
