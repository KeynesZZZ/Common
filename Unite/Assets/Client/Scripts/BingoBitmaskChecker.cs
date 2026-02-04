using System;
using System.Collections.Generic;
using System.Text;

namespace BingoGame.Core
{
    /// <summary>
    /// 高性能 Bingo 判定器
    /// 适用于服务端每秒处理数万次判定的场景
    /// </summary>
    public class BingoBitmaskChecker
    {
        // 存储所有的获胜掩码 (共12种基本赢法 + 4角)
        // Static 只需初始化一次，节省内存
        private static readonly List<int> WinMasks = new List<int>();

        // 静态构造函数：预计算所有赢法
        static BingoBitmaskChecker()
        {
            // 1. 横向 5 行 (Rows)
            // Row 0: 00000...11111 (二进制) = 0x1F
            for (int row = 0; row < 5; row++)
            {
                WinMasks.Add(0x1F << (row * 5)); 
            }

            // 2. 纵向 5 列 (Cols)
            // Col 0: indices 0, 5, 10, 15, 20
            for (int col = 0; col < 5; col++)
            {
                int colMask = 0;
                for (int row = 0; row < 5; row++)
                {
                    colMask |= (1 << (row * 5 + col));
                }
                WinMasks.Add(colMask);
            }

            // 3. 对角线 (Diagonals)
            int diag1 = 0; // 左上 -> 右下 (0, 6, 12, 18, 24)
            int diag2 = 0; // 右上 -> 左下 (4, 8, 12, 16, 20)
            for (int i = 0; i < 5; i++)
            {
                diag1 |= (1 << (i * 5 + i));
                diag2 |= (1 << (i * 5 + (4 - i)));
            }
            WinMasks.Add(diag1);
            WinMasks.Add(diag2);

            // 4. 特殊玩法：四角 (Four Corners)
            // Indices: 0, 4, 20, 24
            int corners = (1 << 0) | (1 << 4) | (1 << 20) | (1 << 24);
            WinMasks.Add(corners);
            
            // Console.WriteLine($"[System] 已预加载 {WinMasks.Count} 种 Bingo 赢法掩码。");
        }

        // ==========================================
        // 实例逻辑
        // ==========================================

        // 当前玩家的棋盘状态 (32位整数足以存下25个格子的状态)
        private int _playerBoardState = 0;

        /// <summary>
        /// 初始化棋盘
        /// </summary>
        /// <param name="hasFreeSpace">是否包含中间的 Free 格子</param>
        public BingoBitmaskChecker(bool hasFreeSpace = true)
        {
            _playerBoardState = 0;
            if (hasFreeSpace)
            {
                MarkSlot(12); // Index 12 是中间格子 (2,2)
            }
        }

        /// <summary>
        /// 标记某个格子
        /// </summary>
        /// <param name="index">格子索引 0-24</param>
        public void MarkSlot(int index)
        {
            if (index < 0 || index >= 25) return;
            
            // 位运算：将第 index 位设为 1
            _playerBoardState |= (1 << index);
        }

        /// <summary>
        /// 检查是否达成 Bingo
        /// </summary>
        /// <returns>返回达成 Bingo 的线条数量</returns>
        public int CheckBingos()
        {
            int bingoCount = 0;

            foreach (var mask in WinMasks)
            {
                // 核心判定公式：
                // 如果 (当前状态 & 目标掩码) 结果等于 目标掩码，说明掩码中的每一位在当前状态中都为1
                if ((_playerBoardState & mask) == mask)
                {
                    bingoCount++;
                }
            }
            return bingoCount;
        }
        
        /// <summary>
        /// 检查是否包含特定图案（扩展性接口）
        /// </summary>
        public bool CheckSpecificPattern(int patternMask)
        {
            return (_playerBoardState & patternMask) == patternMask;
        }

        // ==========================================
        // 调试辅助工具
        // ==========================================
        public string GetDebugString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Current Board State (Bitmask):");
            for (int row = 0; row < 5; row++)
            {
                sb.Append("| ");
                for (int col = 0; col < 5; col++)
                {
                    int index = row * 5 + col;
                    bool isMarked = (_playerBoardState & (1 << index)) != 0;
                    sb.Append(isMarked ? "X " : "O ");
                }
                sb.AppendLine("|");
            }
            return sb.ToString();
        }
    }
}