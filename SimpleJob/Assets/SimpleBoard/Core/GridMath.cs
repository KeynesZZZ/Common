using SimpleBoard.Core;
using SimpleBoard.Interfaces;

namespace SimpleBoard.Core
{
    public static class GridMath
    {
        public static bool IsPositionOnGrid(this IGrid grid, GridPosition gridPosition)
        {
            return IsPositionOnGrid(gridPosition, grid.RowCount, grid.ColumnCount);
        }

        public static bool IsPositionOnGrid(GridPosition gridPosition, int rowCount, int columnCount)
        {
            return gridPosition.RowIndex >= 0 &&
                   gridPosition.RowIndex < rowCount &&
                   gridPosition.ColumnIndex >= 0 &&
                   gridPosition.ColumnIndex < columnCount;
        }
    }
}