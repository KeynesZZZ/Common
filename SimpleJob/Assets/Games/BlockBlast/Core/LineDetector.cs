using Match3.Core;
using Match3.Interfaces;

namespace BlockBlast.Core
{
    public class LineDetector<TGridSlot> where TGridSlot : IGridSlot
    {
        public bool IsLineFull(IGameBoard<TGridSlot> gameBoard, int index, bool isRow)
        {
            if (isRow)
            {
                if (index < 0 || index >= gameBoard.RowCount)
                    return false;

                for (int columnIndex = 0; columnIndex < gameBoard.ColumnCount; columnIndex++)
                {
                    if (!gameBoard[index, columnIndex].HasItem)
                        return false;
                }
            }
            else
            {
                if (index < 0 || index >= gameBoard.ColumnCount)
                    return false;

                for (int rowIndex = 0; rowIndex < gameBoard.RowCount; rowIndex++)
                {
                    if (!gameBoard[rowIndex, index].HasItem)
                        return false;
                }
            }

            return true;
        }

        public int GetFullLinesCount(IGameBoard<TGridSlot> gameBoard)
        {
            int fullLines = 0;

            // 检查行
            for (int rowIndex = 0; rowIndex < gameBoard.RowCount; rowIndex++)
            {
                if (IsLineFull(gameBoard, rowIndex, true))
                    fullLines++;
            }

            // 检查列
            for (int columnIndex = 0; columnIndex < gameBoard.ColumnCount; columnIndex++)
            {
                if (IsLineFull(gameBoard, columnIndex, false))
                    fullLines++;
            }

            return fullLines;
        }
    }
}
