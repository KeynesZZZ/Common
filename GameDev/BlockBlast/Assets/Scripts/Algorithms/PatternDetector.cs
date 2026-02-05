using BlockBlast.Core;

namespace BlockBlast.Algorithms
{
    public class PatternDetector
    {
        private const int Size = 8;

        public bool IsWaitingForLongBar(byte[] board)
        {
            for (int x = 0; x < Size; x++)
            {
                int continuousEmpty = 0;
                for (int y = 0; y < Size; y++)
                {
                    if (board[y * Size + x] == 0)
                        continuousEmpty++;
                    else
                        continuousEmpty = 0;

                    if (continuousEmpty >= 4)
                    {
                        if (IsNarrowSlot(board, x, y, "vertical"))
                            return true;
                    }
                }
            }

            for (int y = 0; y < Size; y++)
            {
                int continuousEmpty = 0;
                for (int x = 0; x < Size; x++)
                {
                    if (board[y * Size + x] == 0)
                        continuousEmpty++;
                    else
                        continuousEmpty = 0;

                    if (continuousEmpty >= 4)
                    {
                        if (IsNarrowSlot(board, x, y, "horizontal"))
                            return true;
                    }
                }
            }

            return false;
        }

        private bool IsNarrowSlot(byte[] board, int x, int y, string direction)
        {
            if (direction == "vertical")
            {
                bool leftBlocked = x == 0 || board[y * Size + (x - 1)] == 1;
                bool rightBlocked = x == Size - 1 || board[y * Size + (x + 1)] == 1;
                return leftBlocked || rightBlocked;
            }
            else
            {
                bool topBlocked = y == 0 || board[(y - 1) * Size + x] == 1;
                bool bottomBlocked = y == Size - 1 || board[(y + 1) * Size + x] == 1;
                return topBlocked || bottomBlocked;
            }
        }

        public float GetLongBarWeightModifier(BlockShape shape, bool isWaitingForLongBar)
        {
            if (!isWaitingForLongBar)
                return 1.0f;

            if (shape.id == 9 || shape.id == 10 || shape.id == 11 || shape.id == 12)
                return 0.05f;

            return 1.5f;
        }
    }
}
