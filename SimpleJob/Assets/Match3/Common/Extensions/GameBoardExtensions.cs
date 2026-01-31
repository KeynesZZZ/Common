using Match3.Core;
using Match3.Interfaces;
using Match3Game.Interfaces;

namespace Match3Game.Extensions
{
    public static class GameBoardExtensions
    {
        public static bool CanMoveDown(this IGameBoard<IUnityGridSlot> gameBoard, IUnityGridSlot gridSlot,
            out GridPosition gridPosition)
        {
            var bottomGridSlot = gameBoard.GetSideGridSlot(gridSlot, GridPosition.Down);
            if (bottomGridSlot is { CanSetItem: true })
            {
                gridPosition = bottomGridSlot.GridPosition;
                return true;
            }

            gridPosition = GridPosition.Zero;
            return false;
        }

        public static IUnityGridSlot GetSideGridSlot(this IGameBoard<IUnityGridSlot> gameBoard, IUnityGridSlot gridSlot,
            GridPosition direction)
        {
            var sideGridSlotPosition = gridSlot.GridPosition + direction;

            return gameBoard.IsPositionOnGrid(sideGridSlotPosition)
                ? gameBoard[sideGridSlotPosition]
                : null;
        }
    }
}