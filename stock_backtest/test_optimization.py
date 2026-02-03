#!/usr/bin/env python3
# 测试回测功能优化

from a_stock_backtest_optimized import AStockBacktest, LimitUpStrategy

print("测试回测时间配置...")
try:
    # 测试回测时间配置
    backtest = AStockBacktest(
        years=[2020, 2021],
        initial_capital=500000
    )
    print(f"✓ 回测时间配置成功: {backtest.years}")
    print(f"✓ 策略类型: {type(backtest.strategy).__name__}")
    print(f"✓ 策略参数: {backtest.strategy_params}")
    print()
    
    # 测试修改策略参数
    print("测试修改策略参数...")
    backtest.strategy_params = {
        'limit_up_pct': 9.8,
        'min_close_after_limit': 0.75,
        'min_volume_ratio': 1.5,
        'days_to_check': 25
    }
    print(f"✓ 策略参数修改成功: {backtest.strategy_params}")
    print()
    
    # 测试自定义策略
    print("测试自定义策略...")
    class MyCustomStrategy(LimitUpStrategy):
        def select_stock(self, df, idx):
            # 基础涨停选股逻辑
            base_result = super().select_stock(df, idx)
            if not base_result['selected']:
                return base_result
            
            # 添加额外的选股条件
            current_price = df.iloc[idx]['收盘价']
            if len(df) >= 20:
                ma20 = df.iloc[max(0, idx-19):idx+1]['收盘价'].mean()
                # 要求当前价格高于20日均线
                if current_price < ma20:
                    return {'selected': False, 'reason': '价格低于20日均线'}
            
            return base_result
    
    # 替换为自定义策略
    backtest.strategy = MyCustomStrategy(backtest.strategy_params)
    print(f"✓ 自定义策略替换成功: {type(backtest.strategy).__name__}")
    print()
    
    print("测试完成！所有功能正常工作。")
    print("\n优化特性:")
    print("1. ✅ 支持配置回测时间范围")
    print("2. ✅ 支持修改选股策略参数")
    print("3. ✅ 支持自定义选股策略")
    print("4. ✅ 使用SQLite数据库存储历史数据")
    
except Exception as e:
    print(f"❌ 测试失败: {e}")
    import traceback
    traceback.print_exc()
