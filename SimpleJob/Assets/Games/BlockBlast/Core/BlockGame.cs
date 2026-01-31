using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Match3.Core;
using Match3.Interfaces;
using UnityEngine;

namespace BlockBlast.Core
{
    public abstract class BlockGame<TGridSlot> : BaseGame<TGridSlot> where TGridSlot : IGridSlot
    {
        private readonly JobsExecutor _jobsExecutor;
        private readonly IBlockPlacer<TGridSlot> _blockPlacer;
        private readonly IBalancedBlockSelector _blockSelector;

        private AsyncLazy _placeBlockTask;
        private IBoardFillStrategy<TGridSlot> _fillStrategy;

        protected BlockGame(BlockGameConfig<TGridSlot> config) : base(config)
        {
            _blockPlacer = config.BlockPlacer;
            _blockSelector = config.BlockSelector;
            _jobsExecutor = new JobsExecutor();
        }

        protected bool IsPlaceBlockCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _placeBlockTask == null || _placeBlockTask.Task.Status.IsCompleted();
        }

        public async UniTask StopAsync()
        {
            if (IsPlaceBlockCompleted == false)
            {
                await _placeBlockTask;
            }

            StopGame();
        }

        protected override void OnGameStarted()
        {

        }

        protected List<IGridSlot> GetGridSlots()
        {
            List<IGridSlot> slots = new List<IGridSlot>();
            for (var rowIndex = 0; rowIndex < GameBoard.RowCount; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < GameBoard.ColumnCount; columnIndex++)
                {
                    var slot = GameBoard[rowIndex, columnIndex];
                    slots.Add(slot);
                }
            }

            return slots;
        }

        public void SetGameBoardFillStrategy(IBoardFillStrategy<TGridSlot> fillStrategy)
        {
            _fillStrategy = fillStrategy;
        }

        protected async UniTask PlaceBlockAsync(GridPosition position, BlockShape blockShape, int blockType,
            CancellationToken cancellationToken = default)
        {
            if (_placeBlockTask?.Task.Status.IsCompleted() ?? true)
            {
                _placeBlockTask = PlaceBlockAsync(_fillStrategy, position, blockShape, blockType, cancellationToken).ToAsyncLazy();
            }

            await _placeBlockTask;
            await DoNext(cancellationToken);
        }

        private async UniTask FillAsync(IBoardFillStrategy<TGridSlot> fillStrategy,
            CancellationToken cancellationToken = default)
        {
            await _jobsExecutor.ExecuteJobsAsync(fillStrategy.GetFillJobs(GameBoard), cancellationToken);
        }

        private async UniTask PlaceBlockAsync(IBoardFillStrategy<TGridSlot> fillStrategy, GridPosition position, BlockShape blockShape, int blockType,
            CancellationToken cancellationToken = default)
        {
            if (_blockPlacer.CanPlaceBlock(GameBoard, position, blockShape))
            {
                await _blockPlacer.PlaceBlockAsync(GameBoard, position, blockShape, blockType, cancellationToken);
                
                var clearedLines = await ScanAndClearLinesAsync(cancellationToken);
                if (clearedLines > 0)
                {
                    NotifyLinesCleared(clearedLines);
                    await _jobsExecutor.ExecuteJobsAsync(fillStrategy.GetSolveJobs(GameBoard, null), cancellationToken);
                }
            }
        }

        private async UniTask<int> ScanAndClearLinesAsync(CancellationToken cancellationToken = default)
        {
            var lineDetector = new LineDetector<TGridSlot>();
            var clearedLines = 0;

            // 扫描行
            for (var rowIndex = 0; rowIndex < GameBoard.RowCount; rowIndex++)
            {
                if (lineDetector.IsLineFull(GameBoard, rowIndex, true))
                {
                    clearedLines++;
                    await ClearLineAsync(rowIndex, true, cancellationToken);
                }
            }

            // 扫描列
            for (var columnIndex = 0; columnIndex < GameBoard.ColumnCount; columnIndex++)
            {
                if (lineDetector.IsLineFull(GameBoard, columnIndex, false))
                {
                    clearedLines++;
                    await ClearLineAsync(columnIndex, false, cancellationToken);
                }
            }

            return clearedLines;
        }

        private async UniTask ClearLineAsync(int index, bool isRow, CancellationToken cancellationToken = default)
        {
            var slotsToClear = new List<TGridSlot>();

            if (isRow)
            {
                for (var columnIndex = 0; columnIndex < GameBoard.ColumnCount; columnIndex++)
                {
                    slotsToClear.Add(GameBoard[index, columnIndex]);
                }
            }
            else
            {
                for (var rowIndex = 0; rowIndex < GameBoard.RowCount; rowIndex++)
                {
                    slotsToClear.Add(GameBoard[rowIndex, index]);
                }
            }

            foreach (var slot in slotsToClear)
            {
                slot.Clear();
            }

            await UniTask.Yield(cancellationToken);
        }

        private async UniTask DoNext(CancellationToken cancellationToken = default)
        {
            var list = _fillStrategy.GetChangedSlots;
            if (list == null || list.Count() == 0)
            {
                return;
            }

            var posList = new List<GridPosition>();
            foreach (var slot in list)
            {
                posList.Add(slot.GridPosition);
            }

            var clearedLines = await ScanAndClearLinesAsync(cancellationToken);
            if (clearedLines > 0)
            {
                NotifyLinesCleared(clearedLines);
                await _jobsExecutor.ExecuteJobsAsync(_fillStrategy.GetSolveJobs(GameBoard, null),
                    cancellationToken);
                await DoNext(cancellationToken);
            }
            else
            {
                return;
            }
        }

        protected virtual void NotifyLinesCleared(int lineCount)
        {
            // 触发Combo累加
        }

        public bool IsGameOver()
        {
            // 检查是否还有可放置的位置
            for (var rowIndex = 0; rowIndex < GameBoard.RowCount; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < GameBoard.ColumnCount; columnIndex++)
                {
                    var position = new GridPosition(rowIndex, columnIndex);
                    // 检查是否可以放置任何形状的方块
                    foreach (var shape in BlockShape.AllShapes)
                    {
                        if (_blockPlacer.CanPlaceBlock(GameBoard, position, shape))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public List<BlockShape> GenerateNextBlocks(int count = 3)
        {
            return _blockSelector.SelectBlocks(count);
        }
    }
}
