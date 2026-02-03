#!/usr/bin/env python3
# 测试自定义选股策略

from a_stock_backtest_optimized import AStockBacktest
from custom_strategy import CustomLimitUpStrategy

if __name__ == '__main__':
    print("=" * 80)
    print("测试自定义选股策略")
    print("=" * 80)
    print()

    # 创建回测实例
    backtest = AStockBacktest(
        years=[2020, 2021],  # 只回测这两年的数据
        initial_capital=1000000,
        stop_loss_pct=-0.08,      # 止损8%
        take_profit_trigger=0.15,  # 止盈触发15%
        take_profit_fallback=-0.05, # 回落5%止盈
        max_holding_days=10,       # 最长持仓10天
        max_positions=3,           # 最大持仓3只
        position_size=0.3          # 单股仓位30%
    )

    # 替换为自定义策略
    backtest.strategy = CustomLimitUpStrategy(backtest.strategy_params)
    print(f"策略替换成功: {type(backtest.strategy).__name__}")
    print(f"策略参数: {backtest.strategy.params}")
    print()

    # 运行回测（使用小股票池测试）
    test_stocks = ['sh600000', 'sz000001', 'sh600519', 'sz300750', 'sh601318']
    print("开始回测...")
    print(f"测试股票池: {test_stocks}")
    print()

    results = backtest.run(stock_pool=test_stocks, verbose=True)

    if results:
        print()
        print("=" * 80)
        print("回测完成！")
        print("=" * 80)
        print()
        
        # 保存结果
        backtest.plot_results(results, save_path='custom_strategy_results.png')
        print("回测结果图表已保存到: custom_strategy_results.png")
        print()
        
        # 显示核心指标
        print("核心指标:")
        print(f"总收益率: {results.get('total_return', 0):.2%}")
        print(f"年化收益率: {results.get('annualized_return', 0):.2%}")
        print(f"夏普比率: {results.get('sharpe_ratio', 0):.2f}")
        print(f"最大回撤: {results.get('max_drawdown', 0):.2%}")
        print(f"总交易次数: {results.get('total_trades', 0)}")
        print(f"胜率: {results.get('win_rate', 0):.2%}")
    else:
        print("未生成回测结果")

    print()
    print("=" * 80)
    print("测试完成！")
    print("=" * 80)
