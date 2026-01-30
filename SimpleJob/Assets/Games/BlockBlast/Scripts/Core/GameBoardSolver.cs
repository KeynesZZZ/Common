using SimpleBoard.Core;
using SimpleBoard.Data;
using SimpleBoard.Interfaces;

namespace BlockBlast
{
    public class GameBoardSolver : IGameBoardSolver<IBlockGridSlot>
    {
        public SolvedData<IBlockGridSlot> Solve(IGameBoard<IBlockGridSlot> gameBoard, params GridPosition[] gridPositions)
        {
            throw new System.NotImplementedException();
        }
    }
}