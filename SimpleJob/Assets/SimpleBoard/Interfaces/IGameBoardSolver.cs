using SimpleBoard.Core;
using SimpleBoard.Data;

namespace SimpleBoard.Interfaces
{
    public interface IGameBoardSolver<TGridSlot> where TGridSlot : IGridSlot
    {
        SolvedData<TGridSlot> Solve(IGameBoard<TGridSlot> gameBoard, params GridPosition[] gridPositions);
    }
}