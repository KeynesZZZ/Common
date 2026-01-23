namespace SimpleBoard.Data
{
    public interface IGridSlotState
    {
        int GroupId { get; }
        bool IsLocked { get; }
        bool CanContainItem { get; }
    }
}