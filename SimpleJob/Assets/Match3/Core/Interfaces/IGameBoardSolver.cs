using Match3.Core;

namespace Match3.Interfaces
{
    public interface IGameBoardSolver<TGridSlot> where TGridSlot : IGridSlot
    {
        SolvedData<TGridSlot> Solve(IGameBoard<TGridSlot> gameBoard, params GridPosition[] gridPositions);
    }
}