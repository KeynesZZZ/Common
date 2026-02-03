#!/usr/bin/env python3
# 最终版自定义选股策略

import pandas as pd
from a_stock_backtest_optimized import AStockBacktest, LimitUpStrategy

class FinalCustomLimitUpStrategy(LimitUpStrategy):
    """
    自定义选股策略
    条件：
    1. 20天内只有一次涨停
    2. 非ST股票
    3. 涨停后三天内收盘价在涨停价2/3以上
    4. 换手率5-10%
    5. 量比大于1
    """
    def __init__(self, params):
        super().__init__(params)
        # 添加额外的策略参数
        self.params.update({
            'max_limit_up_days': 1,  # 20天内最多涨停次数
            'min_close_ratio': 2/3,  # 涨停后收盘价最低要求（相对于涨停价）
            'min_turnover': 5,       # 最小换手率(%)
            'max_turnover': 10,      # 最大换手率(%)
            'min_volume_ratio': 1,    # 最小量比
            'check_days_after_limit': 3,  # 涨停后观察天数
        })
    
    def is_stock(self, stock_name):
        """判断是否为ST股票"""
        if isinstance(stock_name, str):
            return 'ST' in stock_name or '*ST' in stock_name
        return False
    
    def calculate_turnover(self, df, idx):
        """计算换手率"""
        # 假设数据中包含换手率列
        if '换手率' in df.columns:
            return df.iloc[idx].get('换手率', 0)
        # 如果没有换手率列，返回0
        return 0
    
    def calculate_volume_ratio(self, df, idx):
        """计算量比"""
        if idx < 5:
            return 0
        
        current_volume = df.iloc[idx].get('成交量', 0)
        avg_volume = df.iloc[max(0, idx-5):idx]['成交量'].mean()
        
        if avg_volume > 0 and current_volume > 0:
            return current_volume / avg_volume
        return 0
    
    def check_post_limit_performance(self, df, limit_up_idx, current_idx):
        """检查涨停后的表现"""
        if current_idx - limit_up_idx > self.params['check_days_after_limit']:
            return False
        
        limit_up_price = df.iloc[limit_up_idx]['收盘价']
        min_required_price = limit_up_price * self.params['min_close_ratio']
        
        # 检查涨停后三天内的收盘价
        for i in range(limit_up_idx + 1, min(current_idx + 1, len(df))):
            close_price = df.iloc[i]['收盘价']
            if close_price < min_required_price:
                return False
        
        return True
    
    def select_stock(self, df, idx):
        """选股逻辑"""
        if idx < 25:
            return {'selected': False, 'reason': '数据不足'}
        
        # 检查是否为ST股票
        stock_name = df.iloc[idx].get('名称', '')
        if self.is_stock(stock_name):
            return {'selected': False, 'reason': 'ST股票'}
        
        # 检查20天内涨停次数
        count = self.count_limit_up_days(df, idx)
        if count != self.params['max_limit_up_days']:
            return {'selected': False, 'reason': f'涨停次数={count}'}
        
        # 获取最近一次涨停信息
        limit_up_idx, limit_up_price = self.get_latest_limit_up_info(df, idx)
        if limit_up_idx == -1:
            return {'selected': False, 'reason': '未找到涨停'}
        
        # 检查涨停后的表现
        if not self.check_post_limit_performance(df, limit_up_idx, idx):
            return {'selected': False, 'reason': '涨停后表现不佳'}
        
        # 计算换手率
        turnover = self.calculate_turnover(df, idx)
        if turnover < self.params['min_turnover'] or turnover > self.params['max_turnover']:
            return {'selected': False, 'reason': f'换手率={turnover:.2f}%'}
        
        # 计算量比
        volume_ratio = self.calculate_volume_ratio(df, idx)
        if volume_ratio < self.params['min_volume_ratio']:
            return {'selected': False, 'reason': f'量比={volume_ratio:.2f}'}
        
        # 所有条件满足
        return {
            'selected': True,
            'limit_up_price': limit_up_price,
            'current_price': df.iloc[idx]['收盘价'],
            'volume_ratio': volume_ratio,
            'turnover': turnover,
            'limit_up_date': df.iloc[limit_up_idx]['日期'],
        }

if __name__ == '__main__':
    print("=" * 80)
    print("最终版自定义选股策略")
    print("=" * 80)
    print()

    # 创建回测实例
    backtest = AStockBacktest(
        years=[2020, 2021],
        initial_capital=1000000,
        stop_loss_pct=-0.08,      # 止损8%
        take_profit_trigger=0.15,  # 止盈触发15%
        take_profit_fallback=-0.05, # 回落5%止盈
        max_holding_days=10,       # 最长持仓10天
        max_positions=3,           # 最大持仓3只
        position_size=0.3          # 单股仓位30%
    )

    # 替换为最终版自定义策略
    backtest.strategy = FinalCustomLimitUpStrategy(backtest.strategy_params)
    print(f"策略替换成功: {type(backtest.strategy).__name__}")
    print("策略条件:")
    print("1. 20天内只有一次涨停")
    print("2. 非ST股票")
    print("3. 涨停后三天内收盘价在涨停价2/3以上")
    print("4. 换手率5-10%")
    print("5. 量比大于1")
    print()

    # 运行全市场回测（使用小批量测试）
    print("开始回测...")
    print("使用数据库中的历史数据")
    print()

    # 使用较小的批处理大小以加快测试
    results = backtest.run(batch_size=50, verbose=True)

    if results:
        print()
        print("=" * 80)
        print("回测完成！")
        print("=" * 80)
        print()
        
        # 保存结果
        backtest.plot_results(results, save_path='final_strategy_results.png')
        print("回测结果图表已保存到: final_strategy_results.png")
        print()
        
        # 显示核心指标
        print("核心指标:")
        print(f"总收益率: {results.get('total_return', 0):.2%}")
        print(f"年化收益率: {results.get('annualized_return', 0):.2%}")
        print(f"夏普比率: {results.get('sharpe_ratio', 0):.2f}")
        print(f"最大回撤: {results.get('max_drawdown', 0):.2%}")
        print(f"总交易次数: {results.get('total_trades', 0)}")
        print(f"胜率: {results.get('win_rate', 0):.2%}")
        print(f"平均持仓天数: {results.get('avg_holding_days', 0):.1f} 天")
    else:
        print("未生成回测结果，可能是没有股票满足条件")

    print()
    print("=" * 80)
    print("测试完成！")
    print("=" * 80)
    print()
    print("使用方法:")
    print("1. 导入策略: from final_custom_strategy import FinalCustomLimitUpStrategy")
    print("2. 创建回测实例并替换策略")
    print("3. 运行回测: results = backtest.run()")
