using UnityEngine;
using BingoClient.Utilities;
using BingoClient.Events;
using BingoClient.Services;
using BingoClient.Controllers;

namespace BingoClient
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private string _serverUrl = "ws://localhost:8080";

        private async void Start()
        {
            InitializeServices();
            await ConnectToServer();
        }

        private void InitializeServices()
        {
            var serviceLocator = ServiceLocator.Instance;

            var eventBus = new ClientEventBus();
            serviceLocator.RegisterService(eventBus);

            var networkService = gameObject.AddComponent<NetworkService>();
            serviceLocator.RegisterService(networkService);

            var animationService = gameObject.AddComponent<AnimationService>();
            serviceLocator.RegisterService(animationService);

            var feedbackService = gameObject.AddComponent<FeedbackService>();
            serviceLocator.RegisterService(feedbackService);

            var gameController = gameObject.AddComponent<Controllers.GameController>();
            serviceLocator.RegisterService(gameController);
        }

        private async Task ConnectToServer()
        {
            var networkService = ServiceLocator.Instance.GetService<NetworkService>();
            await networkService.ConnectAsync(_serverUrl);
        }

        public async void JoinGame(string roomId, string playerId)
        {
            var gameController = ServiceLocator.Instance.GetService<Controllers.GameController>();
            await gameController.JoinRoomAsync(roomId, playerId);
        }
    }
}