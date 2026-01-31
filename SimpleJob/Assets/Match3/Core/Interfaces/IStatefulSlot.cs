namespace Match3.Interfaces
{
    public interface IStatefulSlot
    {
        bool NextState();
        void ResetState();
    }
}