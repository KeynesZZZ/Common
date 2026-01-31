
namespace Match3.Core
{
    public class HorizontalLineDetector<TGridSlot> : LineDetector<TGridSlot> where TGridSlot : Interfaces.IGridSlot
    {
        private readonly GridPosition[] _lineDirections;

        public HorizontalLineDetector()
        {
            _lineDirections = new[] { GridPosition.Left, GridPosition.Right };
        }

        public override ItemSequence<TGridSlot> GetSequence(Interfaces.IGameBoard<TGridSlot> gameBoard, GridPosition gridPosition)
        {
            return GetSequenceByDirection(gameBoard, gridPosition, _lineDirections);
        }
    }
}