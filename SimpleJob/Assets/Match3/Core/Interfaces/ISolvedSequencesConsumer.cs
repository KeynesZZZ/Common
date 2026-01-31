
using Match3.Core;

namespace Match3.Interfaces
{
    public interface ISolvedSequencesConsumer<TGridSlot> where TGridSlot : IGridSlot
    {
        void OnSequencesSolved(SolvedData<TGridSlot> solvedData);
    }
}