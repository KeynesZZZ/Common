
using Match3.Core;

namespace Match3.Interfaces
{
    public interface IGrid
    {
        int RowCount { get; }
        int ColumnCount { get; }

        bool IsPositionOnGrid(GridPosition gridPosition);
    }
}