import pandas as pd
import numpy as np
from typing import Dict, List, Tuple
from datetime import datetime, timedelta
from config import BACKTEST_CONFIG, STRATEGY_CONFIG, RISK_FREE_RATE, TRADING_DAYS_PER_YEAR

class Position:
    def __init__(self, stock_code: str, entry_price: float, entry_date: pd.Timestamp, 
                 quantity: int, stop_loss_price: float):
        self.stock_code = stock_code
        self.entry_price = entry_price
        self.entry_date = entry_date
        self.quantity = quantity
        self.stop_loss_price = stop_loss_price
        self.highest_price = entry_price
        self.take_profit_active = False
        self.exit_price = None
        self.exit_date = None
        self.exit_reason = None

    def update_highest_price(self, current_price: float):
        if current_price > self.highest_price:
            self.highest_price = current_price

    def check_exit_signal(self, current_price: float, current_date: pd.Timestamp, 
                          holding_days: int) -> Tuple[bool, str]:
        pct_return = (current_price - self.entry_price) / self.entry_price
        
        if pct_return <= STRATEGY_CONFIG['stop_loss_pct']:
            return True, f'止损: {pct_return:.2%}'
        
        if pct_return >= STRATEGY_CONFIG['take_profit_trigger']:
            self.take_profit_active = True
        
        if self.take_profit_active:
            drawdown = (self.highest_price - current_price) / self.highest_price
            if drawdown >= abs(STRATEGY_CONFIG['take_profit_fallback']):
                return True, f'止盈: {pct_return:.2%} (最高回撤{drawdown:.2%})'
        
        if holding_days >= STRATEGY_CONFIG['max_holding_days']:
            return True, f'时间止损: 持仓{holding_days}天'
        
        return False, ''

class BacktestEngine:
    def __init__(self, initial_capital: float = None):
        self.initial_capital = initial_capital or BACKTEST_CONFIG['initial_capital']
        self.cash = self.initial_capital
        self.positions: Dict[str, Position] = {}
        self.trades: List[Dict] = []
        self.daily_returns: List[float] = []
        self.portfolio_values: List[float] = []
        self.dates: List[pd.Timestamp] = []
        self.commission = BACKTEST_CONFIG['commission']
        self.slippage = BACKTEST_CONFIG['slippage']

    def get_buy_price(self, market_price: float) -> float:
        return market_price * (1 + self.slippage)

    def get_sell_price(self, market_price: float) -> float:
        return market_price * (1 - self.slippage)

    def buy_stock(self, stock_code: str, price: float, date: pd.Timestamp) -> bool:
        if len(self.positions) >= STRATEGY_CONFIG['max_positions']:
            return False
        
        position_value = self.cash * STRATEGY_CONFIG['position_size']
        buy_price = self.get_buy_price(price)
        quantity = int(position_value / buy_price / 100) * 100
        
        if quantity == 0:
            return False
        
        commission = buy_price * quantity * self.commission
        total_cost = buy_price * quantity + commission
        
        if total_cost > self.cash:
            return False
        
        stop_loss_price = buy_price * (1 + STRATEGY_CONFIG['stop_loss_pct'])
        
        self.cash -= total_cost
        self.positions[stock_code] = Position(stock_code, buy_price, date, quantity, stop_loss_price)
        
        return True

    def sell_stock(self, stock_code: str, price: float, date: pd.Timestamp, reason: str):
        if stock_code not in self.positions:
            return
        
        position = self.positions[stock_code]
        sell_price = self.get_sell_price(price)
        commission = sell_price * position.quantity * self.commission
        total_value = sell_price * position.quantity - commission
        
        self.cash += total_value
        
        position.exit_price = sell_price
        position.exit_date = date
        position.exit_reason = reason
        
        pct_return = (sell_price - position.entry_price) / position.entry_price
        
        self.trades.append({
            'stock_code': stock_code,
            'entry_date': position.entry_date,
            'exit_date': date,
            'entry_price': position.entry_price,
            'exit_price': sell_price,
            'quantity': position.quantity,
            'return_pct': pct_return,
            'holding_days': (date - position.entry_date).days,
            'reason': reason,
            'profit': total_value - position.entry_price * position.quantity
        })
        
        del self.positions[stock_code]

    def update_positions(self, stock_data: Dict[str, pd.DataFrame], current_date: pd.Timestamp):
        to_sell = []
        
        for stock_code, position in self.positions.items():
            if stock_code not in stock_data:
                continue
            
            df = stock_data[stock_code]
            row = df[df['日期'] == current_date]
            
            if row.empty:
                continue
            
            current_price = row.iloc[0]['收盘']
            position.update_highest_price(current_price)
            
            holding_days = (current_date - position.entry_date).days
            should_exit, reason = position.check_exit_signal(current_price, current_date, holding_days)
            
            if should_exit:
                to_sell.append((stock_code, current_price, reason))
        
        for stock_code, price, reason in to_sell:
            self.sell_stock(stock_code, price, current_date, reason)

    def run(self, data: Dict[str, pd.DataFrame], signals_df: pd.DataFrame):
        all_dates = sorted(signals_df['date'].unique())
        
        for i, date in enumerate(all_dates):
            daily_signals = signals_df[signals_df['date'] == date]
            
            for _, signal in daily_signals.iterrows():
                self.buy_stock(signal['stock_code'], signal['price'], date)
            
            self.update_positions(data, date)
            
            portfolio_value = self.cash
            for stock_code, position in self.positions.items():
                if stock_code in data:
                    df = data[stock_code]
                    row = df[df['日期'] == date]
                    if not row.empty:
                        portfolio_value += row.iloc[0]['收盘'] * position.quantity
            
            self.portfolio_values.append(portfolio_value)
            self.dates.append(date)
            
            if i > 0:
                daily_return = (portfolio_value - self.portfolio_values[i-1]) / self.portfolio_values[i-1]
                self.daily_returns.append(daily_return)
            else:
                self.daily_returns.append(0)
        
        for stock_code in list(self.positions.keys()):
            if stock_code in data:
                df = data[stock_code]
                last_date = df.iloc[-1]['日期']
                self.sell_stock(stock_code, df.iloc[-1]['收盘'], last_date, '回测结束强制平仓')

    def calculate_metrics(self) -> Dict:
        if not self.trades and len(self.portfolio_values) < 2:
            return {}
        
        final_value = self.portfolio_values[-1] if self.portfolio_values else self.initial_capital
        total_return = (final_value - self.initial_capital) / self.initial_capital
        
        days = len(self.portfolio_values)
        years = days / TRADING_DAYS_PER_YEAR
        
        annualized_return = (1 + total_return) ** (1 / years) - 1 if years > 0 else 0
        
        if len(self.daily_returns) > 1:
            volatility = np.std(self.daily_returns) * np.sqrt(TRADING_DAYS_PER_YEAR)
            excess_returns = np.array(self.daily_returns) - RISK_FREE_RATE / TRADING_DAYS_PER_YEAR
            sharpe_ratio = np.mean(excess_returns) / np.std(excess_returns) * np.sqrt(TRADING_DAYS_PER_YEAR) if np.std(excess_returns) > 0 else 0
        else:
            volatility = 0
            sharpe_ratio = 0
        
        win_trades = [t for t in self.trades if t['return_pct'] > 0]
        lose_trades = [t for t in self.trades if t['return_pct'] <= 0]
        
        win_rate = len(win_trades) / len(self.trades) if self.trades else 0
        
        avg_win = np.mean([t['return_pct'] for t in win_trades]) if win_trades else 0
        avg_loss = np.mean([t['return_pct'] for t in lose_trades]) if lose_trades else 0
        
        profit_factor = abs(avg_win / avg_loss) * (len(win_trades) / len(lose_trades)) if lose_trades and avg_loss != 0 else 0
        
        max_drawdown = self._calculate_max_drawdown()
        
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
            'avg_win': avg_win,
            'avg_loss': avg_loss,
            'profit_factor': profit_factor,
            'avg_holding_days': avg_holding_days,
        }

    def _calculate_max_drawdown(self) -> float:
        if len(self.portfolio_values) < 2:
            return 0
        
        values = np.array(self.portfolio_values)
        peak = np.maximum.accumulate(values)
        drawdown = (values - peak) / peak
        return np.min(drawdown)

    def get_trades_df(self) -> pd.DataFrame:
        return pd.DataFrame(self.trades)

    def get_equity_curve(self) -> pd.DataFrame:
        return pd.DataFrame({
            'date': self.dates,
            'portfolio_value': self.portfolio_values,
            'daily_return': self.daily_returns
        })
