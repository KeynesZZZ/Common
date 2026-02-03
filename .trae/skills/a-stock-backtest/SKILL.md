---
name: "a-stock-backtest"
description: "A股股票策略回测工具，使用本地60分钟数据转换为日线，支持全市场回测、参数配置、结果可视化和策略对比。当用户要求进行A股策略回测、分析历史数据表现或优化交易策略时调用。"
---

# A股策略回测工具

这是一个完整的A股股票策略回测系统，使用本地60分钟K线数据转换为日线数据，并存储在SQLite数据库中进行高效回测。支持自定义回测时间范围、选股策略配置和多策略对比。

## 功能特性

### 1. 全市场回测
- 支持加载本地所有A股历史数据（2015-2024年）
- 自动将60分钟K线转换为日线数据并存储到SQLite数据库
- 支持自定义股票池或全市场扫描
- 支持配置回测时间范围，灵活选择历史时期

### 2. 参数可配置

#### 回测时间配置
- **years**：回测年份范围，默认[2015, 2024]
- 支持自定义具体年份列表，如[2020, 2021, 2022]

#### 交易参数
- **止损比例**：默认-5%，可自定义
- **止盈触发**：默认10%，可调整
- **止盈回撤**：默认从高点回落3%止盈
- **持仓周期**：默认3-7天，可扩展
- **最大持仓数**：默认5只股票
- **单股仓位**：默认15%
- **手续费**：默认0.03%
- **滑点**：默认0.1%

#### 选股策略参数
- **涨停判断阈值**：默认9.9%
- **涨停后观察天数**：默认1-3天
- **涨停后价格最低要求**：默认涨停价70%以上
- **量比阈值**：默认>1.2
- **观察周期**：默认20天
- **可自定义选股逻辑**：支持扩展不同选股策略

### 3. 结果可视化
自动生成以下可视化图表：
- 资金曲线图（净值曲线）
- 回撤曲线图
- 每日收益率分布
- 盈亏交易统计
- 月度/年度收益热力图

### 4. 策略对比
支持多策略对比：
- 对比不同参数设置
- 对比不同选股条件
- 生成策略对比报告

## 快速开始

### 基本用法

```python
# 创建回测实例
from stock_backtest.a_stock_backtest_optimized import AStockBacktest

backtest = AStockBacktest(
    data_dir=r'D:\BaiduNetdiskDownload\沪深个股60分钟_按年汇总',
    initial_capital=1000000
)

# 运行默认策略回测
results = backtest.run()
```

### 自定义参数

#### 配置回测时间范围

```python
# 配置特定年份的回测
backtest = AStockBacktest(
    data_dir=r'D:\BaiduNetdiskDownload\沪深个股60分钟_按年汇总',
    initial_capital=1000000,
    years=[2020, 2021, 2022]  # 只回测这三年的数据
)

results = backtest.run()
```

#### 自定义交易参数

```python
# 自定义交易参数
backtest = AStockBacktest(
    data_dir=r'D:\BaiduNetdiskDownload\沪深个股60分钟_按年汇总',
    initial_capital=1000000,
    years=[2018, 2019, 2020, 2021],  # 配置回测年份
    stop_loss_pct=-0.08,      # 止损8%
    take_profit_trigger=0.12,   # 止盈触发12%
    take_profit_fallback=-0.04,  # 回落4%止盈
    max_holding_days=10,       # 最长持仓10天
    commission=0.0003,        # 手续费0.03%
    slippage=0.001            # 滑点0.1%
)

results = backtest.run()
```

### 使用指定股票池

```python
# 只回测特定股票
stock_codes = ['sh600000', 'sz000001', 'sz300750']
results = backtest.run(stock_pool=stock_codes)
```

### 配置选股策略

#### 修改默认策略参数

```python
# 创建回测实例并修改策略参数
backtest = AStockBacktest(
    data_dir=r'D:\BaiduNetdiskDownload\沪深个股60分钟_按年汇总',
    initial_capital=1000000,
    years=[2020, 2021, 2022]
)

# 修改选股策略参数
backtest.strategy_params = {
    'limit_up_pct': 9.8,         # 调整涨停判断阈值
    'min_close_after_limit': 0.75, # 涨停后价格最低要求提高到75%
    'min_volume_ratio': 1.5,      # 量比阈值提高到1.5
    'days_to_check': 25           # 观察周期延长到25天
}

results = backtest.run()
```

#### 扩展新的选股策略

```python
from stock_backtest.a_stock_backtest_optimized import AStockBacktest, LimitUpStrategy

# 自定义选股策略
class MyCustomStrategy(LimitUpStrategy):
    def select_stock(self, df, idx):
        # 基础涨停选股逻辑
        base_result = super().select_stock(df, idx)
        if not base_result['selected']:
            return base_result
        
        # 添加额外的选股条件
        current_price = df.iloc[idx]['收盘价']
        ma20 = df.iloc[max(0, idx-19):idx+1]['收盘价'].mean()
        
        # 要求当前价格高于20日均线
        if current_price < ma20:
            return {'selected': False, 'reason': '价格低于20日均线'}
        
        return base_result

# 创建回测实例
backtest = AStockBacktest(
    data_dir=r'D:\BaiduNetdiskDownload\沪深个股60分钟_按年汇总',
    initial_capital=1000000,
    years=[2020, 2021]
)

# 替换为自定义策略
backtest.strategy = MyCustomStrategy(backtest.strategy_params)

results = backtest.run()
```

### 策略对比

```python
# 对比不同止损设置
strategies = [
    {'stop_loss_pct': -0.05, 'name': '止损5%'},
    {'stop_loss_pct': -0.08, 'name': '止损8%'},
    {'stop_loss_pct': -0.10, 'name': '止损10%'},
]

comparison = backtest.compare_strategies(strategies)
backtest.plot_comparison(comparison)
```

## 输出指标

回测结果包含以下核心指标：

| 指标 | 说明 |
|------|------|
| 总收益率 | 整体盈亏比例 |
| 年化收益率 | 按年计算的复合增长率 |
| 夏普比率 | 风险调整后收益指标 |
| 最大回撤 | 资金曲线最大跌幅 |
| 总交易次数 | 完整买入卖出次数 |
| 胜率 | 盈利交易占比 |
| 盈亏比 | 平均盈利/平均亏损 |
| 平均持仓天数 | 单笔交易平均持有时间 |
| 年化波动率 | 收益率的标准差 |

## 使用场景

### 1. 验证策略有效性
在实盘前使用历史数据验证策略，了解策略在不同市场环境下的表现。

### 2. 参数优化
通过调整不同参数组合，找到最优参数设置。

### 3. 风险评估
了解策略的最大回撤和波动率，评估风险承受能力。

### 4. 策略对比
对比不同策略的优劣，选择最适合的策略。

## 数据说明

- **数据来源**：本地60分钟K线数据
- **时间范围**：2015年-2024年
- **股票范围**：全A股市场
- **数据转换**：自动将60分钟数据聚合为日线数据

## 注意事项

1. **数据质量**：60分钟数据转换为日线可能存在一定偏差
2. **交易成本**：短线策略受交易成本影响较大
3. **市场环境**：历史表现不代表未来，策略需持续优化
4. **滑点影响**：实际交易可能面临更大的滑点成本
5. **风险提示**：任何策略都有风险，回测结果仅供参考

## 文件结构

```
stock_backtest/
├── a_stock_backtest.py      # 主回测引擎
├── data_loader.py           # 数据加载器
├── strategy.py             # 策略实现
├── backtest_engine.py      # 回测引擎
├── metrics.py             # 绩效指标计算
├── visualizer.py          # 结果可视化
└── config.py             # 配置文件
```

## 扩展开发

### 添加新策略

继承 `BaseStrategy` 类：

```python
from strategy import BaseStrategy

class MyStrategy(BaseStrategy):
    def select_stock(self, df, idx):
        # 实现你的选股逻辑
        if self.should_buy(df, idx):
            return {
                'selected': True,
                'price': df.iloc[idx]['收盘价'],
                'reason': '自定义买入信号'
            }
        return {'selected': False, 'reason': '不满足条件'}
```

### 添加新指标

在 `metrics.py` 中添加：

```python
def calculate_custom_metric(self):
    # 计算自定义指标
    pass
```

## 最佳实践

1. **先用模拟盘验证**：回测通过后，先用模拟盘验证1-3个月
2. **从小资金开始**：实盘时从小资金开始，逐步增加
3. **持续监控**：定期检查策略表现，及时调整
4. **分散风险**：不要把所有资金投入单一策略
5. **保持纪律**：严格执行交易规则，不被情绪左右
