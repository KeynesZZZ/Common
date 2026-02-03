import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
import zipfile
import pandas as pd
import numpy as np
from datetime import datetime
from typing import Dict, List, Optional
import os
import io
from multiprocessing import Pool, cpu_count
from data_db_cache import DatabaseCache

plt.rcParams['font.sans-serif'] = ['SimHei', 'Microsoft YaHei']
plt.rcParams['axes.unicode_minus'] = False

class AStockBacktest:
    def __init__(
        self,
        data_dir: str = r'D:\BaiduNetdiskDownload\沪深个股60分钟_按年汇总',
        initial_capital: float = 1000000,
        stop_loss_pct: float = -0.05,
        take_profit_trigger: float = 0.10,
        take_profit_fallback: float = -0.03,
        max_holding_days: int = 7,
        min_holding_days: int = 3,
        max_positions: int = 5,
        position_size: float = 0.15,
        commission: float = 0.0003,
        slippage: float = 0.001,
        years: List[int] = None
    ):
        self.data_dir = data_dir
        self.initial_capital = initial_capital
        
        self.stop_loss_pct = stop_loss_pct
        self.take_profit_trigger = take_profit_trigger
        self.take_profit_fallback = take_profit_fallback
        self.max_holding_days = max_holding_days
        self.min_holding_days = min_holding_days
        self.max_positions = max_positions
        self.position_size = position_size
        self.commission = commission
        self.slippage = slippage
        self.years = years or list(range(2015, 2025))
        
        self.strategy_params = {
            'limit_up_pct': 9.9,
            'min_close_after_limit': 0.70,
            'min_volume_ratio': 1.2,
            'days_to_check': 20,
        }
        
        self.strategy = LimitUpStrategy(self.strategy_params)
        self.data_cache = DatabaseCache('stock_data.db')

    def run(self, stock_pool: Optional[List[str]] = None, verbose: bool = True, batch_size: int = 100) -> Dict:
        if verbose:
            print("=" * 80)
            print("A股策略回测")
            print("=" * 80)
        
        data = self._load_data(stock_pool, verbose, batch_size)
        if not data:
            if verbose:
                print("未加载到任何数据，回测终止")
            return {}
        
        signals = self._generate_signals(data, verbose)
        if signals.empty:
            if verbose:
                print("未生成任何交易信号，回测终止")
            return {}
        
        engine = BacktestEngine(
            initial_capital=self.initial_capital,
            stop_loss_pct=self.stop_loss_pct,
            take_profit_trigger=self.take_profit_trigger,
            take_profit_fallback=self.take_profit_fallback,
            max_holding_days=self.max_holding_days,
            max_positions=self.max_positions,
            position_size=self.position_size,
            commission=self.commission,
            slippage=self.slippage
        )
        
        engine.run(data, signals)
        results = engine.calculate_metrics()
        
        if verbose:
            self._print_results(results)
        
        results['trades'] = engine.trades
        results['equity_curve'] = engine.equity_curve
        results['daily_returns'] = engine.daily_returns
        
        return results

    def _load_data(self, stock_pool: Optional[List[str]], verbose: bool, batch_size: int) -> Dict:
        if verbose:
            print(f"\n加载数据...")
        
        all_stock_codes = set()
        for year in self.years:
            zip_path = os.path.join(self.data_dir, f'{year}_60min.zip')
            if os.path.exists(zip_path):
                with zipfile.ZipFile(zip_path) as z:
                    filenames = z.namelist()
                    codes = [f.split('_')[0] for f in filenames if f.endswith('.csv')]
                    all_stock_codes.update(codes)
        
        stock_codes = stock_pool or list(all_stock_codes)
        
        if verbose:
            print(f"总股票数: {len(all_stock_codes)}")
            print(f"股票池大小: {len(stock_codes)}")
            print(f"\n加载历史数据并转换为日线...")
        
        data = {}
        total = len(stock_codes)
        
        for i in range(0, total, batch_size):
            batch_codes = stock_codes[i:i+batch_size]
            
            with Pool(min(cpu_count(), 4)) as pool:
                batch_results = pool.map(self._process_single_stock, batch_codes)
            
            for stock_code, daily_df in batch_results:
                if not daily_df.empty:
                    data[stock_code] = daily_df
            
            if verbose:
                processed = min(i + batch_size, total)
                print(f"已加载 {processed}/{total} 只股票")
        
        if verbose:
            print(f"成功加载 {len(data)} 只股票的日线数据\n")
        
        return data

    def _process_single_stock(self, stock_code: str) -> tuple:
        if self.data_cache.is_cached(stock_code, self.years):
            daily_df = self.data_cache.load_from_cache(stock_code, self.years)
            if not daily_df.empty:
                return stock_code, daily_df
        
        df = self._load_single_stock(stock_code)
        if df.empty:
            return stock_code, pd.DataFrame()
        
        daily_df = self._convert_to_daily(df)
        self.data_cache.save_to_cache(stock_code, self.years, daily_df)
        return stock_code, daily_df

    def _load_single_stock(self, stock_code: str) -> pd.DataFrame:
        all_data = []
        
        for year in self.years:
            zip_path = os.path.join(self.data_dir, f'{year}_60min.zip')
            if not os.path.exists(zip_path):
                continue
            
            try:
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
            except Exception:
                pass
        
        if not all_data:
            return pd.DataFrame()
        
        df = pd.concat(all_data, ignore_index=True)
        df = df.sort_values('时间').reset_index(drop=True)
        return df

    def _convert_to_daily(self, df: pd.DataFrame) -> pd.DataFrame:
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
        
        daily['涨跌幅'] = daily['收盘价'].pct_change() * 100
        daily['振幅'] = ((daily['最高价'] - daily['最低价']) / daily['收盘价'].shift(1) * 100).fillna(0)
        
        return daily

    def _generate_signals(self, data: Dict, verbose: bool) -> pd.DataFrame:
        if verbose:
            print("生成选股信号...")
        
        signals = []
        
        for stock_code, df in data.items():
            if len(df) < 25:
                continue
            
            for idx in range(25, len(df)):
                result = self.strategy.select_stock(df, idx)
                if result['selected']:
                    signals.append({
                        'stock_code': stock_code,
                        'date': df.iloc[idx]['日期'],
                        'price': result['current_price'],
                        'limit_up_price': result['limit_up_price'],
                        'volume_ratio': result['volume_ratio'],
                    })
        
        signals_df = pd.DataFrame(signals)
        
        if verbose:
            print(f"共生成 {len(signals_df)} 个信号\n")
        
        return signals_df

    def _print_results(self, metrics: Dict):
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

    def plot_results(self, results: Dict, save_path: Optional[str] = None):
        fig, axes = plt.subplots(2, 2, figsize=(15, 10))
        fig.suptitle('策略回测结果', fontsize=16, fontweight='bold')
        
        equity = results.get('equity_curve', [])
        if equity:
            dates = [e['date'] for e in equity]
            values = [e['portfolio_value'] for e in equity]
            
            axes[0, 0].plot(dates, values, linewidth=2)
            axes[0, 0].axhline(y=self.initial_capital, color='r', linestyle='--', alpha=0.5, label='初始资金')
            axes[0, 0].set_title('资金曲线', fontsize=12, fontweight='bold')
            axes[0, 0].set_xlabel('日期')
            axes[0, 0].set_ylabel('资金（元）')
            axes[0, 0].legend()
            axes[0, 0].grid(True, alpha=0.3)
            axes[0, 0].tick_params(axis='x', rotation=45)
            
            daily_returns = results.get('daily_returns', [])
            if daily_returns:
                axes[0, 1].hist(daily_returns, bins=50, edgecolor='black', alpha=0.7)
                axes[0, 1].axvline(x=0, color='r', linestyle='--', linewidth=2)
                axes[0, 1].set_title('每日收益率分布', fontsize=12, fontweight='bold')
                axes[0, 1].set_xlabel('收益率')
                axes[0, 1].set_ylabel('频数')
                axes[0, 1].grid(True, alpha=0.3, axis='y')
        
        trades = results.get('trades', [])
        if trades:
            returns = [t['return_pct'] for t in trades]
            colors = ['green' if r > 0 else 'red' for r in returns]
            axes[1, 0].bar(range(len(returns)), returns, color=colors, alpha=0.7)
            axes[1, 0].axhline(y=0, color='black', linestyle='-', linewidth=1)
            axes[1, 0].set_title('单笔交易盈亏', fontsize=12, fontweight='bold')
            axes[1, 0].set_xlabel('交易序号')
            axes[1, 0].set_ylabel('收益率 (%)')
            axes[1, 0].grid(True, alpha=0.3, axis='y')
            
            win_trades = [t for t in trades if t['return_pct'] > 0]
            lose_trades = [t for t in trades if t['return_pct'] <= 0]
            
            labels = ['盈利', '亏损']
            sizes = [len(win_trades), len(lose_trades)]
            colors_pie = ['green', 'red']
            axes[1, 1].pie(sizes, labels=labels, colors=colors_pie, autopct='%1.1f%%', startangle=90)
            axes[1, 1].set_title(f'胜率: {len(win_trades)/len(trades)*100:.1f}%', fontsize=12, fontweight='bold')
        
        plt.tight_layout()
        
        if save_path:
            plt.savefig(save_path, dpi=300, bbox_inches='tight')
            print(f"图表已保存到: {save_path}")
        else:
            plt.show()

    def compare_strategies(self, strategies: List[Dict], stock_pool: Optional[List[str]] = None) -> pd.DataFrame:
        print("=" * 80)
        print("策略对比回测")
        print("=" * 80)
        print()
        
        comparison_results = []
        
        for i, strategy_config in enumerate(strategies):
            print(f"\n回测策略 {i+1}/{len(strategies)}: {strategy_config.get('name', '未命名')}")
            
            params = strategy_config.copy()
            strategy_name = params.pop('name', f'策略{i+1}')
            
            backtest = AStockBacktest(
                data_dir=self.data_dir,
                initial_capital=self.initial_capital,
                years=self.years,
                **params
            )
            
            results = backtest.run(stock_pool=stock_pool, verbose=False)
            
            if results:
                results['strategy_name'] = strategy_name
                comparison_results.append(results)
        
        if not comparison_results:
            print("未获得任何回测结果")
            return pd.DataFrame()
        
        comparison_df = pd.DataFrame(comparison_results)
        
        metrics_to_show = [
            'strategy_name', 'total_return', 'annualized_return', 'sharpe_ratio',
            'max_drawdown', 'total_trades', 'win_rate', 'profit_factor'
        ]
        
        comparison_df = comparison_df[metrics_to_show]
        
        print()
        print("=" * 80)
        print("策略对比结果")
        print("=" * 80)
        print()
        print(comparison_df.to_string(index=False))
        print("=" * 80)
        
        return comparison_df

    def plot_comparison(self, comparison_df: pd.DataFrame, save_path: Optional[str] = None):
        if comparison_df.empty:
            return
        
        fig, axes = plt.subplots(2, 3, figsize=(18, 10))
        fig.suptitle('策略对比分析', fontsize=16, fontweight='bold')
        
        metrics = [
            ('annualized_return', '年化收益率 (%)', 100),
            ('sharpe_ratio', '夏普比率', 1),
            ('max_drawdown', '最大回撤 (%)', 100),
            ('win_rate', '胜率 (%)', 100),
            ('profit_factor', '盈亏比', 1),
            ('total_trades', '交易次数', 1),
        ]
        
        for idx, (metric, title, scale) in enumerate(metrics):
            ax = axes[idx // 3, idx % 3]
            
            values = comparison_df[metric] * scale
            names = comparison_df['strategy_name']
            
            colors = plt.cm.viridis(np.linspace(0, 1, len(names)))
            bars = ax.bar(names, values, color=colors)
            
            ax.set_title(title, fontsize=11, fontweight='bold')
            ax.set_ylabel(title.split(' ')[0])
            ax.grid(True, alpha=0.3, axis='y')
            
            for bar, val in zip(bars, values):
                height = bar.get_height()
                ax.text(bar.get_x() + bar.get_width()/2., height,
                        f'{val:.2f}' if scale == 1 else f'{val:.1f}',
                        ha='center', va='bottom', fontsize=9)
            
            ax.tick_params(axis='x', rotation=30)
        
        plt.tight_layout()
        
        if save_path:
            plt.savefig(save_path, dpi=300, bbox_inches='tight')
            print(f"对比图表已保存到: {save_path}")
        else:
            plt.show()


class LimitUpStrategy:
    def __init__(self, params: Dict):
        self.params = params

    def is_limit_up(self, pct_change: float) -> bool:
        return pct_change >= self.params['limit_up_pct'] * 0.95

    def count_limit_up_days(self, df: pd.DataFrame, end_idx: int) -> int:
        count = 0
        lookback = min(self.params['days_to_check'], end_idx + 1)
        for i in range(end_idx - lookback + 1, end_idx + 1):
            if i >= 0 and i < len(df):
                pct = df.iloc[i]['涨跌幅']
                if pd.notna(pct) and self.is_limit_up(pct):
                    count += 1
        return count

    def get_latest_limit_up_info(self, df: pd.DataFrame, end_idx: int):
        for i in range(end_idx, -1, -1):
            if i >= 0 and i < len(df):
                pct = df.iloc[i]['涨跌幅']
                if pd.notna(pct) and self.is_limit_up(pct):
                    return i, df.iloc[i]['收盘价']
        return -1, 0

    def select_stock(self, df: pd.DataFrame, idx: int) -> Dict:
        if idx < 25:
            return {'selected': False, 'reason': '数据不足'}
        
        pct_change = df.iloc[idx]['涨跌幅']
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
            if df.iloc[i]['收盘价'] < limit_up_price * self.params['min_close_after_limit']:
                return {'selected': False, 'reason': '涨停后价格跌破'}
        
        volume_current = df.iloc[idx]['成交量']
        volume_avg = df.iloc[max(0, idx-5):idx]['成交量'].mean()
        volume_ratio = volume_current / volume_avg if volume_avg > 0 and volume_current > 0 else 0
        
        if volume_ratio < self.params['min_volume_ratio']:
            return {'selected': False, 'reason': f'量比={volume_ratio:.2f}'}
        
        return {
            'selected': True,
            'limit_up_price': limit_up_price,
            'current_price': df.iloc[idx]['收盘价'],
            'volume_ratio': volume_ratio,
        }


class BacktestEngine:
    def __init__(
        self,
        initial_capital: float,
        stop_loss_pct: float,
        take_profit_trigger: float,
        take_profit_fallback: float,
        max_holding_days: int,
        max_positions: int,
        position_size: float,
        commission: float,
        slippage: float
    ):
        self.initial_capital = initial_capital
        self.cash = initial_capital
        self.positions = {}
        self.trades = []
        self.equity_curve = []
        self.daily_returns = []
        self.commission = commission
        self.slippage = slippage
        self.stop_loss_pct = stop_loss_pct
        self.take_profit_trigger = take_profit_trigger
        self.take_profit_fallback = take_profit_fallback
        self.max_holding_days = max_holding_days
        self.max_positions = max_positions
        self.position_size = position_size

    def buy(self, stock_code: str, price: float, date: pd.Timestamp) -> bool:
        if len(self.positions) >= self.max_positions:
            return False
        
        position_value = self.cash * self.position_size
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
            
            if pct_return <= self.stop_loss_pct:
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
            
            self.equity_curve.append({
                'date': date,
                'portfolio_value': portfolio_value
            })
            
            if len(self.equity_curve) > 1:
                prev_value = self.equity_curve[-2]['portfolio_value']
                daily_return = (portfolio_value - prev_value) / prev_value
                self.daily_returns.append(daily_return)
            else:
                self.daily_returns.append(0)
        
        for stock_code in list(self.positions.keys()):
            if stock_code in data:
                df = data[stock_code]
                last_date = df.iloc[-1]['日期']
                self.sell(stock_code, df.iloc[-1]['收盘价'], last_date, '回测结束')

    def calculate_metrics(self) -> Dict:
        if not self.trades:
            return {}
        
        final_value = self.equity_curve[-1]['portfolio_value'] if self.equity_curve else self.initial_capital
        total_return = (final_value - self.initial_capital) / self.initial_capital
        
        days = len(self.equity_curve)
        years = days / 252
        annualized_return = (1 + total_return) ** (1 / years) - 1 if years > 0 else 0
        
        if len(self.daily_returns) > 1:
            volatility = np.std(self.daily_returns) * np.sqrt(252)
            excess_returns = np.array(self.daily_returns) - 0.03 / 252
            sharpe_ratio = np.mean(excess_returns) / np.std(excess_returns) * np.sqrt(252) if np.std(excess_returns) > 0 else 0
        else:
            volatility = 0
            sharpe_ratio = 0
        
        win_trades = [t for t in self.trades if t['return_pct'] > 0]
        lose_trades = [t for t in self.trades if t['return_pct'] <= 0]
        
        win_rate = len(win_trades) / len(self.trades) if self.trades else 0
        
        values = np.array([e['portfolio_value'] for e in self.equity_curve])
        peak = np.maximum.accumulate(values)
        drawdown = (values - peak) / peak
        max_drawdown = np.min(drawdown) if len(drawdown) > 0 else 0
        
        trades_df = pd.DataFrame(self.trades)
        avg_holding_days = trades_df['holding_days'].mean() if not trades_df.empty else 0
        
        profit_factor = 0
        if lose_trades:
            avg_loss = np.mean([t['return_pct'] for t in lose_trades])
            avg_win = np.mean([t['return_pct'] for t in win_trades]) if win_trades else 0
            profit_factor = abs(avg_win / avg_loss) * (len(win_trades) / len(lose_trades)) if avg_loss != 0 else 0
        
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
            'profit_factor': profit_factor,
            'avg_holding_days': avg_holding_days,
        }
