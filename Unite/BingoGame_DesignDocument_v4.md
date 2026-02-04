# Bingo 游戏设计文档 v4.0
## 客户端-服务器分离架构

---

## 目录
1. [架构概述](#1-架构概述)
2. [服务器端架构](#2-服务器端架构)
3. [客户端架构](#3-客户端架构)
4. [网络通信协议](#4-网络通信协议)
5. [游戏流程设计](#5-游戏流程设计)
6. [核心系统设计](#6-核心系统设计)
7. [UI 布局设计](#7-ui-布局设计)
8. [数据模型](#8-数据模型)
9. [实现示例](#9-实现示例)

---

## 1. 架构概述

### 1.1 整体架构图

```
┌─────────────────────────────────────────────────────────────┐
│                        客户端 (Unity3D)                      │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │
│  │   Models    │  │    Views    │  │ Controllers │         │
│  │  (数据模型)  │  │   (UGUI)    │  │  (业务逻辑)  │         │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘         │
│         │                 │                 │                 │
│         └─────────────────┴─────────────────┘                 │
│                           │                                   │
│                    ┌──────▼──────┐                            │
│                    │ NetworkClient│                            │
│                    │  (网络客户端) │                            │
│                    └──────┬──────┘                            │
└───────────────────────────┼───────────────────────────────────┘
                            │
                    ┌───────▼────────┐
                    │   WebSocket/   │
                    │   HTTP/TCP     │
                    └───────┬────────┘
                            │
┌───────────────────────────┼───────────────────────────────────┐
│                    ┌──────▼──────┐                            │
│                    │ NetworkServer│                            │
│                    │  (网络服务器) │                            │
│                    └──────┬──────┘                                                       │
│                           │                                   │
│  ┌─────────────┐  ┌──────▼──────┐  ┌─────────────┐         │
│  │  Services   │  │  Handlers   │  │  Managers   │         │
│  │  (业务服务)  │  │ (请求处理器) │  │  (游戏管理)  │         │
│  └─────────────┘  └─────────────┘  └─────────────┘         │
│                                                               │
│                        服务器端 (C#)                          │
└───────────────────────────────────────────────────────────────┘
```

### 1.2 设计原则

| 原则 | 说明 |
|------|------|
| **职责分离** | 客户端负责UI展示和用户交互，服务器负责游戏逻辑和数据验证 |
| **依赖注入** | 使用DI容器管理服务生命周期，降低耦合度 |
| **事件驱动** | 使用事件系统解耦模块间通信 |
| **模块化设计** | 模块化设计，易于扩展新功能 |
| **模板方法模式** | 通过模板方法模式支持多种玩法 |
| **状态同步** | 服务器是权威数据源，客户端负责状态同步和预测 |
| **安全性** | 所有关键逻辑在服务器端执行，防止作弊 |

### 1.3 设计模式应用

| 设计模式 | 应用场景 |
|----------|----------|
| **MVC** | 客户端整体架构 |
| **模板方法模式** | 玩法系统，定义玩法框架，子类实现具体逻辑 |
| **策略模式** | 呼叫系统（随机模式、动态平衡模式） |
| **观察者模式** | 事件系统 |
| **依赖注入** | 服务管理 |
| **对象池模式** | 动画效果、粒子系统 |

---

## 2. 服务器端架构

### 2.1 目录结构

```
BingoServer/
├── BingoServer.Core/              # 核心业务逻辑
│   ├── Models/                    # 数据模型
│   │   ├── Room.cs
│   │   ├── Player.cs
│   │   ├── Board.cs
│   │   ├── Slot.cs
│   │   └── PowerUp.cs
│   ├── Services/                  # 业务服务
│   │   ├── IRoomService.cs
│   │   ├── RoomService.cs
│   │   ├── IGameService.cs
│   │   ├── GameService.cs
│   │   ├── ICallerService.cs
│   │   ├── CallerService.cs
│   │   ├── IWinDetectionService.cs
│   │   ├── WinDetectionService.cs
│   │   └── IPowerUpService.cs
│   ├── GameModes/                 # 玩法系统（模板方法模式）
│   │   ├── BaseGameMode.cs
│   │   ├── ClassicBingoMode.cs
│   │   ├── SpeedBingoMode.cs
│   │   └── PowerUpBingoMode.cs
│   ├── Handlers/                  # 请求处理器
│   │   ├── JoinRoomHandler.cs
│   │   ├── ClickSlotHandler.cs
│   │   ├── CallNumberHandler.cs
│   │   └── GameEndHandler.cs
│   └── Events/                    # 事件系统
│       ├── GameEventBus.cs
│       ├── GameEvents.cs
│       └── EventHandlers.cs
├── BingoServer.Infrastructure/    # 基础设施
│   ├── Network/                   # 网络层
│   │   ├── INetworkServer.cs
│   │   ├── WebSocketServer.cs
│   │   └── PacketHandler.cs
│   ├── Database/                  # 数据库
│   │   ├── IDatabase.cs
│   │   └── InMemoryDatabase.cs
│   └── DI/                        # 依赖注入
│       ├── ServiceCollection.cs
│       └── ServiceProvider.cs
└── BingoServer.API/               # API层
    ├── Controllers/
    │   └── GameController.cs
    └── Program.cs
```

### 2.2 核心服务接口

```csharp
namespace BingoServer.Core.Services
{
    public interface IRoomService
    {
        Task<Room> CreateRoomAsync(string roomId, int maxPlayers);
        Task<Room> GetRoomAsync(string roomId);
        Task<Player> JoinRoomAsync(string roomId, string playerId);
        Task<List<Player>> GetPlayersAsync(string roomId);
    }

    public interface IGameService
    {
        Task<GameData> InitializeGameAsync(string roomId);
        Task StartGameAsync(string roomId);
        Task<ClickResult> ProcessClickAsync(string roomId, string playerId, int slotIndex);
        Task<GameResult> EndGameAsync(string roomId);
    }

    public interface ICallerService
    {
        Task<int> GenerateNextNumberAsync(string roomId);
        Task AddToCallQueueAsync(string roomId, int number);
        Task<List<int>> GetCalledNumbersAsync(string roomId);
    }

    public interface IWinDetectionService
    {
        List<WinLine> DetectWins(Board board);
        bool IsBingoComplete(Board board);
    }

    public interface IPowerUpService
    {
        Task<PowerUpResult> ActivatePowerUpAsync(string roomId, string playerId, int slotIndex);
        Task<List<PowerUp>> GeneratePowerUpsAsync(string roomId);
    }
}
```

### 2.3 玩法系统（模板方法模式）

```csharp
namespace BingoServer.Core.GameModes
{
    public abstract class BaseGameMode
    {
        protected readonly IRoomService _roomService;
        protected readonly ICallerService _callerService;
        protected readonly IWinDetectionService _winDetection;
        protected readonly GameEventBus _eventBus;

        public abstract GameModeType ModeType { get; }

        protected BaseGameMode(
            IRoomService roomService,
            ICallerService callerService,
            IWinDetectionService winDetection,
            GameEventBus eventBus)
        {
            _roomService = roomService;
            _callerService = callerService;
            _winDetection = winDetection;
            _eventBus = eventBus;
        }

        public async Task InitializeAsync(string roomId)
        {
            await OnInitializeAsync(roomId);
        }

        public async Task<ClickResult> ProcessClickAsync(string roomId, string playerId, int slotIndex)
        {
            var room = await _roomService.GetRoomAsync(roomId);
            var player = room.Players.FirstOrDefault(p => p.Id == playerId);
            var board = player.Boards.FirstOrDefault();
            var slot = board.Slots[slotIndex];

            var result = new ClickResult
            {
                SlotIndex = slotIndex,
                Number = slot.Number
            };

            if (slot.IsMarked)
            {
                result.Success = false;
                result.Message = "Slot already marked";
                return result;
            }

            await OnBeforeMarkAsync(roomId, playerId, slotIndex);

            slot.IsMarked = true;
            result.IsMarked = true;

            await OnAfterMarkAsync(roomId, playerId, slotIndex);

            var winLines = _winDetection.DetectWins(board);
            if (winLines.Any())
            {
                result.IsBingo = true;
                result.WinLines = winLines;
                room.BingoCount--;

                await OnBingoAchievedAsync(roomId, playerId, winLines);

                if (room.BingoCount == 0)
                {
                    await OnGameCompleteAsync(roomId);
                }
            }

            result.RemainingBingo = room.BingoCount;
            return result;
        }

        protected virtual Task OnInitializeAsync(string roomId)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnBeforeMarkAsync(string roomId, string playerId, int slotIndex)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnAfterMarkAsync(string roomId, string playerId, int slotIndex)
        {
            return Task.CompletedTask;
        }

        protected virtual async Task OnBingoAchievedAsync(string roomId, string playerId, List<WinLine> winLines)
        {
            _eventBus.Publish(new GameEvents.BingoAchieved
            {
                RoomId = roomId,
                PlayerId = playerId,
                WinLines = winLines
            });
        }

        protected virtual Task OnGameCompleteAsync(string roomId)
        {
            return Task.CompletedTask;
        }
    }

    public class ClassicBingoMode : BaseGameMode
    {
        public override GameModeType ModeType => GameModeType.ClassicBingo;

        public ClassicBingoMode(
            IRoomService roomService,
            ICallerService callerService,
            IWinDetectionService winDetection,
            GameEventBus eventBus)
            : base(roomService, callerService, winDetection, eventBus)
        {
        }
    }

    public class SpeedBingoMode : BaseGameMode
    {
        public override GameModeType ModeType => GameModeType.SpeedBingo;
        private float _speedMultiplier = 1.5f;

        public SpeedBingoMode(
            IRoomService roomService,
            ICallerService callerService,
            IWinDetectionService winDetection,
            GameEventBus eventBus)
            : base(roomService, callerService, winDetection, eventBus)
        {
        }

        protected override async Task OnAfterMarkAsync(string roomId, string playerId, int slotIndex)
        {
            await base.OnAfterMarkAsync(roomId, playerId, slotIndex);
            
            var room = await _roomService.GetRoomAsync(roomId);
            var player = room.Players.FirstOrDefault(p => p.Id == playerId);
            player.Score += (int)(10 * _speedMultiplier);
        }
    }

    public class PowerUpBingoMode : BaseGameMode
    {
        private readonly IPowerUpService _powerUpService;

        public override GameModeType ModeType => GameModeType.PowerUpBingo;

        public PowerUpBingoMode(
            IRoomService roomService,
            ICallerService callerService,
            IWinDetectionService winDetection,
            IPowerUpService powerUpService,
            GameEventBus eventBus)
            : base(roomService, callerService, winDetection, eventBus)
        {
            _powerUpService = powerUpService;
        }

        protected override async Task OnInitializeAsync(string roomId)
        {
            await base.OnInitializeAsync(roomId);
            await _powerUpService.GeneratePowerUpsAsync(roomId);
        }

        protected override async Task OnAfterMarkAsync(string roomId, string playerId, int slotIndex)
        {
            await base.OnAfterMarkAsync(roomId, playerId, slotIndex);

            var room = await _roomService.GetRoomAsync(roomId);
            var player = room.Players.FirstOrDefault(p => p.Id == playerId);
            var board = player.Boards.FirstOrDefault();
            var slot = board.Slots[slotIndex];

            if (slot.HasPowerUp)
            {
                var powerUpResult = await _powerUpService.ActivatePowerUpAsync(roomId, playerId, slotIndex);
                _eventBus.Publish(new GameEvents.PowerUpActivated
                {
                    RoomId = roomId,
                    PlayerId = playerId,
                    PowerUpResult = powerUpResult
                });
            }
        }
    }

    public enum GameModeType
    {
        ClassicBingo,
        SpeedBingo,
        PowerUpBingo
    }
}
```

### 2.4 事件系统

```csharp
namespace BingoServer.Core.Events
{
    public class GameEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _eventHandlers = new();

        public void Subscribe<T>(Action<T> handler)
        {
            var eventType = typeof(T);
            if (!_eventHandlers.ContainsKey(eventType))
                _eventHandlers[eventType] = new List<Delegate>();
            _eventHandlers[eventType].Add(handler);
        }

        public void Publish<T>(T eventData)
        {
            var eventType = typeof(T);
            if (_eventHandlers.ContainsKey(eventType))
            {
                foreach (var handler in _eventHandlers[eventType])
                {
                    ((Action<T>)handler)(eventData);
                }
            }
        }
    }

    public class GameEvents
    {
        public class PlayerJoined
        {
            public string RoomId { get; set; }
            public string PlayerId { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class GameStarted
        {
            public string RoomId { get; set; }
            public DateTime StartTime { get; set; }
        }

        public class SlotClicked
        {
            public string RoomId { get; set; }
            public string PlayerId { get; set; }
            public int SlotIndex { get; set; }
            public int Number { get; set; }
        }

        public class BingoAchieved
        {
            public string RoomId { get; set; }
            public string PlayerId { get; set; }
            public List<WinLine> WinLines { get; set; }
        }

        public class PowerUpActivated
        {
            public string RoomId { get; set; }
            public string PlayerId { get; set; }
            public PowerUpResult PowerUpResult { get; set; }
        }

        public class GameEnded
        {
            public string RoomId { get; set; }
            public List<PlayerResult> Results { get; set; }
        }
    }
}
```

### 2.5 请求处理器

```csharp
namespace BingoServer.Core.Handlers
{
    public class JoinRoomHandler
    {
        private readonly IRoomService _roomService;
        private readonly GameEventBus _eventBus;

        public JoinRoomHandler(IRoomService roomService, GameEventBus eventBus)
        {
            _roomService = roomService;
            _eventBus = eventBus;
        }

        public async Task<JoinRoomResponse> HandleAsync(JoinRoomRequest request)
        {
            var player = await _roomService.JoinRoomAsync(request.RoomId, request.PlayerId);
            var room = await _roomService.GetRoomAsync(request.RoomId);

            _eventBus.Publish(new GameEvents.PlayerJoined
            {
                RoomId = request.RoomId,
                PlayerId = request.PlayerId,
                Timestamp = DateTime.UtcNow
            });

            return new JoinRoomResponse
            {
                Success = true,
                RoomId = room.Id,
                BingoCount = room.BingoCount,
                Boards = room.Boards,
                Players = await _roomService.GetPlayersAsync(request.RoomId)
            };
        }
    }

    public class ClickSlotHandler
    {
        private readonly BaseGameMode _gameMode;
        private readonly GameEventBus _eventBus;

        public ClickSlotHandler(BaseGameMode gameMode, GameEventBus eventBus)
        {
            _gameMode = gameMode;
            _eventBus = eventBus;
        }

        public async Task<ClickSlotResponse> HandleAsync(ClickSlotRequest request)
        {
            var result = await _gameMode.ProcessClickAsync(
                request.RoomId, request.PlayerId, request.SlotIndex);

            return new ClickSlotResponse
            {
                Success = result.Success,
                SlotIndex = result.SlotIndex,
                IsMarked = result.IsMarked,
                HasPowerUp = result.HasPowerUp,
                PowerUpResult = result.PowerUpResult,
                IsBingo = result.IsBingo,
                WinLines = result.WinLines,
                RemainingBingo = result.RemainingBingo
            };
        }
    }
}
```

---

## 3. 客户端架构

### 3.1 目录结构

```
BingoClient/
├── Assets/
│   └── Scripts/
│       ├── Core/                     # 核心框架
│       │   ├── Models/               # 数据模型
│       │   │   ├── RoomData.cs
│       │   │   ├── PlayerData.cs
│       │   │   ├── BoardData.cs
│       │   │   └── SlotData.cs
│       │   ├── Events/               # 事件系统
│       │   │   ├── GameEventBus.cs
│       │   │   └── ClientEvents.cs
│       │   └── DI/                   # 依赖注入
│       │       └── ServiceLocator.cs
│       ├── Controllers/              # 控制器层
│       │   ├── GameController.cs
│       │   ├── RoomController.cs
│       │   ├── BoardController.cs
│       │   └── UIController.cs
│       ├── Views/                    # 视图层（UGUI）
│       │   ├── GameView.cs           # 主游戏视图
│       │   ├── BoardView.cs          # 游戏板视图
│       │   ├── SlotView.cs           # 格子视图
│       │   ├── PlayerListView.cs      # 玩家列表视图
│       │   ├── CallerView.cs         # 呼叫器视图
│       │   ├── PowerUpBarView.cs     # 道具栏视图
│       │   ├── SettingsView.cs       # 设置视图
│       │   └── GameEndView.cs       # 游戏结束视图
│       ├── Services/                 # 服务层
│       │   ├── NetworkService.cs
│       │   ├── AnimationService.cs
│       │   ├── AudioService.cs
│       │   └── FeedbackService.cs
│       ├── Systems/                  # 游戏系统
│       │   ├── CallerSystem.cs
│       │   ├── ComboSystem.cs
│       │   └── PowerUpSystem.cs
│       └── UI/                       # UGUI组件
│           ├── SlotButton.cs
│           ├── NumberBall.cs
│           ├── EnergyBar.cs
│           └── CountdownTimer.cs
```

### 3.2 MVC 架构实现

```csharp
namespace BingoClient.Controllers
{
    public class GameController : MonoBehaviour
    {
        private RoomController _roomController;
        private BoardController _boardController;
        private UIController _uiController;
        private NetworkService _networkService;
        private GameEventBus _eventBus;

        private void Awake()
        {
            var serviceLocator = ServiceLocator.Instance;
            _networkService = serviceLocator.GetService<NetworkService>();
            _eventBus = serviceLocator.GetService<GameEventBus>();

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
            _boardController.InitializeBoards(eventData.Boards);
            _uiController.ShowPlayerList(eventData.Players);
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
            _uiController.ShowGameEndEndView(eventData.Results);
        }
    }

    public class BoardController
    {
        private readonly NetworkService _networkService;
        private readonly GameEventBus _eventBus;
        private List<BoardData> _boards;
        private bool _interactionEnabled;

        public BoardController(NetworkService networkService, GameEventBus eventBus)
        {
            _networkService = networkService;
            _eventBus = eventBus;
        }

        public void InitializeBoards(List<BoardData> boards)
        {
            _boards = boards;
            _eventBus.Publish(new ClientEvents.BoardsInitialized { Boards = boards });
        }

        public void EnableInteraction()
        {
            _interactionEnabled = true;
        }

        public async Task ClickSlotAsync(int boardIndex, int slotIndex)
        {
            if (!_interactionEnabled) return;

            var response = await _networkService.SendClickAsync(boardIndex, slotIndex);

            _eventBus.Publish(new ClientEvents.SlotClicked
            {
                BoardIndex = boardIndex,
                SlotIndex = slotIndex,
                IsMarked = response.IsMarked,
                HasPowerUp = response.HasPowerUp,
                PowerUpResult = response.PowerUpResult,
                IsBingo = response.IsBingo,
                WinLines = response.WinLines,
                RemainingBingo = response.RemainingBingo
            });
        }

        public void MarkSlot(int slotIndex, bool isMarked)
        {
            _eventBus.Publish(new ClientEvents.SlotMarked
            {
                SlotIndex = slotIndex,
                IsMarked = isMarked
            });
        }
    }

    public class UIController
    {
        private readonly GameEventBus _eventBus;
        private GameView _gameView;
        private BoardView _boardView;
        private PlayerListView _playerListView;
        private CallerView _callerView;
        private PowerUpBarView _powerUpBarView;
        private GameEndView _gameEndView;

        public UIController(GameEventBus eventBus)
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
            _gameView = FindObjectOfType<GameView>();
            _gameView.Initialize(roomData);
        }

        public void ShowPlayerList(List<PlayerData> players)
        {
            _playerListView = FindObjectOfType<PlayerListView>();
            _playerListView.UpdatePlayers(players);
        }

        public void StartCountdown(int seconds)
        {
            _gameView?.StartCountdown(seconds, OnCountdownComplete);
        }

        private void OnCountdownComplete()
        {
            _eventBus.Publish(new ClientEvents.CountdownComplete());
        }

        public void ShowFeedback(FeedbackData feedback)
        {
            var feedbackService = ServiceLocator.Instance.GetService<FeedbackService>();
            feedbackService.ShowFeedback(feedback);
        }

        public void ShowGameEndView(List<PlayerResult> results)
        {
            _gameEndView = FindObjectOfType<GameEndView>();
            _gameEndView.ShowResults(results);
        }

        private void OnBoardsInitialized(ClientEvents.BoardsInitialized eventData)
        {
            _boardView = FindObjectOfType<BoardView>();
            _boardView.Initialize(eventData.Boards);
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
            _callerView?.AddNumber(eventData.Number);
        }
    }
}
```

### 3.3 UGUI 视图层实现

```csharp
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
            settingsView.Show();
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
    }

    public class BoardView : MonoBehaviour
    {
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private Transform _boardContainer;
        [SerializeField] private GridLayoutGroup _gridLayout;

        private List<SlotView> _slotViews = new();
        private List<BoardData> _boards;

        public void Initialize(List<BoardData> boards)
        {
            _boards = boards;
            CreateSlots();
        }

        private void CreateSlots()
        {
            foreach (var board in _boards)
            {
                var boardObject = Instantiate(_boardPrefab, _boardContainer);
                var boardTransform = boardObject.transform;
                var slotContainer = boardTransform.Find("SlotContainer");

                foreach (var slot in board.Slots)
                {
                    var slotView = Instantiate(_slotPrefab, slotContainer).GetComponent<SlotView>();
                    slotView.Initialize(slot);
                    slotView.OnClicked += OnSlotClicked;
                    _slotViews.Add(slotView);
                }
            }
        }

        private void OnSlotClicked(SlotView slotView)
        {
            var controller = ServiceLocator.Instance.GetService<BoardController>();
            controller.ClickSlotAsync(slotView.BoardIndex, slotView.SlotIndex);
        }

        public void UpdateSlot(int slotIndex, bool isMarked)
        {
            if (slotIndex >= 0 && slotIndex < _slotViews.Count)
            {
                _slotViews[slotIndex].SetMarked(isMarked);
            }
        }

        public void HighlightWinLines(List<WinLine> winLines)
        {
            foreach (var winLine in winLines)
            {
                foreach (var slotIndex in winLine.SlotIndices)
                {
                    _slotViews[slotIndex].HighlightWin();
                }
            }
        }
    }

    public class SlotView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _numberText;
        [SerializeField] private Image _background;
        [SerializeField] private Image _powerUpIcon;
        [SerializeField] private Image _numberBallIcon;

        private SlotData _slotData;
        private int _boardIndex;
        private int _slotIndex;

        public event Action<SlotView> OnClicked;

        public int BoardIndex => _boardIndex;
        public int SlotIndex => _slotIndex;

        public void Initialize(SlotData slotData)
        {
            _slotData = slotData;
            _numberText.text = slotData.Number.ToString();
            _button.onClick.AddListener(OnButtonClick);

            if (slotData.HasPowerUp)
            {
                ShowPowerUp(slotData.PowerUp.Type);
            }
        }

        private void OnButtonClick()
        {
            OnClicked?.Invoke(this);
        }

        public void SetMarked(bool isMarked)
        {
            _background.color = isMarked ? Color.green : Color.white;
            _numberBallIcon.gameObject.SetActive(!isMarked);
        }

        public void HighlightWin()
        {
            _background.color = Color.yellow;
        }

        public void ShowPowerUp(PowerUpType type)
        {
            _powerUpIcon.gameObject.SetActive(true);
        }

        public void Shake()
        {
            transform.DOShake(0.5f, 0.1f, 10, 90, false);
        }
    }

    public class CallerView : MonoBehaviour
    {
        [SerializeField] private Transform _numberContainer;
        [SerializeField] private GameObject _numberBallPrefab;
        [SerializeField] private float _spacing = 50f;
        [SerializeField] private int _maxVisibleNumbers = 10;

        private List<GameObject> _numberBalls = new();
        private Queue<int> _calledNumbers = new();

        public void AddNumber(int number)
        {
            _calledNumbers.Enqueue(number);
            CreateNumberBall(number);
            UpdateLayout();
        }

        private void CreateNumberBall(int number)
        {
            var numberBall = Instantiate(_numberBallPrefab, _numberContainer);
            var text = numberBall.GetComponent<TextMeshProUGUI>();
            text.text = number.ToString();
            _numberBalls.Add(numberBall);

            var rectTransform = numberBall.GetComponent<RectTransform>();
            rectTransform.DOAnchorPosX(_numberBalls.Count * _spacing, 0.3f).SetEase(Ease.OutBack);
        }

        private void UpdateLayout()
        {
            while (_numberBalls.Count > _maxVisibleNumbers)
            {
                var oldBall = _numberBalls[0];
                _numberBalls.RemoveAt(0);
                Destroy(oldBall);
            }

            for (int i = 0; i < _numberBalls.Count; i++)
            {
                var rectTransform = _numberBalls[i].GetComponent<RectTransform>();
                rectTransform.DOAnchorPosX(i * _spacing, 0.3f).SetEase(Ease.OutBack);
            }
        }
    }

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

    public class PowerUpBarView : MonoBehaviour
    {
        [SerializeField] private Transform _powerUpContainer;
        [SerializeField] private GameObject _powerUpButtonPrefab;
        [SerializeField] private Slider _energySlider;

        private List<PowerUpButton> _powerUpButtons = new();
        private float _currentEnergy;

        public void AddPowerUp(PowerUpType type)
        {
            var powerUpButton = Instantiate(_powerUpButtonPrefab, _powerUpContainer)
                .GetComponent<PowerUpButton>();
            powerUpButton.Initialize(type);
            powerUpButton.OnClicked += OnPowerUpClicked;
            _powerUpButtons.Add(powerUpButton);
        }

        private void OnPowerUpClicked(PowerUpButton button)
        {
            if (_currentEnergy >= button.RequiredEnergy)
            {
                _currentEnergy -= button.RequiredEnergy;
                UpdateEnergyBar();
                button.Activate();
            }
        }

        public void AddEnergy(float amount)
        {
            _currentEnergy = Mathf.Min(_currentEnergy + amount, 1f);
            UpdateEnergyBar();
        }

        private void UpdateEnergyBar()
        {
            _energySlider.value = _currentEnergy;
        }
    }

    public class PowerUpButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _energyText;

        private PowerUpType _powerUpType;

        public event Action<PowerUpButton> OnClicked;
        public float RequiredEnergy { get; private set; }

        public void Initialize(PowerUpType type)
        {
            _powerUpType = type;
            RequiredEnergy = GetRequiredEnergy(type);
            _energyText.text = RequiredEnergy.ToString();
            _button.onClick.AddListener(OnButtonClick);
        }

        private float GetRequiredEnergy(PowerUpType type)
        {
            return type switch
            {
                PowerUpType.DoublePayout => 0.5f,
                PowerUpType.DoubleDaub => 0.3f,
                PowerUpType.Box => 0.8f,
                PowerUpType.Coin => 0.2f,
                _ => 0.5f
            };
        }

        private void OnButtonClick()
        {
            OnClicked?.Invoke(this);
        }

        public void Activate()
        {
            _button.interactable = false;
            _icon.DOColor(Color.gray, 0.3f);
        }
    }

    public class SettingsView : MonoBehaviour
    {
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private Button _closeButton;

        private void Awake()
        {
            _closeButton.onClick.AddListener(OnCloseClicked);
            _settingsPanel.SetActive(false);
        }

        public void Show()
        {
            _settingsPanel.SetActive(true);
            _settingsPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        private void OnCloseClicked()
        {
            _settingsPanel.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => _settingsPanel.SetActive(false));
        }
    }

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
            var gameController = ServiceLocator.Instance.GetService<GameController>();
            gameController.RestartGame();
        }

        private void OnExitClicked()
        {
            SceneManager.LoadScene("Lobby");
        }
    }
}
```

---

## 4. 网络通信协议

### 4.1 消息类型定义

```csharp
namespace BingoShared.Protocol
{
    public enum MessageType
    {
        JoinRoom = 1,
        JoinRoomResponse = 2,
        GameStart = 3,
        ClickSlot = 4,
        ClickSlotResponse = 5,
        CallNumber = 6,
        CallNumberResponse = 7,
        GameEnd = 8,
        GameEndResponse = 9,
        PlayerUpdate = 10,
        BingoAchieved = 11,
        PowerUpActivated = 12
    }

    public class NetworkMessage
    {
        public MessageType Type { get; set; }
        public string Data { get; set; }
        public long Timestamp { get; set; }
    }
}
```

### 4.2 请求/响应模型

```csharp
namespace BingoShared.Protocol
{
    public class JoinRoomRequest
    {
        public string RoomId { get; set; }
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
    }

    public class JoinRoomResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public RoomData Room { get; set; }
    }

    public class ClickSlotRequest
    {
        public string RoomId { get; set; }
        public string PlayerId { get; set; }
        public int BoardIndex { get; set; }
        public int SlotIndex { get; set; }
    }

    public class ClickSlotResponse
    {
        public bool Success { get; set; }
        public int SlotIndex { get; set; }
        public bool IsMarked { get; set; }
        public bool HasPowerUp { get; set; }
        public PowerUpResult PowerUpResult { get; set; }
        public bool IsBingo { get; set; }
        public List<WinLine> WinLines { get; set; }
        public int RemainingBingo { get; set; }
    }

    public class CallNumberRequest
    {
        public string RoomId { get; set; }
        public string PlayerId { get; set; }
    }

    public class CallNumberResponse
    {
        public int Number { get; set; }
        public List<int> CalledNumbers { get; set; }
    }

    public class GameEndResponse
    {
        public List<PlayerResult> Results { get; set; }
        public DateTime EndTime { get; set; }
    }
}
```

### 4.3 网络服务实现（异步优化）

```csharp
namespace BingoClient.Services
{
    public class NetworkService : MonoBehaviour
    {
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly Dictionary<MessageType, Action<string>> _messageHandlers = new();
        private readonly Dictionary<MessageType, TaskCompletionSource<string>> _pendingRequests = new();

        private void Awake()
        {
            RegisterMessageHandlers();
        }

        private void RegisterMessageHandlers()
        {
            _messageHandlers[MessageType.JoinRoomResponse] = HandleJoinRoomResponse;
            _messageHandlers[MessageType.ClickSlotResponse] = HandleClickSlotResponse;
            _messageHandlers[MessageType.CallNumberResponse] = HandleCallNumberResponse;
            _messageHandlers[MessageType.GameEnd] = HandleGameEnd;
        }

        public async Task ConnectAsync(string serverUrl)
        {
            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();

            await _webSocket.ConnectAsync(new Uri(serverUrl), _cancellationTokenSource.Token);
            StartCoroutine(ReceiveMessages());
        }

        private IEnumerator ReceiveMessages()
        {
            var buffer = new byte[4096];

            while (_webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);

                if (result.Message.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", _cancellationTokenSource.Token);
                    break;
                }

                var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var message = JsonSerializer.Deserialize<NetworkMessage>(messageJson);

                if (_messageHandlers.TryGetValue(message.Type, out var handler))
                {
                    handler(message.Data);
                }

                if (_pendingRequests.TryGetValue(message.Type, out var tcs))
                {
                    tcs.SetResult(message.Data);
                    _pendingRequests.Remove(message.Type);
                }
            }

            yield return null;
        }

        public async Task<JoinRoomResponse> SendJoinRoomAsync(JoinRoomRequest request)
        {
            var message = new NetworkMessage
            {
                Type = MessageType.JoinRoom,
                Data = JsonSerializer.Serialize(request),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            await SendMessageAsync(message);
            var responseJson = await WaitForResponseAsync(MessageType.JoinRoomResponse);
            return JsonSerializer.Deserialize<JoinRoomResponse>(responseJson);
        }

        public async Task<ClickSlotResponse> SendClickAsync(int boardIndex, int slotIndex)
        {
            var request = new ClickSlotRequest
            {
                RoomId = GameData.Instance.RoomId,
                PlayerId = GameData.Instance.PlayerId,
                BoardIndex = boardIndex,
                SlotIndex = slotIndex
            };

            var message = new NetworkMessage
            {
                Type = MessageType.ClickSlot,
                Data = JsonSerializer.Serialize(request),
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            await SendMessageAsync(message);
            var responseJson = await WaitForResponseAsync(MessageType.ClickSlotResponse);
            return JsonSerializer.Deserialize<ClickSlotResponse>(responseJson);
        }

        private async Task SendMessageAsync(NetworkMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(json);
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
        }

        private async Task<string> WaitForResponseAsync(MessageType expectedType)
        {
            var tcs = new TaskCompletionSource<string>();
            _pendingRequests[expectedType] = tcs;
            return await tcs.Task;
        }

        private void HandleJoinRoomResponse(string data)
        {
            var response = JsonSerializer.Deserialize<JoinRoomResponse>(data);
            var eventBus = ServiceLocator.Instance.GetService<GameEventBus>();
            eventBus.Publish(new ClientEvents.GameInitialized
            {
                RoomData = response.Room
            });
        }

        private void HandleClickSlotResponse(string data)
        {
            var response = JsonSerializer.Deserialize<ClickSlotResponse>(data);
            var eventBus = ServiceLocator.Instance.GetService<GameEventBus>();
            eventBus.Publish(new ClientEvents.SlotClicked
            {
                SlotIndex = response.SlotIndex,
                IsMarked = response.IsMarked,
                HasPowerUp = response.HasPowerUp,
                PowerUpResult = response.PowerUpResult,
                IsBingo = response.IsBingo,
                WinLines = response.WinLines,
                RemainingBingo = response.RemainingBingo
            });
        }

        private void HandleCallNumberResponse(string data)
        {
            var response = JsonSerializer.Deserialize<CallNumberResponse>(data);
            var eventBus = ServiceLocator.Instance.GetService<GameEventBus>();
            eventBus.Publish(new ClientEvents.NumberCalled
            {
                Number = response.Number,
                CalledNumbers = response.CalledNumbers
            });
        }

        private void HandleGameEnd(string data)
        {
            var response = JsonSerializer.Deserialize<GameEndResponse>(data);
            var eventBus = ServiceLocator.Instance.GetService<GameEventBus>();
            eventBus.Publish(new ClientEvents.GameEnded
            {
                Results = response.Results
            });
        }
    }
}
```

---

## 5. 游戏流程设计

### 5.1 完整流程图

```
客户端                          服务器
  │                               │
  │  1. JoinRoom Request          │
  │──────────────────────────────>│
  │                               │
  │  2. JoinRoom Response         │
  │<──────────────────────────────│
  │  (RoomId, Boards, Players)    │
  │                               │
  │  3. 初始化游戏界面             │
  │  (显示玩家列表、游戏板)        │
  │                               │
  │  4. 开始倒计时 (3秒)           │
  │                               │
  │  5. 倒计时结束                 │
  │                               │
  │  6. GameStart Signal           │
  │──────────────────────────────>│
  │                               │
  │  7. GameStart Broadcast        │
  │<──────────────────────────────│
  │                               │
  │  8. 启用游戏交互               │
  │                               │
  │  9. ClickSlot Request          │
  │──────────────────────────────>│
  │                               │
  │  10. 检查道具                  │
  │  11. 更新游戏板                │
  │  12. 检查Bingo                 │
  │                               │
  │  13. ClickSlot Response        │
  │<──────────────────────────────│
  │  (IsMarked, PowerUp, Bingo)   │
  │                               │
  │  14. 更新UI显示                │
  │                               │
  │  [重复 9-14]                   │
  │                               │
  │  15. Bingo数量 = 0             │
  │                               │
  │  16. GameEnd Broadcast         │
  │<──────────────────────────────│
  │  (Results, Rankings)           │
  │                               │
  │  17. 显示游戏结束界面           │
```

### 5.2 服务器端游戏流程

```csharp
namespace BingoServer.Core.Services
{
    public class GameService : IGameService
    {
        private readonly IRoomService _roomService;
        private readonly ICallerService _callerService;
        private readonly BaseGameMode _gameMode;
        private readonly GameEventBus _eventBus;

        public GameService(
            IRoomService roomService,
            ICallerService callerService,
            BaseGameMode gameMode,
            GameEventBus eventBus)
        {
            _roomService = roomService;
            _callerService = callerService;
            _gameMode = gameMode;
            _eventBus = eventBus;
        }

        public async Task<GameData> InitializeGameAsync(string roomId)
        {
            var room = await _roomService.GetRoomAsync(roomId);
            await _gameMode.InitializeAsync(roomId);

            var gameData = new GameData
            {
                RoomId = roomId,
                Boards = room.Boards,
                Players = await _roomService.GetPlayersAsync(roomId),
                BingoCount = room.BingoCount
            };

            return gameData;
        }

        public async Task StartGameAsync(string roomId)
        {
            var room = await _roomService.GetRoomAsync(roomId);
            room.State = GameState.Started;
            room.StartTime = DateTime.UtcNow;

            _eventBus.Publish(new GameEvents.GameStarted
            {
                RoomId = roomId,
                StartTime = room.StartTime
            });
        }

        public async Task<ClickResult> ProcessClickAsync(string roomId, string playerId, int slotIndex)
        {
            return await _gameMode.ProcessClickAsync(roomId, playerId, slotIndex);
        }

        public async Task<GameResult> EndGameAsync(string roomId)
        {
            var room = await _roomService.GetRoomAsync(roomId);
            room.State = GameState.Ended;
            room.EndTime = DateTime.UtcNow;

            var results = room.Players
                .Select(p => new PlayerResult
                {
                    PlayerId = p.Id,
                    PlayerName = p.Name,
                    Score = p.Score,
                    BingoCount = p.BingoCount,
                    Rank = 0
                })
                .OrderByDescending(r => r.Score)
                .ToList();

            for (int i = 0; i < results.Count; i++)
            {
                results[i].Rank = i + 1;
            }

            _eventBus.Publish(new GameEvents.GameEnded
            {
                RoomId = roomId,
                Results = results
            });

            return new GameResult { Results = results };
        }
    }
}
```

---

## 6. 核心系统设计

### 6.1 呼叫系统

```csharp
namespace BingoServer.Core.Services
{
    public class CallerService : ICallerService
    {
        private readonly Dictionary<string, RoomCallerState> _callerStates = new();
        private readonly Random _random = new();

        public async Task<int> GenerateNextNumberAsync(string roomId)
        {
            if (!_callerStates.ContainsKey(roomId))
            {
                _callerStates[roomId] = new RoomCallerState();
            }

            var state = _callerStates[roomId];
            var availableNumbers = state.AvailableNumbers;

            if (availableNumbers.Count == 0)
            {
                return -1;
            }

            var index = _random.Next(availableNumbers.Count);
            var number = availableNumbers[index];
            availableNumbers.RemoveAt(index);

            state.CalledNumbers.Add(number);

            return number;
        }

        public async Task AddToCallQueueAsync(string roomId, int number)
        {
            if (!_callerStates.ContainsKey(roomId))
            {
                _callerStates[roomId] = new RoomCallerState();
            }

            _callerStates[roomId].CallQueue.Enqueue(number);
        }

        public async Task<List<int>> GetCalledNumbersAsync(string roomId)
        {
            if (_callerStates.TryGetValue(roomId, out var state))
            {
                return new List<int>(state.CalledNumbers);
            }
            return new List<int>();
        }

        private class RoomCallerState
        {
            public List<int> AvailableNumbers { get; } = Enumerable.Range(1, 75).ToList();
            public List<int> CalledNumbers { get; } = new();
            public Queue<int> CallQueue { get; } = new();
        }
    }
}
```

### 6.2 连线判定系统

```csharp
namespace BingoServer.Core.Services
{
    public class WinDetectionService : IWinDetectionService
    {
        private static readonly int[][] _horizontalLines = new int[][]
        {
            new[] { 0, 1, 2, 3, 4 },
            new[] { 5, 6, 7, 8, 9 },
            new[] { 10, 11, 12, 13, 14 },
            new[] { 15, 16, 17, 18, 19 },
            new[] { 20, 21, 22, 23, 24 }
        };

        private static readonly int[][] _verticalLines = new int[][]
        {
            new[] { 0, 5, 10, 15, 20 },
            new[] { 1, 6, 11, 16, 21 },
            new[] { 2, 7, 12, 17, 22 },
            new[] { 3, 8, 13, 18, 23 },
            new[] { 4, 9, 14, 19, 24 }
        };

        private static readonly int[][] _diagonalLines = new int[][]
        {
            new[] { 0, 6, 12, 18, 24 },
            new[] { 4, 8, 12, 16, 20 }
        };

        private static readonly int[] _fourCorners = new[] { 0, 4, 20, 24 };

        public List<WinLine> DetectWins(Board board)
        {
            var winLines = new List<WinLine>();

            winLines.AddRange(CheckHorizontalLines(board));
            winLines.AddRange(CheckVerticalLines(board));
            winLines.AddRange(CheckDiagonalLines(board));

            if (CheckFourCorners(board))
            {
                winLines.Add(new WinLine
                {
                    Type = WinLineType.FourCorners,
                    SlotIndices = _fourCorners.ToList()
                });
            }

            if (CheckFullBoard(board))
            {
                winLines.Add(new WinLine
                {
                    Type = WinLineType.FullBoard,
                    SlotIndices = Enumerable.Range(0, 25).ToList()
                });
            }

            return winLines;
        }

        private List<WinLine> CheckHorizontalLines(Board board)
        {
            var winLines = new List<WinLine>();

            for (int row = 0; row < 5; row++)
            {
                var indices = _horizontalLines[row];
                if (indices.All(i => board.Slots[i].IsMarked))
                {
                    winLines.Add(new WinLine
                    {
                        Type = WinLineType.Horizontal,
                        Row = row,
                        SlotIndices = indices.ToList()
                    });
                }
            }

            return winLines;
        }

        private List<WinLine> CheckVerticalLines(Board board)
        {
            var winLines = new List<WinLine>();

            for (int col = 0; col < 5; col++)
            {
                var indices = _verticalLines[col];
                if (indices.All(i => board.Slots[i].IsMarked))
                {
                    winLines.Add(new WinLine
                    {
                        Type = WinLineType.Vertical,
                        Column = col,
                        SlotIndices = indices.ToList()
                    });
                }
            }

            return winLines;
        }

        private List<WinLine> CheckDiagonalLines(Board board)
        {
            var winLines = new List<WinLine>();

            for (int i = 0; i < 2; i++)
            {
                var indices = _diagonalLines[i];
                if (indices.All(idx => board.Slots[idx].IsMarked))
                {
                    winLines.Add(new WinLine
                    {
                        Type = WinLineType.Diagonal,
                        DiagonalIndex = i,
                        SlotIndices = indices.ToList()
                    });
                }
            }

            return winLines;
        }

        private bool CheckFourCorners(Board board)
        {
            return _fourCorners.All(i => board.Slots[i].IsMarked);
        }

        private bool CheckFullFullBoard(Board board)
        {
            return board.Slots.All(s => s.IsMarked);
        }

        public bool IsBingoComplete(Board board)
        {
            return DetectWins(board).Any();
        }
    }
}
```

### 6.3 道具系统

```csharp
namespace BingoServer.Core.Services
{
    public class PowerUpService : IPowerUpService
    {
        private readonly IRoomService _roomService;
        private readonly Random _random = new();

        public PowerUpService(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public async Task<PowerUpResult> ActivatePowerUpAsync(string roomId, string playerId, int slotIndex)
        {
            var result = new PowerUpResult();

            var room = await _roomService.GetRoomAsync(roomId);
            var player = room.Players.FirstOrDefault(p => p.Id == playerId);
            var board = player.Boards.FirstOrDefault();
            var slot = board.Slots[slotIndex];

            switch (slot.PowerUp.Type)
            {
                case PowerUpType.DoublePayout:
                    result.Type = PowerUpType.DoublePayout;
                    result.Description = "双倍收益已激活";
                    player.Score *= 2;
                    break;

                case PowerUpType.DoubleDaub:
                    result.Type = PowerUpType.DoubleDaub;
                    result.Description = "双倍点击反馈";
                    break;

                case PowerUpType.Box:
                    result.Type = PowerUpType.Box;
                    result.Description = "随机开出奖励";
                    var reward = GenerateRandomReward();
                    result.Reward = reward;
                    player.Score += reward.Score;
                    player.Coins += reward.Coins;
                    break;

                case PowerUpType.Coin:
                    result.Type = PowerUpType.Coin;
                    result.Description = "获得金币";
                    var coins = _random.Next(10, 100);
                    result.Coins = coins;
                    player.Coins += coins;
                    break;
            }

            return result;
        }

        public async Task<List<PowerUp>> GeneratePowerUpsAsync(string roomId)
        {
            var powerUps = new List<PowerUp>();
            var room = await _roomService.GetRoomAsync(roomId);

            foreach (var board in room.Boards)
            {
                foreach (var slot in board.Slots)
                {
                    if (_random.NextDouble() < 0.1)
                    {
                        var powerUpType = (PowerUpType)_random.Next(1, 5);
                        slot.PowerUp = new PowerUp
                        {
                            Type = powerUpType,
                            IsActive = true
                        };
                        slot.HasPowerUp = true;
                        powerUps.Add(slot.PowerUp);
                    }
                }
            }

            return powerUps;
        }

        private Reward GenerateRandomReward()
        {
            return new Reward
            {
                Score = _random.Next(100, 500),
                Coins = _random.Next(50, 200)
            };
        }
    }
}
```

### 6.4 视觉反馈系统（流畅动画）

```csharp
namespace BingoClient.Services
{
    public class FeedbackService : MonoBehaviour
    {
        [SerializeField] private GameObject _feedbackPrefab;
        [SerializeField] private Transform _feedbackContainer;
        [SerializeField] private ParticleSystem _daubEffect;
        [SerializeField] private AudioClip _perfectSound;
        [SerializeField] private AudioClip _greatSound;
        [SerializeField] private AudioClip _missSound;

        private AudioSource _audioSource;
        private ComboSystem _comboSystem;
        private AnimationService _animationService;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _comboSystem = GetComponent<ComboSystem>();
            _animationService = ServiceLocator.Instance.GetService<AnimationService>();
        }

        public void ShowFeedback(FeedbackData feedback)
        {
            switch (feedback.Type)
            {
                case FeedbackType.Perfect:
                    ShowPerfectFeedback(feedback.Position);
                    break;
                case FeedbackType.Great:
                    ShowGreatFeedback(feedback.Position);
                    break;
                case FeedbackType.Miss:
                    ShowMissFeedback(feedback.Position);
                    break;
                case FeedbackType.Bingo:
                    ShowBingoFeedback();
                    break;
            }
        }

        private void ShowPerfectFeedback(Vector3 position)
        {
            var feedback = Instantiate(_feedbackPrefab, position, Quaternion.identity, _feedbackContainer);
            var text = feedback.GetComponent<TextMeshProUGUI>();
            text.text = "Perfect!";
            text.color = Color.yellow;

            _daubEffect.transform.position = position;
            _daubEffect.Play();

            _audioSource.PlayOneShot(_perfectSound);

            _comboSystem.AddCombo();

            feedback.transform.DOScale(Vector3.one * 1.5f, 0.2f).SetEase(Ease.OutBack);
            feedback.transform.DOMoveY(position.y + 50f, 0.5f).SetEase(Ease.OutQuad);
            feedback.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).SetDelay(0.3f);
            Destroy(feedback, 1f);
        }

        private void ShowGreatFeedback(Vector3 position)
        {
            var feedback = Instantiate(_feedbackPrefab, position, Quaternion.identity, _feedbackContainer);
            var text = feedback.GetComponent<TextMeshProUGUI>();
            text.text = "Great!";
            text.color = Color.green;

            _daubEffect.transform.position = position;
            _daubEffect.Play();

            _audioSource.PlayOneShot(_greatSound);

            _comboSystem.AddCombo();

            feedback.transform.DOScale(Vector3.one * 1.3f, 0.2f).SetEase(Ease.OutBack);
            feedback.transform.DOMoveY(position.y + 40f, 0.5f).SetEase(Ease.OutQuad);
            feedback.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).SetDelay(0.3f);
            Destroy(feedback, 1f);
        }

        private void ShowMissFeedback(Vector3 position)
        {
            var feedback = Instantiate(_feedbackPrefab, position, Quaternion.identity, _feedbackContainer);
            var text = feedback.GetComponent<TextMeshProUGUI>();
            text.text = "Miss!";
            text.color = Color.red;

            _audioSource.PlayOneShot(_missSound);

            _comboSystem.ResetCombo();

            var slot = position.GetComponent<SlotView>();
            slot.Shake();

            feedback.transform.DOScale(Vector3.one * 1.2f, 0.1f).SetEase(Ease.OutBack);
            feedback.GetComponent<CanvasGroup>().DOFade(0f, 0.3f);
            Destroy(feedback, 0.5f);
        }

        private void ShowBingoFeedback()
        {
            var feedback = Instantiate(_feedbackPrefab, _feedbackContainer);
            var text = feedback.GetComponent<TextMeshProUGUI>();
            text.text = "BINGO!";
            text.color = Color.magenta;
            text.fontSize = 72;

            feedback.transform.DOScale(Vector3.one * 2f, 0.5f).SetEase(Ease.OutElastic);
            feedback.transform.DORotate(Vector3.forward * 360f, 0.5f).SetEase(Ease.OutBack);
            Destroy(feedback, 2f);
        }
    }

    public class ComboSystem : MonoBehaviour
    {
        [SerializeField] private Slider _energyBar;
        [SerializeField] private int _maxCombo = 10;
        [SerializeField] private float _comboTimeout = 3f;

        private int _currentCombo;
        private float _lastComboTime;

        public void AddCombo()
        {
            _currentCombo = Mathf.Min(_currentCombo + 1, _maxCombo);
            _lastComboTime = Time.time;
            UpdateEnergyBar();
        }

        public void ResetCombo()
        {
            _currentCombo = 0;
            UpdateEnergyBar();
        }

        private void Update()
        {
            if (_currentCombo > 0 && Time.time - _lastComboTime > _comboTimeout)
            {
                ResetCombo();
            }
        }

        private void UpdateEnergyBar()
        {
            _energyBar.DOValue((float)_currentCombo / _maxCombo, 0.3f).SetEase(Ease.OutQuad);
        }
    }
}
```

### 6.5 动画服务

```csharp
namespace BingoClient.Services
{
    public class AnimationService : MonoBehaviour
    {
        private static AnimationService _instance;

        public static AnimationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AnimationService>();
                }
                return _instance;
            }
        }

        public void PlayDaubEffect(Vector3 position)
        {
            var effect = ObjectPool.Instance.Get("DaubEffect");
            effect.transform.position = position;
            effect.SetActive(true);

            var particleSystem = effect.GetComponent<ParticleSystem>();
            particleSystem.Play();

            StartCoroutine(ReturnToPoolAfterDelay(effect, particleSystem.main.duration));
        }

        public void PlayNumberBallAnimation(Transform ballTransform)
        {
            ballTransform.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutBack);
            ballTransform.DORotate(Vector3.forward * 360f, 0.5f).SetEase(Ease.OutBack);
        }

        public void PlayWinLineAnimation(List<Transform> slotTransforms)
        {
            for (int i = 0; i < slotTransforms.Count; i++)
            {
                var delay = i * 0.1f;
                slotTransforms[i].DOScale(Vector3.one * 1.3f, 0.3f).SetDelay(delay).SetEase(Ease.OutBack);
                slotTransforms[i].DORotate(Vector3.forward * 180f, 0.5f).SetDelay(delay).SetEase(Ease.OutBack);
            }
        }

        private IEnumerator ReturnToPoolAfterDelay(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            obj.SetActive(false);
            ObjectPool.Instance.Return(obj);
        }
    }
}
```

---

## 7. UI 布局设计

### 7.1 整体布局结构

```
┌─────────────────────────────────────────────────────────────────┐
│  顶部区域 (Top Bar)                                        │
│  ┌──────────┐  ┌──────────────────────┐  ┌──────────┐  │
│  │ Bingo: 5 │  │ [75] [12] [34] ...  │  │ [设置]  │  │
│  └──────────┘  └──────────────────────┘  └──────────┘  │
├─────────────────────────────────────────────────────────────────┤
│  左侧区域 (Left Panel)      │  中间区域 (Center Panel)      │
│  ┌──────────────────────┐  │  ┌──────────────────────┐  │
│  │ 玩家列表            │  │  │  Board 1            │  │
│  │ ┌────────────────┐  │  │  │  ┌───┬───┬───┐  │  │
│  │ │ 玩家1  100分  │  │  │  │  │ 1 │ 2 │ 3 │  │  │
│  │ │ Bingo: 2      │  │  │  │  ├───┼───┼───┤  │  │
│  │ ├────────────────┤  │  │  │  │ 4 │ 5 │ 6 │  │  │
│  │ │ 玩家2   80分  │  │  │  │  ├───┼───┼───┤  │  │
│  │ │ Bingo: 1      │  │  │  │  │ 7 │ 8 │ 9 │  │  │
│  │ ├────────────────┤  │  │  │  ├───┼───┼───┤  │  │
│  │ │ 玩家3   60分  │  │  │  │  │10 │11 │12 │  │  │
│  │ │ Bingo: 0      │  │  │  │  ├───┼───┼───┤  │  │
│  │ └────────────────┘  │  │  │  │13 │14 │15 │  │  │
│  └──────────────────────┘  │  │  │  ├───┼───┼───┤  │  │
│                            │  │  │  │16 │17 │18 │  │  │
│                            │  │  │  ├───┼───┼───┤  │  │
│                            │  │  │  │19 │20 │21 │  │  │
│                            │  │  │  ├───┼───┼───┤  │  │
│                            │  │  │  │22 │23 │24 │  │  │
│                            │  │  │  └───┴───┴───┘  │  │
│                            │  │  └──────────────────────┘  │
│                            │  │  ┌──────────────────────┐  │
│                            │  │  │  Board 2            │  │
│                            │  │  │  ...                 │  │
│                            │  │  └──────────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│  底部区域 (Bottom Bar)                                      │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ 道具栏: [双倍收益] [双倍点击] [宝箱] [金币]    │  │
│  │ 能量条: ████████████░░░░░░░░░░░░░░░░░░░░░░░░  │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### 7.2 移动端多卡布局适配

```csharp
namespace BingoClient.Views
{
    public class BoardLayoutAdapter : MonoBehaviour
    {
        [SerializeField] private BoardView _boardView;
        [SerializeField] private RectTransform _boardContainer;
        [SerializeField] private float _overviewScale = 0.5f;
        [SerializeField] private float _focusScale = 1f;

        private LayoutMode _currentMode = LayoutMode.Overview;
        private int _focusedBoardIndex = -1;

        public void SetLayoutMode(LayoutMode mode)
        {
            _currentMode = mode;
            UpdateLayout();
        }

        public void FocusBoard(int boardIndex)
        {
            _focusedBoardIndex = boardIndex;
            _currentMode = LayoutMode.Focus;
            UpdateLayout();
        }

        public void ShowOverview()
        {
            _focusedBoardIndex = -1;
            _currentMode = LayoutMode.Overview;
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            switch (_currentMode)
            {
                case LayoutMode.Overview:
                    ShowOverviewLayout();
                    break;
                case LayoutMode.Focus:
                    ShowFocusLayout();
                    break;
            }
        }

        private void ShowOverviewLayout()
        {
            _boardContainer.DOScale(Vector3.one * _overviewScale, 0.3f).SetEase(Ease.OutBack);
            _boardContainer.DOAnchorPos(Vector2.zero, 0.3f).SetEase(Ease.OutBack);

            var boards = _boardView.GetBoards();
            for (int i = 0; i < boards.Count; i++)
            {
                var board = boards[i];
                board.gameObject.SetActive(true);
                board.SetInteractable(false);

                var row = i / 2;
                var col = i % 2;
                var targetPos = new Vector2(col * 300, -row * 300);
                board.RectTransform.DOAnchorPos(targetPos, 0.3f).SetEase(Ease.OutBack);
            }
        }

        private void ShowFocusLayout()
        {
            _boardContainer.DOScale(Vector3.one * _focusScale, 0.3f).SetEase(Ease.OutBack);

            var boards = _boardView.GetBoards();
            for (int i = 0; i < boards.Count; i++)
            {
                var board = boards[i];
                board.gameObject.SetActive(i == _focusedBoardIndex);
                board.SetInteractable(i == _focusedBoardIndex);

                if (i == _focusedBoardIndex)
                {
                    board.RectTransform.DOAnchorPos(Vector2.zero, 0.3f).SetEase(Ease.OutBack);
                }
            }
        }
    }

    public enum LayoutMode
    {
        Overview,
        Focus
    }
}
```

---

## 8. 数据模型

### 8.1 共享数据模型

```csharp
namespace BingoShared.Models
{
    public class Room
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int MaxPlayers { get; set; }
        public List<Player> Players { get; set; }
        public List<Board> Boards { get; set; }
        public int BingoCount { get; set; }
        public GameState State { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Board> Boards { get; set; }
        public int Score { get; set; }
        public int Coins { get; set; }
        public int BingoCount { get; set; }
        public bool IsReady { get; set; }
    }

    public class Board
    {
        public string Id { get; set; }
        public string PlayerId { get; set; }
        public List<Slot> Slots { get; set; }
        public int BoardIndex { get; set; }
    }

    public class Slot
    {
        public int Index { get; set; }
        public int Number { get; set; }
        public bool IsMarked { get; set; }
        public bool HasPowerUp { get; set; }
        public PowerUp PowerUp { get; set; }
    }

    public class PowerUp
    {
        public PowerUpType Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime ExpiryTime { get; set; }
    }

    public class WinLine
    {
        public WinLineType Type { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int DiagonalIndex { get; set; }
        public List<int> SlotIndices { get; set; }
    }

    public class PlayerResult
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public int BingoCount { get; set; }
        public int Rank { get; set; }
    }

    public class Reward
    {
        public int Score { get; set; }
        public int Coins { get; set; }
    }

    public enum GameState
    {
        Waiting,
        Starting,
        Started,
        Ended
    }

    public enum PowerUpType
    {
        None,
        DoublePayout,
        DoubleDaub,
        Box,
        Coin
    }

    public enum WinLineType
    {
        Horizontal,
        Vertical,
        Diagonal,
        FourCorners,
        FullBoard
    }
}
```

### 8.2 客户端数据模型

```csharp
namespace BingoClient.Models
{
    public class GameData
    {
        public static GameData Instance { get; } = new();

        public string RoomId { get; set; }
        public string PlayerId { get; set; }
        public RoomData Room { get; set; }
        public List<BoardData> Boards { get; set; }
        public List<PlayerData> Players { get; set; }
    }

    public class RoomData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int BingoCount { get; set; }
        public List<BoardData> Boards { get; set; }
        public List<PlayerData> Players { get; set; }
    }

    public class PlayerData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public int Coins { get; set; }
        public int BingoCount { get; set; }
        public bool IsLocalPlayer { get; set; }
    }

    public class BoardData
    {
        public string Id { get; set; }
        public string PlayerId { get; set; }
        public List<SlotData> Slots { get; set; }
        public int BoardIndex { get; set; }
    }

    public class SlotData
    {
        public int Index { get; set; }
        public int Number { get; set; }
        public bool IsMarked { get; set; }
        public bool HasPowerUp { get; set; }
        public PowerUpData PowerUp { get; set; }
    }

    public class PowerUpData
    {
        public PowerUpType Type { get; set; }
        public bool IsActive { get; set; }
    }

    public class FeedbackData
    {
        public FeedbackType Type { get; set; }
        public Vector3 Position { get; set; }
        public string Message { get; set; }
    }

    public enum FeedbackType
    {
        Perfect,
        Great,
        Miss,
        Bingo
    }
}
```

---

## 9. 实现示例

### 9.1 服务器端启动代码

```csharp
namespace BingoServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = BuildServiceProvider();

            var server = serviceProvider.GetRequiredService<INetworkServer>();
            await server.StartAsync("ws://localhost:8080");

            Console.WriteLine("Bingo Server started on ws://localhost:8080");
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();

            await server.StopAsync();
        }

        private static IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddSingleton<INetworkServer, WebSocketServer>();
            services.AddSingleton<GameEventBus>();

            services.AddSingleton<IRoomService, RoomService>();
            services.AddSingleton<IGameService, GameService>();
            services.AddSingleton<ICallerService, CallerService>();
            services.AddSingleton<IWinDetectionService, WinDetectionService>();
            services.AddSingleton<IPowerUpService, PowerUpService>();

            services.AddSingleton<JoinRoomHandler>();
            services.AddSingleton<ClickSlotHandler>();
            services.AddSingleton<CallNumberHandler>();
            services.AddSingleton<GameEndHandler>();

            return services.BuildServiceProvider();
        }
    }
}
```

### 9.2 客户端启动代码

```csharp
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

            var eventBus = new GameEventBus();
            serviceLocator.RegisterService(eventBus);

            var networkService = gameObject.AddComponent<NetworkService>();
            serviceLocator.RegisterService(networkService);

            var animationService = gameObject.AddComponent<AnimationService>();
            serviceLocator.RegisterService(animationService);

            var audioService = gameObject.AddComponent<AudioService>();
            serviceLocator.RegisterService(audioService);

            var feedbackService = gameObject.AddComponent<FeedbackService>();
            serviceLocator.RegisterService(feedbackService);

            var gameController = gameObject.AddComponent<GameController>();
            serviceLocator.RegisterService(gameController);
        }

        private async Task ConnectToServer()
        {
            var networkService = ServiceLocator.Instance.GetService<NetworkService>();
            await networkService.ConnectAsync(_serverUrl);
        }

        public async void JoinGame(string roomId, string playerId)
        {
            var gameController = ServiceLocator.Instance.GetService<GameController>();
            await gameController.JoinRoomAsync(roomId, playerId);
        }
    }
}
```

### 9.3 依赖注入容器

```csharp
namespace BingoClient.Core.DI
{
    public class ServiceLocator
    {
        private static ServiceLocator _instance;
        private readonly Dictionary<Type, object> _services = new();

        public static ServiceLocator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ServiceLocator();
                }
                return _instance;
            }
        }

        public void RegisterService<T>(T service)
        {
            var serviceType = typeof(T);
            _services[serviceType] = service;
        }

        public T GetService<T>()
        {
            var serviceType = typeof(T);
            if (_services.TryGetValue(serviceType, out var service))
            {
                return (T)service;
            }
            throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered.");
        }

        public bool TryGetService<T>(out T service)
        {
            var serviceType = typeof(T);
            if (_services.TryGetValue(serviceType, out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = default;
            return false;
        }
    }
}
```

---

## 总结

本设计文档 v4.0 实现了以下目标：

1. **客户端-服务器分离**：明确划分了客户端（Unity3D）和服务器端（C#）的职责
2. **MVC 架构**：客户端采用标准的 Model-View-Controller 架构，使用 UGUI 实现
3. **依赖注入**：使用 DI 容器管理服务生命周期，降低耦合度
4. **事件系统**：使用事件总线解耦模块间通信
5. **模块化设计**：模块化设计，易于扩展新功能
6. **模板方法模式**：通过模板方法模式支持多种玩法（经典Bingo、速度Bingo、道具Bingo）
7. **完整游戏流程**：从进入游戏到游戏结束的完整流程设计
8. **核心系统**：呼叫系统、连线判定、道具系统、视觉反馈、移动端适配
9. **网络通信**：基于 WebSocket 的实时通信协议，异步操作优化
10. **流畅动画**：使用 DOTween 实现流畅的动画效果
11. **UI 布局**：完整的 UI 布局设计，包括顶部、左侧、中间、底部区域

该架构具有良好的可扩展性和可维护性，适合多人在线 Bingo 游戏的开发。
