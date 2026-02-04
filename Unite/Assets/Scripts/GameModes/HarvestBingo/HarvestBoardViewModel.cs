using UnityEngine;
using Cysharp.Threading.Tasks;
using BingoGame.Core;
using BingoGame.Core.Models;
using BingoGame.GameModes.HarvestBingo;

namespace BingoGame.GameModes.HarvestBingo
{
    /// <summary>
    /// 割草棋盘视图模型
    /// </summary>
    public class HarvestBoardViewModel : GameBoardViewModel
    {
        /// <summary>
        /// 割草棋盘
        /// </summary>
        private HarvestBoard harvestBoard;

        /// <summary>
        /// 割草棋盘视图接口
        /// </summary>
        private IHarvestBoardView view;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="board">割草棋盘</param>
        /// <param name="view">割草棋盘视图</param>
        public HarvestBoardViewModel(HarvestBoard board, IHarvestBoardView view) : base(board)
        {
            this.harvestBoard = board;
            this.view = view;
        }

        /// <summary>
        /// 初始化视图
        /// </summary>
        public override async UniTask InitializeAsync()
        {
            await view.InitializeBoardAsync(harvestBoard);
            Debug.Log("割草棋盘视图模型初始化完成");
        }

        /// <summary>
        /// 交互指定位置的单元格
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        public override async UniTask InteractCellAsync(int row, int col)
        {
            var cell = harvestBoard.GetCell(row, col) as GrassCell;

            if (cell == null)
            {
                Debug.LogWarning($"无效的单元格位置: ({row}, {col})");
                return;
            }

            if (!cell.IsHarvested)
            {
                if (cell.HasCar)
                {
                    harvestBoard.InteractCell(row, col);
                    await view.UpdateCellAsync(row, col);
                    
                    if (AnimationService.Instance != null)
                    {
                        await AnimationService.Instance.PlayHarvestAnimationAsync(row, col);
                    }
                }
                else if (cell.HasKey)
                {
                    harvestBoard.InteractCell(row, col);
                    await view.UpdateColumnAsync(col);
                    
                    if (AnimationService.Instance != null)
                    {
                        await AnimationService.Instance.PlayColumnHarvestAnimationAsync(col);
                    }
                }
                else
                {
                    Debug.LogWarning($"单元格 ({row}, {col}) 无法收割，需要道具");
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
            harvestBoard.Reset();
            await view.ResetBoardAsync();
            Debug.Log("割草棋盘视图已重置");
        }
    }

    /// <summary>
    /// 割草棋盘视图接口
    /// </summary>
    public interface IHarvestBoardView
    {
        /// <summary>
        /// 初始化棋盘视图
        /// </summary>
        /// <param name="board">割草棋盘</param>
        UniTask InitializeBoardAsync(HarvestBoard board);

        /// <summary>
        /// 更新指定位置的单元格
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        UniTask UpdateCellAsync(int row, int col);

        /// <summary>
        /// 更新整列
        /// </summary>
        /// <param name="col">列索引</param>
        UniTask UpdateColumnAsync(int col);

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
