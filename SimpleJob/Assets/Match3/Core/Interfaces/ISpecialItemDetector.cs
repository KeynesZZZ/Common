using System.Collections.Generic;

namespace Match3.Interfaces
{
    public interface ISpecialItemDetector<TGridSlot> where TGridSlot : IGridSlot
    {
        IEnumerable<TGridSlot> GetSpecialItemGridSlots(IGameBoard<TGridSlot> gameBoard, TGridSlot gridSlot);
    }
}