# Unity Bingo Game 设计文档

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
- **架构模式**: MVVM + 事件驱动 + 依赖注入

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
├── Systems/                 # 游戏系统
│   ├── GameSystem/         # 游戏主系统
│   ├── BoardSystem/        # 棋盘系统
│   ├── NumberSystem/       # 数字系统
│   ├── WinConditionSystem/ # 胜利条件系统
│   ├── NetworkSystem/      # 网络系统
│   └── ItemSystem/        # 道具系统
├── Services/                # 服务层
│   ├── AudioService/       # 音频服务
│   ├── AnimationService/   # 动画服务
│   ├── DataService/        # 数据服务
│   ├── ConfigService/      # 配置服务
│   ├── NetworkService/     # 网络服务
│   └── ItemService/       # 道具服务
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
| 工厂模式 | 数字生成 | 灵活创建数字对象 |
| 依赖注入 | 服务注入 | 降低耦合度 |
| 模板方法模式 | 玩法系统 | 定义玩法框架，子类实现具体逻辑 |
| 抽象工厂模式 | 玩法创建 | 根据玩法类型创建对应实例 |

---

## 3. 核心模块设计

### 3.1 游戏状态管理

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

### 3.2 棋盘系统

#### 3.2.1 数据模型

```csharp
public class BingoCell
{
    public int Number { get; set; }
    public bool IsMarked { get; set; }
    public Vector2Int Position { get; set; }
    public bool IsFreeSpace { get; set; }
}

public class BingoBoard
{
    public const int BoardSize = 5;
    private BingoCell[,] cells;
    
    public BingoCell GetCell(int row, int col);
    public void MarkCell(int row, int col);
    public bool IsCellMarked(int row, int col);
    public BingoCell[,] GetAllCells();
    public void Reset();
}
```

#### 3.2.2 视图模型

```csharp
public class BingoBoardViewModel
{
    private BingoBoard model;
    private IBingoBoardView view;
    
    public UniTask InitializeAsync();
    public UniTask MarkCellAsync(int row, int col);
    public UniTask HighlightCellAsync(int row, int col);
    public UniTask ResetBoardAsync();
}
```

### 3.3 割草Bingo系统

#### 3.3.1 割草Bingo数据模型

```csharp
public enum GrassColor
{
    Red,
    Blue,
    Green,
    Yellow,
    Purple
}

public class GrassCell
{
    public GrassColor Color { get; set; }
    public bool IsHarvested { get; set; }
    public Vector2Int Position { get; set; }
    public float GrowthProgress { get; set; }
    public bool HasCar { get; set; }
    public bool HasKey { get; set; }
}

public class HarvestBoard
{
    public const int BoardSize = 5;
    private GrassCell[,] cells;
    private Dictionary<GrassColor, int> carCounts;
    private Dictionary<GrassColor, int> keyCounts;
    
    public GrassCell GetCell(int row, int col);
    public void HarvestCell(int row, int col);
    public bool IsCellHarvested(int row, int col);
    public GrassCell[,] GetAllCells();
    public void Reset();
    public bool IsAllHarvested();
    public void AddCar(GrassColor color);
    public void AddKey(GrassColor color);
    public bool HasCar(GrassColor color);
    public bool HasKey(GrassColor color);
    public int GetCarCount(GrassColor color);
    public int GetKeyCount(GrassColor color);
}
```

#### 3.3.2 割草Bingo视图模型

```csharp
public class HarvestBoardViewModel
{
    private HarvestBoard model;
    private IHarvestBoardView view;
    
    public UniTask InitializeAsync();
    public UniTask HarvestCellAsync(int row, int col);
    public UniTask PlayHarvestAnimationAsync(int row, int col);
    public UniTask PlayCarRefuelAnimationAsync(GrassColor color);
    public UniTask PlayColumnHarvestAnimationAsync(int col);
    public UniTask ResetBoardAsync();
}
```

#### 3.3.3 割草Bingo道具系统

```csharp
public enum HarvestItemType
{
    Gasoline,           // 汽油：给对应颜色的小车加油
    Key,               // 钥匙：直接收割对应颜色的整列
    SpeedBoost,          // 加速：加快割草动画速度
    DoubleReward,        // 双倍奖励：获得双倍分数
    AutoHarvest,         // 自动收割：自动收割一个格子
    MysteryBox          // 神秘箱：随机获得道具
}

[Serializable]
public class HarvestItemData
{
    public string itemId;
    public string itemName;
    public string description;
    public HarvestItemType itemType;
    public GrassColor? targetColor;
    public Sprite icon;
    public int cost;
    public int maxStack;
    public float cooldown;
    public bool isConsumable;
}

[Serializable]
public class HarvestInventoryItem
{
    public string itemId;
    public int count;
    public DateTime lastUsedTime;
}

public class HarvestItemService
{
    private Dictionary<string, HarvestItemData> itemConfigs;
    private Dictionary<string, HarvestInventoryItem> inventory;
    
    public async UniTask InitializeAsync();
    public async UniTask<bool> UseItemAsync(string itemId);
    public async UniTask<bool> AddItemAsync(string itemId, int count);
    public HarvestItemData GetItem(string itemId);
    public int GetItemCount(string itemId);
    
    private async UniTask HandleGasolineAsync(string itemId)
    {
        var itemData = GetItem(itemId);
        
        if (itemData.itemType == HarvestItemType.Gasoline && itemData.targetColor.HasValue)
        {
            var color = itemData.targetColor.Value;
            await HarvestBoard.Instance.AddCarAsync(color);
            await AnimationService.Instance.PlayCarRefuelAnimationAsync(color);
        }
    }
    
    private async UniTask HandleKeyAsync(string itemId)
    {
        var itemData = GetItem(itemId);
        
        if (itemData.itemType == HarvestItemType.Key && itemData.targetColor.HasValue)
        {
            var color = itemData.targetColor.Value;
            await HarvestBoard.Instance.AddKeyAsync(color);
            
            var board = HarvestBoard.Instance;
            for (int row = 0; row < HarvestBoard.BoardSize; row++)
            {
                var cell = board.GetCell(row, 0);
                if (cell.Color == color && !cell.IsHarvested)
                {
                    await HarvestBoard.Instance.HarvestColumnAsync(0);
                    break;
                }
            }
        }
    }
    
    private async UniTask HandleAutoHarvestAsync(string itemId)
    {
        var board = HarvestBoard.Instance;
        var unharvestedCells = board.GetAllCells()
            .Where(c => !c.IsHarvested)
            .ToList();
        
        if (unharvestedCells.Count > 0)
        {
            var randomCell = unharvestedCells[Random.Range(0, unharvestedCells.Count)];
            await HarvestBoard.Instance.HarvestCellAsync(randomCell.Position.x, randomCell.Position.y);
        }
    }
}
```

#### 3.3.4 割草Bingo网络同步

```csharp
[Serializable]
public class CellHarvestedMessage
{
    public string playerId;
    public int row;
    public int col;
    public GrassColor color;
}

[Serializable]
public class CarRefueledMessage
{
    public string playerId;
    public GrassColor color;
}

[Serializable]
public class ColumnHarvestedMessage
{
    public string playerId;
    public int col;
    public GrassColor color;
}

[Serializable]
public class ItemObtainedMessage
{
    public string playerId;
    public string itemId;
    public int count;
}

public class HarvestNetworkService
{
    public async UniTask SendCellHarvestedAsync(int row, int col, GrassColor color)
    {
        var data = new CellHarvestedMessage
        {
            playerId = NetworkManager.Instance.PlayerId,
            row = row,
            col = col,
            color = color
        };
        
        var message = new NetworkMessage
        {
            type = MessageType.CellHarvested,
            senderId = NetworkManager.Instance.PlayerId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            data = JsonUtility.ToJson(data)
        };
        
        await NetworkManager.Instance.SendMessageAsync(message);
    }
    
    public async UniTask SendCarRefueledAsync(GrassColor color)
    {
        var data = new CarRefueledMessage
        {
            playerId = NetworkManager.Instance.PlayerId,
            color = color
        };
        
        var message = new NetworkMessage
        {
            type = MessageType.CarRefueled,
            senderId = NetworkManager.Instance.PlayerId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            data = JsonUtility.ToJson(data)
        };
        
        await NetworkManager.Instance.SendMessageAsync(message);
    }
    
    public async UniTask SendColumnHarvestedAsync(int col, GrassColor color)
    {
        var data = new ColumnHarvestedMessage
        {
            playerId = NetworkManager.Instance.PlayerId,
            col = col,
            color = color
        };
        
        var message = new NetworkMessage
        {
            type = MessageType.ColumnHarvested,
            senderId = NetworkManager.Instance.PlayerId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            data = JsonUtility.ToJson(data)
        };
        
        await NetworkManager.Instance.SendMessageAsync(message);
    }
}
```

### 3.4 数字系统

```csharp
public interface INumberGenerator
{
    UniTask<int[]> GenerateNumbersAsync(int count, int min, int max);
}

public class RandomNumberGenerator : INumberGenerator
{
    public async UniTask<int[]> GenerateNumbersAsync(int count, int min, int max);
}

public class BingoNumberSystem
{
    private INumberGenerator numberGenerator;
    private Queue<int> calledNumbers;
    private HashSet<int> availableNumbers;
    
    public UniTask<int> CallNextNumberAsync();
    public UniTask<int[]> GenerateBoardNumbersAsync();
    public bool HasNumberBeenCalled(int number);
}
```

### 3.4 胜利条件系统

```csharp
public interface IWinCondition
{
    string Name { get; }
    bool CheckWin(BingoBoard board);
    UniTask HighlightWinningCellsAsync(BingoBoard board);
}

public class RowWinCondition : IWinCondition
{
    public string Name => "行连线";
    public bool CheckWin(BingoBoard board);
    public async UniTask HighlightWinningCellsAsync(BingoBoard board);
}

public class ColumnWinCondition : IWinCondition
{
    public string Name => "列连线";
    public bool CheckWin(BingoBoard board);
    public async UniTask HighlightWinningCellsAsync(BingoBoard board);
}

public class DiagonalWinCondition : IWinCondition
{
    public string Name => "对角线";
    public bool CheckWin(BingoBoard board);
    public async UniTask HighlightWinningCellsAsync(BingoBoard board);
}

public class FourCornersWinCondition : IWinCondition
{
    public string Name => "四角";
    public bool CheckWin(BingoBoard board);
    public async UniTask HighlightWinningCellsAsync(BingoBoard board);
}

public class XPatternWinCondition : IWinCondition
{
    public string Name => "X形";
    public bool CheckWin(BingoBoard board);
    public async UniTaskTask HighlightWinningCellsAsync(BingoBoard board);
}

public class WinConditionChecker
{
    private List<IWinCondition> conditions;
    
    public void AddCondition(IWinCondition condition);
    public void RemoveCondition(IWinCondition condition);
    public async UniTask<WinResult> CheckWinAsync(BingoBoard board);
}

public class WinResult
{
    public bool IsWin { get; set; }
    public List<IWinCondition> WinningConditions { get; set; }
    public List<Vector2Int> WinningCells { get; set; }
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
    LuckyNumber,          // 幸运数字：指定数字更容易出现
    Shield,               // 护盾：防止负面效果
    SpeedUp               // 加速：加快数字抽取速度
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

#### 3.5.3 道具配置

```csharp
[CreateAssetMenu(fileName = "ItemConfig", menuName = "Bingo/Item Config")]
public class ItemConfig : ScriptableObject
{
    public List<ItemData> items;
    
    public ItemData GetItem(string itemId);
    public List<ItemData> GetItemsByType(ItemType type);
    public List<ItemData> GetItemsByRarity(ItemRarity rarity);
}
```

#### 3.5.4 道具服务

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
            { ItemType.LuckyNumber, HandleLuckyNumber },
            { ItemType.Shield, HandleShield },
            { ItemType.SpeedUp, HandleSpeedUp }
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
            .Where(c => !c.IsMarked && !c.IsFreeSpace)
            .ToList();
        
        if (unmarkedCells.Count > 0)
        {
            var randomCell = unmarkedCells[Random.Range(0, unmarkedCells.Count)];
            await BoardSystem.Instance.MarkCellAsync(randomCell.Position.x, randomCell.Position.y);
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
    
    private async UniTask HandleFreeDaub(string playerId)
    {
        GameEvents.OnFreeDaubActivated?.Invoke(playerId);
    }
    
    private async UniTask HandleTreasureChest(string playerId)
    {
        var rewards = new List<string> { "coins", "gems", "items" };
        var reward = rewards[Random.Range(0, rewards.Count)];
        
        GameData.Instance.AddReward(reward, Random.Range(1, 10));
        GameEvents.OnTreasureOpened?.Invoke(playerId, reward);
    }
    
    private async UniTask HandleMagicDaub(string playerId)
    {
        GameEvents.OnMagicDaubActivated?.Invoke(playerId);
    }
    
    private async UniTask HandleLuckyNumber(string playerId)
    {
        var luckyNumber = Random.Range(1, 76);
        GameEvents.OnLuckyNumberSet?.Invoke(playerId, luckyNumber);
    }
    
    private async UniTask HandleShield(string playerId)
    {
        var effect = new ActiveItemEffect
        {
            itemId = "shield",
            playerId = playerId,
            startTime = Time.time,
            duration = 60f
        };
        
        activeEffects.Add(effect);
        GameEvents.OnShieldActivated?.Invoke(playerId);
    }
    
    private async UniTask HandleSpeedUp(string playerId)
    {
        if (NetworkManager.Instance.IsHost)
        {
            GameEvents.OnSpeedUpRequested?.Invoke();
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

#### 3.5.5 道具UI

```csharp
public class ItemSlotView : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemCount;
    [SerializeField] private Button useButton;
    [SerializeField] private Image cooldownOverlay;
    
    private string itemId;
    private ItemData itemData;
    
    public void Initialize(string itemId, ItemData data)
    {
        this.itemId = itemId;
        this.itemData = data;
        
        itemIcon.sprite = data.icon;
        UpdateCount();
        
        useButton.onClick.AddListener(OnUseButtonClicked);
    }
    
    public void UpdateCount()
    {
        var inventory = ItemService.Instance.GetInventoryAsync().GetAwaiter().GetResult();
        var item = inventory.FirstOrDefault(i => i.itemId == itemId);
        
        if (item != null)
        {
            itemCount.text = item.count.ToString();
            useButton.interactable = item.count > 0;
        }
        else
        {
            itemCount.text = "0";
            useButton.interactable = false;
        }
    }
    
    private async void OnUseButtonClicked()
    {
        var success = await ItemService.Instance.UseItemAsync(itemId);
        
        if (success)
        {
            UpdateCount();
            await AnimationService.Instance.PlayItemUseAnimationAsync(transform);
        }
    }
}

public class ItemBarView : MonoBehaviour
{
    [SerializeField] private Transform itemSlotsContainer;
    [SerializeField] private GameObject itemSlotPrefab;
    
    private List<ItemSlotView> itemSlots;
    
    private void Start()
    {
        itemSlots = new List<ItemSlotView>();
        InitializeItemSlots();
    }
    
    private void InitializeItemSlots()
    {
        var config = Resources.Load<ItemConfig>("ItemConfig");
        var items = config.items.Where(i => i.isActiveInGame).ToList();
        
        foreach (var item in items)
        {
            var slot = Instantiate(itemSlotPrefab, itemSlotsContainer)
                .GetComponent<ItemSlotView>();
            
            slot.Initialize(item.itemId, item);
            itemSlots.Add(slot);
        }
    }
    
    public void RefreshAllSlots()
    {
        foreach (var slot in itemSlots)
        {
            slot.UpdateCount();
        }
    }
}
```

#### 3.5.6 道具网络同步

```csharp
[Serializable]
public class ItemUsedMessage
{
    public string playerId;
    public string itemId;
    public float timestamp;
}

[Serializable]
public class ItemEffectActivatedMessage
{
    public string playerId;
    public string itemId;
    public float duration;
    public Dictionary<string, object> parameters;
}

public class NetworkItemService
{
    public async UniTask SendItemUsedAsync(string itemId)
    {
        var data = new ItemUsedMessage
        {
            playerId = NetworkManager.Instance.PlayerId,
            itemId = itemId,
            timestamp = Time.time
        };
        
        var message = new NetworkMessage
        {
            type = MessageType.ItemUsed,
            senderId = NetworkManager.Instance.PlayerId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            data = JsonUtility.ToJson(data)
        };
        
        await NetworkManager.Instance.SendMessageAsync(message);
    }
    
    public async UniTask BroadcastItemEffectAsync(string playerId, string itemId, float duration, Dictionary<string, object> parameters)
    {
        var data = new ItemEffectActivatedMessage
        {
            playerId = playerId,
            itemId = itemId,
            duration = duration,
            parameters = parameters
        };
        
        var message = new NetworkMessage
        {
            type = MessageType.ItemEffectActivated,
            senderId = NetworkManager.Instance.PlayerId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            data = JsonUtility.ToJson(data)
        };
        
        await NetworkManager.Instance.SendMessageAsync(message);
    }
}
```

### 3.6 网络系统设计

#### 3.6.1 网络协议定义

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

[Serializable]
public class ConnectRequest
{
    public string playerId;
    public string playerName;
    public string roomCode;
}

[Serializable]
public class RoomJoinedResponse
{
    public string roomId;
    public string roomCode;
    public List<PlayerInfo> players;
    public bool isHost;
}

[Serializable]
public class NumberCalledMessage
{
    public int number;
    public int callIndex;
}

[Serializable]
public class CellMarkedMessage
{
    public string playerId;
    public int number;
    public int row;
    public int col;
}

[Serializable]
public class PlayerWinMessage
{
    public string playerId;
    public string playerName;
    public List<string> winConditions;
    public List<Vector2Int> winningCells;
}

[Serializable]
public class PlayerInfo
{
    public string playerId;
    public string playerName;
    public bool isHost;
    public bool isReady;
    public int score;
}

[Serializable]
public class GameStateSyncMessage
{
    public List<int> calledNumbers;
    public int currentCallIndex;
    public Dictionary<string, PlayerBoardState> playerBoards;
}

[Serializable]
public class PlayerBoardState
{
    public string playerId;
    public List<CellData> markedCells;
}
```

#### 3.6.2 网络服务接口

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

#### 3.6.3 WebSocket网络服务实现

```csharp
public class WebSocketNetworkService : INetworkService
{
    private ClientWebSocket webSocket;
    private string serverUrl;
    private string playerId;
    private string roomId;
    private bool isHost;
    private CancellationTokenSource cts;
    
    public bool IsConnected => webSocket?.State == WebSocketState.Open;
    public bool IsHost => isHost;
    public string PlayerId => playerId;
    public string RoomId => roomId;
    
    public event Action<NetworkMessage> OnMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnError;
    
    public async UniTask ConnectAsync(string serverUrl)
    {
        this.serverUrl = serverUrl;
        webSocket = new ClientWebSocket();
        cts = new CancellationTokenSource();
        
        await webSocket.ConnectAsync(new Uri(serverUrl), cts.Token);
        
        playerId = GeneratePlayerId();
        
        OnConnected?.Invoke();
        
        _ = ReceiveMessagesAsync();
    }
    
    public async UniTask JoinRoomAsync(string roomCode, string playerName)
    {
        var request = new ConnectRequest
        {
            playerId = playerId,
            playerName = playerName,
            roomCode = roomCode
        };
        
        var message = new NetworkMessage
        {
            type = MessageType.JoinRoom,
            senderId = playerId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            data = JsonUtility.ToJson(request)
        };
        
        await SendMessageAsync(message);
    }
    
    public async UniTask SendMessageAsync(NetworkMessage message)
    {
        if (!IsConnected)
        {
            OnError?.Invoke("Not connected to server");
            return;
        }
        
        string json = JsonUtility.ToJson(message);
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            cts.Token
        );
    }
    
    private async UniTaskVoid ReceiveMessagesAsync()
    {
        var buffer = new byte[4096];
        
        try
        {
            while (IsConnected && !cts.Token.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cts.Token
                );
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await DisconnectAsync();
                    return;
                }
                
                string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var message = JsonUtility.FromJson<NetworkMessage>(json);
                
                HandleMessage(message);
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke(ex.Message);
        }
    }
    
    private void HandleMessage(NetworkMessage message)
    {
        switch (message.type)
        {
            case MessageType.RoomJoined:
                var roomData = JsonUtility.FromJson<RoomJoinedResponse>(message.data);
                roomId = roomData.roomId;
                isHost = roomData.isHost;
                break;
        }
        
        OnMessageReceived?.Invoke(message);
    }
    
    public async UniTask DisconnectAsync()
    {
        if (webSocket?.State == WebSocketState.Open)
        {
            await webSocket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Disconnecting",
                CancellationToken.None
            );
        }
        
        cts?.Cancel();
        webSocket?.Dispose();
        
        OnDisconnected?.Invoke();
    }
    
    private string GeneratePlayerId()
    {
        return Guid.NewGuid().ToString("N");
    }
}
```

#### 3.6.4 网络管理器

```csharp
public class NetworkManager : MonoBehaviour
{
    private INetworkService networkService;
    private Dictionary<MessageType, List<Action<NetworkMessage>>> messageHandlers;
    
    public bool IsConnected => networkService?.IsConnected ?? false;
    public bool IsHost => networkService?.IsHost ?? false;
    
    private void Awake()
    {
        messageHandlers = new Dictionary<MessageType, List<Action<NetworkMessage>>>();
        
        networkService = new WebSocketNetworkService();
        networkService.OnMessageReceived += HandleMessage;
        networkService.OnConnected += OnConnected;
        networkService.OnDisconnected += OnDisconnected;
        networkService.OnError += OnError;
    }
    
    public async UniTask ConnectAsync(string serverUrl)
    {
        await networkService.ConnectAsync(serverUrl);
    }
    
    public async UniTask JoinRoomAsync(string roomCode, string playerName)
    {
        await networkService.JoinRoomAsync(roomCode, playerName);
    }
    
    public void RegisterHandler(MessageType type, Action<NetworkMessage> handler)
    {
        if (!messageHandlers.ContainsKey(type))
        {
            messageHandlers[type] = new List<Action<NetworkMessage>>();
        }
        
        messageHandlers[type].Add(handler);
    }
    
    public void UnregisterHandler(MessageType type, Action<NetworkMessage> handler)
    {
        if (messageHandlers.ContainsKey(type))
        {
            messageHandlers[type].Remove(handler);
        }
    }
    
    private void HandleMessage(NetworkMessage message)
    {
        if (messageHandlers.TryGetValue(message.type, out var handlers))
        {
            foreach (var handler in handlers)
            {
                handler?.Invoke(message);
            }
        }
    }
    
    public async UniTask SendNumberCalledAsync(int number, int callIndex)
    {
        var data = new NumberCalledMessage
        {
            number = number,
            callIndex = callIndex
        };
        
        var message = new NetworkMessage
        {
            type = MessageType.NumberCalled,
            senderId = networkService.PlayerId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            data = JsonUtility.ToJson(data)
        };
        
        await networkService.SendMessageAsync(message);
    }
    
    public async UniTask SendCellMarkedAsync(int number, int row, int col)
    {
        var data = new CellMarkedMessage
        {
            playerId = networkService.PlayerId,
            number = number,
            row = row,
            col = col
        };
        
        var message = new NetworkMessage
        {
            type = MessageType.CellMarked,
            senderId = networkService.PlayerId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            data = JsonUtility.ToJson(data)
        };
        
        await networkService.SendMessageAsync(message);
    }
    
    public async UniTask SendPlayerWinAsync(List<string> winConditions, List<Vector2Int> winningCells)
    {
        var data = new PlayerWinMessage
        {
            playerId = networkService.PlayerId,
            winConditions = winConditions,
            winningCells = winningCells
        };
        
        var message = new NetworkMessage
        {
            type = MessageType.PlayerWin,
            senderId = networkService.PlayerId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            data = JsonUtility.ToJson(data)
        };
        
        await networkService.SendMessageAsync(message);
    }
    
    private void OnConnected()
    {
        Debug.Log("Connected to server");
        NetworkEvents.OnConnected?.Invoke();
    }
    
    private void OnDisconnected()
    {
        Debug.Log("Disconnected from server");
        NetworkEvents.OnDisconnected?.Invoke();
    }
    
    private void OnError(string error)
    {
        Debug.LogError($"Network error: {error}");
        NetworkEvents.OnError?.Invoke(error);
    }
    
    private void OnDestroy()
    {
        networkService?.DisconnectAsync().Forget();
    }
}
```

#### 3.6.5 网络事件

```csharp
public static class NetworkEvents
{
    public static event Action OnConnected;
    public static event Action OnDisconnected;
    public static event Action<string> OnError;
    public static event Action<RoomJoinedResponse> OnRoomJoined;
    public static event Action<NumberCalledMessage> OnNumberCalled;
    public static event Action<CellMarkedMessage> OnCellMarked;
    public static event Action<PlayerWinMessage> OnPlayerWin;
    public static event Action<List<PlayerInfo>> OnPlayerListUpdated;
    public static event Action<GameStateSyncMessage> OnGameStateSynced;
    
    public static void NotifyConnected() => OnConnected?.Invoke();
    public static void NotifyDisconnected() => OnDisconnected?.Invoke();
    public static void NotifyError(string error) => OnError?.Invoke(error);
    public static void NotifyRoomJoined(RoomJoinedResponse data) => OnRoomJoined?.Invoke(data);
    public static void NotifyNumberCalled(NumberCalledMessage data) => OnNumberCalled?.Invoke(data);
    public static void NotifyCellMarked(CellMarkedMessage data) => OnCellMarked?.Invoke(data);
    public static void NotifyPlayerWin(PlayerWinMessage data) => OnPlayerWin?.Invoke(data);
    public static void NotifyPlayerListUpdated(List<PlayerInfo> players) => OnPlayerListUpdated?.Invoke(players);
    public static void NotifyGameStateSynced(GameStateSyncMessage data) => OnGameStateSynced?.Invoke(data);
}
```

---

## 4. 服务器端设计

### 4.1 服务器架构

```
BingoServer/
├── Core/                    # 核心系统
│   ├── Models/             # 数据模型
│   └── Controllers/       # 控制器
├── Services/                # 服务层
│   ├── RoomService/        # 房间服务
│   ├── PlayerService/      # 玩家服务
│   ├── GameService/        # 游戏服务
│   └── WebSocketService/   # WebSocket服务
├── Handlers/               # 消息处理器
│   ├── ConnectionHandler/   # 连接处理
│   ├── RoomHandler/        # 房间处理
│   └── GameHandler/       # 游戏处理
└── Utilities/               # 工具类
    ├── Extensions/         # 扩展方法
    └── Helpers/            # 辅助类
```

### 4.2 房间管理

```csharp
public class Room
{
    public string RoomId { get; set; }
    public string RoomCode { get; set; }
    public Dictionary<string, Player> Players { get; set; }
    public GameState GameState { get; set; }
    public Queue<int> NumberPool { get; set; }
    public List<int> CalledNumbers { get; set; }
    public string HostId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public bool IsFull => Players.Count >= MaxPlayers;
    public bool IsGameStarted => GameState != GameState.None && GameState != GameState.Ready;
    
    public void AddPlayer(Player player);
    public void RemovePlayer(string playerId);
    public Player GetPlayer(string playerId);
    public bool HasPlayer(string playerId);
    public void StartGame();
    public int CallNextNumber();
    public void ResetGame();
}

public class RoomService
{
    private Dictionary<string, Room> rooms;
    private Dictionary<string, string> roomCodeToId;
    private int maxRooms = 100;
    private int maxPlayersPerRoom = 10;
    
    public Room CreateRoom(string hostId, string hostName);
    public Room JoinRoom(string roomCode, string playerId, string playerName);
    public void LeaveRoom(string roomId, string playerId);
    public Room GetRoom(string roomId);
    public Room GetRoomByCode(string roomCode);
    public void RemoveRoom(string roomId);
    public List<Room> GetAllRooms();
    public string GenerateRoomCode();
}
```

### 4.3 游戏服务

```csharp
public class GameService
{
    private RoomService roomService;
    private INumberGenerator numberGenerator;
    
    public async UniTask StartGameAsync(string roomId);
    public async UniTask CallNumberAsync(string roomId);
    public async UniTask MarkCellAsync(string roomId, string playerId, int number, int row, int col);
    public async UniTask CheckWinAsync(string roomId, string playerId);
    public async UniTask EndGameAsync(string roomId);
    
    private async UniTask BroadcastToRoomAsync(string roomId, NetworkMessage message);
    private async UniTask SendToPlayerAsync(string playerId, NetworkMessage message);
    private int[] GenerateBoardNumbers();
}

public class NumberGenerator
{
    private Random random;
    
    public int[] GenerateNumbers(int count, int min, int max);
    public int[] ShuffleNumbers(int[] numbers);
}
```

### 4.4 WebSocket服务器

```csharp
public class WebSocketServer
{
    private HttpListener httpListener;
    private Dictionary<string, WebSocket> connections;
    private Dictionary<string, string> connectionIdToPlayerId;
    private CancellationTokenSource cts;
    
    public event Action<string, NetworkMessage> OnMessageReceived;
    public event Action<string> OnConnected;
    public event Action<string> OnDisconnected;
    
    public async UniTask StartAsync(string url);
    public async UniTask StopAsync();
    public async UniTask SendAsync(string playerId, NetworkMessage message);
    public async UniTask BroadcastAsync(List<string> playerIds, NetworkMessage message);
    
    private async UniTask HandleConnectionAsync(WebSocket webSocket, string connectionId);
    private async UniTask ReceiveMessagesAsync(WebSocket webSocket, string connectionId);
    private string GenerateConnectionId();
}

public class WebSocketServerService
{
    private WebSocketServer server;
    private RoomService roomService;
    private GameService gameService;
    private Dictionary<MessageType, Func<string, NetworkMessage, UniTask>> messageHandlers;
    
    public async UniTask StartAsync(string url);
    public async UniTask StopAsync();
    
    private void RegisterHandlers();
    private async UniTask HandleMessageAsync(string playerId, NetworkMessage message);
    
    private async UniTask HandleJoinRoomAsync(string playerId, NetworkMessage message);
    private async UniTask HandleLeaveRoomAsync(string playerId, NetworkMessage message);
    private async UniTask HandleStartGameAsync(string playerId, NetworkMessage message);
    private async UniTask HandleCellMarkedAsync(string playerId, NetworkMessage message);
    private async UniTask HandlePlayerWinAsync(string playerId, NetworkMessage message);
}
```

### 4.5 消息处理流程

```
客户端发送消息
    ↓
WebSocketServer接收
    ↓
解析消息类型
    ↓
路由到对应处理器
    ↓
处理器执行业务逻辑
    ↓
更新游戏状态
    ↓
广播消息到房间内所有玩家
    ↓
客户端接收并更新UI
```

### 4.6 游戏流程（服务器端）

```
1. 玩家加入房间 (PlayerJoinRoom)
   ↓
2. 等待所有玩家准备 (WaitForAllPlayersReady)
   ↓
3. 房主点击开始游戏 (HostStartGame)
   ↓
4. 生成数字池 (GenerateNumberPool)
   ↓
5. 为每个玩家生成棋盘数字 (GenerateBoardNumbersForPlayers)
   ↓
6. 发送游戏开始消息 (SendGameStartMessage)
   ↓
7. 循环:
   a. 从数字池中抽取数字 (DrawNumberFromPool)
   b. 广播数字到所有玩家 (BroadcastNumber)
   c. 等待玩家标记 (WaitForPlayerMark)
   d. 检查玩家是否胜利 (CheckPlayerWin)
   e. 如果有玩家胜利:
      - 广播胜利消息 (BroadcastWinMessage)
      - 结束游戏 (EndGame)
   f. 如果数字池为空:
      - 广播平局消息 (BroadcastDrawMessage)
      - 结束游戏 (EndGame)
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
    void StopAllAnimations();
}

public class DOTweenAnimationService : IAnimationService
{
    private AnimationConfig config;
    
    public async UniTask PlayCellMarkAnimationAsync(Transform cellTransform)
    {
        await cellTransform.DOScale(1.2f, 0.1f).ToUniTask();
        await cellTransform.DOScale(1f, 0.1f).ToUniTask();
        await cellTransform.DOColor(config.markedColor, 0.2f).ToUniTask();
    }
    
    public async UniTask PlayNumberCallAnimationAsync(int number)
    {
        var sequence = DOTween.Sequence();
        sequence.Append(numberDisplay.DOFade(0, 0.1f));
        sequence.Append(numberDisplay.DOFade(1, 0.3f));
        sequence.Append(numberDisplay.transform.DOScale(1.5f, 0.2f));
        sequence.Append(numberDisplay.transform.DOScale(1f, 0.2f));
        await sequence.ToUniTask();
    }
    
    public async UniTask PlayWinAnimationAsync(List<Transform> winningCells)
{
        var sequence = DOTween.Sequence();
        
        foreach (var cell in winningCells)
        {
            sequence.Join(cell.DOColor(config.winColor, 0.3f));
            sequence.Join(cell.DOScale(1.3f, 0.3f));
        }
        
        await sequence.ToUniTask();
        
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        
        foreach (var cell in winningCells)
        {
            await cell.DOScale(1f, 0.2f).ToUniTask();
        }
    }
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

public enum SoundType
{
    CellMark,
    NumberCall,
    Win,
    Lose,
    ButtonClick
}

public enum MusicType
{
    Background,
    Win,
    Lose
}

public class AudioService : IAudioService
{
    private Dictionary<SoundType, AudioClip> soundClips;
    private Dictionary<MusicType, AudioClip> musicClips;
    private AudioSource musicSource;
    private AudioSource sfxSource;
    
    public async UniTask PlaySoundAsync(SoundType soundType)
    {
        if (soundClips.TryGetValue(soundType, out var clip))
        {
            sfxSource.PlayOneShot(clip);
            await UniTask.Delay(TimeSpan.FromSeconds(clip.length));
        }
    }
}
```

### 5.3 配置服务

```csharp
[CreateAssetMenu(fileName = "BingoGameConfig", menuName = "Bingo/Game Config")]
public class BingoGameConfig : ScriptableObject
{
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
    
    public static void NotifyGameStateChanged(GameState state) => OnGameStateChanged?.Invoke(state);
    public static void NotifyNumberCalled(int number) => OnNumberCalled?.Invoke(number);
    public static void NotifyCellMarked(Vector2Int position) => OnCellMarked?.Invoke(position);
    public static void NotifyWinChecked(WinResult result) => OnWinChecked?.Invoke(result);
    public static void NotifyGameReset() => OnGameReset?.Invoke();
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
    
    public static void NotifyStartButtonClicked() => OnStartButtonClicked?.Invoke();
    public static void NotifyPauseButtonClicked() => OnPauseButtonClicked?.Invoke();
    public static void NotifyResumeButtonClicked() => OnResumeButtonClicked?.Invoke();
    public static void NotifyResetButtonClicked() => OnResetButtonClicked?.Invoke();
    public static void NotifyCellClicked(int cellIndex) => OnCellClicked?.Invoke(cellIndex);
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
3. 初始化系统 (InitializeSystems)
   - BoardSystem

   - NumberSystem
   - WinConditionSystem
   - NetworkSystem
   ↓
4. 连接服务器 (ConnectToServer)
   ↓
5. 加入房间 (JoinRoom)
   ↓
6. 等待服务器发送棋盘数字 (WaitForBoardNumbers)
   ↓
7. 初始化UI (InitializeUI)
   ↓
8. 切换到Ready状态 (ChangeState: Ready)
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
   a. 服务器下发数字 (ReceiveNumberFromServer)
   b. 播放数字动画 (PlayNumberAnimation)
   c. 玩家点击对应格子 (PlayerClickCell)
   d. 发送标记请求到服务器 (SendMarkRequest)
   e. 服务器广播标记结果 (BroadcastMarkResult)
   f. 播放标记动画 (PlayMarkAnimation)
   g. 检查胜利条件 (CheckWin)
   h. 如果胜利 → 发送胜利消息到服务器 (SendWinMessage)
   i. 服务器广播胜利结果 (BroadcastWinResult)
   j. 切换到Win状态 (ChangeState: Win)
```

### 7.3 胜利检查流程

```
1. 切换到CheckingWin状态 (ChangeState: CheckingWin)
   ↓
2. 遍历所有胜利条件 (Iterate WinConditions)
   ↓
3. 对每个条件检查是否满足 (CheckCondition)
   ↓
4. 收集所有满足的条件 (Collect WinningConditions)
   ↓
5. 如果有满足的条件:
   a. 高亮胜利格子 (HighlightWinningCells)
   b. 播放胜利动画 (PlayWinAnimation)
   c. 切换到Win状态 (ChangeState: Win)
   ↓
6. 如果没有满足的条件:
   a. 切换回Playing状态 (ChangeState: Playing)
```

---

## 8. 扩展性设计

### 7.1 自定义胜利条件

开发者可以通过实现 `IWinCondition` 接口来添加自定义胜利条件：

```csharp
public class CustomPatternWinCondition : IWinCondition
{
    public string Name => "自定义模式";
    
    public bool CheckWin(BingoBoard board)
    {
        for (int row = 0; row < BingoBoard.BoardSize; row++)
        {
            for (int col = 0; col < BingoBoard.BoardSize; col++)
            {
                if (!board.IsCellMarked(row, col))
                    return false;
            }
        }
        return true;
    }
    
    public async UniTask HighlightWinningCellsAsync(BingoBoard board)
    {
        var cells = board.GetAllCells();
        foreach (var cell in cells)
        {
            await AnimationService.PlayCellMarkAnimationAsync(cell.Transform);
        }
    }
}
```

### 7.2 自定义数字生成器

开发者可以通过实现 `INumberGenerator` 接口来自定义数字生成逻辑：

```csharp
public class WeightedNumberGenerator : INumberGenerator
{
    private Dictionary<int, float> numberWeights;
    
    public async UniTask<int[]> GenerateNumbersAsync(int count, int min, int max)
    {
        var numbers = new List<int>();
        var available = new List<int>();
        
        for (int i = min; i <= max; i++)
        {
            available.Add(i);
        }
        
        for (int i = 0; i < count; i++)
        {
            var selected = SelectWeightedNumber(available);
            numbers.Add(selected);
            available.Remove(selected);
        }
        
        return numbers.ToArray();
    }
    
    private int SelectWeightedNumber(List<int> available)
    {
        float totalWeight = available.Sum(n => numberWeights.GetValueOrDefault(n, 1f));
        float random = Random.value * totalWeight;
        
        float currentWeight = 0;
        foreach (var number in available)
        {
            currentWeight += numberWeights.GetValueOrDefault(number, 1f);
            if (random <= currentWeight)
                return number;
        }
        
        return available.Last();
    }
}
```

### 7.3 自定义动画效果

开发者可以通过实现 `IAnimationService` 接口来自定义动画效果：

```csharp
public class CustomAnimationService : IAnimationService
{
    public async UniTask PlayCellMarkAnimationAsync(Transform cellTransform)
    {
        var sequence = DOTween.Sequence();
        sequence.Append(cellTransform.DORotate(new Vector3(0, 360, 0), 0.3f, RotateMode.FastBeyond360));
        sequence.Append(cellTransform.DOScale(1.2f, 0.1f));
        sequence.Append(cellTransform.DOScale(1f, 0.1f));
        await sequence.ToUniTask();
    }
}
```

---

## 8. 性能优化

### 8.1 对象池

```csharp
public class CellViewPool
{
    private Queue<CellView> pool;
    private CellView prefab;
    private Transform parent;
    
    public CellViewPool(CellView prefab, int initialSize, Transform parent)
    {
        this.prefab = prefab;
        this.parent = parent;
        pool = new Queue<CellView>();
        
        for (int i = 0; i < initialSize; i++)
        {
            var cell = Instantiate(prefab, parent);
            cell.gameObject.SetActive(false);
            pool.Enqueue(cell);
        }
    }
    
    public CellView Get()
    {
        if (pool.Count > 0)
        {
            var cell = pool.Dequeue();
            cell.gameObject.SetActive(true);
            return cell;
        }
        
        return Instantiate(prefab, parent);
    }
    
    public void Return(CellView cell)
    {
        cell.gameObject.SetActive(false);
        cell.Reset();
        pool.Enqueue(cell);
    }
}
```

### 8.2 异步优化

使用 UniTask 避免主线程阻塞：

```csharp
public async UniTask InitializeGameAsync()
{
    var tasks = new List<UniTask>
    {
        InitializeBoardAsync(),
        InitializeNumbersAsync(),
        InitializeUIAsync()
    };
    
    await UniTask.WhenAll(tasks);
}

public async UniTask CheckAllWinConditionsAsync(BingoBoard board)
{
    var checkTasks = winConditions
        .Select(condition => UniTask.Run(() => condition.CheckWin(board)))
        .ToArray();
    
    var results = await UniTask.WhenAll(checkTasks);
    
    for (int i = 0; i < results.Length; i++)
    {
        if (results[i])
        {
            return winConditions[i];
        }
    }
    
    return null;
}
```

---

## 9. 数据持久化

### 9.1 游戏数据模型

```csharp
[Serializable]
public class GameSaveData
{
    public int version;
    public long timestamp;
    public List<CellData> cells;
    public List<int> calledNumbers;
    public int score;
    public int gamesPlayed;
    public int gamesWon;
}

[Serializable]
public class CellData
{
    public int number;
    public bool isMarked;
    public int row;
    public int col;
}

public interface IDataService
{
    UniTask SaveGameAsync(GameSaveData data);
    UniTask<GameSaveData> LoadGameAsync();
    UniTask<bool> HasSaveDataAsync();
    UniTask DeleteSaveDataAsync();
}

public class JsonDataService : IDataService
{
    private string savePath;
    
    public async UniTask SaveGameAsync(GameSaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        await File.WriteAllTextAsync(savePath, json);
    }
    
    public async UniTask<GameSaveData> LoadGameAsync()
    {
        string json = await File.ReadAllTextAsync(savePath);
        return JsonUtility.FromJson<GameSaveData>(json);
    }
}
```

---

## 10. UI设计

### 10.1 UI层级结构

```
Canvas
├── MainMenu
│   ├── Title
│   ├── StartButton
│   ├── SettingsButton
│   └── QuitButton
├── GameUI
│   ├── TopBar
│   │   ├── ScoreDisplay
│   │   ├── TimerDisplay
│   │   └── PauseButton
│   ├── NumberDisplay
│   │   └── CurrentNumber
│   ├── BoardContainer
│   │   └── Cell[5x5]
│   └── BottomBar
│       ├── ResetButton
│       └── AutoMarkToggle
├── PauseMenu
│   ├── ResumeButton
│   ├── RestartButton
│   └── MainMenuButton
└── ResultScreen
    ├── ResultTitle
    ├── ScoreDisplay
    ├── WinConditionsList
    └── ContinueButton
```

### 10.2 Cell View

```csharp
public class CellView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image markIndicator;
    [SerializeField] private Button cellButton;
    
    private int cellIndex;
    private bool isInteractable = true;
    
    public void Initialize(int number, int index)
    {
        numberText.text = number.ToString();
        cellIndex = index;
        Reset();
    }
    
    public void SetMarked(bool marked)
    {
        markIndicator.gameObject.SetActive(marked);
        backgroundImage.color = marked ? markedColor : defaultColor;
    }
    
    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        cellButton.interactable = interactable;
    }
    
    public void Reset()
    {
        SetMarked(false);
        SetInteractable(true);
    }
}
```

---

## 11. 测试策略

### 11.1 单元测试

```csharp
[Test]
public void TestBingoBoard_MarkCell()
{
    var board = new BingoBoard();
    board.Initialize();
    
    board.MarkCell(0, 0);
    
    Assert.IsTrue(board.IsCellMarked(0, 0));
    Assert.IsFalse(board.IsCellMarked(0, 1));
}

[Test]
public void TestRowWinCondition_CheckWin()
{
    var board = new BingoBoard();
    board.Initialize();
    
    for (int col = 0; col < 5; col++)
    {
        board.MarkCell(0, col);
    }
    
    var condition = new RowWinCondition();
    Assert.IsTrue(condition.CheckWin(board));
}

[Test]
public async UniTask TestNumberGenerator_GenerateNumbers()
{
    var generator = new RandomNumberGenerator();
    var numbers = await generator.GenerateNumbersAsync(25, 1, 75);
    
    Assert.AreEqual(25, numbers.Length);
    Assert.AreEqual(25, numbers.Distinct().Count());
}
```

### 11.2 集成测试

```csharp
[Test]
public async UniTask TestGameFlow_CompleteGame()
{
    var gameController = CreateTestGameController();
    await gameController.InitializeAsync();
    
    gameController.StartGame();
    
    for (int i = 0; i < 25; i++)
    {
        var number = await gameController.CallNextNumberAsync();
        gameController.MarkNumber(number);
    }
    
    Assert.AreEqual(GameState.GameOver, gameController.CurrentState);
}
```

---

## 12. 部署配置

### 12.1 包依赖

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

### 12.2 构建设置

- **目标平台**: Windows, macOS, iOS, Android
- **Scripting Backend**: IL2CPP
- **API Compatibility Level**: .NET Standard 2.1
- **Minimum API Level**: Android 7.0 (API Level 24)
- **Target SDK Version**: Android 13.0 (API Level 33)

---

## 13. 开发计划

### Phase 1: 核心框架 (Week 1-2)
- [ ] 搭建项目结构
- [ ] 实现状态机
- [ ] 实现事件系统
- [ ] 实现服务容器
- [ ] 集成UniTask和DOTween

### Phase 2: 网络系统 (Week 3-4)
- [ ] 实现网络协议定义
- [ ] 实现WebSocket网络服务
- [ ] 实现网络管理器
- [ ] 实现网络事件系统
- [ ] 实现断线重连机制

### Phase 3: 游戏系统 (Week 5-6)
- [ ] 实现棋盘系统
- [ ] 实现数字系统
- [ ] 实现胜利条件系统
- [ ] 实现游戏控制器
- [ ] 集成网络同步

### Phase 4: UI系统 (Week 7-8)
- [ ] 实现主菜单
- [ ] 实现房间列表UI
- [ ] 实现游戏UI
- [ ] 实现结果界面
- [ ] 实现暂停菜单
- [ ] 实现玩家列表UI

### Phase 5: 动画和音效 (Week 9)
- [ ] 实现动画服务
- [ ] 实现音频服务
- [ ] 添加游戏音效
- [ ] 添加背景音乐

### Phase 6: 优化和测试 (Week 10)
- [ ] 性能优化
- [ ] 网络优化
- [ ] 单元测试
- [ ] 集成测试
- [ ] 网络测试
- [ ] Bug修复

### Phase 7: 打包和发布 (Week 11)
- [ ] 多平台构建
- [ ] 应用商店准备
- [ ] 文档完善
- [ ] 版本发布

---

## 14. 总结

本设计文档提供了一个可扩展、高性能的Unity多人在线Bingo游戏架构，具有以下特点：

1. **模块化设计**: 各系统独立，易于维护和扩展
2. **异步优化**: 使用UniTask优化异步操作
3. **流畅动画**: 使用DOTween实现丰富的动画效果
4. **可扩展性**: 通过接口和策略模式支持自定义功能
5. **事件驱动**: 解耦模块间通信
6. **数据持久化**: 支持游戏进度保存
7. **跨平台**: 支持多平台部署
8. **多人在线**: 支持实时多人对战
9. **服务器控制**: 数字由服务器统一下发，确保公平性
10. **实时同步**: 游戏状态实时同步到所有玩家
11. **WebSocket通信**: 使用WebSocket实现高效的双向通信
12. **断线重连**: 支持网络断线后的自动重连

该架构为后续功能扩展提供了良好的基础，开发者可以轻松添加新的胜利条件、动画效果和游戏模式。网络系统设计支持多种网络协议，可根据需求切换不同的网络实现。
