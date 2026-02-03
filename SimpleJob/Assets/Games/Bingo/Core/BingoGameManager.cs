using UnityEngine; using Bingo.Interfaces;

namespace Bingo.Core
{
    public class BingoGameManager : MonoBehaviour
    {
        public GameConfig GameConfig;
        private IBingoGame currentGame;

        public IBingoGame CurrentGame => currentGame;
        public bool IsGameActive => currentGame != null && !currentGame.IsGameOver;

        public void StartNewGame()
        {
            currentGame = new BingoGame(GameConfig.CardSize);
            currentGame.OnNumberCalled += HandleNumberCalled;
            currentGame.OnBingoAchieved += HandleBingoAchieved;
        }

        public void CallNextNumber()
        {
            if (IsGameActive)
            {
                currentGame.CallNextNumber();
            }
        }

        public void EndGame()
        {
            if (currentGame != null)
            {
                currentGame.OnNumberCalled -= HandleNumberCalled;
                currentGame.OnBingoAchieved -= HandleBingoAchieved;
                currentGame = null;
            }
        }

        private void HandleNumberCalled(int number)
        {
            Debug.Log($"Called number: {number}");
        }

        private void HandleBingoAchieved()
        {
            Debug.Log("Bingo achieved!");
        }

        private void OnDestroy()
        {
            EndGame();
        }
    }
}
