using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Match3.Interfaces;
using UnityEngine;

namespace Match3.Core
{
    public abstract class Match3Game<TGridSlot> : BaseGame<TGridSlot> where TGridSlot : IGridSlot
    {
        private readonly JobsExecutor _jobsExecutor;
        private readonly IItemSwapper<TGridSlot> _itemSwapper;

        private AsyncLazy _swapItemsTask;
        private IBoardFillStrategy<TGridSlot> _fillStrategy;

        protected Match3Game(GameConfig<TGridSlot> config) : base(config)
        {
            _itemSwapper = config.ItemSwapper;
            _jobsExecutor = new JobsExecutor();
        }

        protected bool IsSwapItemsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _swapItemsTask == null || _swapItemsTask.Task.Status.IsCompleted();
        }

        public async UniTask StartAsync(CancellationToken cancellationToken = default)
        {
            if (_fillStrategy == null)
            {
                throw new NullReferenceException(nameof(_fillStrategy));
            }

            await FillAsync(_fillStrategy, cancellationToken);
            StartGame();
        }

        public async UniTask StopAsync()
        {
            if (IsSwapItemsCompleted == false)
            {
                await _swapItemsTask;
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

        protected async UniTask SwapItemsAsync(GridPosition position1, GridPosition position2,
            CancellationToken cancellationToken = default)
        {
            if (_swapItemsTask?.Task.Status.IsCompleted() ?? true)
            {
                _swapItemsTask = SwapItemsAsync(_fillStrategy, position1, position2, cancellationToken).ToAsyncLazy();
            }

            await _swapItemsTask;
            await DoNext(cancellationToken);
            Debug.Log("SwapItem end");
        }

        private async UniTask FillAsync(IBoardFillStrategy<TGridSlot> fillStrategy,
            CancellationToken cancellationToken = default)
        {
            await _jobsExecutor.ExecuteJobsAsync(fillStrategy.GetFillJobs(GameBoard), cancellationToken);
        }

        private async UniTask SwapItemsAsync(IBoardFillStrategy<TGridSlot> fillStrategy, GridPosition position1,
            GridPosition position2, CancellationToken cancellationToken = default)
        {
            await SwapItems(position1, position2, cancellationToken);
            if (IsSolved(position1, position2, out var solvedData))
            {
                NotifySequencesSolved(solvedData);
                await _jobsExecutor.ExecuteJobsAsync(fillStrategy.GetSolveJobs(GameBoard, solvedData), cancellationToken);
            }
            else
            {
                await SwapItems(position1, position2, cancellationToken);
            }
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

            bool isSolved = IsSolved(posList, out var newSolvedData);
            if (isSolved)
            {
                NotifySequencesSolved(newSolvedData);
                await _jobsExecutor.ExecuteJobsAsync(_fillStrategy.GetSolveJobs(GameBoard, newSolvedData),
                    cancellationToken);
                await DoNext(cancellationToken);
            }
            else
            {
                return;
            }
        }

        private async UniTask SwapItems(GridPosition position1, GridPosition position2,
            CancellationToken cancellationToken = default)
        {
            var gridSlot1 = GameBoard[position1.RowIndex, position1.ColumnIndex];
            var gridSlot2 = GameBoard[position2.RowIndex, position2.ColumnIndex];

            await _itemSwapper.SwapItemsAsync(gridSlot1, gridSlot2, cancellationToken);
        }

        protected override void OnAllGoalsAchieved()
        {
            RaiseGameFinishedAsync().Forget();
        }

        private async UniTask RaiseGameFinishedAsync()
        {
            if (IsSwapItemsCompleted == false)
            {
                await _swapItemsTask;
            }

            base.OnAllGoalsAchieved();
        }
    }
}