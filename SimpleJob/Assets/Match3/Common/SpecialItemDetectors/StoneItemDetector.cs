using System.Collections.Generic;
using Match3.Core;
using Match3.Interfaces;
using Match3Game.Enums;
using Match3Game.Interfaces;


namespace Match3Game.SpecialItemDetectors
{
    public class StoneItemDetector : ISpecialItemDetector<IUnityGridSlot>
    {
        private readonly GridPosition[] _lookupDirections;

        public StoneItemDetector()
        {
            _lookupDirections = new[]
            {
                GridPosition.Up,
                GridPosition.Down,
                GridPosition.Left,
                GridPosition.Right
            };
        }

        public IEnumerable<IUnityGridSlot> GetSpecialItemGridSlots(IGameBoard<IUnityGridSlot> gameBoard,
            IUnityGridSlot gridSlot)
        {
            if (gridSlot.IsMovable == false)
            {
                yield break;
            }

            foreach (var lookupDirection in _lookupDirections)
            {
                var lookupPosition = gridSlot.GridPosition + lookupDirection;
                if (gameBoard.IsPositionOnGrid(lookupPosition) == false)
                {
                    continue;
                }

                var lookupGridSlot = gameBoard[lookupPosition];
                if (lookupGridSlot.State.GroupId == (int)TileGroup.Stone)
                {
                    yield return lookupGridSlot;
                }
            }
        }
    }
}