using System;
using System.Collections.Generic;
using SimpleBoard.Core;

namespace SimpleBoard.Games.Bingo
{
    /// <summary>
    /// Bingo 游戏板类 - 管理 5x5 Bingo 游戏板的核心逻辑
    /// </summary>
    public class BingoBoard : GameBoard<BingoSlotState>
    {
        /// <summary>游戏板大小</summary>
        public const int BOARD_SIZE = 5;
        
        /// <summary>中心行索引</summary>
        public const int CENTER_ROW = 2;
        
        /// <summary>中心列索引</summary>
        public const int CENTER_COL = 2;
        
        // 随机数生成器
        private readonly Random _random = new Random();
        
        // 已呼叫的数字
        private readonly List<int> _calledNumbers = new List<int>();

        // 事件系统
        public event Action<BingoSlotState> OnSlotMarked;
        public event Action<List<BingoSlotState>> OnBingo;
        public event Action<int> OnNumberCalled;

        /// <summary>已呼叫的数字列表</summary>
        public IReadOnlyList<int> CalledNumbers => _calledNumbers.AsReadOnly();
        
        /// <summary>是否获得 Bingo</summary>
        public bool HasBingo { get; private set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public BingoBoard()
        {
            InitializeBoard();
        }

        /// <summary>
        /// 初始化游戏板
        /// </summary>
        private void InitializeBoard()
        {
            // 创建 5x5 游戏板
            var gridSlots = new BingoSlotState[BOARD_SIZE, BOARD_SIZE];
            
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    BingoSlotState slot;
                    
                    // 中心格子为 FREE SPACE
                    if (row == CENTER_ROW && col == CENTER_COL)
                    {
                        slot = new BingoSlotState(0, (BingoColumn)col, true);
                    }
                    else
                    {
                        // 生成符合 Bingo 规则的数字
                        int number = GenerateBingoNumber((BingoColumn)col);
                        slot = new BingoSlotState(number, (BingoColumn)col);
                    }
                    
                    slot.GridPosition = new GridPosition(row, col);
                    gridSlots[row, col] = slot;
                }
            }
            
            SetGridSlots(gridSlots);
        }

        /// <summary>
        /// 根据列生成 Bingo 数字
        /// </summary>
        /// <param name="column">列</param>
        /// <returns>生成的数字</returns>
        private int GenerateBingoNumber(BingoColumn column)
        {
            // 定义各列数字范围
            (int min, int max) range = column switch
            {
                BingoColumn.B => (1, 15),
                BingoColumn.I => (16, 30),
                BingoColumn.N => (31, 45),
                BingoColumn.G => (46, 60),
                BingoColumn.O => (61, 75),
                _ => (1, 75)
            };

            // 生成不重复的数字
            int number;
            do
            {
                number = _random.Next(range.min, range.max + 1);
            } while (IsNumberInColumn(number, column));

            return number;
        }

        /// <summary>
        /// 检查数字是否已在列中存在
        /// </summary>
        /// <param name="number">要检查的数字</param>
        /// <param name="column">列</param>
        /// <returns>数字是否存在</returns>
        private bool IsNumberInColumn(int number, BingoColumn column)
        {
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                var slot = this[row, (int)column];
                if (!slot.IsFreeSpace && slot.Number == number)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 呼叫数字
        /// </summary>
        /// <param name="number">要呼叫的数字</param>
        /// <returns>是否有格子被标记</returns>
        public bool CallNumber(int number)
        {
            // 检查数字是否已经呼叫过
            if (_calledNumbers.Contains(number))
            {
                return false;
            }

            // 添加到已呼叫列表
            _calledNumbers.Add(number);
            OnNumberCalled?.Invoke(number);

            // 检查并标记匹配的格子
            bool anySlotMarked = false;
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    var slot = this[row, col];
                    if (slot.IsMatch(number) && slot.CanMark())
                    {
                        slot.Mark();
                        OnSlotMarked?.Invoke(slot);
                        anySlotMarked = true;
                    }
                }
            }

            // 检查 Bingo
            if (anySlotMarked)
            {
                CheckForBingo();
            }

            return anySlotMarked;
        }

        /// <summary>
        /// 检查是否获得 Bingo
        /// </summary>
        private void CheckForBingo()
        {
            var winningLine = FindWinningLine();

            // 如果有 Bingo，触发事件
            if (winningLine != null)
            {
                HasBingo = true;
                OnBingo?.Invoke(winningLine);
            }
        }

        /// <summary>
        /// 查找获胜线
        /// </summary>
        /// <returns>获胜线，没有则返回 null</returns>
        private List<BingoSlotState> FindWinningLine()
        {
            // 检查横向线
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                var line = new List<BingoSlotState>();
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    line.Add(this[row, col]);
                }
                if (IsWinningLine(line))
                {
                    return line;
                }
            }

            // 检查纵向线
            for (int col = 0; col < BOARD_SIZE; col++)
            {
                var line = new List<BingoSlotState>();
                for (int row = 0; row < BOARD_SIZE; row++)
                {
                    line.Add(this[row, col]);
                }
                if (IsWinningLine(line))
                {
                    return line;
                }
            }

            // 检查对角线（左上到右下）
            var diagonal1 = new List<BingoSlotState>();
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                diagonal1.Add(this[i, i]);
            }
            if (IsWinningLine(diagonal1))
            {
                return diagonal1;
            }

            // 检查对角线（右上到左下）
            var diagonal2 = new List<BingoSlotState>();
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                diagonal2.Add(this[i, BOARD_SIZE - 1 - i]);
            }
            if (IsWinningLine(diagonal2))
            {
                return diagonal2;
            }

            return null;
        }

        /// <summary>
        /// 检查是否为获胜线
        /// </summary>
        /// <param name="line">格子线</param>
        /// <returns>是否获胜</returns>
        private bool IsWinningLine(List<BingoSlotState> line)
        {
            // FREE SPACE 可以替代任何格子
            return line.All(slot => slot.IsMarked || slot.IsFreeSpace);
        }

        /// <summary>
        /// 重置游戏板
        /// </summary>
        public void Reset()
        {
            _calledNumbers.Clear();
            HasBingo = false;
            
            // 重置所有格子状态
            for (int row = 0; row < BOARD_SIZE; row++)
            {
                for (int col = 0; col < BOARD_SIZE; col++)
                {
                    this[row, col].Reset();
                }
            }
        }

        /// <summary>
        /// 生成新的卡片
        /// </summary>
        public void GenerateNewCard()
        {
            Reset();
            InitializeBoard();
        }
    }
}