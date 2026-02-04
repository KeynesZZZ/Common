using System.Threading.Tasks;
using UnityEngine;
using BingoClient.Events;
using BingoClient.Models;
using BingoClient.GameModes.Base;
using BingoClient.GameModes.ClassicBingo;
using BingoClient.GameModes.SpeedBingo;
using BingoClient.GameModes.PowerUpBingo;

namespace BingoClient.Controllers
{
    /// <summary>
    /// 游戏主控制器 - 负责管理整个游戏的生命周期和流程
    /// </summary>
    public class GameController : MonoBehaviour
    {
        private RoomController _roomController;
        private BoardController _boardController;
        private UIController _uiController;
        private NetworkService _networkService;
        private ClientEventBus _eventBus;
        private BaseGameMode _currentGameMode;

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
            _eventBus.Subscribe<ClientEvents.BingoAchieved>(OnBingoAchieved);
            _eventBus.Subscribe<ClientEvents.PowerUpActivated>(OnPowerUpActivated);
            _eventBus.Subscribe<ClientEvents.GameEnded>(OnGameEnded);
        }

        public async Task JoinRoomAsync(string roomId, string playerId)
        {
            await _roomController.JoinRoomAsync(roomId, playerId);
        }

        private void OnGameInitialized(ClientEvents.GameInitialized eventData)
        {
            InitializeGameMode(eventData.RoomData);
            _uiController.ShowRoomView(eventData.RoomData);
            _boardController.InitializeBoards(eventData.RoomData.Boards);
            _uiController.ShowPlayerList(eventData.RoomData.Players);
            _uiController.StartCountdown(3);
        }

        private void InitializeGameMode(RoomData roomData)
        {
            _currentGameMode = roomData.BingoCount switch
            {
                1 => new SpeedBingoGame(),
                2 => new PowerUpBingoGame(),
                _ => new ClassicBingoGame()
            };
            
            _currentGameMode.InitializeAsync(roomData.Id);
        }

        private void OnGameStarted(ClientEvents.GameStarted eventData)
        {
            _uiController.HideCountdown();
            _boardController.EnableInteraction();
        }

        private async void OnSlotClicked(ClientEvents.SlotClicked eventData)
        {
            if (_currentGameMode != null)
            {
                await _currentGameMode.OnBeforeMarkAsync(
                    GameData.Instance.RoomId, 
                    GameData.Instance.PlayerId, 
                    eventData.SlotIndex);
            }

            _boardController.MarkSlot(eventData.SlotIndex, eventData.IsMarked);
            _uiController.ShowFeedback(eventData.Feedback);

            if (_currentGameMode != null)
            {
                await _currentGameMode.OnAfterMarkAsync(
                    GameData.Instance.RoomId, 
                    GameData.Instance.PlayerId, 
                    eventData.SlotIndex);
            }
        }

        private async void OnBingoAchieved(ClientEvents.BingoAchieved eventData)
        {
            if (_currentGameMode != null)
            {
                await _currentGameMode.OnBingoAchievedAsync(
                    GameData.Instance.RoomId,
                    GameData.Instance.PlayerId,
                    eventData.WinLines);
            }
        }

        private void OnPowerUpActivated(ClientEvents.PowerUpActivated eventData)
        {
            Debug.Log($"PowerUp Activated: {eventData.PowerUpResult.Type} - {eventData.PowerUpResult.Description}");
        }

        private void OnGameEnded(ClientEvents.GameEnded eventData)
        {
            if (_currentGameMode != null)
            {
                _currentGameMode.OnGameCompleteAsync(GameData.Instance.RoomId);
            }
            _uiController.ShowGameEndView(eventData.Results);
        }

        public void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}