using SimpleBoard.Core;
using SimpleBoard.Data;

namespace SimpleBoard.Interfaces
{
    public interface IGridSlot
    {
        long ItemId { get; }
        int ItemSn { get; }
        bool HasItem { get; }
        bool IsMovable { get; }
        bool CanContainItem { get; }
        IGridSlotState State { get; }
        GridPosition GridPosition { get; }
    }
}