using System; using System.Collections.Generic;

namespace Bingo.Interfaces
{
    public interface IBingoGame
    {
        IBingoCard Card { get; }
        List<int> CalledNumbers { get; }
        int TotalNumbers { get; }
        bool IsGameOver { get; }

        event Action<int> OnNumberCalled;
        event Action OnBingoAchieved;

        int CallNextNumber();
        bool CheckNumber(int number);
        void ResetGame();
        List<int> GetRemainingNumbers();
    }
}
