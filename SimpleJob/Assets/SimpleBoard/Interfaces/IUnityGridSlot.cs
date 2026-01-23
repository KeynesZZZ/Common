using SimpleBoard.Data;

namespace SimpleBoard.Interfaces
{
    public interface IUnityGridSlot : IGridSlot
    {
        bool CanSetItem { get; }
        bool NotAvailable { get; }

        IUnityItem Item { get; }

        void SetItem(IUnityItem item);
        void SetState(IGridSlotState state);
        void Clear();
    }
}