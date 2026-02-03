namespace Bingo.Interfaces
{
    public interface IBingoCard
    {
        int Size { get; }
        bool IsCompleted { get; }
        bool MarkNumber(int number);
        void Reset();
    }
}
