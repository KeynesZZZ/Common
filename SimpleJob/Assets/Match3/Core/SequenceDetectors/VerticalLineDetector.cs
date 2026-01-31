
namespace Match3.Core
{
    public class VerticalLineDetector<TGridSlot> : LineDetector<TGridSlot> where TGridSlot : Interfaces.IGridSlot
    {
        private readonly GridPosition[] _lineDirections;

        public VerticalLineDetector()
        {
            _lineDirections = new[] { GridPosition.Up, GridPosition.Down };
        }

        public override ItemSequence<TGridSlot> GetSequence(Interfaces.IGameBoard<TGridSlot> gameBoard, GridPosition gridPosition)
        {
            return GetSequenceByDirection(gameBoard, gridPosition, _lineDirections);
        }
    }
}