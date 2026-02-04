using System.Collections.Generic;
using BingoClient.Events;
using BingoClient.Models;

namespace BingoClient.Controllers
{
    public class UIController
    {
        private readonly ClientEventBus _eventBus;
        private Views.GameView _gameView;
        private Views.BoardView _boardView;
        private Views.PlayerListView _playerListView;
        private Views.CallerView _callerView;
        private Views.PowerUpBarView _powerUpBarView;
        private Views.GameEndView _gameEndView;

        public UIController(ClientEventBus eventBus)
        {
            _eventBus = eventBus;
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _eventBus.Subscribe<ClientEvents.BoardsInitialized>(OnBoardsInitialized);
            _eventBus.Subscribe<ClientEvents.SlotMarked>(OnSlotMarked);
            _eventBus.Subscribe<ClientEvents.BingoAchieved>(OnBingoAchieved);
            _eventBus.Subscribe<ClientEvents.NumberCalled>(OnNumberCalled);
        }

        public void ShowRoomView(RoomData roomData)
        {
            _gameView = UnityEngine.Object.FindObjectOfType<Views.GameView>();
            _gameView?.Initialize(roomData);
        }

        public void ShowPlayerList(List<PlayerData> players)
        {
            _playerListView = UnityEngine.Object.FindObjectOfType<Views.PlayerListView>();
            _playerListView?.UpdatePlayers(players);
        }

        public void StartCountdown(int seconds)
        {
            _gameView?.StartCountdown(seconds, OnCountdownComplete);
        }

        private void OnCountdownComplete()
        {
            _eventBus.Publish(new ClientEvents.CountdownComplete());
        }

        public void HideCountdown()
        {
        }

        public void ShowFeedback(FeedbackData feedback)
        {
            var feedbackService = BingoClient.Utilities.ServiceLocator.GetService<Services.FeedbackService>();
            feedbackService?.ShowFeedback(feedback);
        }

        public void ShowGameEndView(List<PlayerResult> results)
        {
            _gameEndView = UnityEngine.Object.FindObjectOfType<Views.GameEndView>();
            _gameEndView?.ShowResults(results);
        }

        private void OnBoardsInitialized(ClientEvents.BoardsInitialized eventData)
        {
            _boardView = UnityEngine.Object.FindObjectOfType<Views.BoardView>();
            _boardView?.Initialize(eventData.Boards.FirstOrDefault());
        }

        private void OnSlotMarked(ClientEvents.SlotMarked eventData)
        {
            _boardView?.UpdateSlot(eventData.SlotIndex, eventData.IsMarked);
        }

        private void OnBingoAchieved(ClientEvents.BingoAchieved eventData)
        {
            _boardView?.HighlightWinLines(eventData.WinLines);
        }

        private void OnNumberCalled(ClientEvents.NumberCalled eventData)
        {
            _callerView = UnityEngine.Object.FindObjectOfType<Views.CallerView>();
            _callerView?.AddNumber(eventData.Number);
        }
    }
}