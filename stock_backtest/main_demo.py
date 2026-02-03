import pandas as pd
import numpy as np
from datetime import datetime, timedelta
from typing import Dict
import warnings
warnings.filterwarnings('ignore')

from config import BACKTEST_CONFIG, SELECTION_CONFIG, RESULTS_DIR
import os

class MockDataGenerator:
    def __init__(self, seed=42):
        np.random.seed(seed)
        self.trading_days_per_year = 252

    def generate_stock_data(self, stock_code: str, start_date: str, end_date: str, 
                          drift: float = 0.0005, volatility: float = 0.02) -> pd.DataFrame:
        dates = pd.date_range(start=start_date, end=end_date, freq='B')
        dates = dates[dates.dayofweek < 5]
        
        n = len(dates)
        returns = np.random.normal(drift, volatility, n)
        
        prices = [100]
        for ret in returns[1:]:
            prices.append(prices[-1] * (1 + ret))
        
        volumes = np.random.lognormal(10, 0.5, n)
        
        df = pd.DataFrame({
            '日期': dates,
            '开盘': np.array(prices) * np.random.uniform(0.98, 1.02, n),
            '收盘': prices,
            '最高': np.array(prices) * np.random.uniform(1.0, 1.08, n),
            '最低': np.array(prices) * np.random.uniform(0.92, 1.0, n),
            '成交量': volumes,
            '涨跌幅': np.concatenate([[0], np.diff(prices) / np.array(prices[:-1])]) * 100,
            '换手率': np.random.uniform(2, 15, n),
        })
        
        df['涨跌幅'] = df['涨跌幅'].round(2)
        df['stock_code'] = stock_code
        
        return df

    def generate_stock_pool(self, num_stocks: int = 100, start_date: str = '2015-01-01', 
                          end_date: str = '2024-12-31') -> Dict[str, pd.DataFrame]:
        print(f"正在生成 {num_stocks} 只股票的模拟数据...")
        
        result = {}
        for i in range(num_stocks):
            stock_code = f"{str(i+1).zfill(6)}"
            drift = np.random.uniform(-0.001, 0.002)
            volatility = np.random.uniform(0.015, 0.035)
            
            df = self.generate_stock_data(stock_code, start_date, end_date, drift, volatility)
            result[stock_code] = df
            
            if (i + 1) % 10 == 0:
                print(f"已生成 {i+1}/{num_stocks} 只股票")
        
        print("模拟数据生成完成")
        return result

class LimitUpStrategyDemo:
    def __init__(self):
        self.config = {
            'limit_up_pct': 0.099,
            'min_close_after_limit': 0.70,
            'min_turnover': 0.05,
            'max_turnover': 0.10,
            'min_volume_ratio': 1.2,
            'days_to_check': 20,
        }

    def is_limit_up(self, pct_change: float) -> bool:
        return pct_change >= self.config['limit_up_pct'] * 100 * 0.95

    def count_limit_up_days(self, df: pd.DataFrame, end_idx: int) -> int:
        count = 0
        lookback = min(self.config['days_to_check'], end_idx + 1)
        for i in range(end_idx - lookback + 1, end_idx + 1):
            if self.is_limit_up(df.iloc[i]['涨跌幅']):
                count += 1
        return count

    def get_latest_limit_up_info(self, df: pd.DataFrame, end_idx: int):
        for i in range(end_idx, -1, -1):
            if self.is_limit_up(df.iloc[i]['涨跌幅']):
                return i, df.iloc[i]['收盘']
        return -1, 0

    def select_stock(self, df: pd.DataFrame, idx: int) -> Dict:
        if idx < 25:
            return {'selected': False, 'reason': '数据不足'}
        
        pct_change = df.iloc[idx]['涨跌幅']
        
        count = self.count_limit_up_days(df, idx)
        if count != 1:
            return {'selected': False, 'reason': f'涨停次数={count}'}
        
        limit_up_idx, limit_up_price = self.get_latest_limit_up_info(df, idx)
        
        if limit_up_idx == -1:
            return {'selected': False, 'reason': '未找到涨停'}
        
        if idx - limit_up_idx > 5 or idx - limit_up_idx < 1:
            return {'selected': False, 'reason': '涨停后时间不符合'}
        
        for i in range(limit_up_idx + 1, idx + 1):
            if df.iloc[i]['收盘'] < limit_up_price * self.config['min_close_after_limit']:
                return {'selected': False, 'reason': '涨停后价格跌破'}
        
        turnover = df.iloc[idx]['换手率'] / 100
        if not (self.config['min_turnover'] <= turnover <= self.config['max_turnover']):
            return {'selected': False, 'reason': f'换手率={turnover:.2%}'}
        
        if idx < 5:
            return {'selected': False, 'reason': '量比数据不足'}
        
        current_volume = df.iloc[idx]['成交量']
        avg_volume = df.iloc[idx-5:idx]['成交量'].mean()
        volume_ratio = current_volume / avg_volume if avg_volume > 0 else 0
        
        if volume_ratio < self.config['min_volume_ratio']:
            return {'selected': False, 'reason': f'量比={volume_ratio:.2f}'}
        
        return {
            'selected': True,
            'limit_up_price': limit_up_price,
            'current_price': df.iloc[idx]['收盘'],
            'turnover': turnover,
        }

class BacktestEngineDemo:
    def __init__(self, initial_capital: float = 1000000):
        self.initial_capital = initial_capital
        self.cash = initial_capital
        self.positions = {}
        self.trades = []
        self.portfolio_values = []
        self.dates = []
        self.commission = 0.0003
        self.slippage = 0.001
        self.stop_loss = -0.05
        self.take_profit_trigger = 0.10
        self.take_profit_fallback = -0.03
        self.max_holding_days = 7

    def buy(self, stock_code: str, price: float, date: pd.Timestamp):
        if len(self.positions) >= 5:
            return False
        
        position_value = self.cash * 0.15
        buy_price = price * (1 + self.slippage)
        quantity = int(position_value / buy_price / 100) * 100
        
        if quantity == 0:
            return False
        
        cost = buy_price * quantity * (1 + self.commission)
        if cost > self.cash:
            return False
        
        self.cash -= cost
        self.positions[stock_code] = {
            'entry_price': buy_price,
            'entry_date': date,
            'quantity': quantity,
            'highest_price': buy_price,
            'take_profit_active': False,
        }
        return True

    def sell(self, stock_code: str, price: float, date: pd.Timestamp, reason: str):
        if stock_code not in self.positions:
            return
        
        pos = self.positions[stock_code]
        sell_price = price * (1 - self.slippage)
        revenue = sell_price * pos['quantity'] * (1 - self.commission)
        
        self.cash += revenue
        pct_return = (sell_price - pos['entry_price']) / pos['entry_price']
        
        self.trades.append({
            'stock_code': stock_code,
            'entry_date': pos['entry_date'],
            'exit_date': date,
            'entry_price': pos['entry_price'],
            'exit_price': sell_price,
            'return_pct': pct_return,
            'holding_days': (date - pos['entry_date']).days,
            'reason': reason,
        })
        
        del self.positions[stock_code]

    def update(self, data: Dict[str, pd.DataFrame], date: pd.Timestamp):
        to_sell = []
        
        for stock_code, pos in self.positions.items():
            if stock_code not in data:
                continue
            
            df = data[stock_code]
            row = df[df['日期'] == date]
            if row.empty:
                continue
            
            current_price = row.iloc[0]['收盘']
            
            if current_price > pos['highest_price']:
                pos['highest_price'] = current_price
            
            pct_return = (current_price - pos['entry_price']) / pos['entry_price']
            holding_days = (date - pos['entry_date']).days
            
            if pct_return <= self.stop_loss:
                to_sell.append((stock_code, current_price, f'止损 {pct_return:.2%}'))
            elif pct_return >= self.take_profit_trigger:
                pos['take_profit_active'] = True
                drawdown = (pos['highest_price'] - current_price) / pos['highest_price']
                if drawdown >= abs(self.take_profit_fallback):
                    to_sell.append((stock_code, current_price, f'止盈 {pct_return:.2%}'))
            elif holding_days >= self.max_holding_days:
                to_sell.append((stock_code, current_price, f'时间止损 {holding_days}天'))
        
        for stock_code, price, reason in to_sell:
            self.sell(stock_code, price, date, reason)

    def run(self, data: Dict[str, pd.DataFrame], signals: pd.DataFrame):
        all_dates = sorted(signals['date'].unique())
        
        for date in all_dates:
            daily_signals = signals[signals['date'] == date]
            for _, signal in daily_signals.iterrows():
                self.buy(signal['stock_code'], signal['price'], date)
            
            self.update(data, date)
            
            portfolio_value = self.cash
            for stock_code, pos in self.positions.items():
                if stock_code in data:
                    df = data[stock_code]
                    row = df[df['日期'] == date]
                    if not row.empty:
                        portfolio_value += row.iloc[0]['收盘'] * pos['quantity']
            
            self.portfolio_values.append(portfolio_value)
            self.dates.append(date)
        
        for stock_code in list(self.positions.keys()):
            if stock_code in data:
                df = data[stock_code]
                last_date = df.iloc[-1]['日期']
                self.sell(stock_code, df.iloc[-1]['收盘'], last_date, '回测结束')

    def calculate_metrics(self):
        if not self.trades:
            return {}
        
        final_value = self.portfolio_values[-1] if self.portfolio_values else self.initial_capital
        total_return = (final_value - self.initial_capital) / self.initial_capital
        
        days = len(self.portfolio_values)
        years = days / 252
        annualized_return = (1 + total_return) ** (1 / years) - 1 if years > 0 else 0
        
        returns = []
        for i in range(1, len(self.portfolio_values)):
            returns.append((self.portfolio_values[i] - self.portfolio_values[i-1]) / self.portfolio_values[i-1])
        
        if returns:
            volatility = np.std(returns) * np.sqrt(252)
            excess_returns = np.array(returns) - 0.03 / 252
            sharpe_ratio = np.mean(excess_returns) / np.std(excess_returns) * np.sqrt(252) if np.std(excess_returns) > 0 else 0
        else:
            volatility = 0
            sharpe_ratio = 0
        
        win_trades = [t for t in self.trades if t['return_pct'] > 0]
        lose_trades = [t for t in self.trades if t['return_pct'] <= 0]
        
        win_rate = len(win_trades) / len(self.trades) if self.trades else 0
        
        values = np.array(self.portfolio_values)
        peak = np.maximum.accumulate(values)
        drawdown = (values - peak) / peak
        max_drawdown = np.min(drawdown) if len(drawdown) > 0 else 0
        
        trades_df = pd.DataFrame(self.trades)
        avg_holding_days = trades_df['holding_days'].mean() if not trades_df.empty else 0
        
        return {
            'initial_capital': self.initial_capital,
            'final_value': final_value,
            'total_return': total_return,
            'annualized_return': annualized_return,
            'volatility': volatility,
            'sharpe_ratio': sharpe_ratio,
            'max_drawdown': max_drawdown,
            'total_trades': len(self.trades),
            'win_rate': win_rate,
            'win_trades': len(win_trades),
            'lose_trades': len(lose_trades),
            'avg_win': np.mean([t['return_pct'] for t in win_trades]) if win_trades else 0,
            'avg_loss': np.mean([t['return_pct'] for t in lose_trades]) if lose_trades else 0,
            'avg_holding_days': avg_holding_days,
        }

def main():
    print("=" * 80)
    print("A股涨停短线策略回测（模拟数据演示）")
    print("=" * 80)
    print()
    print("注意：由于网络连接问题，本回测使用模拟数据演示框架功能")
    print("实际使用时请确保网络连接正常，使用真实A股数据")
    print()
    
    generator = MockDataGenerator(seed=42)
    data = generator.generate_stock_pool(num_stocks=100, start_date='2015-01-01', end_date='2024-12-31')
    
    print()
    print("步骤2: 生成选股信号...")
    strategy = LimitUpStrategyDemo()
    signals = []
    
    for stock_code, df in data.items():
        for idx in range(25, len(df)):
            result = strategy.select_stock(df, idx)
            if result['selected']:
                signals.append({
                    'stock_code': stock_code,
                    'date': df.iloc[idx]['日期'],
                    'price': result['current_price'],
                })
    
    signals_df = pd.DataFrame(signals)
    print(f"共生成 {len(signals_df)} 个信号")
    print()
    
    if not signals_df.empty:
        print("步骤3: 运行回测...")
        engine = BacktestEngineDemo(1000000)
        engine.run(data, signals_df)
        
        print()
        print("步骤4: 计算绩效指标...")
        metrics = engine.calculate_metrics()
        
        print()
        print("=" * 80)
        print("回测结果（模拟数据）")
        print("=" * 80)
        print()
        print(f"初始资金:     {metrics['initial_capital']:,.2f} 元")
        print(f"最终资金:     {metrics['final_value']:,.2f} 元")
        print(f"总收益率:     {metrics['total_return']:.2%}")
        print(f"年化收益率:   {metrics['annualized_return']:.2%}")
        print(f"年化波动率:   {metrics['volatility']:.2%}")
        print(f"夏普比率:     {metrics['sharpe_ratio']:.2f}")
        print(f"最大回撤:     {metrics['max_drawdown']:.2%}")
        print()
        print(f"总交易次数:   {metrics['total_trades']}")
        print(f"盈利次数:     {metrics['win_trades']}")
        print(f"亏损次数:     {metrics['lose_trades']}")
        print(f"胜率:         {metrics['win_rate']:.2%}")
        print(f"平均盈利:     {metrics['avg_win']:.2%}")
        print(f"平均亏损:     {metrics['avg_loss']:.2%}")
        print(f"平均持仓天数: {metrics['avg_holding_days']:.1f} 天")
        print("=" * 80)
    else:
        print("未生成任何交易信号")

if __name__ == "__main__":
    main()
