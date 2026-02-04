# Bingo Game 设计文档 v3.0

## 1. 项目概述

### 1.1 游戏简介
Bingo是一款经典的数字匹配游戏，支持多卡玩法模式。玩家需要在4张5x5的卡片上标记被呼叫的数字，当达成特定连线模式时获胜。

### 1.2 技术栈
- **Unity版本**: 2022.3 LTS 或更高
- **编程语言**: C# 10+
- **异步框架**: UniTask 2.x（仅在需要异步操作时使用）
- **动画框架**: DOTween 1.x
- **架构模式**: MVC + 策略模式 + 事件驱动 + 依赖注入

### 1.3 核心特性
- 多卡自适应支持（移动端4张卡，PC端可支持更多）
- 智能卡片生成算法（避免数字重合）
- 动态呼叫平衡系统（提升玩家爽感）
- 丰富的道具系统
- 深度UI/UX反馈系统
- 实时连线判定
- Combo系统和Miss惩罚机制

---

## 2. 架构设计

### 2.1 整体架构

```
BingoGame/
├── Core/                    # 核心系统
│   ├── Models/             # 数据模型
│   ├── Views/              # 视图层
│   ├── Controllers/       # 控制器层
│   └── Services/          # 服务层
├── Systems/                 # 游戏系统
│   ├── CardSystem/        # 卡片生成系统
│   ├── CallerSystem/      # 呼叫系统
│   ├── WinDetectionSystem/ # 连线判定系统
│   ├── PowerUpSystem/     # 道具系统
│   └── FeedbackSystem/     # 反馈系统
├── UI/                      # UI层
│   ├── CardView/          # 卡片视图
│   ├── PowerUpView/       # 道具UI
│   └── FeedbackView/       # 反馈UI
├── Utilities/               # 工具类
│   ├── Extensions/         # 扩展方法
│   └── Helpers/            # 辅助类
└── Events/                  # 事件系统
    ├── GameEvents/         # 游戏事件
    └── UIEvents/           # UI事件
```

### 2.2 设计模式应用

| 模式 | 应用场景 | 说明 |
|------|----------|------|
| MVC | 整体架构 | 分离模型、视图和控制器 |
| 单例模式 | 服务管理 | 全局服务访问 |
| 观察者模式 | 事件系统 | 解耦模块间通信 |
| 策略模式 | 呼叫平衡 | 可切换的平衡策略 |
| 对象池模式 | 动画效果 | 复用动画对象 |
| 依赖注入 | 服务注入 | 降低耦合度 |

---

## 3. 核心模块设计

### 3.1 卡片生成系统（Card Generation System）

#### 3.1.1 卡片数据模型

```csharp
public enum CardType
{
    Normal,      // 普通格：纯数字
    PaintBucket, // 道具格：油漆桶
    Coin         // 道具格：金币
}

public class BingoCard
{
    public int CardIndex { get; set; }
    public int Number { get; set; }
    public CardType CardType { get; set; }
    public bool IsMarked { get; set; }
    public bool IsPowerUpActive { get; set; }
    public bool IsPowerUpCollected { get; set; }
    public Vector2Int Position { get; set; }
}
```

#### 3.1.2 卡片生成器

```csharp
public interface ICardGenerator
{
    BingoCard[] GenerateCards(int cardCount, int minNumber, int maxNumber);
    float CalculateOverlapRate(BingoCard[] cards1, BingoCard[] cards2);
}

public class SmartCardGenerator : ICardGenerator
{
    private const int MaxOverlapRate = 80; // 最大重合率80%

    public BingoCard[] GenerateCards(int cardCount, int minNumber, int maxNumber)
    {
        var cards = new BingoCard[cardCount];
        var availableNumbers = new List<int>();

        for (int i = minNumber; i <= maxNumber; i++)
        {
            availableNumbers.Add(i);
        }

        var random = new System.Random();

        for (int i = 0; i < cardCount; i++)
        {
            int number = availableNumbers[random.Next(0, availableNumbers.Count)];
            availableNumbers.Remove(number);

            cards[i] = new BingoCard
            {
                CardIndex = i,
                Number = number,
                CardType = CardType.Normal,
                IsMarked = false,
                IsPowerUpActive = false,
                IsPowerUpCollected = false,
                Position = new Vector2Int(i / 5, i % 5)
            };
        }

        AssignPowerUpCards(cards);

        return cards;
    }

    private void AssignPowerUpCards(BingoCard[] cards)
    {
        var powerUpCount = (int)(cards.Length * 0.1); // 10%的格子是道具格
        var powerUpTypes = new List<CardType> { CardType.PaintBucket, CardType.Coin };
        var random = new System.Random();

        for (int i = 0; i < powerUpCount; i++)
        {
            int index = random.Next(0, cards.Length);
            if (cards[index].CardType == CardType.Normal)
            {
                cards[index].CardType = powerUpTypes[random.Next(0, powerUpTypes.Count)];
            }
        }
    }

    public float CalculateOverlapRate(BingoCard[] cards1, BingoCard[] cards2)
    {
        var set1 = new HashSet<int>(cards1.Select(c => c.Number));
        var set2 = new HashSet<int>(cards2.Select(c => c.Number));

        int overlapCount = 0;
        foreach (var num in set1)
        {
            if (set2.Contains(num))
                overlapCount++;
        }

        int totalUniqueNumbers = set1.Count + set2.Count;
        return (float)overlapCount / totalUniqueNumbers * 100f;
    }
}
```

#### 3.1.3 多卡管理器

```csharp
public class MultiCardManager
{
    private BingoCard[] playerCards;
    private BingoCard[] opponentCards;
    private int cardCount = 4;

    public BingoCard[] PlayerCards => playerCards;
    public BingoCard[] OpponentCards => opponentCards;

    public void InitializeCards(int cardCount)
    {
        this.cardCount = cardCount;
        var generator = new SmartCardGenerator();
        playerCards = generator.GenerateCards(cardCount, 1, 75);
        opponentCards = generator.GenerateCards(cardCount, 1, 75);

        float overlapRate = generator.CalculateOverlapRate(playerCards, opponentCards);
        if (overlapRate > MaxOverlapRate)
        {
            RegeneratePlayerCards();
        }
    }

    public void RegeneratePlayerCards()
    {
        var generator = new SmartCardGenerator();
        playerCards = generator.GenerateCards(cardCount, 1, 75);
    }
}
```

---

### 3.2 呼叫系统（Caller System）

#### 3.2.1 呼叫策略接口

```csharp
public enum CallerMode
{
    Random,       // 全随机模式
    DynamicBalancing // 动态干预模式
}

public interface ICallerStrategy
{
    int GetNextNumber(List<int> calledNumbers, List<int> remainingNumbers, Dictionary<string, object> userStatus);
}
```

#### 3.2.2 全随机策略

```csharp
public class RandomCallerStrategy : ICallerStrategy
{
    public int GetNextNumber(List<int> calledNumbers, List<int> remainingNumbers, Dictionary<string, object> userStatus)
    {
        if (remainingNumbers.Count == 0)
            return -1;

        var random = new System.Random();
        int index = random.Next(0, remainingNumbers.Count);
        return remainingNumbers[index];
    }
}
```

#### 3.2.3 动态平衡策略

```csharp
public class DynamicBalancingStrategy : ICallerStrategy
{
    private const float IdleThreshold = 30f; // 30秒无操作触发干预
    private Dictionary<string, float> lastActionTime;

    public int GetNextNumber(List<int> calledNumbers, List<int> remainingNumbers, Dictionary<string, object> userStatus)
    {
        var playerId = GetPlayerId(userStatus);
        float currentTime = Time.time;

        if (!lastActionTime.ContainsKey(playerId))
        {
            lastActionTime[playerId] = currentTime;
        }

        float timeSinceLastAction = currentTime - lastActionTime[playerId];

        if (timeSinceLastAction > IdleThreshold && remainingNumbers.Count > 0)
        {
            var playerNumbers = GetPlayerNumbers(userStatus);
            var unmarkedNumbers = remainingNumbers.Where(n => playerNumbers.Contains(n)).ToList();

            if (unmarkedNumbers.Count > 0)
            {
                var random = new System.Random();
                int index = random.Next(0, unmarkedNumbers.Count);
                lastActionTime[playerId] = currentTime;
                return unmarkedNumbers[index];
            }
        }

        var random = new System.Random();
        int index = random.Next(0, remainingNumbers.Count);
        return remainingNumbers[index];
    }

    private string GetPlayerId(Dictionary<string, object> userStatus)
    {
        return userStatus.ContainsKey("playerId") ? userStatus["playerId"].ToString() : "default";
    }

    private List<int> GetPlayerNumbers(Dictionary<string, object> userStatus)
    {
        return userStatus.ContainsKey("playerNumbers") ? 
            userStatus["playerNumbers"] as List<int> : 
            new List<int>();
    }
}
```

#### 3.2.4 呼叫管理器

```csharp
public class CallerManager
{
    private ICallerStrategy strategy;
    private List<int> allNumbers;
    private List<int> calledNumbers;
    private List<int> remainingNumbers;
    private float callInterval = 3f;

    public void Initialize(CallerMode mode)
    {
        allNumbers = new List<int>();
        for (int i = 1; i <= 75; i++)
        {
            allNumbers.Add(i);
        }

        calledNumbers = new List<int>();
        remainingNumbers = new List<int>(allNumbers);

        switch (mode)
        {
            case CallerMode.Random:
                strategy = new RandomCallerStrategy();
                break;
            case CallerMode.DynamicBalancing:
                strategy = new DynamicBalancingStrategy();
                break;
        }
    }

    public int CallNextNumber(Dictionary<string, object> userStatus)
    {
        if (remainingNumbers.Count == 0)
            return -1;

        int number = strategy.GetNextNumber(calledNumbers, remainingNumbers, userStatus);

        if (number > 0)
        {
            calledNumbers.Add(number);
            remainingNumbers.Remove(number);
        }

        return number;
    }

    public void Reset()
    {
        calledNumbers.Clear();
        remainingNumbers = new List<int>(allNumbers);
    }
}
```

---

### 3.3 连线判定系统（Win Detection System）

#### 3.3.1 连线类型定义

```csharp
public enum WinLineType
{
    Horizontal,    // 横向：5行
    Vertical,      // 纵向：5列
    Diagonal,      // 斜向：2条对角线
    FourCorners    // 特殊：四角点
}

public class WinLine
{
    public WinLineType LineType { get; set; }
    public Vector2Int[] CellPositions { get; set; }
    public bool IsComplete { get; set; }
}
```

#### 3.3.2 连线判定器

```csharp
public class WinDetectionSystem
{
    private List<WinLine> allWinLines;
    private int boardSize = 5;

    public WinDetectionSystem(int cardCount)
    {
        boardSize = (int)Math.Sqrt(cardCount);
        InitializeWinLines();
    }

    private void InitializeWinLines()
    {
        allWinLines = new List<WinLine>();

        for (int row = 0; row < boardSize; row++)
        {
            var positions = new Vector2Int[boardSize];
            for (int col = 0; col < boardSize; col++)
            {
                positions[col] = new Vector2Int(row, col);
            }
            allWinLines.Add(new WinLine
            {
                LineType = WinLineType.Horizontal,
                CellPositions = positions,
                IsComplete = false
            });
        }

        for (int col = 0; col < boardSize; col++)
        {
            var positions = new Vector2Int[boardSize];
            for (int row = 0; row < boardSize; row++)
            {
                positions[row] = new Vector2Int(row, col);
            }
            allWinLines.Add(new WinLine
            {
                LineType = WinLineType.Vertical,
                CellPositions = positions,
                IsComplete = false
            });
        }

        var diagonal1 = new Vector2Int[boardSize];
        var diagonal2 = new Vector2Int[boardSize];
        for (int i = 0; i < boardSize; i++)
        {
            diagonal1[i] = new Vector2Int(i, i);
            diagonal2[i] = new Vector2Int(i, boardSize - 1 - i);
        }
        allWinLines.Add(new WinLine
        {
            LineType = WinLineType.Diagonal,
            CellPositions = diagonal1,
            IsComplete = false
        });
        allWinLines.Add(new WinLine
        {
            LineType = WinLineType.Diagonal,
            CellPositions = diagonal2,
            IsComplete = false
        });
    }

    public bool CheckWinCondition(BingoCard[] cards)
    {
        foreach (var line in allWinLines)
        {
            bool allMarked = true;

            foreach (var pos in line.CellPositions)
            {
                int index = pos.x * boardSize + pos.y;
                if (index >= 0 && index < cards.Length)
                {
                    if (!cards[index].IsMarked)
                    {
                        allMarked = false;
                        break;
                    }
                }
            }

            line.IsComplete = allMarked;

            if (allMarked)
            {
                return true;
            }
        }

        return false;
    }

    public List<WinLine> GetCompletedLines(BingoCard[] cards)
    {
        var completedLines = new List<WinLine>();

        foreach (var line in allWinLines)
        {
            if (line.IsComplete)
            {
                completedLines.Add(line);
            }
        }

        return completedLines;
    }

    public List<WinLine> GetFourCorners(BingoCard[] cards)
    {
        var corners = new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, boardSize - 1),
            new Vector2Int(boardSize - 1, 0),
            new Vector2Int(boardSize - 1, boardSize - 1)
        };

        bool allMarked = true;
        foreach (var pos in corners)
        {
            int index = pos.x * boardSize + pos.y;
            if (index >= 0 && index < cards.Length)
            {
                if (!cards[index].IsMarked)
                {
                    allMarked = false;
                    break;
                }
            }
        }

        if (allMarked)
        {
            return new List<WinLine>
            {
                new WinLine
                {
                    LineType = WinLineType.FourCorners,
                    CellPositions = corners.ToArray(),
                    IsComplete = true
                }
            };
        }
        else
        {
            return new List<WinLine>();
        }
    }
}
```

---

### 3.4 道具系统（Power-up System）

#### 3.4.1 道具类型定义

```csharp
public enum PowerUpType
{
    FreeDaub,        // 随机标记：直接随机点亮1-2个未叫到的数字
    DoublePayout,     // 双倍收益：未来10秒内获得的金币或经验翻倍
    InstantBingo,     // 即时中奖：极其稀有，直接点亮形成一条线
    Shield            // 保护：防止因漏点或错点导致的扣分
}

public class PowerUpConfig
{
    public PowerUpType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Sprite Icon { get; set; }
    public float Cooldown { get; set; }
    public float Duration { get; set; }
    public float Rarity { get; set; } // 稀有度权重
}
```

#### 3.4.2 道具管理器

```csharp
public class PowerUpManager
{
    private Dictionary<PowerUpType, PowerUpConfig> configs;
    private Dictionary<PowerUpType, int> inventory;
    private Dictionary<PowerUpType, float> lastUsedTime;

    public void Initialize()
    {
        LoadConfigs();
        LoadInventory();
    }

    public bool CanUsePowerUp(PowerUpType type)
    {
        if (!inventory.ContainsKey(type) || inventory[type] <= 0)
            return false;

        if (configs[type].Cooldown > 0)
        {
            float timeSinceLastUse = Time.time - lastUsedTime[type];
            if (timeSinceLastUse < configs[type].Cooldown)
                return false;
        }
        }

        return true;
    }

    public void UsePowerUp(PowerUpType type, BingoCard[] cards, List<int> calledNumbers)
    {
        if (!CanUsePowerUp(type))
            return;

        switch (type)
        {
            case PowerUpType.FreeDaub:
                UseFreeDaub(cards, calledNumbers);
                break;
            case PowerUpType.DoublePayout:
                UseDoublePayout();
                break;
            case PowerUpType.InstantBingo:
                UseInstantBingo(cards);
                break;
            case PowerUpType.Shield:
                UseShield();
                break;
        }

        inventory[type]--;
        lastUsedTime[type] = Time.time;
    }

    private void UseFreeDaub(BingoCard[] cards, List<int> calledNumbers)
    {
        var unmarkedCards = cards.Where(c => !c.IsMarked).ToList();
        var unmarkedCalledNumbers = unmarkedCards.Where(c => calledNumbers.Contains(c.Number)).ToList();

        if (unmarkedCalledNumbers.Count > 0)
        {
            var random = new System.Random();
            int count = Mathf.Min(2, unmarkedCalledNumbers.Count);
            
            for (int i = 0; i < count; i++)
            {
                int index = random.Next(0, unmarkedCalledNumbers.Count);
                var card = unmarkedCalledNumbers[index];
                card.IsMarked = true;
                GameEvents.NotifyCardMarked(card.Position);
            }
        }
    }

    private void UseDoublePayout()
    {
        var effect = new ActivePowerUpEffect
        {
            Type = PowerUpType.DoublePayout,
            StartTime = Time.time,
            Duration = 10f
        };

        GameEvents.NotifyPowerUpActivated(effect);
    }

    private void UseInstantBingo(BingoCard[] cards)
    {
        var unmarkedCards = cards.Where(c => !c.IsMarked).ToList();
        var winDetection = new WinDetectionSystem(cards.Length);
        
        foreach (var card in unmarkedCards)
        {
            card.IsMarked = true;
            GameEvents.NotifyCardMarked(card.Position);
        }

        var completedLines = winDetection.GetCompletedLines(cards);
        if (completedLines.Count > 0)
        {
            GameEvents.NotifyBingo(completedLines);
        }
    }

    private void UseShield()
    {
        var effect = new ActivePowerUpEffect
        {
            Type = PowerUpType.Shield,
            StartTime = Time.time,
            Duration = 60f
        };

        GameEvents.NotifyPowerUpActivated(effect);
    }

    public bool HasShield()
    {
        var shieldEffect = GetActiveEffect(PowerUpType.Shield);
        return shieldEffect != null && (Time.time - shieldEffect.StartTime) < shieldEffect.Duration;
    }

    public float GetScoreMultiplier()
    {
        var doubleEffect = GetActiveEffect(PowerUpType.DoublePayout);
        if (doubleEffect != null && (Time.time - doubleEffect.StartTime) < doubleEffect.Duration)
        {
            return 2f;
        }
        return 1f;
    }
}
```

---

### 3.5 反馈系统（Feedback System）

#### 3.5.1 反馈类型定义

```csharp
public enum FeedbackType
{
    Perfect,      // 完美：号码球消失前点击
    Great,        // 优秀：快速点击
    Miss,         // 失误：点击未呼叫的数字
    Normal        // 普通：正常点击
}
```

#### 3.5.2 Combo系统

```csharp
public class ComboSystem
{
    private const float ComboWindow = 2f; // 2秒内的连续正确点击
    private List<float> clickTimes;
    private int comboCount = 0;
    private float energy = 0f;

    public void RecordClick(bool isCorrect, float timeBeforeCall)
    {
        float timeSinceLastClick = Time.time - timeBeforeCall;
        
        if (isCorrect && timeSinceLastClick < ComboWindow)
        {
            clickTimes.Add(timeSinceLastClick);
            comboCount++;
            energy += 10f * comboCount;
        }
        else
        {
            comboCount = 0;
            clickTimes.Clear();
        }

        UpdateEnergy();
    }

    public FeedbackType GetFeedbackType(bool isCorrect, float timeBeforeCall)
    {
        float timeSinceLastClick = Time.time - timeBeforeCall;

        if (!isCorrect)
        {
            return FeedbackType.Miss;
        }

        if (timeSinceLastClick < 0.5f)
        {
            return FeedbackType.Perfect;
        }

        if (timeSinceLastClick < 1.5f)
        {
            return FeedbackType.Great;
        }

        return FeedbackType.Normal;
    }

    private void UpdateEnergy()
    {
        GameEvents.NotifyEnergyChanged(energy);
    }

    public void Reset()
    {
        clickTimes.Clear();
        comboCount = 0;
        energy = 0f;
    }
}
```

#### 3.5.3 反馈管理器

```csharp
public class FeedbackManager
{
    private ComboSystem comboSystem;
    private Dictionary<int, BingoCard> cards;

    public void Initialize(BingoCard[] cards)
    {
        comboSystem = new ComboSystem();
        for (int i = 0; i < cards.Length; i++)
        {
            cards[cards[i].CardIndex] = cards[i];
        }
    }

    public FeedbackType HandleCardClick(int cardIndex, int calledNumber, float timeBeforeCall)
    {
        if (!cards.ContainsKey(cardIndex))
            return FeedbackType.Miss;

        var card = cards[cardIndex];
        bool isCorrect = (card.Number == calledNumber);

        var feedbackType = comboSystem.RecordClick(isCorrect, timeBeforeCall);

        if (isCorrect)
        {
            card.IsMarked = true;
            GameEvents.NotifyCardMarked(card.Position);
        }

        return feedbackType;
    }

    public void TriggerMissPenalty(int cardIndex)
    {
        if (cards.ContainsKey(cardIndex))
        {
            var card = cards[cardIndex];
            GameEvents.NotifyMissPenalty(card.Position);
        }
    }

    public float GetEnergy()
    {
        return comboSystem.GetEnergy();
    }
}
```

---

### 3.6 UI/UX深度拆解（Interface Details）

#### 3.6.1 视觉反馈层（Daub Effect）

```csharp
public class DaubEffectController
{
    [SerializeField] private GameObject stampPrefab;
    [SerializeField] private Transform effectContainer;
    private Queue<GameObject> effectPool;
    private const int PoolSize = 10;

    public void Initialize()
    {
        effectPool = new Queue<GameObject>();

        for (int i = 0; i < PoolSize; i++)
        {
            var effect = Instantiate(stampPrefab, effectContainer);
            effect.SetActive(false);
            effectPool.Enqueue(effect);
        }
    }

    public void PlayDaubEffect(Vector2Int cardPosition, FeedbackType feedbackType)
    {
        GameObject effect = null;

        switch (feedbackType)
        {
            case FeedbackType.Perfect:
                effect = GetEffectFromPool();
                effect.transform.position = GetWorldPosition(cardPosition);
                effect.SetActive(true);
                StartCoroutine(DeactivateAfterDelay(effect, 0.8f));
                break;
            case FeedbackType.Great:
                effect = GetEffectFromPool();
                effect.transform.position = GetWorldPosition(cardPosition);
                effect.SetActive(true);
                StartCoroutine(DeactivateAfterDelay(effect, 0.6f));
                break;
            case FeedbackType.Normal:
                effect = GetEffectFromPool();
                effect.transform.position = GetWorldPosition(cardPosition);
                effect.SetActive(true);
                StartCoroutine(DeactivateAfterDelay(effect, 0.4f));
                break;
        }
    }

    private GameObject GetEffectFromPool()
    {
        if (effectPool.Count > 0)
        {
            var effect = effectPool.Dequeue();
            effect.SetActive(true);
            return effect;
        }
        return Instantiate(stampPrefab, effectContainer);
    }

    private IEnumerator DeactivateAfterDelay(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        effect.SetActive(false);
        effectPool.Enqueue(effect);
    }
}
```

#### 3.6.2 能量条UI

```csharp
public class EnergyBarView : MonoBehaviour
{
    [SerializeField] private Image energyFill;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private float maxEnergy = 100f;

    public void UpdateEnergy(float energy)
    {
        float normalizedEnergy = Mathf.Clamp01(energy / maxEnergy);
        energyFill.fillAmount = normalizedEnergy;
        energyText.text = $"{(int)energy}%";
    }
}
```

#### 3.6.3 移动端多卡适配

```csharp
public class CardLayoutAdapter
{
    private int cardCount = 4;
    private GridLayoutGroup gridLayout;
    private RectTransform cardContainer;

    public enum ViewMode
    {
        Overview,      // 缩放视图：四宫格全览
        Focus         // 聚焦视图：双击某张卡片放大
    }

    public void Initialize(int cardCount, GridLayoutGroup gridLayout, RectTransform cardContainer)
    {
        this.cardCount = cardCount;
        this.gridLayout = gridLayout;
        this.cardContainer = cardContainer;
    }

    public void SetViewMode(ViewMode mode)
    {
        switch (mode)
        {
            case ViewMode.Overview:
                SetOverviewMode();
                break;
            case ViewMode.Focus:
                SetFocusMode();
                break;
        }
    }

    private void SetOverviewMode()
    {
        gridLayout.cellSize = new Vector2(1, 1);
        gridLayout.spacing = new Vector2(10, 10);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = cardCount / 2;
    }

    private void SetFocusMode()
    {
        gridLayout.cellSize = new Vector2(2, 2);
        gridLayout.spacing = new Vector2(5, 5);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = cardCount;
    }
}
```

---

## 4. 控制器设计

### 4.1 游戏控制器

```csharp
public class GameController : MonoBehaviour
{
    private static GameController instance;

    public static GameController Instance => instance;

    private MultiCardManager cardManager;
    private CallerManager callerManager;
    private WinDetectionSystem winDetection;
    private PowerUpManager powerUpManager;
    private FeedbackManager feedbackManager;
    private Dictionary<string, object> userStatus;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeSystems();
    }

    private void InitializeSystems()
    {
        int cardCount = GetCardCountForPlatform();
        cardManager = new MultiCardManager();
        cardManager.InitializeCards(cardCount);

        callerManager = new CallerManager();
        callerManager.Initialize(CallerMode.DynamicBalancing);

        winDetection = new WinDetectionSystem(cardCount);

        powerUpManager = new PowerUpManager();
        powerUpManager.Initialize();

        feedbackManager = new FeedbackManager();
        feedbackManager.Initialize(cardManager.PlayerCards);

        userStatus = new Dictionary<string, object>
        {
            { "playerId", "default" },
            { "playerNumbers", cardManager.PlayerCards.Select(c => c.Number).ToList() }
        };
    }

    public void OnNumberCalled(int number)
    {
        feedbackManager.HandleNumberCalled(number);
    }

    public void OnCardClicked(int cardIndex)
    {
        int calledNumber = callerManager.GetLastCalledNumber();
        var feedbackType = feedbackManager.HandleCardClick(cardIndex, calledNumber);
        
        if (feedbackType == FeedbackType.Miss)
        {
            feedbackManager.TriggerMissPenalty(cardIndex);
        }

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        bool isWin = winDetection.CheckWinCondition(cardManager.PlayerCards);
        
        if (isWin)
        {
            var completedLines = winDetection.GetCompletedLines(cardManager.PlayerCards);
            var fourCorners = winDetection.GetFourCorners(cardManager.PlayerCards);
            
            GameEvents.NotifyBingo(completedLines, fourCorners);
        }
    }

    public void UsePowerUp(PowerUpType type)
    {
        if (powerUpManager.CanUsePowerUp(type))
        {
            powerUpManager.UsePowerUp(type, cardManager.PlayerCards, callerManager.GetCalledNumbers());
        }
    }

    private int GetCardCountForPlatform()
    {
        return Application.isMobilePlatform ? 4 : 4;
    }
}
```

---

## 5. 事件系统设计

### 5.1 游戏事件

```csharp
public static class GameEvents
{
    public static event System.Action<int> OnNumberCalled;
    public static event System.Action<Vector2Int> OnCardMarked;
    public static event System.Action<List<WinLine>, List<WinLine>> OnBingo;
    public static event System.Action<ActivePowerUpEffect> OnPowerUpActivated;
    public static event System.Action<float> OnEnergyChanged;
    public static event System.Action<Vector2Int> OnMissPenalty;
    public static event System.Action OnNumberCallStart;

    public static void NotifyNumberCalled(int number)
    {
        OnNumberCalled?.Invoke(number);
    }

    public static void NotifyCardMarked(Vector2Int position)
    {
        OnCardMarked?.Invoke(position);
    }

    public static void NotifyBingo(List<WinLine> completedLines, List<WinLine> fourCorners)
    {
        OnBingo?.Invoke(completedLines, fourCorners);
    }

    public static void NotifyPowerUpActivated(ActivePowerUpEffect effect)
    {
        OnPowerUpActivated?.Invoke(effect);
    }

    public static void NotifyEnergyChanged(float energy)
    {
        OnEnergyChanged?.Invoke(energy);
    }

    public static void NotifyMissPenalty(Vector2Int position)
    {
        OnMissPenalty?.Invoke(position);
    }

    public static void NotifyNumberCallStart()
    {
        OnNumberCallStart?.Invoke();
    }
}
```

### 5.2 UI事件

```csharp
public static class UIEvents
{
    public static event System.Action<PowerUpType> OnPowerUpSelected;
    public static event System.Action<int> OnCardCountChanged;

    public static void NotifyPowerUpSelected(PowerUpType type)
    {
        OnPowerUpSelected?.Invoke(type);
    }

    public static void NotifyCardCountChanged(int count)
    {
        OnCardCountChanged?.Invoke(count);
    }
}
```

---

## 6. 服务层设计

### 6.1 配置服务

```csharp
[CreateAssetMenu(fileName = "BingoGameConfig", menuName = "Bingo/Game Config")]
public class BingoGameConfig : ScriptableObject
{
    [Header("Caller Settings")]
    public CallerMode defaultCallerMode = CallerMode.DynamicBalancing;
    public float callInterval = 3f;
    public float idleThreshold = 30f;

    [Header("Card Settings")]
    public int defaultCardCount = 4;
    public int minNumber = 1;
    public int maxNumber = 75;
    public float maxOverlapRate = 80f;

    [Header("PowerUp Settings")]
    public List<PowerUpConfig> powerUpConfigs;

    [Header("Feedback Settings")]
    public float comboWindow = 2f;
    public float energyPerCombo = 10f;
    public float maxEnergy = 100f;
}
```

---

## 7. UI设计

### 7.1 卡片视图

```csharp
public class CardView : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private Image powerUpIndicator;
    [SerializeField] private Image markIndicator;
    [SerializeField] private Button cardButton;

    private BingoCard cardData;
    private int cardIndex;

    public void Initialize(BingoCard card, int index)
    {
        cardData = card;
        cardIndex = index;

        if (numberText != null)
        {
            numberText.text = card.Number.ToString();
        }

        UpdateCardType();
        Reset();
    }

    private void UpdateCardType()
    {
        switch (cardData.CardType)
        {
            case CardType.PaintBucket:
                powerUpIndicator.gameObject.SetActive(true);
                powerUpIndicator.color = Color.yellow;
                break;
            case CardType.Coin:
                powerUpIndicator.gameObject.SetActive(true);
                powerUpIndicator.color = Color.yellow;
                break;
            case CardType.Normal:
                powerUpIndicator.gameObject.SetActive(false);
                break;
        }
    }

    public void SetMarked(bool marked)
    {
        markIndicator.gameObject.SetActive(marked);
        
        if (marked)
        {
            backgroundImage.color = new Color(0.8f, 0.8f, 0.8f);
        numberText.color = Color.white;
        powerUpIndicator.gameObject.SetActive(false);
        powerUpIndicator.color = Color.yellow;
        cardData.IsPowerUpCollected = true;
        cardData.IsPowerUpActive = false;
        GameEvents.NotifyCardMarked(cardData.Position);
        }
    }

    public void SetInteractable(bool interactable)
    {
        cardButton.interactable = interactable;
    }

    public void Reset()
    {
        SetMarked(false);
        SetInteractable(true);
        cardData.IsMarked = false;
        cardData.IsPowerUpActive = false;
        cardData.IsPowerUpCollected = false;
        UpdateCardType();
    }
}
```

### 7.2 道具UI

```csharp
public class PowerUpBarView : MonoBehaviour
{
    [SerializeField] private Transform powerUpContainer;
    [SerializeField] private GameObject powerUpButtonPrefab;

    private Dictionary<PowerUpType, PowerUpButton> buttons;

    public void Initialize(List<PowerUpConfig> configs)
    {
        foreach (var config in configs)
        {
            var button = Instantiate(powerUpButtonPrefab, powerUpContainer);
            var buttonView = button.GetComponent<PowerUpButton>();
            buttonView.Initialize(config);
            buttons[config.Type] = buttonView;
        }
    }

    public void UpdateButtonState(PowerUpType type, bool canUse)
    {
        if (buttons.ContainsKey(type))
        {
            buttons[type].SetCanUse(canUse);
        }
    }

    public void UpdateButtonCount(PowerUpType type, int count)
    {
        if (buttons.ContainsKey(type))
        {
            buttons[type].UpdateCount(count);
        }
    }
}
```

---

## 8. 游戏流程设计

### 8.1 游戏初始化流程

```
1. 加载配置 (LoadConfig)
   ↓
2. 初始化服务 (InitializeServices)
   - CardSystem
   - CallerSystem
   - WinDetectionSystem
   - PowerUpSystem
   - FeedbackSystem
   ↓
3. 生成卡片 (GenerateCards)
   - 确保重合率不超过80%
   - 分配道具格
   ↓
4. 初始化UI (InitializeUI)
   - 创建卡片视图
   - 创建道具栏
   - 创建能量条
   ↓
5. 注册事件监听 (RegisterEventListeners)
   ↓
6. 切换到Ready状态 (ChangeState: Ready)
```

### 8.2 游戏进行流程

```
1. 玩家点击开始 (StartGame)
   ↓
2. 呼叫系统开始 (StartCaller)
   ↓
3. 循环:
   a. 呼叫下一个数字 (CallNextNumber)
   b. 播放呼叫动画 (PlayCallAnimation)
   c. 通知所有客户端 (NotifyNumberCalled)
   d. 等待玩家点击 (WaitForPlayerClick)
   e. 玩家点击卡片 (PlayerClickCard)
   f. 检查反馈类型 (CheckFeedbackType)
   g. 播放反馈效果 (PlayFeedbackEffect)
   h. 更新能量条 (UpdateEnergy)
   i. 检查胜利条件 (CheckWinCondition)
   j. 如果胜利:
      - 播放胜利动画 (PlayWinAnimation)
      - 显示BINGO按钮 (ShowBingoButton)
      - 结束游戏 (EndGame)
```

### 8.3 道具使用流程

```
1. 玩家点击道具 (PlayerClickPowerUp)
   ↓
2. 检查是否可用 (CheckCanUse)
   ↓
3. 如果可用:
   a. 扣除道具库存 (DeductInventory)
   b. 执行道具效果 (ExecutePowerUpEffect)
   c. 更新UI (UpdateUI)
   ↓
4. 如果不可用:
   a. 显示冷却提示 (ShowCooldownMessage)
```

---

## 9. 扩展性设计

### 9.1 添加新连线模式

开发者可以通过实现 `IWinLineType` 接口来添加自定义连线模式：

```csharp
public interface IWinLineType
{
    string Name { get; }
    bool CheckWin(BingoCard[] cards);
    List<Vector2Int> GetWinningCells(BingoCard[] cards);
}
```

### 9.2 添加新道具类型

开发者可以通过添加新的 `PowerUpType` 枚举值和对应的处理逻辑来扩展道具系统。

### 9.3 添加新反馈效果

开发者可以通过扩展 `FeedbackType` 枚举和 `DaubEffectController` 来添加新的反馈效果。

---

## 10. 性能优化

### 10.1 对象池

```csharp
public class EffectPool
{
    private Queue<GameObject> pool;
    private GameObject prefab;
    private Transform parent;
    private const int MaxPoolSize = 20;

    public EffectPool(GameObject prefab, Transform parent, int initialSize)
    {
        this.prefab = prefab;
        this.parent = parent;
        pool = new Queue<GameObject>();

        for (int i = 0; i < initialSize; i++)
        {
            var effect = Instantiate(prefab, parent);
            effect.SetActive(false);
            pool.Enqueue(effect);
        }
    }

    public GameObject Get()
    {
        if (pool.Count > 0)
        {
            var effect = pool.Dequeue();
            effect.SetActive(true);
            return effect;
        }
        return Instantiate(prefab, parent);
    }

    public void Return(GameObject effect)
    {
        effect.SetActive(false);
        pool.Enqueue(effect);
    }
}
```

### 10.2 异步优化

仅在需要异步操作时使用 UniTask，避免不必要的异步包装：

```csharp
// 好的做法
public async UniTask SomeAsyncMethod()
{
    await UniTask.Delay(100);
}

// 好的做法
public void SomeMethod()
{
    // 直接执行，不需要async
}
```

---

## 11. 总结

本设计文档提供了一个完整的Bingo游戏架构，具有以下核心特点：

1. **MVC架构**: 清晰的模型-视图-控制器分离
2. **智能卡片生成**: 避免数字重合，保证游戏公平性
3. **动态呼叫平衡**: 提升玩家爽感，避免长时间无操作
4. **实时连线判定**: 13种连线模式，即时反馈
5. **丰富道具系统**: 4种基础道具，增强游戏策略
6. **深度反馈系统**: Combo、Miss惩罚、视觉反馈
7. **多卡自适应**: 支持移动端4张卡和PC端更多卡
8. **对象池优化**: 复用特效对象，提升性能
9. **事件驱动**: 解耦模块间通信
10. **可扩展性**: 支持自定义连线模式、道具类型和反馈效果

该架构为后续功能扩展提供了良好的基础，开发者可以轻松添加新的连线模式、道具效果和游戏机制。
