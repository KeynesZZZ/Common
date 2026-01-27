using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using SimpleBoard.Games.Bingo;

namespace SimpleBoard.Games.Bingo.Unity
{
    /// <summary>
    /// Bingo 游戏管理器 - 管理 Bingo 游戏的 Unity 集成和用户界面
    /// </summary>
    public class BingoGameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private int _cardCount = 1;
        [SerializeField] private bool _autoPlay = false;
        [SerializeField] private float _autoPlayDelay = 2f;

        [Header("References")]
        [SerializeField] private Transform _cardParent;
        [SerializeField] private GameObject _cardPrefab;
        [SerializeField] private GameObject _tilePrefab;
        [SerializeField] private AudioSource _audioSource;

        [Header("UI Elements")]
        [SerializeField] private Button _callButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _autoPlayButton;
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _bingoCountText;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip _callSound;
        [SerializeField] private AudioClip _bingoSound;
        [SerializeField] private AudioClip _markSound;

        // 游戏状态
        private List<BingoBoard> _bingoBoards = new List<BingoBoard>();
        private List<int> _calledNumbers = new List<int>();
        private int _totalBingoCount = 0;
        private bool _isAutoPlaying = false;
        private CancellationTokenSource _cancellationTokenSource;

        // 事件系统
        public event Action<int> OnNumberCalled;
        public event Action<BingoBoard, List<BingoSlotState>> OnBingo;
        public event Action<BingoBoard, GridPosition> OnCardMarked;

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            StartGame(_cardCount);
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        /// <summary>
        /// 初始化游戏管理器
        /// </summary>
        private void Initialize()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            // 设置按钮事件
            _callButton?.onClick.AddListener(CallNumber);
            _resetButton?.onClick.AddListener(ResetGame);
            _autoPlayButton?.onClick.AddListener(ToggleAutoPlay);

            UpdateUI();
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        /// <param name="cardCount">要使用的卡片数量</param>
        public void StartGame(int cardCount)
        {
            _cardCount = Mathf.Clamp(cardCount, 1, 6);
            InitializeGameWithCards();
            UpdateUI();
        }

        /// <summary>
        /// 使用指定数量的卡片初始化游戏
        /// </summary>
        private void InitializeGameWithCards()
        {
            CleanupBoards();
            _bingoBoards.Clear();
            _calledNumbers.Clear();
            _totalBingoCount = 0;

            for (int i = 0; i < _cardCount; i++)
            {
                CreateCard(i);
            }

            UpdateCardLayout();
        }

        /// <summary>
        /// 创建一张 Bingo 卡片
        /// </summary>
        /// <param name="cardId">卡片 ID</param>
        private void CreateCard(int cardId)
        {
            // 创建游戏板
            var board = new BingoBoard();
            _bingoBoards.Add(board);

            // 创建卡片 UI
            var cardObj = Instantiate(_cardPrefab, _cardParent);
            cardObj.name = $"BingoCard_{cardId}";

            // 创建格子
            for (int row = 0; row < BingoBoard.BOARD_SIZE; row++)
            {
                for (int col = 0; col < BingoBoard.BOARD_SIZE; col++)
                {
                    var tileObj = Instantiate(_tilePrefab, cardObj.transform);
                    tileObj.name = $"Tile_{row}_{col}";

                    var tile = tileObj.GetComponent<BingoTile>();
                    if (tile != null)
                    {
                        var slot = board.GetSlot(row, col);
                        tile.Initialize(slot, row, col, cardId);
                        tile.OnTileClicked += (id, r, c) => OnTileClick(id, r, c);
                    }
                }
            }

            // 设置游戏板事件
            board.OnSlotMarked += (slot) => OnCardSlotMarked(board, slot);
            board.OnBingo += (line) => OnBingoDetected(board, line, cardId);
        }

        /// <summary>
        /// 处理格子点击
        /// </summary>
        /// <param name="cardId">卡片 ID</param>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        private void OnTileClick(int cardId, int row, int col)
        {
            if (cardId < 0 || cardId >= _bingoBoards.Count)
                return;

            var board = _bingoBoards[cardId];
            var slot = board.GetSlot(row, col);

            if (slot.CanMark() && _calledNumbers.Contains(slot.Number))
            {
                board.CallNumber(slot.Number);
                PlaySound(_markSound);
            }
        }

        /// <summary>
        /// 处理格子被标记的事件
        /// </summary>
        /// <param name="board">游戏板</param>
        /// <param name="slot">被标记的格子</param>
        private void OnCardSlotMarked(BingoBoard board, BingoSlotState slot)
        {
            OnCardMarked?.Invoke(board, slot.GridPosition);
            PlaySound(_markSound);
        }

        /// <summary>
        /// 处理 Bingo 检测事件
        /// </summary>
        /// <param name="board">游戏板</param>
        /// <param name="line">获胜线</param>
        /// <param name="cardId">卡片 ID</param>
        private void OnBingoDetected(BingoBoard board, List<BingoSlotState> line, int cardId)
        {
            PlaySound(_bingoSound);
            _totalBingoCount++;
            OnBingo?.Invoke(board, line);
            UpdateUI();
        }

        /// <summary>
        /// 呼叫数字
        /// </summary>
        public void CallNumber()
        {
            if (_bingoBoards.Count == 0)
                return;

            int number = GenerateRandomNumber();
            if (number == -1)
                return;

            _calledNumbers.Add(number);
            OnNumberCalled?.Invoke(number);

            PlaySound(_callSound);

            // 在所有卡片上标记数字
            foreach (var board in _bingoBoards)
            {
                board.CallNumber(number);
            }

            UpdateUI();
        }

        /// <summary>
        /// 生成随机数字
        /// </summary>
        /// <returns>生成的数字</returns>
        private int GenerateRandomNumber()
        {
            var availableNumbers = new List<int>();
            for (int i = 1; i <= 75; i++)
            {
                if (!_calledNumbers.Contains(i))
                {
                    availableNumbers.Add(i);
                }
            }

            if (availableNumbers.Count == 0)
                return -1;

            int randomIndex = UnityEngine.Random.Range(0, availableNumbers.Count);
            return availableNumbers[randomIndex];
        }

        /// <summary>
        /// 重置游戏
        /// </summary>
        public void ResetGame()
        {
            _isAutoPlaying = false;
            _calledNumbers.Clear();
            _totalBingoCount = 0;

            foreach (var board in _bingoBoards)
            {
                board.Reset();
            }

            UpdateUI();
        }

        /// <summary>
        /// 切换自动播放模式
        /// </summary>
        public void ToggleAutoPlay()
        {
            _isAutoPlaying = !_isAutoPlaying;
            UpdateAutoPlayButton();

            if (_isAutoPlaying)
            {
                StartAutoPlay();
            }
        }

        /// <summary>
        /// 开始自动播放
        /// </summary>
        private async void StartAutoPlay()
        {
            while (_isAutoPlaying && _calledNumbers.Count < 75)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_autoPlayDelay), cancellationToken: _cancellationTokenSource.Token);
                if (_isAutoPlaying)
                {
                    CallNumber();
                }
            }
        }

        /// <summary>
        /// 更新自动播放按钮状态
        /// </summary>
        private void UpdateAutoPlayButton()
        {
            if (_autoPlayButton != null)
            {
                var text = _autoPlayButton.GetComponentInChildren<TMP_Text>();
                if (text != null)
                {
                    text.text = _isAutoPlaying ? "Stop" : "Auto Play";
                }
            }
        }

        /// <summary>
        /// 更新卡片布局
        /// </summary>
        private void UpdateCardLayout()
        {
            if (_cardParent == null)
                return;

            float spacing = 20f;
            float cardWidth = 520f;
            float totalWidth = _cardCount * cardWidth + (_cardCount - 1) * spacing;
            float startX = -totalWidth / 2 + cardWidth / 2;

            for (int i = 0; i < _bingoBoards.Count; i++)
            {
                var cardObj = _cardParent.GetChild(i);
                var rectTransform = cardObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(startX + i * (cardWidth + spacing), 0);
                }
            }
        }

        /// <summary>
        /// 更新 UI 显示
        /// </summary>
        private void UpdateUI()
        {
            if (_statusText != null)
            {
                _statusText.text = $"Called: {_calledNumbers.Count}/75";
            }

            if (_bingoCountText != null)
            {
                _bingoCountText.text = $"Bingo: {_totalBingoCount}";
            }

            UpdateAutoPlayButton();
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip">音效剪辑</param>
        private void PlaySound(AudioClip clip)
        {
            if (clip != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// 清理游戏板
        /// </summary>
        private void CleanupBoards()
        {
            // 清理 UI 对象
            foreach (Transform child in _cardParent)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private void Cleanup()
        {
            _isAutoPlaying = false;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            CleanupBoards();
        }

        // 公共接口
        public List<int> GetCalledNumbers() => _calledNumbers;
        public int GetTotalBingoCount() => _totalBingoCount;
        public int GetCardCount() => _cardCount;
        public bool IsAutoPlaying() => _isAutoPlaying;
    }
}