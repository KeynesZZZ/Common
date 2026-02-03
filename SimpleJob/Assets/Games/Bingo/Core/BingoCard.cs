using System; using System.Collections.Generic; using Bingo.Interfaces;

namespace Bingo.Core
{
    public class BingoCard : IBingoCard
    {
        public int Size { get; private set; }
        public BingoCell[,] Cells { get; private set; }
        public bool IsCompleted { get; private set; }

        public BingoCard(int size)
        {
            Size = size;
            Cells = new BingoCell[size, size];
            GenerateCard();
        }

        private void GenerateCard()
        {
            int numberRange = Size * Size;
            List<int> numbers = new List<int>();

            for (int i = 1; i <= numberRange; i++)
            {
                numbers.Add(i);
            }

            Shuffle(numbers);

            int index = 0;
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    int number = numbers[index++];
                    Cells[row, col] = new BingoCell(number, row, col);
                }
            }

            // 设置中心为自由格（如果是奇数尺寸）
            if (Size % 2 != 0)
            {
                int center = Size / 2;
                Cells[center, center].MarkAsCalled();
            }
        }

        private void Shuffle<T>(List<T> list)
        {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public bool MarkNumber(int number)
        {
            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (Cells[row, col].Number == number)
                    {
                        Cells[row, col].MarkAsCalled();
                        CheckForBingo();
                        return true;
                    }
                }
            }
            return false;
        }

        private void CheckForBingo()
        {
            // 检查行
            for (int row = 0; row < Size; row++)
            {
                bool rowComplete = true;
                for (int col = 0; col < Size; col++)
                {
                    if (!Cells[row, col].IsCalled)
                    {
                        rowComplete = false;
                        break;
                    }
                }
                if (rowComplete)
                {
                    IsCompleted = true;
                    return;
                }
            }

            // 检查列
            for (int col = 0; col < Size; col++)
            {
                bool colComplete = true;
                for (int row = 0; row < Size; row++)
                {
                    if (!Cells[row, col].IsCalled)
                    {
                        colComplete = false;
                        break;
                    }
                }
                if (colComplete)
                {
                    IsCompleted = true;
                    return;
                }
            }

            // 检查对角线
            bool diagonal1Complete = true;
            for (int i = 0; i < Size; i++)
            {
                if (!Cells[i, i].IsCalled)
                {
                    diagonal1Complete = false;
                    break;
                }
            }
            if (diagonal1Complete)
            {
                IsCompleted = true;
                return;
            }

            bool diagonal2Complete = true;
            for (int i = 0; i < Size; i++)
            {
                if (!Cells[i, Size - 1 - i].IsCalled)
                {
                    diagonal2Complete = false;
                    break;
                }
            }
            if (diagonal2Complete)
            {
                IsCompleted = true;
                return;
            }
        }

        public void Reset()
        {
            GenerateCard();
            IsCompleted = false;
        }
    }

    public class BingoCell
    {
        public int Number { get; private set; }
        public int Row { get; private set; }
        public int Column { get; private set; }
        public bool IsCalled { get; private set; }

        public BingoCell(int number, int row, int column)
        {
            Number = number;
            Row = row;
            Column = column;
            IsCalled = false;
        }

        public void MarkAsCalled()
        {
            IsCalled = true;
        }

        public void Reset()
        {
            IsCalled = false;
        }
    }
}
