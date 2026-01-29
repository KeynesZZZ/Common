using System.Collections.Generic;
using SimpleBoard.Core;
using UnityEngine;

namespace BlockBlast
{
    /// <summary>
    /// 方块数据类 - 不可变数据对象
    /// </summary>
    public class BlockData
    {
        public BlockShape Shape { get; }
        public Color Color { get; }
        public IReadOnlyList<GridPosition> Cells { get; }
        public int Width { get; }
        public int Height { get; }

        public BlockData(BlockShape shape, Color color)
        {
            Shape = shape;
            Color = color;
            Cells = GetShapeCells(shape);
            (Width, Height) = CalculateDimensions(Cells);
        }

        /// <summary>
        /// 获取方块形状的单元格位置
        /// </summary>
        private static List<GridPosition> GetShapeCells(BlockShape shape)
        {
            var cells = new List<GridPosition>();

            switch (shape)
            {
                case BlockShape.Single:
                    cells.Add(GridPosition.Zero);
                    break;

                case BlockShape.Horizontal2:
                    cells.Add(GridPosition.Zero);
                    cells.Add(GridPosition.Right);
                    break;

                case BlockShape.Vertical2:
                    cells.Add(GridPosition.Zero);
                    cells.Add(GridPosition.Down);
                    break;

                case BlockShape.Horizontal3:
                    cells.Add(GridPosition.Zero);
                    cells.Add(GridPosition.Right);
                    cells.Add(GridPosition.Right + GridPosition.Right);
                    break;

                case BlockShape.Vertical3:
                    cells.Add(GridPosition.Zero);
                    cells.Add(GridPosition.Down);
                    cells.Add(GridPosition.Down + GridPosition.Down);
                    break;

                case BlockShape.Square2x2:
                    cells.Add(GridPosition.Zero);
                    cells.Add(GridPosition.Right);
                    cells.Add(GridPosition.Down);
                    cells.Add(GridPosition.Down + GridPosition.Right);
                    break;

                case BlockShape.LShape:
                    cells.Add(GridPosition.Zero);
                    cells.Add(GridPosition.Down);
                    cells.Add(GridPosition.Down + GridPosition.Right);
                    break;

                case BlockShape.LShapeReverse:
                    cells.Add(GridPosition.Right);
                    cells.Add(GridPosition.Down);
                    cells.Add(GridPosition.Down + GridPosition.Right);
                    break;

                case BlockShape.TShape:
                    cells.Add(GridPosition.Zero);
                    cells.Add(GridPosition.Right);
                    cells.Add(GridPosition.Right + GridPosition.Right);
                    cells.Add(GridPosition.Down + GridPosition.Right);
                    break;

                case BlockShape.ZShape:
                    cells.Add(GridPosition.Zero);
                    cells.Add(GridPosition.Right);
                    cells.Add(GridPosition.Down + GridPosition.Right);
                    cells.Add(GridPosition.Down + GridPosition.Right + GridPosition.Right);
                    break;

                case BlockShape.ZShapeReverse:
                    cells.Add(GridPosition.Right);
                    cells.Add(GridPosition.Right + GridPosition.Right);
                    cells.Add(GridPosition.Down);
                    cells.Add(GridPosition.Down + GridPosition.Right);
                    break;

                case BlockShape.Cross:
                    cells.Add(GridPosition.Right);
                    cells.Add(GridPosition.Down);
                    cells.Add(GridPosition.Down + GridPosition.Right);
                    cells.Add(GridPosition.Down + GridPosition.Right + GridPosition.Right);
                    cells.Add(GridPosition.Down + GridPosition.Down + GridPosition.Right);
                    break;
            }

            return cells;
        }

        /// <summary>
        /// 计算方块的宽度和高度
        /// </summary>
        private static (int width, int height) CalculateDimensions(IReadOnlyList<GridPosition> cells)
        {
            int maxRow = 0;
            int maxCol = 0;

            foreach (var cell in cells)
            {
                maxRow = Mathf.Max(maxRow, cell.RowIndex);
                maxCol = Mathf.Max(maxCol, cell.ColumnIndex);
            }

            return (maxCol + 1, maxRow + 1);
        }

        /// <summary>
        /// 获取方块在指定位置的单元格
        /// </summary>
        public List<GridPosition> GetWorldPositions(GridPosition origin)
        {
            var positions = new List<GridPosition>(Cells.Count);
            foreach (var cell in Cells)
            {
                positions.Add(new GridPosition(
                    origin.RowIndex + cell.RowIndex,
                    origin.ColumnIndex + cell.ColumnIndex));
            }
            return positions;
        }

        /// <summary>
        /// 创建随机方块
        /// </summary>
        public static BlockData CreateRandom()
        {
            var shapes = (BlockShape[])System.Enum.GetValues(typeof(BlockShape));
            var randomShape = shapes[Random.Range(0, shapes.Length)];
            var randomColor = GetRandomColor();
            return new BlockData(randomShape, randomColor);
        }

        /// <summary>
        /// 获取随机颜色
        /// </summary>
        private static Color GetRandomColor()
        {
            Color[] colors = new Color[]
            {
                new Color(1f, 0.42f, 0.42f),      // 红色 #ff6b6b
                new Color(0.31f, 0.8f, 0.77f),    // 蓝色 #4ecdc4
                new Color(0.58f, 0.88f, 0.83f),   // 绿色 #95e1d3
                new Color(1f, 0.85f, 0.24f),      // 黄色 #ffd93d
                new Color(0.66f, 0.9f, 0.81f),    // 紫色 #a8e6cf
                new Color(1f, 0.55f, 0.58f),      // 橙色 #ff8b94
            };
            return colors[Random.Range(0, colors.Length)];
        }
    }
}
