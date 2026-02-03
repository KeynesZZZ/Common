import pandas as pd
import numpy as np
from datetime import datetime, timedelta
from typing import Dict
import warnings
warnings.filterwarnings('ignore')

from data_fetcher import DataFetcher
from strategy import LimitUpStrategy
from backtest import BacktestEngine
from config import BACKTEST_CONFIG, SELECTION_CONFIG, RESULTS_DIR
import os

class BacktestRunner:
    def __init__(self):
        self.data_fetcher = DataFetcher()
        self.strategy = LimitUpStrategy()

    def generate_signals(self, data: Dict[str, pd.DataFrame]) -> pd.DataFrame:
        print("开始生成选股信号...")
        
        all_signals = []
        
        for stock_code, df in data.items():
            df = df.sort_values('日期').reset_index(drop=True)
            
            start_idx = 25
            for idx in range(start_idx, len(df)):
                current_date = df.iloc[idx]['日期']
                
                count = self.strategy.count_limit_up_days(df.iloc[:idx+1], SELECTION_CONFIG['days_to_check'])
                if count != 1:
                    continue
                
                limit_up_idx, limit_up_price = self.strategy.get_latest_limit_up_day(df.iloc[:idx+1])
                
                if limit_up_idx == -1:
                    continue
                
                if not self.strategy.check_price_after_limit_up(df.iloc[:idx+1], limit_up_idx, limit_up_price):
                    continue
                
                if not self.strategy.check_turnover(df.iloc[:idx+1], idx):
                    continue
                
                if not self.strategy.check_volume_ratio(df.iloc[:idx+1], idx):
                    continue
                
                if not self.strategy.check_ma_trend(df.iloc[:idx+1], idx):
                    continue
                
                all_signals.append({
                    'stock_code': stock_code,
                    'date': current_date,
                    'price': df.iloc[idx]['收盘'],
                    'limit_up_price': limit_up_price,
                    'turnover': df.iloc[idx]['换手率'] / 100,
                })
        
        signals_df = pd.DataFrame(all_signals)
        if not signals_df.empty:
            signals_df = signals_df.sort_values('date')
        
        print(f"共生成 {len(signals_df)} 个信号")
        return signals_df

    def run_backtest(self) -> Dict:
        print("=" * 80)
        print("A股涨停短线策略回测")
        print("=" * 80)
        print(f"回测期间: {BACKTEST_CONFIG['start_date']} 至 {BACKTEST_CONFIG['end_date']}")
        print(f"初始资金: {BACKTEST_CONFIG['initial_capital']:,.0f} 元")
        print()
        
        print("步骤1: 获取股票池...")
        stock_codes = self.data_fetcher.get_stock_pool(
            min_market_cap=SELECTION_CONFIG['min_market_cap'],
            max_market_cap=SELECTION_CONFIG['max_market_cap'],
            min_listed_days=SELECTION_CONFIG['min_listed_days'],
            pool_size=BACKTEST_CONFIG['stock_pool_size']
        )
        print(f"股票池大小: {len(stock_codes)} 只股票")
        print()
        
        if not stock_codes:
            print("无法获取股票池，回测终止")
            return {}
        
        print("步骤2: 获取历史数据...")
        data = self.data_fetcher.get_multiple_stocks_history(
            stock_codes,
            BACKTEST_CONFIG['start_date'],
            BACKTEST_CONFIG['end_date']
        )
        print(f"成功获取 {len(data)} 只股票的历史数据")
        print()
        
        if not data:
            print("无法获取历史数据，回测终止")
            return {}
        
        print("步骤3: 生成交易信号...")
        signals_df = self.generate_signals(data)
        
        if signals_df.empty:
            print("未生成任何交易信号，回测终止")
            return {}
        
        print()
        print("步骤4: 运行回测...")
        engine = BacktestEngine(BACKTEST_CONFIG['initial_capital'])
        engine.run(data, signals_df)
        
        print()
        print("步骤5: 计算绩效指标...")
        metrics = engine.calculate_metrics()
        
        self.print_results(metrics)
        
        self.save_results(metrics, engine.get_trades_df(), engine.get_equity_curve(), signals_df)
        
        return metrics

    def print_results(self, metrics: Dict):
        print()
        print("=" * 80)
        print("回测结果")
        print("=" * 80)
        print()
        print(f"初始资金:     {metrics.get('initial_capital', 0):,.2f} 元")
        print(f"最终资金:     {metrics.get('final_value', 0):,.2f} 元")
        print(f"总收益率:     {metrics.get('total_return', 0):.2%}")
        print(f"年化收益率:   {metrics.get('annualized_return', 0):.2%}")
        print(f"年化波动率:   {metrics.get('volatility', 0):.2%}")
        print(f"夏普比率:     {metrics.get('sharpe_ratio', 0):.2f}")
        print(f"最大回撤:     {metrics.get('max_drawdown', 0):.2%}")
        print()
        print(f"总交易次数:   {metrics.get('total_trades', 0)}")
        print(f"盈利次数:     {metrics.get('win_trades', 0)}")
        print(f"亏损次数:     {metrics.get('lose_trades', 0)}")
        print(f"胜率:         {metrics.get('win_rate', 0):.2%}")
        print(f"平均盈利:     {metrics.get('avg_win', 0):.2%}")
        print(f"平均亏损:     {metrics.get('avg_loss', 0):.2%}")
        print(f"盈亏比:       {metrics.get('profit_factor', 0):.2f}")
        print(f"平均持仓天数: {metrics.get('avg_holding_days', 0):.1f} 天")
        print("=" * 80)

    def save_results(self, metrics: Dict, trades_df: pd.DataFrame, equity_df: pd.DataFrame, signals_df: pd.DataFrame):
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        
        metrics_df = pd.DataFrame([metrics])
        metrics_df.to_csv(os.path.join(RESULTS_DIR, f'metrics_{timestamp}.csv'), index=False, encoding='utf-8-sig')
        
        if not trades_df.empty:
            trades_df.to_csv(os.path.join(RESULTS_DIR, f'trades_{timestamp}.csv'), index=False, encoding='utf-8-sig')
        
        if not equity_df.empty:
            equity_df.to_csv(os.path.join(RESULTS_DIR, f'equity_{timestamp}.csv'), index=False, encoding='utf-8-sig')
        
        if not signals_df.empty:
            signals_df.to_csv(os.path.join(RESULTS_DIR, f'signals_{timestamp}.csv'), index=False, encoding='utf-8-sig')
        
        print()
        print(f"结果已保存到: {RESULTS_DIR}")

if __name__ == "__main__":
    runner = BacktestRunner()
    results = runner.run_backtest()
