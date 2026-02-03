#!/usr/bin/env python3
# 使用热门股票测试自定义策略

import pandas as pd
from a_stock_backtest_optimized import AStockBacktest
from custom_strategy import CustomLimitUpStrategy

if __name__ == '__main__':
    print("=" * 80)
    print("测试热门股票")
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
            # 打印涨停情况
            pct_change = df.iloc[idx].get('涨跌幅', 0)
            is_limit_up = self.is_limit_up(pct_change)
            
            # 计算20天内涨停次数
            limit_up_count = self.count_limit_up_days(df, idx)
            
            # 打印详细信息
            if is_limit_up:
                print(f"{df.iloc[idx]['日期']} {df.iloc[idx]['名称']}: 涨停！涨幅: {pct_change:.2f}%, 20天内涨停次数: {limit_up_count}")
            
            result = super().select_stock(df, idx)
            if not result['selected'] and limit_up_count > 0:
                print(f"{df.iloc[idx]['日期']} {df.iloc[idx]['名称']}: 涨停次数={limit_up_count}, 但{result['reason']}")
            return result

    backtest.strategy = DebugCustomStrategy(backtest.strategy_params)
    print(f"策略替换成功: {type(backtest.strategy).__name__}")
    print()

    # 选择可能更容易涨停的股票
    hot_stocks = ['sz300750', 'sh601899', 'sz002759', 'sh603505', 'sz300676']
    print(f"测试股票池: {hot_stocks}")
    print("开始回测...")
    print()

    # 运行回测
    results = backtest.run(stock_pool=hot_stocks, verbose=True)

    if results:
        print()
        print("=" * 80)
        print("回测完成！")
        print("=" * 80)
        print()
        
        # 显示核心指标
        print("核心指标:")
        print(f"总收益率: {results.get('total_return', 0):.2%}")
        print(f"年化收益率: {results.get('annualized_return', 0):.2%}")
        print(f"夏普比率: {results.get('sharpe_ratio', 0):.2f}")
        print(f"最大回撤: {results.get('max_drawdown', 0):.2%}")
        print(f"总交易次数: {results.get('total_trades', 0)}")
        print(f"胜率: {results.get('win_rate', 0):.2%}")
        
        # 保存结果
        backtest.plot_results(results, save_path='hot_stock_results.png')
        print("\n回测结果图表已保存到: hot_stock_results.png")
    else:
        print("未生成回测结果，可能是没有股票满足条件")

    print()
    print("=" * 80)
    print("测试完成！")
    print("=" * 80)
