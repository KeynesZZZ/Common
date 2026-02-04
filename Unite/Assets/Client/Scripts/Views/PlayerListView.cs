using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BingoClient.Models;

namespace BingoClient.Views
{
    public class PlayerListView : MonoBehaviour
    {
        [SerializeField] private Transform _playerListContainer;
        [SerializeField] private GameObject _playerItemPrefab;

        private List<PlayerItemView> _playerItems = new();

        public void UpdatePlayers(List<PlayerData> players)
        {
            ClearPlayers();
            CreatePlayers(players);
        }

        private void ClearPlayers()
        {
            foreach (var item in _playerItems)
            {
                Destroy(item.gameObject);
            }
            _playerItems.Clear();
        }

        private void CreatePlayers(List<PlayerData> players)
        {
            foreach (var player in players)
            {
                var playerItem = Instantiate(_playerItemPrefab, _playerListContainer)
                    .GetComponent<PlayerItemView>();
                playerItem.Initialize(player);
                _playerItems.Add(playerItem);
            }
        }

        public void UpdatePlayerScore(string playerId, int score)
        {
            var playerItem = _playerItems.FirstOrDefault(p => p.PlayerId == playerId);
            playerItem?.UpdateScore(score);
        }
    }

    public class PlayerItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _bingoCountText;

        private string _playerId;

        public string PlayerId => _playerId;

        public void Initialize(PlayerData playerData)
        {
            _playerId = playerData.Id;
            _playerNameText.text = playerData.Name;
            _scoreText.text = playerData.Score.ToString();
            _bingoCountText.text = playerData.BingoCount.ToString();
        }

        public void UpdateScore(int score)
        {
            _scoreText.text = score.ToString();
            _scoreText.transform.DOPunch(Vector3.one * 0.2f, 0.3f);
        }
    }
}