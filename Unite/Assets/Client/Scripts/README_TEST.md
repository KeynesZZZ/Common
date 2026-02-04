# Bingo 游戏测试说明

## 测试环境设置

### 1. 场景设置
在 Unity 编辑器中创建测试场景：

1. 创建空场景 `TestScene`
2. 添加以下 GameObject 到场景中：
   - `GameBootstrap` (挂载 GameBootstrap 脚本)
   - `GameFlowTest` (挂载 GameFlowTest 脚本)
   - `BoardController` (挂载 BoardController 脚本)

### 2. 服务注册
GameBootstrap 会自动注册以下服务：
- NetworkService
- AnimationService
- FeedbackService
- ClientEventBus
- GameController

## 测试流程

### 自动化测试流程

运行场景后，`GameFlowTest` 会自动执行以下步骤：

```
步骤1: 模拟玩家加入房间
  ↓
步骤2: 模拟游戏初始化
  ↓
步骤3: 模拟游戏开始
  ↓
步骤4: 模拟玩家点击格子 (3次)
  ↓
步骤5: 模拟达成Bingo
  ↓
步骤6: 模拟游戏结束
```

### 测试覆盖的功能

#### 1. 游戏模式系统
- ✓ BaseGameMode 初始化
- ✓ 不同游戏模式的创建 (ClassicBingo, SpeedBingo, PowerUpBingo)
- ✓ 游戏模式生命周期钩子调用

#### 2. 事件系统
- ✓ GameInitialized 事件
- ✓ GameStarted 事件
- ✓ SlotClicked 事件
- ✓ BingoAchieved 事件
- ✓ PowerUpActivated 事件
- ✓ GameEnded 事件

#### 3. 控制器系统
- ✓ GameController 事件订阅和处理
- ✓ BoardController 格子点击处理
- ✓ UIController 界面更新

#### 4. 数据模型
- ✓ RoomData 创建和使用
- ✓ BoardData 创建和使用
- ✓ SlotData 创建和使用
- ✓ PlayerData 创建和使用

## 手动测试步骤

### 测试1: 验证游戏模式初始化
```csharp
// 在 GameController.InitializeGameMode 中设置断点
// 检查 _currentGameMode 是否正确创建
Debug.Log($"当前游戏模式: {_currentGameMode.ModeType}");
```

### 测试2: 验证事件发布
```csharp
// 在各个事件处理函数中设置断点
// 检查事件数据是否正确传递
```

### 测试3: 验证格子点击
```csharp
// 在 BoardView.OnSlotViewClicked 中设置断点
// 检查是否正确调用 BoardController.ClickSlotAsync
```

## 预期输出

运行测试后，控制台应该输出：

```
=== 开始游戏流程测试 ===
步骤1: 模拟玩家加入房间
玩家 player-001 尝试加入房间 test-room-001
✓ 玩家成功加入房间，房间ID: test-room-001
步骤2: 模拟游戏初始化
游戏初始化完成，等待游戏开始...
步骤3: 模拟游戏开始
✓ 游戏已开始，玩家可以开始点击格子
步骤4: 模拟玩家点击3个格子
模拟玩家点击3个格子
✓ 玩家点击了格子 0，标记状态: True
✓ 玩家点击了格子 4，标记状态: True
✓ 玩家点击了格子 8，标记状态: True
步骤5: 模拟达成Bingo
✓ 玩家达成Bingo！连线类型: Horizontal
步骤6: 模拟游戏结束
✓ 游戏结束，玩家排名: 第1名，分数: 150
=== 游戏流程测试完成 ===
```

## 已修复的问题

### 1. GameMode 系统未被使用
**问题**: BaseGameMode 的所有方法都没有被调用
**修复**: 在 GameController 中添加了游戏模式初始化和调用逻辑

### 2. BoardController.ClickSlotAsync 未被调用
**问题**: BoardController.ClickSlotAsync 存在但没有任何地方调用
**修复**: 在 BoardView 中添加了 SlotView.OnClicked 事件连接

### 3. 缺少游戏模式初始化逻辑
**问题**: GameController 没有创建或使用任何 GameMode 实例
**修复**: 添加了 InitializeGameMode 方法，根据 BingoCount 选择游戏模式

### 4. SlotView 点击事件未连接
**问题**: SlotView 的 OnClicked 事件没有连接到 BoardController
**修复**: 在 BoardView.CreateSlots 中添加了事件订阅

## 下一步

1. 实现服务器端逻辑
2. 添加真实的网络通信
3. 完善 UI 界面
4. 添加音效和动画
5. 实现多人游戏逻辑

## 注意事项

- 测试场景需要包含所有必需的组件
- 确保服务定位器正确注册所有服务
- 检查事件总线正确订阅所有事件
- 验证游戏模式正确初始化和调用
