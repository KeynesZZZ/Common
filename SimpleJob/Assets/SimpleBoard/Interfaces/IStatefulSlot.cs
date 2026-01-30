namespace SimpleBoard.Interfaces
{
    public interface IStatefulSlot
    {
        bool NextState();
        void ResetState();
    }
}