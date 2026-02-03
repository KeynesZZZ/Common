using System; using System.Collections.Generic; using Bingo.Interfaces; using UnityEngine;

namespace Bingo.Core
{
    public class BingoGame : IBingoGame
    {
        public IBingoCard Card { get; private set; }
        public List<int> CalledNumbers { get; private set; }
        public int TotalNumbers { get; private set; }
        public bool IsGameOver { get; private set; }

        public event Action<int> OnNumberCalled;
        public event Action OnBingoAchieved;

        public BingoGame(int cardSize)
        {
            Card = new BingoCard(cardSize);
            CalledNumbers = new List<int>();
            TotalNumbers = cardSize * cardSize;
            IsGameOver = false;
        }

        public int CallNextNumber()
        {
            if (IsGameOver || CalledNumbers.Count >= TotalNumbers)
            {
                return -1;
            }

            int number;
            do
            {
                number = UnityEngine.Random.Range(1, TotalNumbers + 1);
            } while (CalledNumbers.Contains(number));

            CalledNumbers.Add(number);
            Card.MarkNumber(number);

            OnNumberCalled?.Invoke(number);

            if (Card.IsCompleted)
            {
                IsGameOver = true;
                OnBingoAchieved?.Invoke();
            }

            return number;
        }

        public bool CheckNumber(int number)
        {
            return CalledNumbers.Contains(number);
        }

        public void ResetGame()
        {
            Card.Reset();
            CalledNumbers.Clear();
            IsGameOver = false;
        }

        public List<int> GetRemainingNumbers()
        {
            List<int> remaining = new List<int>();
            for (int i = 1; i <= TotalNumbers; i++)
            {
                if (!CalledNumbers.Contains(i))
                {
                    remaining.Add(i);
                }
            }
            return remaining;
        }
    }
}
