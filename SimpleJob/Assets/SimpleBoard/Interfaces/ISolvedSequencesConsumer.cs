using SimpleBoard.Data;

namespace SimpleBoard.Interfaces
{
    public interface ISolvedSequencesConsumer<TGridSlot> where TGridSlot : IGridSlot
    {
        void OnSequencesSolved(SolvedData<TGridSlot> solvedData);
    }
}