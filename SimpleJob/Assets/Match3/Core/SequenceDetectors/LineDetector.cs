using System.Collections.Generic;
using Match3.Interfaces;

namespace Match3.Core
{
    public abstract class LineDetector<TGridSlot> : ISequenceDetector<TGridSlot> where TGridSlot : Interfaces.IGridSlot
    {
        public abstract ItemSequence<TGridSlot> GetSequence(Interfaces.IGameBoard<TGridSlot> gameBoard, GridPosition gridPosition);

        protected ItemSequence<TGridSlot> GetSequenceByDirection(Interfaces.IGameBoard<TGridSlot> gameBoard, GridPosition gridPosition,
            IEnumerable<GridPosition> directions)
        {
            var gridSlot = gameBoard[gridPosition];
            var gridSlots = new List<TGridSlot>();

            foreach (var direction in directions)
            {
                gridSlots.AddRange(GetSequenceOfGridSlots(gameBoard, gridSlot, gridPosition, direction));
            }

            if (gridSlots.Count < 2)
            {
                return null;
            }

            gridSlots.Add(gridSlot);

            return new ItemSequence<TGridSlot>(GetType(), gridSlots);
        }

        private IEnumerable<TGridSlot> GetSequenceOfGridSlots(Interfaces.IGameBoard<TGridSlot> gameBoard,
            TGridSlot gridSlot, GridPosition gridPosition, GridPosition direction)
        {
            var newPosition = gridPosition + direction;
            var slotsSequence = new List<TGridSlot>();

            while (gameBoard.IsPositionOnBoard(newPosition))
            {
                var currentSlot = gameBoard[newPosition];
                if (currentSlot.HasItem == false || gridSlot.HasItem == false)
                {
                    break;
                }

                if (currentSlot.ItemSn == gridSlot.ItemSn)
                {
                    newPosition += direction;
                    slotsSequence.Add(currentSlot);
                }
                else
                {
                    break;
                }
            }

            return slotsSequence;
        }
    }
}