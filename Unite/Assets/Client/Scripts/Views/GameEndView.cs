using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BingoShared.Models;

namespace BingoClient.Views
{
    public class GameEndView : MonoBehaviour
    {
        [SerializeField] private GameObject _gameEndPanel;
        [SerializeField] private Transform _resultsContainer;
        [SerializeField] private GameObject _resultItemPrefab;
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private Button _exitButton;

        private void Awake()
        {
            _playAgainButton.onClick.AddListener(OnPlayAgainClicked);
            _exitButton.onClick.AddListener(OnExitClicked);
            _gameEndPanel.SetActive(false);
        }

        public void ShowResults(List<PlayerResult> results)
        {
            _gameEndPanel.SetActive(true);
            _gameEndPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

            ClearResults();
            CreateResults(results);
        }

        private void ClearResults()
        {
            foreach (Transform child in _resultsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        private void CreateResults(List<PlayerResult> results)
        {
            foreach (var result in results)
            {
                var resultItem = Instantiate(_resultItemPrefab, _resultsContainer);
                var texts = resultItem.GetComponentsInChildren<TextMeshProUGUI>();
                texts[0].text = $"#{result.Rank}";
                texts[1].text = result.PlayerName;
                texts[2].text = result.Score.ToString();
                texts[3].text = result.BingoCount.ToString();
            }
        }

        private void OnPlayAgainClicked()
        {
            var gameController = FindObjectOfType<Controllers.GameController>();
            gameController?.RestartGame();
        }

        private void OnExitClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }
    }
}