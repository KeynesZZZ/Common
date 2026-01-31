
using Match3.Core;

namespace Match3.Interfaces
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