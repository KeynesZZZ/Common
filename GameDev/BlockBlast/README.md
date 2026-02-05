# Block Blast! Unity 实现

基于设计文档的完整Unity实现，注重性能优化。

## 项目结构

```
Assets/
├── Scripts/
│   ├── Core/           # 核心数据结构
│   │   └── BlockShape.cs
│   ├── Managers/       # 游戏管理器
│   │   ├── GameManager.cs
│   │   ├── BoardManager.cs
│   │   ├── BlockGenerator.cs
│   │   └── ScoreManager.cs
│   ├── UI/            # UI相关
│   │   ├── UIManager.cs
│   │   └── BlockDragHandler.cs
│   └── Utils/         # 工具类
│       └── ObjectPool.cs
├── Prefabs/           # 预制体
├── Scenes/           # 场景
├── Sprites/          # 图片资源
└── Resources/        # 资源文件
```

## 核心功能

### 1. 游戏机制
- 8x8棋盘
- 拖拽放置方块
- 行/列消除
- Combo连击系统
- 动态难度调整

### 2. 性能优化
- 使用`byte[]`存储棋盘状态（内存优化）
- 对象池管理方块实例
- 缓存列表避免GC
- DOTween动画而非Animator

### 3. 代码特点
- 清晰的命名空间组织
- 事件驱动的UI更新
- 可扩展的方块形状系统
- 完整的注释

## 依赖项

- Unity 2021.3 LTS 或更高版本
- TextMeshPro
- DOTween (HOTween v2)

## 快速开始

### 1. 导入DOTween
1. 打开Package Manager
2. 点击"+" -> Add package from git URL
3. 输入: `com.demigiant.dotween`

### 2. 创建UI预制体

#### Cell Prefab
1. 创建Empty GameObject，命名为"Cell"
2. 添加Image组件
3. 设置颜色为白色/灰色
4. 保存到Prefabs文件夹

#### Block Cell Prefab
1. 创建Empty GameObject，命名为"BlockCell"
2. 添加Image组件
3. 添加RectTransform组件
4. 保存到Prefabs文件夹

### 3. 场景设置

#### 创建Canvas
1. 创建Canvas (Screen Space - Overlay)
2. 设置Reference Resolution为1080x1920

#### 创建GameManager
1. 创建Empty GameObject，命名为"GameManager"
2. 添加以下组件:
   - GameManager
   - BoardManager
   - BlockGenerator
   - ScoreManager

#### 创建UIManager
1. 在Canvas下创建Empty GameObject，命名为"UIManager"
2. 添加UIManager组件

#### 设置UI元素
```
Canvas
├── Board (Panel)
│   └── BoardCells (Empty)
├── BlockPreviews (Empty)
│   ├── Preview0 (RectTransform)
│   ├── Preview1 (RectTransform)
│   └── Preview2 (RectTransform)
├── ScorePanel (Empty)
│   ├── ScoreText (TextMeshPro)
│   ├── HighScoreText (TextMeshPro)
│   └── ComboDisplay (Empty)
│       └── ComboText (TextMeshPro)
└── GameOverPanel (Panel)
    ├── FinalScoreText (TextMeshPro)
    └── RestartButton (Button)
```

### 4. 配置UIManager
在UIManager组件中设置:
- Board Rect: 棋盘RectTransform
- Cell Prefab: 格子预制体
- Block Cell Prefab: 方块格子预制体
- Board Cells Parent: 格子父对象
- Placed Blocks Parent: 已放置方块父对象
- Preview Rects: 3个预览区域RectTransform
- Score Text: 分数TextMeshPro
- High Score Text: 最高分TextMeshPro
- Combo Text: Combo TextMeshPro
- Combo Display: Combo显示对象
- Game Over Panel: 游戏结束面板
- Final Score Text: 最终分数TextMeshPro
- Restart Button: 重新开始按钮

### 5. 配置GameManager
在GameManager组件中设置:
- Board Manager: 引用BoardManager
- Block Generator: 引用BlockGenerator
- Score Manager: 引用ScoreManager
- UI Manager: 引用UIManager

## 方块类型

| ID | 类型 | 形状 | 格子数 |
|----|------|------|--------|
| 1 | 单格 | ■ | 1 |
| 2 | 双格横 | ■■ | 2 |
| 3 | 双格竖 | ■<br>■ | 2 |
| 4 | 三格横 | ■■■ | 3 |
| 5 | 三格竖 | ■<br>■<br>■ | 3 |
| 6 | 2x2方块 | ■■<br>■■ | 4 |
| 7 | L形 | ■<br>■■ | 3 |
| 8 | T形 | ■■■<br>&nbsp;■ | 4 |
| 9 | 四格横 | ■■■■ | 4 |
| 10 | 四格竖 | ■<br>■<br>■<br>■ | 4 |
| 11 | 五格横 | ■■■■■ | 5 |
| 12 | 五格竖 | ■<br>■<br>■<br>■<br>■ | 5 |

## 计分规则

### 基础分数
- 1行/列: 100分
- 2行/列: 250分
- 3行/列: 450分
- 4行/列: 700分
- 5行/列: 1000分
- 6+行/列: 1400+分

### Combo倍率
- x1: 1.0x
- x2: 1.2x
- x3: 1.4x
- x4: 1.6x
- x5+: 1.8x+ (上限2.0x)

## 性能优化说明

### 内存优化
1. 使用`byte[]`而非`bool[]`存储棋盘
2. BlockShape使用`struct`而非`class`
3. 预分配List容量避免扩容

### 渲染优化
1. 使用对象池管理方块实例
2. DOTween动画而非Animator
3. 批量更新UI而非逐帧更新

### 算法优化
1. 消除检测使用缓存列表
2. 放置检测提前返回
3. 游戏结束检测优化

## 扩展建议

### 添加新方块形状
在`BlockShape.cs`中添加新的静态工厂方法:
```csharp
public static BlockShape CreateNewShape()
{
    return new BlockShape
    {
        id = 13,
        width = 3,
        height = 3,
        cells = new byte[] { 1,1,1, 0,1,0, 0,1,0 },
        color = Color.cyan,
        cellCount = 5
    };
}
```

### 添加道具系统
1. 创建PowerUp枚举
2. 在GameManager中添加道具逻辑
3. 在UIManager中添加道具UI

### 添加音效
1. 使用AudioManager管理音效
2. 在放置、消除时播放音效
3. 支持音量调节

## 许可证

MIT License
