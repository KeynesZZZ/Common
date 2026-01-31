
using Match3.Core;

namespace Match3.Interfaces
{
    public interface ILevelGoalsProvider<TGridSlot> where TGridSlot : IGridSlot
    {
        LevelGoal<TGridSlot>[] GetLevelGoals(int level, IGameBoard<TGridSlot> gameBoard);
    }
}