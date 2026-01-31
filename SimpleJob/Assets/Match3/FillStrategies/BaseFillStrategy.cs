using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Match3Game.Interfaces;
using FillStrategies.Jobs;
using UnityEngine;
using Match3.Interfaces;
using Match3.Core;

namespace  Match3Game.FillStrategies
{
    public abstract class BaseFillStrategy : IBoardFillStrategy<IUnityGridSlot>
    {
        protected readonly IItemsPool<IUnityItem> _itemsPool;
        private readonly IUnityGameBoardRenderer _gameBoardRenderer;
        protected List<IUnityGridSlot> _changedSlots;
        protected BaseFillStrategy(IAppContext appContext)
        {
            _itemsPool = appContext.Resolve<IItemsPool<IUnityItem>>();
            _gameBoardRenderer = appContext.Resolve<IUnityGameBoardRenderer>();
        }

        public List<IUnityGridSlot> GetChangedSlots
        {
            get { return _changedSlots; }
        }

        public abstract string Name { get; }

        public virtual IEnumerable<IJob> GetFillJobs(IGameBoard<IUnityGridSlot> gameBoard)
        {
            var itemsToShow = new List<IUnityItem>();
            for (var rowIndex = 0; rowIndex < gameBoard.RowCount; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < gameBoard.ColumnCount; columnIndex++)
                {
                    var gridSlot = gameBoard[rowIndex, columnIndex];
                    if (gridSlot.CanSetItem == false)
                    {
                        continue;
                    }
                    var item = GetItemFromPool();
                    item.SetWorldPosition(GetWorldPosition(gridSlot.GridPosition));
                    gridSlot.SetItem(item);
                    itemsToShow.Add(item);
                }
            }

            return new[] { new ItemsShowJob(itemsToShow) };
        }

        public abstract IEnumerable<IJob> GetSolveJobs(IGameBoard<IUnityGridSlot> gameBoard,
            SolvedData<IUnityGridSlot> solvedData);
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Vector3 GetWorldPosition(GridPosition gridPosition)
        {
            return _gameBoardRenderer.GetWorldPosition(gridPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IUnityItem GetItemFromPool()
        {
            return _itemsPool.GetItem();
        }
  
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ReturnItemToPool(IUnityItem item)
        {
            _itemsPool.ReturnItem(item);
        }
    }
}