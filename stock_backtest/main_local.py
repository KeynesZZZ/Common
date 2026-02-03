import zipfile
import pandas as pd
import numpy as np
from datetime import datetime
from typing import Dict, List
import os
import io

DATA_DIR = r'D:\BaiduNetdiskDownload\沪深个股60分钟_按年汇总'

class LocalDataLoader:
    def __init__(self, data_dir: str = DATA_DIR):
        self.data_dir = data_dir

    def get_stock_list_from_zip(self, year: int) -> List[str]:
        zip_path = os.path.join(self.data_dir, f'{year}_60min.zip')
        if not os.path.exists(zip_path):
            return []
        
        with zipfile.ZipFile(zip_path) as z:
            filenames = z.namelist()
            stock_codes = [f.split('_')[0] for f in filenames if f.endswith('.csv')]
            return list(set(stock_codes))

    def load_stock_data(self, stock_code: str, years: List[int]) -> pd.DataFrame:
        all_data = []
        
        for year in years:
            zip_path = os.path.join(self.data_dir, f'{year}_60min.zip')
            if not os.path.exists(zip_path):
                continue
            
            with zipfile.ZipFile(zip_path) as z:
                filename = f'{stock_code}_{year}.csv'
                if filename not in z.namelist():
                    continue
                
                data = z.read(filename)
                try:
                    df = pd.read_csv(io.BytesIO(data), encoding='gbk')
                except UnicodeDecodeError:
                    try:
                        df = pd.read_csv(io.BytesIO(data), encoding='utf-8')
                    except UnicodeDecodeError:
                        df = pd.read_csv(io.BytesIO(data), encoding='gb18030')
                all_data.append(df)
        
        if not all_data:
            return pd.DataFrame()
        
        df = pd.concat(all_data, ignore_index=True)
        df = df.sort_values('时间').reset_index(drop=True)
        return df

    def convert_to_daily(self, df: pd.DataFrame) -> pd.DataFrame:
        if df.empty:
            return pd.DataFrame()
        
        df['日期'] = pd.to_datetime(df['时间']).dt.date
        df['日期'] = pd.to_datetime(df['日期'])
        
        daily = df.groupby('日期').agg({
            '代码': 'first',
            '名称': 'first',
            '开盘价': 'first',
            '收盘价': 'last',
            '最高价': 'max',
            '最低价': 'min',
            '成交量': 'sum',
            '成交额': 'sum',
        }).reset_index()
        
        daily['涨幅'] = daily['收盘价'].pct_change() * 100
        daily['振幅'] = ((daily['最高价'] - daily['最低价']) / daily['收盘价'].shift(1) * 100).fillna(0)
        
        return daily

class LimitUpStrategy:
    def __init__(self):
        self.config = {
            'limit_up_pct': 9.9,
            'min_close_after_limit': 0.70,
            'min_turnover': 0.05,
            'max_turnover': 0.10,
            'min_volume_ratio': 1.2,
            'days_to_check': 20,
        }

    def is_limit_up(self, pct_change: float) -> bool:
        return pct_change >= self.config['limit_up_pct'] * 0.95

    def count_limit_up_days(self, df: pd.DataFrame, end_idx: int) -> int:
        count = 0
        lookback = min(self.config['days_to_check'], end_idx + 1)
        for i in range(end_idx - lookback + 1, end_idx + 1):
            if i >= 0 and i < len(df):
                pct = df.iloc[i]['涨幅']
                if pd.notna(pct) and self.is_limit_up(pct):
                    count += 1
        return count

    def get_latest_limit_up_info(self, df: pd.DataFrame, end_idx: int):
        for i in range(end_idx, -1, -1):
            if i >= 0 and i < len(df):
                pct = df.iloc[i]['涨幅']
                if pd.notna(pct) and self.is_limit_up(pct):
                    return i, df.iloc[i]['收盘价']
        return -1, 0

    def select_stock(self, df: pd.DataFrame, idx: int) -> Dict:
        if idx < 25:
            return {'selected': False, 'reason': '数据不足'}
        
        pct_change = df.iloc[idx]['涨幅']
        if pd.isna(pct_change):
            return {'selected': False, 'reason': '涨幅为空'}
        
        count = self.count_limit_up_days(df, idx)
        if count != 1:
            return {'selected': False, 'reason': f'涨停次数={count}'}
        
        limit_up_idx, limit_up_price = self.get_latest_limit_up_info(df, idx)
        
        if limit_up_idx == -1:
            return {'selected': False, 'reason': '未找到涨停'}
        
        if idx - limit_up_idx > 5 or idx - limit_up_idx < 1:
            return {'selected': False, 'reason': '涨停后时间不符合'}
        
        for i in range(limit_up_idx + 1, min(idx + 1, len(df))):
            if df.iloc[i]['收盘价'] < limit_up_price * self.config['min_close_after_limit']:
                return {'selected': False, 'reason': '涨停后价格跌破'}
        
        volume_current = df.iloc[idx]['成交量']
        volume_avg = df.iloc[max(0, idx-5):idx]['成交量'].mean()
        volume_ratio = volume_current / volume_avg if volume_avg > 0 and volume_current > 0 else 0
        
        if volume_ratio < self.config['min_volume_ratio']:
            return {'selected': False, 'reason': f'量比={volume_ratio:.2f}'}
        
        return {
            'selected': True,
            'limit_up_price': limit_up_price,
            'current_price': df.iloc[idx]['收盘价'],
            'volume_ratio': volume_ratio,
        }

class BacktestEngine:
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
            
            current_price = row.iloc[0]['收盘价']
            
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
                        portfolio_value += row.iloc[0]['收盘价'] * pos['quantity']
            
            self.portfolio_values.append(portfolio_value)
            self.dates.append(date)
        
        for stock_code in list(self.positions.keys()):
            if stock_code in data:
                df = data[stock_code]
                last_date = df.iloc[-1]['日期']
                self.sell(stock_code, df.iloc[-1]['收盘价'], last_date, '回测结束')

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
    print("A股涨停短线策略回测（使用本地数据）")
    print("=" * 80)
    
    loader = LocalDataLoader()
    years = list(range(2015, 2025))
    
    print("\n步骤1: 获取股票池...")
    all_stock_codes = set()
    for year in years:
        codes = loader.get_stock_list_from_zip(year)
        all_stock_codes.update(codes)
    
    stock_pool = list(all_stock_codes)[:100]
    print(f"总股票数: {len(all_stock_codes)}")
    print(f"股票池大小: {len(stock_pool)}")
    
    print("\n步骤2: 加载并转换数据...")
    data = {}
    for i, stock_code in enumerate(stock_pool):
        df = loader.load_stock_data(stock_code, years)
        if not df.empty:
            daily_df = loader.convert_to_daily(df)
            if not daily_df.empty:
                data[stock_code] = daily_df
        
        if (i + 1) % 10 == 0:
            print(f"已加载 {i+1}/{len(stock_pool)} 只股票")
    
    print(f"成功加载 {len(data)} 只股票的日线数据")
    
    print("\n步骤3: 生成选股信号...")
    strategy = LimitUpStrategy()
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
    
    if not signals_df.empty:
        print("\n步骤4: 运行回测...")
        engine = BacktestEngine(1000000)
        engine.run(data, signals_df)
        
        print("\n步骤5: 计算绩效指标...")
        metrics = engine.calculate_metrics()
        
        print()
        print("=" * 80)
        print("回测结果")
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
