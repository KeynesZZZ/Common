# Block Blast 游戏

## 项目概述

Block Blast 是一款令人上瘾的方块消除益智游戏，结合了俄罗斯方块和三消游戏的元素。玩家需要将不同形状的方块拖放到游戏板上，填满整行或整列来消除方块并获得分数。

## 游戏规则

### 核心玩法
1. **拖动方块**: 将下方提供的方块拖放到游戏板上
2. **消除规则**: 填满整行或整列即可消除
3. **得分机制**: 消除后获得分数，连击有额外奖励
4. **游戏结束**: 当无法放置任何方块时游戏结束

### 游戏板
- **尺寸**: 8x8 网格
- **方块**: 11种不同形状（单格、双格、三格、2x2、L形、T形、Z形、十字形等）
- **颜色**: 6种不同颜色

### 计分系统
- 消除1行/列: 100分
- 同时消除行和列: 300分（额外奖励）
- 连击倍数: 1.5x, 2x, 2.5x, 3x...

## 项目结构

```
Assets/Games/BlockBlast/
├── Scripts/
│   ├── Core/
│   │   ├── GameBoard.cs          # 游戏板管理
│   │   ├── ScoreManager.cs       # 分数管理
│   │   └── BlockBlastGame.cs     # 游戏主控制器
│   ├── Data/
│   │   ├── BlockShape.cs         # 方块形状枚举
│   │   ├── BlockData.cs          # 方块数据
│   │   ├── GridPosition.cs       # 网格坐标
│   │   └── GameConfig.cs         # 游戏配置
│   └── UI/
│       ├── GameBoardUI.cs        # 游戏板UI
│       ├── DraggableBlock.cs     # 可拖拽方块
│       ├── BlockTrayUI.cs        # 方块托盘
│       └── GameUI.cs             # 游戏主UI
├── Prefabs/
│   ├── Cell.prefab               # 网格单元格预制体
│   ├── BlockCell.prefab          # 方块单元格预制体
│   └── DraggableBlock.prefab     # 可拖拽方块预制体
├── Resources/
│   └── GameConfig.asset          # 游戏配置文件
└── Docs/
    ├── BlockBlast_GDD.md         # 游戏设计文档
    ├── Development_Tasks.md      # 开发任务清单
    └── README.md                 # 项目说明
```

## 核心类说明

### GameBoard
管理8x8游戏板的状态，包括：
- 方块放置验证
- 行/列消除检测
- 有效位置计算

### BlockBlastGame
游戏主控制器，负责：
- 游戏状态管理
- 回合控制
- 事件分发

### ScoreManager
分数管理系统：
- 分数计算
- 连击管理
- 最高分记录

### BlockData
方块数据类：
- 方块形状定义
- 颜色管理
- 位置计算

## 使用方法

### 1. 创建游戏场景
1. 在Unity中创建新场景
2. 添加Canvas（UI根节点）
3. 添加GameUI组件

### 2. 配置游戏
1. 创建GameConfig配置文件
2. 设置游戏板大小、分数规则等参数
3. 配置预制体引用

### 3. 运行游戏
```csharp
// 代码示例
var game = new BlockBlastGame(gameConfig);
game.StartGame();
```

## 扩展功能

### 添加新方块形状
在 `BlockShape.cs` 中添加新形状：
```csharp
public enum BlockShape
{
    // ... 现有形状
    NewShape,  // 新形状
}
```

在 `BlockData.GetShapeCells()` 中实现形状：
```csharp
case BlockShape.NewShape:
    cells.Add(new GridPosition(0, 0));
    cells.Add(new GridPosition(0, 1));
    // ... 添加更多单元格
    break;
```

### 自定义游戏配置
创建ScriptableObject配置文件：
```csharp
var config = ScriptableObject.CreateInstance<GameConfig>();
config.BoardWidth = 10;
config.BoardHeight = 10;
config.BaseScore = 150;
```

## 技术栈

- **引擎**: Unity 2022.3 LTS
- **语言**: C#
- **UI**: Unity UI (uGUI)
- **输入**: Unity Event System

## 性能优化

1. **对象池**: 可添加对象池管理方块实例
2. **算法优化**: 使用HashSet优化位置检测
3. **内存管理**: 预分配列表容量减少GC

## 许可证

MIT License

## 作者

开发团队

## 更新日志

### v1.0.0 (2024-01-29)
- 初始版本发布
- 实现核心游戏玩法
- 添加基础UI和交互
- 支持11种方块形状
- 实现分数和连击系统
