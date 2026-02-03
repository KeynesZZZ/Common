#!/usr/bin/env python3
# 调试自定义选股策略

import pandas as pd
from a_stock_backtest_optimized import AStockBacktest
from custom_strategy import CustomLimitUpStrategy

if __name__ == '__main__':
    print("=" * 80)
    print("调试自定义选股策略")
    print("=" * 80)
    print()

    # 创建回测实例
    backtest = AStockBacktest(
        years=[2020, 2021],
        initial_capital=1000000
    )

    # 替换为调试版本的策略
    class DebugCustomStrategy(CustomLimitUpStrategy):
        def select_stock(self, df, idx):
            result = super().select_stock(df, idx)
            if not result['selected']:
                # 打印被过滤的原因
                print(f"{df.iloc[idx]['日期']} {df.iloc[idx]['名称']}: {result['reason']}")
            return result

    backtest.strategy = DebugCustomStrategy(backtest.strategy_params)
    print(f"策略替换成功: {type(backtest.strategy).__name__}")
    print()

    # 加载单个股票数据进行调试
    test_stock = 'sh600519'  # 贵州茅台
    print(f"调试股票: {test_stock}")
    print("加载数据...")
    
    # 直接调用内部方法加载数据
    stock_data = backtest._process_single_stock(test_stock)
    stock_code, df = stock_data
    
    if not df.empty:
        print(f"成功加载 {stock_code} 的数据，共 {len(df)} 条记录")
        print(f"时间范围: {df.iloc[0]['日期']} 到 {df.iloc[-1]['日期']}")
        print()
        
        # 手动运行选股逻辑
        print("运行选股逻辑...")
        print("=" * 60)
        signals = []
        
        for idx in range(25, min(100, len(df))):  # 只测试前100条记录
            result = backtest.strategy.select_stock(df, idx)
            if result['selected']:
                signals.append({
                    'date': df.iloc[idx]['日期'],
                    'price': result['current_price'],
                    'reason': '满足所有条件'
                })
                print(f"✓ {df.iloc[idx]['日期']}: 满足条件，价格: {result['current_price']:.2f}")
        
        print("=" * 60)
        print(f"共生成 {len(signals)} 个信号")
        
        if signals:
            print("信号详情:")
            for signal in signals:
                print(f"  {signal['date']}: {signal['price']:.2f} - {signal['reason']}")
        else:
            print("未生成信号，可能是条件过于严格")
    else:
        print(f"未加载到 {test_stock} 的数据")

    print()
    print("=" * 80)
    print("调试完成！")
    print("=" * 80)
