# Unity Bingo Game 设计文档 v2.0

## 1. 项目概述

### 1.1 游戏简介
Bingo是一款经典的数字匹配游戏，支持多种玩法模式：

**常规数字Bingo**：玩家需要在5x5的网格中标记随机生成的数字，当达成特定连线模式时获胜。

**割草Bingo**：玩家需要在5x5的网格中收割草丛，通过收集道具来加速割草进度，当所有格子都被收割时获胜。

### 1.2 技术栈
- **Unity版本**: 2022.3 LTS 或更高
- **编程语言**: C# 10+
- **异步框架**: UniTask 2.x
- **动画框架**: DOTween 1.x
- **架构模式**: 模板方法 + 策略模式 + 事件驱动 + 依赖注入

### 1.3 核心特性
- 可扩展的游戏规则系统
- 支持多种Bingo模式（经典、四角、X形等）
- 流畅的动画效果
- 异步操作优化
- 模块化设计，易于扩展新功能
- 多人在线对战
- 服务器统一控制数字抽取
- 实时同步游戏状态
- 道具系统：支持多种游戏道具增强玩法
- 移动端优化：针对移动设备的触控优化
- 多玩法支持：通过模板方法模式支持多种玩法

---

## 2. 架构设计

### 2.1 整体架构

```
BingoGame/
├── Core/                    # 核心系统
│   ├── Models/             # 数据模型
│   ├── Views/              # 视图层
│   ├── ViewModels/         # 视图模型
│   └── Controllers/       # 控制器
├── GameModes/               # 玩法系统
│   ├── Base/               # 玩法基类
│   │   ├── GameMode.cs
│   │   ├── GameBoard.cs
│   │   └── GameBoardViewModel.cs
│   ├── ClassicBingo/       # 经典数字Bingo
│   │   ├── ClassicBingoGame.cs
│   │   ├── BingoBoard.cs
│   │   └── BingoBoardViewModel.cs
│   └── HarvestBingo/       # 割草Bingo
│       ├── HarvestBingoGame.cs
│       ├── HarvestBoard.cs
│       └── HarvestBoardViewModel.cs
├── Systems/                 # 游戏系统
│   ├── GameSystem/         # 游戏主系统
│   ├── NetworkSystem/      # 网络系统
│   └── ItemSystem/        # 道具系统
├── Services/                # 服务层
│   ├── AudioService/       # 音频服务
│   ├── AnimationService/   # 动画服务
│   ├── DataService/        # 数据服务
│   ├── ConfigService/      # 配置服务
│   └── NetworkService/     # 网络服务
├── Utilities/               # 工具类
│   ├── Extensions/         # 扩展方法
│   ├── Helpers/            # 辅助类
│   └── Constants/          # 常量定义
├── Events/                  # 事件系统
│   ├── GameEvents/         # 游戏事件
│   ├── UIEvents/           # UI事件
│   └── NetworkEvents/     # 网络事件
└── Data/                    # 数据资源
    ├── ScriptableObjects/  # 可配置数据
    └── Resources/          # 资源文件
```

### 2.2 设计模式应用

| 模式 | 应用场景 | 说明 |
|------|----------|------|
| MVVM | UI层 | 分离视图、数据和逻辑 |
| 单例模式 | 服务管理 | 全局服务访问 |
| 观察者模式 | 事件系统 | 解耦模块间通信 |
| 对象池模式 | 动画效果 | 复用动画对象 |
| 策略模式 | 胜利条件 | 可扩展的胜利规则 |
| 工厂模式 | 玩法创建 | 根据玩法类型创建对应实例 |
| 模板方法模式 | 玩法系统 | 定义玩法框架，子类实现具体逻辑 |
| 依赖注入 | 服务注入 | 降低耦合度 |

---

## 3. 核心模块设计

### 3.1 玩法模式系统（核心框架）

#### 3.1.1 玩法基类定义

```csharp
public enum GameMode
{
    ClassicBingo,      // 经典数字Bingo
    HarvestBingo       // 割草Bingo
}

public interface IGameMode
{
    GameMode ModeType { get; }
    string ModeName { get; }
    string Description { get; }
    
    UniTask InitializeAsync();
    UniTask StartGameAsync();
    UniTask PauseGameAsync();
    UniTask ResumeGameAsync();
    UniTask EndGameAsync();
    UniTask ResetGameAsync();
    
    bool CheckWinCondition();
    UniTask HandlePlayerInputAsync(Vector2Int position);
}

public abstract class BaseGameMode : IGameMode
{
    protected GameBoard board;
    protected GameBoardViewModel boardViewModel;
    protected GameStateMachine stateMachine;
    
    public abstract GameMode ModeType { get; }
    public abstract string ModeName { get; }
    public abstract string Description { get; }
    
    public virtual async UniTask InitializeAsync()
    {
        board = CreateBoard();
        boardViewModel = CreateBoardViewModel(board);
        stateMachine = new GameStateMachine();
        
        await boardViewModel.InitializeAsync();
    }
    
    public virtual async UniTask StartGameAsync()
    {
        await stateMachine.ChangeStateAsync(GameState.Playing);
    }
    
    public virtual async UniTask PauseGameAsync()
    {
        await stateMachine.ChangeStateAsync(GameState.Paused);
    }
    
    public virtual async UniTask ResumeGameAsync()
    {
        await stateMachine.ChangeStateAsync(GameState.Playing);
    }
    
    public virtual async UniTask EndGameAsync()
    {
        await stateMachine.ChangeStateAsync(GameState.GameOver);
    }
    
    public virtual async UniTask ResetGameAsync()
    {
        board.Reset();
        await boardViewModel.ResetBoardAsync();
        await stateMachine.ChangeStateAsync(GameState.Ready);
    }
    
    public virtual bool CheckWinCondition()
    {
        return board.IsAllCompleted();
    }
    
    public virtual async UniTask HandlePlayerInputAsync(Vector2Int position)
    {
        if (stateMachine.CurrentState == GameState.Playing)
        {
            await boardViewModel.InteractCellAsync(position.x, position.y);
            
            if (CheckWinCondition())
            {
                await EndGameAsync();
            }
        }
    }
    
    protected abstract GameBoard CreateBoard();
    protected abstract GameBoardViewModel CreateBoardViewModel(GameBoard board);
}
```

#### 3.1.2 抽象棋盘模型

```csharp
public abstract class GameBoard
{
    public const int BoardSize = 5;
    protected GameCell[,] cells;
    
    public abstract GameCell GetCell(int row, int col);
    public abstract void InteractCell(int row, int col);
    public abstract bool IsCellInteracted(int row, int col);
    public abstract GameCell[,] GetAllCells();
    public abstract void Reset();
    public abstract bool IsAllCompleted();
}

public abstract class GameCell
{
    public Vector2Int Position { get; set; }
    public abstract bool IsCompleted { get; set; }
}
```

#### 3.1.3 抽象棋盘视图模型

```csharp
public abstract class GameBoardViewModel
{
    protected GameBoard model;
    
    public abstract UniTask InitializeAsync();
    public abstract UniTask InteractCellAsync(int row, int col);
    public abstract UniTask HighlightCellAsync(int row, int col);
    public abstract UniTask ResetBoardAsync();
}
```

#### 3.1.4 玩法工厂

```csharp
public class GameModeFactory
{
    public static IGameMode CreateGameMode(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.ClassicBingo:
                return new ClassicBingoGame();
            case GameMode.HarvestBingo:
                return new HarvestBingoGame();
            default:
                throw new ArgumentException($"Unknown game mode: {mode}");
        }
    }
}
```

### 3.2 经典数字Bingo实现

#### 3.2.1 数据模型

```csharp
public class BingoCell : GameCell
{
    public int Number { get; set; }
    public bool IsMarked { get; set; }
    public bool IsFreeSpace { get; set; }
    
    public override bool IsCompleted
    {
        get => IsMarked;
        set => IsMarked = value;
    }
}

public class BingoBoard : GameBoard
{
    protected BingoCell[,] bingoCells;
    
    public BingoBoard()
    {
        bingoCells = new BingoCell[BoardSize, BoardSize];
        cells = bingoCells;
    }
    
    public override GameCell GetCell(int row, int col)
    {
        return bingoCells[row, col];
    }
    
    public override void InteractCell(int row, int col)
    {
        if (!bingoCells[row, col].IsMarked)
        {
            bingoCells[row, col].IsMarked = true;
        }
    }
    
    public override bool IsCellInteracted(int row, int col)
    {
        return bingoCells[row, col].IsMarked;
    }
    
    public override GameCell[,] GetAllCells()
    {
        return bingoCells;
    }
    
    public override void Reset()
    {
        foreach (var cell in bingoCells)
        {
            cell.IsMarked = false;
        }
    }
    
    public override bool IsAllCompleted()
    {
        return bingoCells.Cast<BingoCell>().All(c => c.IsMarked);
    }
}
```

#### 3.2.2 视图模型

```csharp
public class BingoBoardViewModel : GameBoardViewModel
{
    private BingoBoard bingoBoard;
    private IBingoBoardView view;
    
    public BingoBoardViewModel(BingoBoard board, IBingoBoardView view)
    {
        this.bingoBoard = board;
        this.model = board;
        this.view = view;
    }
    
    public override async UniTask InitializeAsync()
    {
        await view.InitializeBoardAsync(bingoBoard);
    }
    
    public override async UniTask InteractCellAsync(int row, int col)
    {
        bingoBoard.InteractCell(row, col);
        await view.UpdateCellAsync(row, col);
        await AnimationService.Instance.PlayCellMarkAnimationAsync(view.GetCellTransform(row, col));
    }
    
    public override async UniTask HighlightCellAsync(int row, int col)
    {
        await view.HighlightCellAsync(row, col);
    }
    
    public override async UniTask ResetBoardAsync()
    {
        bingoBoard.Reset();
        await view.ResetBoardAsync();
    }
}
```

#### 3.2.3 游戏模式实现

```csharp
public class ClassicBingoGame : BaseGameMode
{
    private BingoNumberSystem numberSystem;
    private WinConditionChecker winConditionChecker;
    
    public override GameMode ModeType => GameMode.ClassicBingo;
    public override string ModeName => "经典数字Bingo";
    public override string Description => "标记数字，达成连线获胜";
    
    public ClassicBingoGame()
    {
        numberSystem = new BingoNumberSystem();
        winConditionChecker = new WinConditionChecker();
    }
    
    public override async UniTask InitializeAsync()
    {
        await base.InitializeAsync();
        
        await numberSystem.InitializeAsync();
        await winConditionChecker.InitializeAsync();
    }
    
    public override bool CheckWinCondition()
    {
        var result = winConditionChecker.CheckWin(bingoBoard);
        return result.IsWin;
    }
    
    protected override GameBoard CreateBoard()
    {
        return new BingoBoard();
    }
    
    protected override GameBoardViewModel CreateBoardViewModel(GameBoard board)
    {
        var bingoBoard = board as BingoBoard;
        var view = ServiceLocator.GetService<IBingoBoardView>();
        return new BingoBoardViewModel(bingoBoard, view);
    }
}
```

### 3.3 割草Bingo实现

#### 3.3.1 数据模型

```csharp
public enum GrassColor
{
    Red,
    Blue,
    Green,
    Yellow,
    Purple
}

public class GrassCell : GameCell
{
    public GrassColor Color { get; set; }
    public bool IsHarvested { get; set; }
    public float GrowthProgress { get; set; }
    public bool HasCar { get; set; }
    public bool HasKey { get; set; }
    
    public override bool IsCompleted
    {
        get => IsHarvested;
        set => IsHarvested = value;
    }
}

public class HarvestBoard : GameBoard
{
    protected GrassCell[,] grassCells;
    private Dictionary<GrassColor, int> carCounts;
    private Dictionary<GrassColor, int> keyCounts;
    
    public HarvestBoard()
    {
        grassCells = new GrassCell[BoardSize, BoardSize];
        cells = grassCells;
        carCounts = new Dictionary<GrassColor, int>();
        keyCounts = new Dictionary<GrassColor, int>();
        
        foreach (GrassColor color in Enum.GetValues(typeof(GrassColor)))
        {
            carCounts[color] = 0;
            keyCounts[color] = 0;
        }
    }
    
    public override GameCell GetCell(int row, int col)
    {
        return grassCells[row, col];
    }
    
    public override void InteractCell(int row, int col)
    {
        var cell = grassCells[row, col];
        
        if (!cell.IsHarvested)
        {
            if (cell.HasCar)
            {
                cell.IsHarvested = true;
                carCounts[cell.Color]--;
            }
            else if (cell.HasKey)
            {
                HarvestColumn(row);
                keyCounts[cell.Color]--;
            }
        }
    }
    
    private void HarvestColumn(int col)
    {
        for (int row = 0; row < BoardSize; row++)
        {
            var cell = grassCells[row, col];
            if (!cell.IsHarvested)
            {
                cell.IsHarvested = true;
            }
        }
    }
    
    public override bool IsCellInteracted(int row, int col)
    {
        return grassCells[row, col].IsHarvested;
    }
    
    public override GameCell[,] GetAllCells()
    {
        return grassCells;
    }
    
    public override void Reset()
    {
        foreach (var cell in grassCells)
        {
            cell.IsHarvested = false;
            cell.HasCar = false;
            cell.HasKey = false;
        }
        
        foreach (GrassColor color in Enum.GetValues(typeof(GrassColor)))
        {
            carCounts[color] = 0;
            keyCounts[color] = 0;
        }
    }
    
    public override bool IsAllCompleted()
    {
        return grassCells.Cast<GrassCell>().All(c => c.IsHarvested);
    }
    
    public void AddCar(GrassColor color)
    {
        carCounts[color]++;
        UpdateCarStatus(color);
    }
    
    public void AddKey(GrassColor color)
    {
        keyCounts[color]++;
        UpdateKeyStatus(color);
    }
    
    private void UpdateCarStatus(GrassColor color)
    {
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                var cell = grassCells[row, col];
                if (cell.Color == color && !cell.IsHarvested)
                {
                    cell.HasCar = carCounts[color] > 0;
                }
            }
        }
    }
    
    private void UpdateKeyStatus(GrassColor color)
    {
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                var cell = grassCells[row, col];
                if (cell.Color == color && !cell.IsHarvested)
                {
                    cell.HasKey = keyCounts[color] > 0;
                }
            }
        }
    }
    
    public bool HasCar(GrassColor color) => carCounts[color] > 0;
    public bool HasKey(GrassColor color) => keyCounts[color] > 0;
}
```

#### 3.3.2 视图模型

```csharp
public class HarvestBoardViewModel : GameBoardViewModel
{
    private HarvestBoard harvestBoard;
    private IHarvestBoardView view;
    
    public HarvestBoardViewModel(HarvestBoard board, IHarvestBoardView view)
    {
        this.harvestBoard = board;
        this.model = board;
        this.view = view;
    }
    
    public override async UniTask InitializeAsync()
    {
        await view.InitializeBoardAsync(harvestBoard);
    }
    
    public override async UniTask InteractCellAsync(int row, int col)
    {
        var cell = harvestBoard.GetCell(row, col) as GrassCell;
        
        if (!cell.IsHarvested)
        {
            if (cell.HasCar)
            {
                harvestBoard.InteractCell(row, col);
                await view.UpdateCellAsync(row, col);
                await AnimationService.Instance.PlayHarvestAnimationAsync(row, col);
            }
            else if (cell.HasKey)
            {
                harvestBoard.InteractCell(row, col);
                await view.UpdateColumnAsync(row);
                await AnimationService.Instance.PlayColumnHarvestAnimationAsync(row);
            }
        }
    }
    
    public override async UniTask HighlightCellAsync(int row, int col)
    {
        await view.HighlightCellAsync(row, col);
    }
    
    public override async UniTask ResetBoardAsync()
    {
        harvestBoard.Reset();
        await view.ResetBoardAsync();
    }
}
```

#### 3.3.3 游戏模式实现

```csharp
public class HarvestBingoGame : BaseGameMode
{
    private HarvestItemService itemService;
    
    public override GameMode ModeType => GameMode.HarvestBingo;
    public override string ModeName => "割草Bingo";
    public override string Description => "收割草丛，收集道具加速";
    
    public HarvestBingoGame()
    {
        itemService = new HarvestItemService();
    }
    
    public override async UniTask InitializeAsync()
    {
        await base.InitializeAsync();
        await itemService.InitializeAsync();
    }
    
    protected override GameBoard CreateBoard()
    {
        return new HarvestBoard();
    }
    
    protected override GameBoardViewModel CreateBoardViewModel(GameBoard board)
    {
        var harvestBoard = board as HarvestBoard;
        var view = ServiceLocator.GetService<IHarvestBoardView>();
        return new HarvestBoardViewModel(harvestBoard, view);
    }
}
```

### 3.4 游戏状态管理

```csharp
public enum GameState
{
    None,
    Initializing,
    Ready,
    Playing,
    Paused,
    CheckingWin,
    Win,
    Lose,
    GameOver
}

public class GameStateMachine
{
    private GameState currentState;
    private Dictionary<GameState, List<GameStateTransition>> transitions;
    
    public UniTask ChangeStateAsync(GameState newState);
    public bool CanTransitionTo(GameState state);
    public GameState CurrentState => currentState;
}
```

### 3.5 道具系统设计

#### 3.5.1 道具类型定义

```csharp
public enum ItemType
{
    None,
    InstantBingo,          // 即时Bingo：自动标记一个数字
    DoublePoints,          // 双倍积分：获得双倍分数
    ExtraTime,            // 额外时间：延长游戏时间
    FreeDaub,            // 免费标记：免费标记一个格子
    TreasureChest,        // 宝箱：随机获得奖励
    CoinBoost,            // 金币加成：增加金币获取
    MagicDaub,           // 魔法标记：标记时触发特效
    LuckyLumber,          // 幸运数字：指定数字更容易出现
    Shield,               // 护盾：防止负面效果
    SpeedUp,              // 加速：加快数字抽取速度
    Gasoline,             // 汽油：给对应颜色的小车加油（割草Bingo）
    Key,                  // 钥匙：直接收割对应颜色的整列（割草Bingo）
    AutoHarvest           // 自动收割：自动收割一个格子（割草Bingo）
}

public enum ItemRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public enum ItemTarget
{
    Self,              // 只对自己生效
    AllPlayers,        // 对所有玩家生效
    RandomPlayer,       // 对随机玩家生效
    Opponents          // 对对手生效
}
```

#### 3.5.2 道具数据模型

```csharp
[Serializable]
public class ItemData
{
    public string itemId;
    public string itemName;
    public string description;
    public ItemType itemType;
    public ItemRarity rarity;
    public Sprite icon;
    public int cost;
    public int maxStack;
    public float cooldown;
    public float duration;
    public ItemTarget target;
    public bool isConsumable;
    public bool isActiveInGame;
    public GameMode? applicableMode; // 适用玩法
}

[Serializable]
public class InventoryItem
{
    public string itemId;
    public int count;
    public DateTime lastUsedTime;
}

[Serializable]
public class ActiveItemEffect
{
    public string itemId;
    public string playerId;
    public float startTime;
    public float duration;
    public Dictionary<string, object> parameters;
}
```

#### 3.5.3 道具服务

```csharp
public interface IItemService
{
    UniTask InitializeAsync();
    UniTask<InventoryItem[]> GetInventoryAsync();
    UniTask<bool> UseItemAsync(string itemId);
    UniTask<bool> AddItemAsync(string itemId, int count);
    UniTask RemoveItemAsync(string itemId, int count);
    UniTask<List<ActiveItemEffect>> GetActiveEffectsAsync();
    UniTask ActivateEffectAsync(string itemId, string playerId);
    UniTask DeactivateEffectAsync(string effectId);
}

public class ItemService : IItemService
{
    private ItemConfig config;
    private Dictionary<string, InventoryItem> inventory;
    private List<ActiveItemEffect> activeEffects;
    private Dictionary<ItemType, Func<string, UniTask>> itemHandlers;
    
    public async UniTask InitializeAsync()
    {
        config = Resources.Load<ItemConfig>("ItemConfig");
        inventory = new Dictionary<string, InventoryItem>();
        activeEffects = new List<ActiveItemEffect>();
        
        RegisterItemHandlers();
        await LoadInventoryAsync();
    }
    
    private void RegisterItemHandlers()
    {
        itemHandlers = new Dictionary<ItemType, Func<string, UniTask>>
        {
            { ItemType.InstantBingo, HandleInstantBingo },
            { ItemType.DoublePoints, HandleDoublePoints },
            { ItemType.ExtraTime, HandleExtraTime },
            { ItemType.FreeDaub, HandleFreeDaub },
            { ItemType.TreasureChest, HandleTreasureChest },
            { ItemType.CoinBoost, HandleCoinBoost },
            { ItemType.MagicDaub, HandleMagicDaub },
            { ItemType.LuckyLumber, HandleLuckyLumber },
            { ItemType.Shield, HandleShield },
            { ItemType.SpeedUp, HandleSpeedUp },
            { ItemType.Gasoline, HandleGasoline },
            { ItemType.Key, HandleKey },
            { ItemType.AutoHarvest, HandleAutoHarvest }
        };
    }
    
    public async UniTask<bool> UseItemAsync(string itemId)
    {
        var itemData = config.GetItem(itemId);
        
        if (itemData == null)
            return false;
        
        if (!inventory.ContainsKey(itemId) || inventory[itemId].count <= 0)
            return false;
        
        if (itemData.cooldown > 0)
        {
            var lastUsed = inventory[itemId].lastUsedTime;
            var cooldownRemaining = (DateTime.Now - lastUsed).TotalSeconds;
            
            if (cooldownRemaining < itemData.cooldown)
                return false;
        }
        
        inventory[itemId].count--;
        inventory[itemId].lastUsedTime = DateTime.Now;
        
        await ActivateEffectAsync(itemId, NetworkManager.Instance.PlayerId);
        
        await SaveInventoryAsync();
        
        return true;
    }
    
    public async UniTask ActivateEffectAsync(string itemId, string playerId)
    {
        var itemData = config.GetItem(itemId);
        
        if (itemHandlers.TryGetValue(itemData.itemType, out var handler))
        {
            await handler.Invoke(playerId);
        }
        
        if (itemData.duration > 0)
        {
            var effect = new ActiveItemEffect
            {
                itemId = itemId,
                playerId = playerId,
                startTime = Time.time,
                duration = itemData.duration
            };
            
            activeEffects.Add(effect);
            
            await UniTask.Delay(TimeSpan.FromSeconds(itemData.duration));
            
            await DeactivateEffectAsync(effect.itemId);
        }
    }
    
    private async UniTask HandleInstantBingo(string playerId)
    {
        var board = BoardSystem.Instance.GetBoard();
        var unmarkedCells = board.GetAllCells()
            .Where(c => !c.IsCompleted)
            .ToList();
        
        if (unmarkedCells.Count > 0)
        {
            var randomCell = unmarkedCells[Random.Range(0, unmarkedCells.Count)];
            await BoardSystem.Instance.InteractCellAsync(randomCell.Position.x, randomCell.Position.y);
        }
    }
    
    private async UniTask HandleDoublePoints(string playerId)
    {
        var effect = new ActiveItemEffect
        {
            itemId = "double_points",
            playerId = playerId,
            startTime = Time.time,
            duration = 30f,
            parameters = new Dictionary<string, object>
            {
                { "multiplier", 2f }
            }
        };
        
        activeEffects.Add(effect);
        GameEvents.OnItemEffectActivated?.Invoke(effect);
    }
    
    private async UniTask HandleGasolineAsync(string playerId)
    {
        var itemData = config.GetItem("gasoline_red");
        var color = GrassColor.Red;
        
        var harvestBoard = BoardSystem.Instance.GetBoard() as HarvestBoard;
        if (harvestBoard != null)
        {
            harvestBoard.AddCar(color);
            await AnimationService.Instance.PlayCarRefuelAnimationAsync(color);
        }
    }
    
    private async UniTask HandleKeyAsync(string playerId)
    {
        var itemData = config.GetItem("key_blue");
        var color = GrassColor.Blue;
        
        var harvestBoard = BoardSystem.Instance.GetBoard() as HarvestBoard;
        if (harvestBoard != null)
        {
            harvestBoard.AddKey(color);
            
            for (int row = 0; row < HarvestBoard.BoardSize; row++)
            {
                var cell = harvestBoard.GetCell(row, 0);
                if (cell.Color == color && !cell.IsHarvested)
                {
                    await harvestBoard.InteractCellAsync(row, 0);
                    break;
                }
            }
        }
    }
    
    private async UniTask HandleAutoHarvestAsync(string playerId)
    {
        var board = BoardSystem.Instance.GetBoard();
        var unharvestedCells = board.GetAllCells()
            .Where(c => !c.IsCompleted)
            .ToList();
        
        if (unharvestedCells.Count > 0)
        {
            var randomCell = unharvestedCells[Random.Range(0, unharvestedCells.Count)];
            await BoardSystem.Instance.InteractCellAsync(randomCell.Position.x, randomCell.Position.y);
        }
    }
    
    public float GetScoreMultiplier(string playerId)
    {
        var effects = activeEffects
            .Where(e => e.playerId == playerId && e.itemId == "double_points")
            .ToList();
        
        return effects.Count > 0 ? 2f : 1f;
    }
    
    public bool HasShield(string playerId)
    {
        return activeEffects.Any(e => 
            e.playerId == playerId && e.itemId == "shield");
    }
}
```

---

## 4. 网络系统设计

### 4.1.1 网络协议定义

```csharp
public enum MessageType
{
    None,
    Connect,
    Disconnect,
    JoinRoom,
    LeaveRoom,
    RoomJoined,
    RoomLeft,
    GameStart,
    GameEnd,
    NumberCalled,
    CellMarked,
    PlayerWin,
    PlayerListUpdate,
    GameStateSync,
    ItemUsed,
    ItemEffectActivated,
    CellHarvested,
    CarRefueled,
    ColumnHarvested,
    Error
}

[Serializable]
public class NetworkMessage
{
    public MessageType type;
    public string senderId;
    public long timestamp;
    public string data;
}
```

### 4.2 网络服务接口

```csharp
public interface INetworkService
{
    bool IsConnected { get; }
    bool IsHost { get; }
    string PlayerId { get; }
    string RoomId { get; }
    
    UniTask ConnectAsync(string serverUrl);
    UniTask DisconnectAsync();
    UniTask JoinRoomAsync(string roomCode, string playerName);
    UniTask LeaveRoomAsync();
    UniTask SendMessageAsync(NetworkMessage message);
    
    event Action<NetworkMessage> OnMessageReceived;
    event Action OnConnected;
    event Action OnDisconnected;
    event Action<string> OnError;
}
```

---

## 5. 服务层设计

### 5.1 动画服务

```csharp
public interface IAnimationService
{
    UniTask PlayCellMarkAnimationAsync(Transform cellTransform);
    UniTask PlayNumberCallAnimationAsync(int number);
    UniTask PlayWinAnimationAsync(List<Transform> winningCells);
    UniTask PlayLoseAnimationAsync();
    UniTask PlayShakeAnimationAsync(Transform target);
    UniTask PlayHarvestAnimationAsync(int row, int col);
    UniTask PlayCarRefuelAnimationAsync(GrassColor color);
    UniTask PlayColumnHarvestAnimationAsync(int col);
    void StopAllAnimations();
}
```

### 5.2 音频服务

```csharp
public interface IAudioService
{
    UniTask PlaySoundAsync(SoundType soundType);
    UniTask PlayMusicAsync(MusicType musicType, bool loop = true);
    void StopMusic();
    void SetVolume(float volume);
}
```

### 5.3 配置服务

```csharp
[CreateAssetMenu(fileName = "BingoGameConfig", menuName = "Bingo/Game Config")]
public class BingoGameConfig : ScriptableObject
{
    [Header("Game Settings")]
    public GameMode defaultGameMode;
    
    [Header("Board Settings")]
    public int boardSize = 5;
    public int minNumber = 1;
    public int maxNumber = 75;
    public bool useFreeSpace = true;
    
    [Header("Animation Settings")]
    public float cellMarkDuration = 0.2f;
    public float numberCallDuration = 0.5f;
    public float winCheckDelay = 0.3f;
    
    [Header("Colors")]
    public Color defaultCellColor = Color.white;
    public Color markedCellColor = Color.yellow;
    public Color winCellColor = Color.green;
    
    [Header("Win Conditions")]
    public bool enableRowWin = true;
    public bool enableColumnWin = true;
    public bool enableDiagonalWin = true;
    public bool enableFourCornersWin = true;
    public bool enableXPatternWin = false;
}
```

---

## 6. 事件系统设计

### 6.1 游戏事件

```csharp
public static class GameEvents
{
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<int> OnNumberCalled;
    public static event Action<Vector2Int> OnCellMarked;
    public static event Action<WinResult> OnWinChecked;
    public static event Action OnGameReset;
    public static event Action<ActiveItemEffect> OnItemEffectActivated;
    
    public static void NotifyGameStateChanged(GameState state) => OnGameStateChanged?.Invoke(state);
    public static void NotifyNumberCalled(int number) => OnNumberCalled?.Invoke(number);
    public static void NotifyCellMarked(Vector2Int position) => OnCellMarked?.Invoke(position);
    public static void NotifyWinChecked(WinResult result) => OnWinChecked?.Invoke(result);
    public static void NotifyGameReset() => OnGameReset?.Invoke();
    public static void NotifyItemEffectActivated(ActiveItemEffect effect) => OnItemEffectActivated?.Invoke(effect);
}
```

### 6.2 UI事件

```csharp
public static class UIEvents
{
    public static event Action OnStartButtonClicked;
    public static event Action OnPauseButtonClicked;
    public static event Action OnResumeButtonClicked;
    public static event Action OnResetButtonClicked;
    public static event Action<int> OnCellClicked;
    public static event Action<GameMode> OnGameModeSelected;
    
    public static void NotifyStartButtonClicked() => OnStartButtonClicked?.Invoke();
    public static void NotifyPauseButtonClicked() => OnPauseButtonClicked?.Invoke();
    public static void NotifyResumeButtonClicked() => OnResumeButtonClicked?.Invoke();
    public static void NotifyResetButtonClicked() => OnResetButtonClicked?.Invoke();
    public static void NotifyCellClicked(int cellIndex) => OnCellClicked?.Invoke(cellIndex);
    public static void NotifyGameModeSelected(GameMode mode) => OnGameModeSelected?.Invoke(mode);
}
```

---

## 7. 游戏流程设计

### 7.1 游戏初始化流程

```
1. 加载配置 (LoadConfig)
   ↓
2. 初始化服务 (InitializeServices)
   - AnimationService
   - AudioService
   - DataService
   - NetworkService
   ↓
3. 选择玩法模式 (SelectGameMode)
   - 经典数字Bingo
   - 割草Bingo
   ↓
4. 创建玩法实例 (CreateGameMode)
   ↓
5. 初始化玩法 (InitializeGameMode)
   ↓
6. 连接服务器 (ConnectToServer)
   ↓
7. 加入房间 (JoinRoom)
   ↓
8. 初始化UI (InitializeUI)
   ↓
9. 切换到Ready状态 (ChangeState: Ready)
```

### 7.2 游戏进行流程

```
1. 玩家点击开始 (StartGame)
   ↓
2. 发送开始游戏请求到服务器 (SendStartGameRequest)
   ↓
3. 等待服务器开始游戏 (WaitForGameStart)
   ↓
4. 切换到Playing状态 (ChangeState: Playing)
   ↓
5. 循环 (由服务器控制):
   a. 服务器下发数字/事件 (ReceiveEventFromServer)
   b. 播放动画 (PlayAnimation)
   c. 玩家点击格子 (PlayerClickCell)
   d. 发送交互请求到服务器 (SendInteractRequest)
   e. 服务器广播交互结果 (BroadcastInteractResult)
   f. 检查胜利条件 (CheckWin)
   g. 如果胜利 → 发送胜利消息到服务器 (SendWinMessage)
   h. 服务器广播胜利结果 (BroadcastWinResult)
   i. 切换到Win状态 (ChangeState: Win)
```

---

## 8. 扩展性设计

### 8.1 添加新玩法

开发者可以通过继承 `BaseGameMode` 类来添加新玩法：

```csharp
public class CustomGameMode : BaseGameMode
{
    public override GameMode ModeType => GameMode.Custom;
    public override string ModeName => "自定义玩法";
    public override string Description => "自定义玩法描述";
    
    protected override GameBoard CreateBoard()
    {
        return new CustomBoard();
    }
    
    protected override GameBoardViewModel CreateBoardViewModel(GameBoard board)
    {
        var customBoard = board as CustomBoard;
        var view = ServiceLocator.GetService<ICustomBoardView>();
        return new CustomBoardViewModel(customBoard, view);
    }
}
```

### 8.2 添加新道具

开发者可以通过实现 `IItemService` 接口来添加自定义道具：

```csharp
public class CustomItemService : IItemService
{
    private Dictionary<ItemType, Func<string, UniTask>> customHandlers;
    
    public async UniTask InitializeAsync()
    {
        customHandlers = new Dictionary<ItemType, Func<string, UniTask>>
        {
            { ItemType.CustomItem, HandleCustomItem }
        };
    }
    
    private async UniTask HandleCustomItem(string playerId)
    {
        await AnimationService.Instance.PlayCustomAnimationAsync();
    }
}
```

---

## 9. 部署配置

### 9.1 包依赖

```json
{
  "dependencies": {
    "com.cysharp.unitask": "2.5.0",
    "com.demigiant.dotween": "1.3.0",
    "com.unity.textmeshpro": "3.0.6",
    "com.unity.nuget.newtonsoft-json": "3.2.1"
  }
}
```

### 9.2 构建设置

- **目标平台**: Windows, macOS, iOS, Android
- **Scripting Backend**: IL2CPP
- **API Compatibility Level**: .NET Standard 2.1
- **Minimum API Level**: Android 7.0 (API Level 24)
- **Target SDK Version**: Android 13.0 (API Level 33)

---

## 10. 开发计划

### Phase 1: 核心框架 (Week 1-2)
- [ ] 搭建项目结构
- [ ] 实现状态机
- [ ] 实现事件系统
- [ ] 实现服务容器
- [ ] 集成UniTask和DOTween

### Phase 2: 玩法系统 (Week 3-4)
- [ ] 实现玩法基类
- [ ] 实现抽象棋盘
- [ ] 实现玩法工厂
- [ ] 实现经典数字Bingo
- [ ] 实现割草Bingo

### Phase 3: 网络系统 (Week 5-6)
- [ ] 实现网络协议定义
- [ ] 实现WebSocket网络服务
- [ ] 实现网络管理器
- [ ] 实现网络事件系统
- [ ] 实现断线重连机制

### Phase 4: 道具系统 (Week 7)
- [ ] 实现道具数据模型
- [ ] 实现道具服务
- [ ] 实现道具UI
- [ ] 实现道具网络同步

### Phase 5: UI系统 (Week 8-9)
- [ ] 实现主菜单
- [ ] 实现玩法选择界面
- [ ] 实现游戏UI
- [ ] 实现结果界面
- [ ] 实现暂停菜单
- [ ] 实现玩家列表UI

### Phase 6: 动画和音效 (Week 10)
- [ ] 实现动画服务
- [ ] 实现音频服务
- [ ] 添加游戏音效
- [ ] 添加背景音乐

### Phase 7: 优化和测试 (Week 11)
- [ ] 性能优化
- [ ] 网络优化
- [ ] 单元测试
- [ ] 集成测试
- [ ] 网络测试
- [ ] Bug修复

### Phase 8: 打包和发布 (Week 12)
- [ ] 多平台构建
- [ ] 应用商店准备
- [ ] 文档完善
- [ ] 版本发布

---

## 11. 总结

本设计文档提供了一个可扩展、高性能的Unity多人在线Bingo游戏架构，具有以下支持多玩法的核心特点：

1. **模板方法模式**: 定义玩法框架，子类实现具体逻辑
2. **抽象工厂模式**: 根据玩法类型创建对应实例
3. **模块化设计**: 各系统独立，易于维护和扩展
4. **异步优化**: 使用UniTask优化异步操作
5. **流畅动画**: 使用DOTween实现丰富的动画效果
6. **可扩展性**: 通过接口和策略模式支持自定义功能
7. **事件驱动**: 解耦模块间通信
8. **数据持久化**: 支持游戏进度保存
9. **跨平台**: 支持多平台部署
10. **多人在线**: 支持实时多人对战
11. **服务器控制**: 数字由服务器统一下发，确保公平性
12. **实时同步**: 游戏状态实时同步到所有玩家
13. **WebSocket通信**: 使用WebSocket实现高效的双向通信
14. **断线重连**: 支持网络断线后的自动重连
15. **多玩法支持**: 通过模板方法模式支持多种玩法

该架构为后续功能扩展提供了良好的基础，开发者可以轻松添加新的玩法模式、胜利条件、动画效果和游戏道具。网络系统设计支持多种网络协议，可根据需求切换不同的网络实现。
