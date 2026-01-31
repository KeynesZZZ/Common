using System.Collections.Generic;
using Match3.Interfaces;
using Match3Game.Interfaces;


namespace Match3Game.Extensions
{
    public static class ItemsPoolExtensions
    {
        public static void ReturnAllItems(this IItemsPool<IUnityItem> itemsPool, IEnumerable<IUnityGridSlot> gridSlots)
        {
            foreach (var gridSlot in gridSlots)
            {
                if (gridSlot.Item == null)
                {
                    continue;
                }

                itemsPool.ReturnItem(gridSlot.Item);
                gridSlot.Item.Hide();
                gridSlot.Clear();
            }
        }
    }
}