# Block Blast 与 SimpleBoard/SimpleJob 集成设计方案

## 1. 架构分析

### 1.1 SimpleBoard 框架核心

```
Core/
  - GridPosition: 网格坐标结构体
  
Interfaces/
  - IGrid: 网格基础接口 (RowCount, ColumnCount, IsPositionOnGrid)
  - IGridSlot: 网格槽位接口 (HasItem, IsMovable, CanContainItem)
  - IGameBoard<T>: 游戏板接口
  - IUnityGridSlot: Unity场景槽位接口
  - IUnityItem: Unity物品接口
  
Logic/
  - GameBoard<T>: 泛型游戏板实现
  - UnityGridSlot: Unity槽位实现
  
Input/
  - CanvasInputSystem: 输入系统
  - PointerEventArgs: 指针事件参数
  
Data/
  - SolvedData<T>: 解决数据
  - ItemSequence<T>: 物品序列
```

### 1.2 SimpleJob 框架核心

```
- IJob: 作业接口
- Job: 抽象作业类
- JobsExecutor: 作业执行器
```

## 2. 集成策略

### 2.1 Block Blast 适配层

| 框架组件 | Block Blast 对应实现 |
|---------|---------------------|
| `IGridSlot` | `IBlockGridSlot` - 方块槽位接口 |
| `IUnityGridSlot` | `BlockGridSlot` - 方块槽位实现 |
| `IUnityItem` | `BlockItem` - 方块物品 |
| `IGameBoard<T>` | `BlockGameBoard` - 方块游戏板 |
| `IJob` | `ClearLineJob`, `PlaceBlockJob` - 游戏作业 |

### 2.2 数据流设计

```
用户输入 (CanvasInputSystem)
    ↓
BlockBlastInputHandler (处理拖拽)
    ↓
BlockBlastGame (游戏逻辑)
    ↓
BlockGameBoard (游戏板管理)
    ↓
JobsExecutor (异步执行)
    ↓
BlockGameBoardRenderer (渲染更新)
```

### 2.3 关键适配点

1. **使用 SimpleBoard.GridPosition** - 替换自定义 GridPosition
2. **继承 GameBoard<T>** - 使用框架的游戏板基类
3. **实现 IUnityGridSlot** - 槽位状态管理
4. **使用 CanvasInputSystem** - 统一输入处理
5. **集成 SimpleJob** - 消除、放置等操作异步化

## 3. 类图设计

```
┌─────────────────────────────────────────────────────────────┐
│                      BlockBlastGame                         │
│                    (游戏主控制器)                            │
├─────────────────────────────────────────────────────────────┤
│ - _board: BlockGameBoard                                    │
│ - _scoreManager: ScoreManager                               │
│ - _jobExecutor: JobsExecutor                                │
├─────────────────────────────────────────────────────────────┤
│ + StartGame()                                               │
│ + PlaceBlockAsync(): UniTask                                │
│ + CanPlaceBlock(): bool                                     │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    BlockGameBoard                           │
│              (继承 GameBoard<IBlockGridSlot>)               │
├─────────────────────────────────────────────────────────────┤
│ - 行/列消除检测                                              │
│ - 有效位置计算                                               │
├─────────────────────────────────────────────────────────────┤
│ + CheckAndClearLinesAsync(): UniTask                        │
│ + GetValidPlacements(): List<GridPosition>                  │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                  IBlockGridSlot                             │
│              (继承 IUnityGridSlot)                          │
├─────────────────────────────────────────────────────────────┤
│ + BlockType: int                                            │
│ + BlockColor: Color                                         │
└─────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                    BlockGridSlot                            │
│              (继承 UnityGridSlot)                           │
├─────────────────────────────────────────────────────────────┤
│ - _blockType: int                                           │
│ - _blockColor: Color                                        │
├─────────────────────────────────────────────────────────────┤
│ + SetBlock(type, color)                                     │
│ + ClearBlock()                                              │
└─────────────────────────────────────────────────────────────┘
```

## 4. 作业系统设计

### 4.1 作业类型

```csharp
// 放置方块作业
public class PlaceBlockJob : Job
{
    private BlockGameBoard _board;
    private BlockData _block;
    private GridPosition _position;
    
    public override async UniTask ExecuteAsync(CancellationToken token)
    {
        // 执行放置逻辑
        // 触发动画
        // 等待完成
    }
}

// 消除行/列作业
public class ClearLinesJob : Job
{
    private BlockGameBoard _board;
    private List<int> _rows;
    private List<int> _cols;
    
    public override async UniTask ExecuteAsync(CancellationToken token)
    {
        // 执行消除动画
        // 更新游戏板状态
        // 计算分数
    }
}

// 分数更新作业
public class UpdateScoreJob : Job
{
    private ScoreManager _scoreManager;
    private int _rowsCleared;
    private int _colsCleared;
    
    public override async UniTask ExecuteAsync(CancellationToken token)
    {
        // 计算分数
        // 更新UI
        // 播放音效
    }
}
```

### 4.2 作业执行流程

```
玩家放置方块
    ↓
PlaceBlockJob (ExecutionOrder: 0)
    ↓
ClearLinesJob (ExecutionOrder: 1)
    ↓
UpdateScoreJob (ExecutionOrder: 2)
    ↓
CheckGameOverJob (ExecutionOrder: 3)
```

## 5. 输入系统集成

### 5.1 输入处理流程

```csharp
public class BlockBlastInputHandler : MonoBehaviour
{
    [SerializeField] private CanvasInputSystem _inputSystem;
    
    private void Awake()
    {
        _inputSystem.PointerDown += OnPointerDown;
        _inputSystem.PointerDrag += OnPointerDrag;
        _inputSystem.PointerUp += OnPointerUp;
    }
    
    private void OnPointerDown(object sender, PointerEventArgs e)
    {
        // 检测是否点击了方块
        // 开始拖拽
    }
    
    private void OnPointerDrag(object sender, PointerEventArgs e)
    {
        // 更新拖拽位置
        // 显示放置预览
    }
    
    private void OnPointerUp(object sender, PointerEventArgs e)
    {
        // 尝试放置方块
        // 触发放置作业
    }
}
```

## 6. 渲染系统集成

### 6.1 渲染器接口实现

```csharp
public interface IBlockGameBoardRenderer
{
    void RenderBlockPlaced(GridPosition position, BlockData block);
    void RenderLinesCleared(List<int> rows, List<int> cols);
    void RenderBlockCreated(GridPosition position, BlockData block);
    void RenderBlockRemoved(GridPosition position);
}

public class BlockGameBoardRenderer : MonoBehaviour, IBlockGameBoardRenderer
{
    // 实现渲染逻辑
    // 使用DOTween或Coroutine实现动画
}
```

## 7. 配置系统集成

### 7.1 使用 ScriptableObject

```csharp
[CreateAssetMenu(fileName = "BlockBlastConfig", menuName = "BlockBlast/Config")]
public class BlockBlastConfig : ScriptableObject
{
    public int BoardWidth = 8;
    public int BoardHeight = 8;
    public int BlocksPerTurn = 3;
    public int BaseScore = 100;
    
    // 其他配置...
}
```

## 8. 优势对比

| 方面 | 重构前 | 重构后 (使用框架) |
|-----|-------|------------------|
| 代码复用 | 低 | 高 (使用框架基类) |
| 可测试性 | 一般 | 好 (接口隔离) |
| 扩展性 | 一般 | 好 (插件化设计) |
| 维护性 | 一般 | 好 (统一架构) |
| 性能 | 一般 | 好 (AggressiveInlining) |
| 异步支持 | 手动 | SimpleJob集成 |
| 输入处理 | 自定义 | 统一输入系统 |
