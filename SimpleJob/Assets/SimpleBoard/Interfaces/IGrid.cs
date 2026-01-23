using SimpleBoard.Core;

namespace SimpleBoard.Interfaces
{
    public interface IGrid
    {
        int RowCount { get; }
        int ColumnCount { get; }
        bool IsPositionOnGrid(GridPosition gridPosition);
    }
}