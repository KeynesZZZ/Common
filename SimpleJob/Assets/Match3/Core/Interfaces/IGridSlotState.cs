namespace Match3.Interfaces
{
    public interface IGridSlotState
    {
        int GroupId { get; }
        bool IsLocked { get; }
        bool CanContainItem { get; }
    }
}