using UnityEngine;
using UnityEngine.UI;
using BingoClient.Models;
using BingoClient.Events;

namespace BingoClient.Views
{
    public class GameView : MonoBehaviour
    {
        [Header("顶部区域")]
        [SerializeField] private TextMeshProUGUI _bingoCountText;
        [SerializeField] private Transform _callerContainer;
        [SerializeField] private Button _settingsButton;

        [Header("左侧区域")]
        [SerializeField] private Transform _playerListContainer;
        [SerializeField] private GameObject _playerItemPrefab;

        [Header("中间区域")]
        [SerializeField] private Transform _boardContainer;
        [SerializeField] private GameObject _boardPrefab;

        [Header("底部区域")]
        [SerializeField] private Transform _powerUpBarContainer;
        [SerializeField] private GameObject _powerUpButtonPrefab;

        [Header("倒计时")]
        [SerializeField] private GameObject _countdownPanel;
        [SerializeField] private TextMeshProUGUI _countdownText;

        private RoomData _roomData;
        private Action _onCountdownComplete;
        private ClientEventBus _eventBus;

        private void Awake()
        {
            _eventBus = BingoClient.Utilities.ServiceLocator.GetService<ClientEventBus>();
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _eventBus.Subscribe<ClientEvents.BoardsInitialized>(OnBoardsInitialized);
            _eventBus.Subscribe<ClientEvents.SlotMarked>(OnSlotMarked);
            _eventBus.Subscribe<ClientEvents.BingoAchieved>(OnBingoAchieved);
            _eventBus.Subscribe<ClientEvents.NumberCalled>(OnNumberCalled);
        }

        public void Initialize(RoomData roomData)
        {
            _roomData = roomData;
            UpdateBingoCount();
            SetupSettingsButton();
        }

        private void UpdateBingoCount()
        {
            _bingoCountText.text = $"Bingo: {_roomData.BingoCount}";
        }

        private void SetupSettingsButton()
        {
            _settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void OnSettingsClicked()
        {
            var settingsView = FindObjectOfType<SettingsView>();
            settingsView?.Show();
        }

        public void StartCountdown(int seconds, Action onComplete)
        {
            _onCountdownComplete = onComplete;
            _countdownPanel.SetActive(true);
            _countdownText.text = seconds.ToString();
            StartCoroutine(CountdownCoroutine(seconds));
        }

        private IEnumerator CountdownCoroutine(int seconds)
        {
            for (int i = seconds; i > 0; i--)
            {
                _countdownText.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }

            _countdownPanel.SetActive(false);
            _onCountdownComplete?.Invoke();
        }

        private void OnBoardsInitialized(ClientEvents.BoardsInitialized eventData)
        {
            CreateBoards(eventData.Boards);
        }

        private void CreateBoards(List<BoardData> boards)
        {
            foreach (var board in boards)
            {
                var boardObject = Instantiate(_boardPrefab, _boardContainer);
                var boardView = boardObject.GetComponent<BoardView>();
                boardView.Initialize(board);
            }
        }

        private void OnSlotMarked(ClientEvents.SlotMarked eventData)
        {
            var boardViews = GetComponentsInChildren<BoardView>();
            foreach (var boardView in boardViews)
            {
                boardView.UpdateSlot(eventData.SlotIndex, eventData.IsMarked);
            }
        }

        private void OnBingoAchieved(ClientEvents.BingoAchieved eventData)
        {
            var boardViews = GetComponentsInChildren<BoardView>();
            foreach (var boardView in boardViews)
            {
                boardView.HighlightWinLines(eventData.WinLines);
            }
        }

        private void OnNumberCalled(ClientEvents.NumberCalled eventData)
        {
            var callerView = FindObjectOfType<CallerView>();
            callerView?.AddNumber(eventData.Number);
        }
    }
}