
using Match3.Core;

namespace Match3.Interfaces
{
    public interface ISequenceDetector<TGridSlot> where TGridSlot : IGridSlot
    {
        ItemSequence<TGridSlot> GetSequence(IGameBoard<TGridSlot> gameBoard, GridPosition gridPosition);
    }
}