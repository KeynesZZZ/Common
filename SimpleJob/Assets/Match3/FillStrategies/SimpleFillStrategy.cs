using System.Collections.Generic;
using Match3Game.Interfaces;
using FillStrategies.Jobs;
using Match3.Interfaces;
using Match3.Core;
using Match3.Extensions;

namespace  Match3Game.FillStrategies
{
    public class SimpleFillStrategy : BaseFillStrategy
    {
        public SimpleFillStrategy(IAppContext appContext) : base(appContext)
        {
        }

        public override string Name => "Simple Fill Strategy";


        public override IEnumerable<IJob> GetSolveJobs(IGameBoard<IUnityGridSlot> gameBoard,
            SolvedData<IUnityGridSlot> solvedData)
        {
            var itemsToHide = new List<IUnityItem>();
            var itemsToShow = new List<IUnityItem>();

            if (null == _changedSlots)
            {
                _changedSlots = new List<IUnityGridSlot>();
            }
            else
            {
                _changedSlots.Clear();
            }

            foreach (var solvedGridSlot in solvedData.GetUniqueSolvedGridSlots(true))
            {
                var newItem = GetItemFromPool();
                var currentItem = solvedGridSlot.Item;

                newItem.SetWorldPosition(currentItem.GetWorldPosition());
                solvedGridSlot.SetItem(newItem);

                itemsToHide.Add(currentItem);
                itemsToShow.Add(newItem);
                if (_changedSlots.Contains(solvedGridSlot) == false)
                {
                    _changedSlots.Add(solvedGridSlot);
                }
                ReturnItemToPool(currentItem);
            }

            foreach (var specialItemGridSlot in solvedData.GetSpecialItemGridSlots(true))
            {
                var item = GetItemFromPool();
                item.SetWorldPosition(GetWorldPosition(specialItemGridSlot.GridPosition));

                specialItemGridSlot.SetItem(item);
                itemsToShow.Add(item);
            }

            return new IJob[] { new ItemsHideJob(itemsToHide), new ItemsShowJob(itemsToShow) };
        }
    }
}