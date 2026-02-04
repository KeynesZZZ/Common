using System.Threading.Tasks;
using UnityEngine;
using BingoClient.Events;
using BingoClient.Models;

namespace BingoClient.Controllers
{
    public class GameController : MonoBehaviour
    {
        private RoomController _roomController;
        private BoardController _boardController;
        private UIController _uiController;
        private NetworkService _networkService;
        private ClientEventBus _eventBus;

        private void Awake()
        {
            var serviceLocator = BingoClient.Utilities.ServiceLocator.Instance;
            _networkService = serviceLocator.GetService<NetworkService>();
            _eventBus = serviceLocator.GetService<ClientEventBus>();

            _roomController = new RoomController(_networkService, _eventBus);
            _boardController = new BoardController(_networkService, _eventBus);
            _uiController = new UIController(_eventBus);

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _eventBus.Subscribe<ClientEvents.GameInitialized>(OnGameInitialized);
            _eventBus.Subscribe<ClientEvents.GameStarted>(OnGameStarted);
            _eventBus.Subscribe<ClientEvents.SlotClicked>(OnSlotClicked);
            _eventBus.Subscribe<ClientEvents.GameEnded>(OnGameEnded);
        }

        public async Task JoinRoomAsync(string roomId, string playerId)
        {
            await _roomController.JoinRoomAsync(roomId, playerId);
        }

        private void OnGameInitialized(ClientEvents.GameInitialized eventData)
        {
            _uiController.ShowRoomView(eventData.RoomData);
            _boardController.InitializeBoards(eventData.RoomData.Boards);
            _uiController.ShowPlayerList(eventData.RoomData.Players);
            _uiController.StartCountdown(3);
        }

        private void OnGameStarted(ClientEvents.GameStarted eventData)
        {
            _uiController.HideCountdown();
            _boardController.EnableInteraction();
        }

        private void OnSlotClicked(ClientEvents.SlotClicked eventData)
        {
            _boardController.MarkSlot(eventData.SlotIndex, eventData.IsMarked);
            _uiController.ShowFeedback(eventData.Feedback);
        }

        private void OnGameEnded(ClientEvents.GameEnded eventData)
        {
            _uiController.ShowGameEndView(eventData.Results);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}