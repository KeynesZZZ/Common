using Match3Game.Interfaces;
using Match3Game.LevelGoals;
using Match3.Interfaces;
using Match3.Core;

namespace Match3Game
{
    public class LevelGoalsProvider : ILevelGoalsProvider<IUnityGridSlot>
    {
        public LevelGoal<IUnityGridSlot>[] GetLevelGoals(int level, IGameBoard<IUnityGridSlot> gameBoard)
        {
            return new LevelGoal<IUnityGridSlot>[] { new CollectRowMaxItems(gameBoard) };
        }
    }
}