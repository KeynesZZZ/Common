using BlockBlast.Core;

namespace BlockBlast.Algorithms
{
    /// <summary>
    /// 模式检测器 - 检测玩家行为模式（如等长条）
    /// </summary>
    public class PatternDetector
    {
        private const int Size = 8;

        /// <summary>
        /// 检测棋盘是否存在特定的"长条空位特征"
        /// </summary>
        public bool IsWaitingForLongBar(byte[] board)
        {
            // 扫描垂直方向
            for (int x = 0; x < Size; x++)
            {
                int continuousEmpty = 0;
                for (int y = 0; y < Size; y++)
                {
                    if (board[y * Size + x] == 0)
                        continuousEmpty++;
                    else
                        continuousEmpty = 0;

                    // 如果发现连续 4-5 个空位，且两侧都被堵死（形成一个窄缝）
                    if (continuousEmpty >= 4)
                    {
                        if (IsNarrowSlot(board, x, y, "vertical"))
                            return true;
                    }
                }
            }

            // 同理扫描水平方向
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

        /// <summary>
        /// 检测缝隙是否被堵死
        /// </summary>
        private bool IsNarrowSlot(byte[] board, int x, int y, string direction)
        {
            if (direction == "vertical")
            {
                // 检测缝隙两侧是否都有方块，如果是，这就被定义为一个"槽位"
                // 系统会判定：玩家正在诱导长条出现
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

        /// <summary>
        /// 获取长条权重修正系数
        /// </summary>
        public float GetLongBarWeightModifier(BlockShape shape, bool isWaitingForLongBar)
        {
            if (!isWaitingForLongBar)
                return 1.0f;

            // 长条形状的 ID: 9=1x4, 10=4x1, 11=1x5, 12=5x1
            if (shape.id == 9 || shape.id == 10 || shape.id == 11 || shape.id == 12)
                return 0.05f; // 降权到近乎为零

            return 1.5f; // 其他形状权重提升，破坏槽位
        }
    }
}
