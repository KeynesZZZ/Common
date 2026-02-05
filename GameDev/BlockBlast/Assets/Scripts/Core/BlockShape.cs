using System;
using UnityEngine;

namespace BlockBlast.Core
{
    [Serializable]
    public struct BlockShape
    {
        public int id;
        public int width;
        public int height;
        public byte[] cells;
        public Color color;
        public int cellCount;

        public static BlockShape CreateSingle()
        {
            return new BlockShape
            {
                id = 1,
                width = 1,
                height = 1,
                cells = new byte[] { 1 },
                color = new Color(1f, 0.3f, 0.3f),
                cellCount = 1
            };
        }

        public static BlockShape CreateDoubleHorizontal()
        {
            return new BlockShape
            {
                id = 2,
                width = 2,
                height = 1,
                cells = new byte[] { 1, 1 },
                color = new Color(0.3f, 0.5f, 1f),
                cellCount = 2
            };
        }

        public static BlockShape CreateDoubleVertical()
        {
            return new BlockShape
            {
                id = 3,
                width = 1,
                height = 2,
                cells = new byte[] { 1, 1 },
                color = new Color(0.3f, 0.5f, 1f),
                cellCount = 2
            };
        }

        public static BlockShape CreateTripleHorizontal()
        {
            return new BlockShape
            {
                id = 4,
                width = 3,
                height = 1,
                cells = new byte[] { 1, 1, 1 },
                color = new Color(0.3f, 0.8f, 0.3f),
                cellCount = 3
            };
        }

        public static BlockShape CreateTripleVertical()
        {
            return new BlockShape
            {
                id = 5,
                width = 1,
                height = 3,
                cells = new byte[] { 1, 1, 1 },
                color = new Color(0.3f, 0.8f, 0.3f),
                cellCount = 3
            };
        }

        public static BlockShape CreateSquare()
        {
            return new BlockShape
            {
                id = 6,
                width = 2,
                height = 2,
                cells = new byte[] { 1, 1, 1, 1 },
                color = new Color(1f, 0.8f, 0.2f),
                cellCount = 4
            };
        }

        public static BlockShape CreateLShape()
        {
            return new BlockShape
            {
                id = 7,
                width = 2,
                height = 2,
                cells = new byte[] { 1, 0, 1, 1 },
                color = new Color(0.8f, 0.3f, 0.8f),
                cellCount = 3
            };
        }

        public static BlockShape CreateTShape()
        {
            return new BlockShape
            {
                id = 8,
                width = 3,
                height = 2,
                cells = new byte[] { 1, 1, 1, 0, 1, 0 },
                color = new Color(1f, 0.5f, 0.2f),
                cellCount = 4
            };
        }

        public static BlockShape CreateLongHorizontal()
        {
            return new BlockShape
            {
                id = 9,
                width = 4,
                height = 1,
                cells = new byte[] { 1, 1, 1, 1 },
                color = new Color(0.2f, 0.6f, 0.8f),
                cellCount = 4
            };
        }

        public static BlockShape CreateLongVertical()
        {
            return new BlockShape
            {
                id = 10,
                width = 1,
                height = 4,
                cells = new byte[] { 1, 1, 1, 1 },
                color = new Color(0.2f, 0.6f, 0.8f),
                cellCount = 4
            };
        }

        public static BlockShape CreateFiveHorizontal()
        {
            return new BlockShape
            {
                id = 11,
                width = 5,
                height = 1,
                cells = new byte[] { 1, 1, 1, 1, 1 },
                color = new Color(0.9f, 0.2f, 0.3f),
                cellCount = 5
            };
        }

        public static BlockShape CreateFiveVertical()
        {
            return new BlockShape
            {
                id = 12,
                width = 1,
                height = 5,
                cells = new byte[] { 1, 1, 1, 1, 1 },
                color = new Color(0.9f, 0.2f, 0.3f),
                cellCount = 5
            };
        }

        public bool IsCellOccupied(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return false;
            return cells[y * width + x] == 1;
        }
    }
}
