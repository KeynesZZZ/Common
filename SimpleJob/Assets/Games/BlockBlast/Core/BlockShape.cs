using System.Collections.Generic;
using Match3.Core;

namespace BlockBlast.Core
{
    public class BlockShape
    {
        public static readonly BlockShape I = new BlockShape(new[] { new GridPosition(0, 0), new GridPosition(1, 0), new GridPosition(2, 0), new GridPosition(3, 0) }, "I");
        public static readonly BlockShape J = new BlockShape(new[] { new GridPosition(0, 0), new GridPosition(1, 0), new GridPosition(2, 0), new GridPosition(2, 1) }, "J");
        public static readonly BlockShape L = new BlockShape(new[] { new GridPosition(0, 1), new GridPosition(1, 1), new GridPosition(2, 1), new GridPosition(2, 0) }, "L");
        public static readonly BlockShape O = new BlockShape(new[] { new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(1, 0), new GridPosition(1, 1) }, "O");
        public static readonly BlockShape S = new BlockShape(new[] { new GridPosition(0, 1), new GridPosition(0, 2), new GridPosition(1, 0), new GridPosition(1, 1) }, "S");
        public static readonly BlockShape T = new BlockShape(new[] { new GridPosition(0, 1), new GridPosition(1, 0), new GridPosition(1, 1), new GridPosition(1, 2) }, "T");
        public static readonly BlockShape Z = new BlockShape(new[] { new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(1, 1), new GridPosition(1, 2) }, "Z");
        public static readonly BlockShape Square = new BlockShape(new[] { new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(1, 0), new GridPosition(1, 1) }, "Square");
        public static readonly BlockShape Line = new BlockShape(new[] { new GridPosition(0, 0), new GridPosition(0, 1), new GridPosition(0, 2) }, "Line");

        public static readonly List<BlockShape> AllShapes = new List<BlockShape>
        {
            I, J, L, O, S, T, Z, Square, Line
        };

        public GridPosition[] Cells { get; }
        public string Name { get; }

        private BlockShape(GridPosition[] cells, string name)
        {
            Cells = cells;
            Name = name;
        }

        public int GetWidth()
        {
            int minX = int.MaxValue;
            int maxX = int.MinValue;

            foreach (var cell in Cells)
            {
                if (cell.ColumnIndex < minX) minX = cell.ColumnIndex;
                if (cell.ColumnIndex > maxX) maxX = cell.ColumnIndex;
            }

            return maxX - minX + 1;
        }

        public int GetHeight()
        {
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            foreach (var cell in Cells)
            {
                if (cell.RowIndex < minY) minY = cell.RowIndex;
                if (cell.RowIndex > maxY) maxY = cell.RowIndex;
            }

            return maxY - minY + 1;
        }

        public BlockShape Rotate()
        {
            var newCells = new GridPosition[Cells.Length];
            for (int i = 0; i < Cells.Length; i++)
            {
                var cell = Cells[i];
                newCells[i] = new GridPosition(-cell.ColumnIndex, cell.RowIndex);
            }
            return new BlockShape(newCells, Name + "_Rotated");
        }
    }
}
