import pandas as pd
import numpy as np
from typing import List, Dict, Tuple
from config import SELECTION_CONFIG, STRATEGY_CONFIG

class LimitUpStrategy:
    def __init__(self, config: dict = None):
        self.config = config or SELECTION_CONFIG

    def is_limit_up(self, df: pd.DataFrame, idx: int) -> bool:
        if idx < 1:
            return False
        pct_change = df.iloc[idx]['涨跌幅'] / 100
        return pct_change >= self.config['limit_up_pct'] * 0.95

    def count_limit_up_days(self, df: pd.DataFrame, lookback_days: int) -> int:
        count = 0
        for i in range(min(lookback_days, len(df))):
            if self.is_limit_up(df, len(df) - 1 - i):
                count += 1
        return count

    def get_latest_limit_up_day(self, df: pd.DataFrame) -> Tuple[int, float]:
        for i in range(len(df)):
            idx = len(df) - 1 - i
            if self.is_limit_up(df, idx):
                return idx, df.iloc[idx]['收盘']
        return -1, 0

    def check_price_after_limit_up(self, df: pd.DataFrame, limit_up_idx: int, limit_up_price: float) -> bool:
        if limit_up_idx == -1:
            return False
        
        days_after = len(df) - 1 - limit_up_idx
        if days_after < 1 or days_after > 5:
            return False
        
        for i in range(limit_up_idx + 1, min(limit_up_idx + 4, len(df))):
            if df.iloc[i]['收盘'] < limit_up_price * self.config['min_close_after_limit']:
                return False
        
        return True

    def check_turnover(self, df: pd.DataFrame, idx: int) -> bool:
        turnover = df.iloc[idx]['换手率'] / 100
        return self.config['min_turnover'] <= turnover <= self.config['max_turnover']

    def check_volume_ratio(self, df: pd.DataFrame, idx: int) -> bool:
        if idx < 5:
            return False
        
        current_volume = df.iloc[idx]['成交量']
        avg_volume = df.iloc[idx-5:idx]['成交量'].mean()
        
        if avg_volume == 0:
            return False
        
        volume_ratio = current_volume / avg_volume
        return volume_ratio >= self.config['min_volume_ratio']

    def check_ma_trend(self, df: pd.DataFrame, idx: int) -> bool:
        if idx < 20:
            return False
        
        close = df.iloc[idx]['收盘']
        ma20 = df.iloc[idx-20:idx+1]['收盘'].mean()
        
        return close >= ma20

    def select_stock(self, df: pd.DataFrame, current_date: pd.Timestamp) -> Dict:
        if len(df) < 25:
            return {'selected': False, 'reason': '数据不足'}
        
        latest_idx = len(df) - 1
        latest_date = df.iloc[latest_idx]['日期']
        
        if pd.to_datetime(latest_date) < current_date:
            return {'selected': False, 'reason': '数据未更新'}
        
        count = self.count_limit_up_days(df, self.config['days_to_check'])
        if count != 1:
            return {'selected': False, 'reason': f'涨停次数={count}，不等于1'}
        
        limit_up_idx, limit_up_price = self.get_latest_limit_up_day(df)
        
        if limit_up_idx == -1:
            return {'selected': False, 'reason': '未找到涨停'}
        
        if not self.check_price_after_limit_up(df, limit_up_idx, limit_up_price):
            return {'selected': False, 'reason': '涨停后价格不满足条件'}
        
        if not self.check_turnover(df, latest_idx):
            turnover = df.iloc[latest_idx]['换手率'] / 100
            return {'selected': False, 'reason': f'换手率={turnover:.2%}，不在范围内'}
        
        if not self.check_volume_ratio(df, latest_idx):
            return {'selected': False, 'reason': '量比不足'}
        
        if not self.check_ma_trend(df, latest_idx):
            return {'selected': False, 'reason': '不在20日均线上方'}
        
        return {
            'selected': True,
            'limit_up_date': df.iloc[limit_up_idx]['日期'],
            'limit_up_price': limit_up_price,
            'current_price': df.iloc[latest_idx]['收盘'],
            'turnover': df.iloc[latest_idx]['换手率'] / 100,
        }

    def get_daily_signals(self, data: Dict[str, pd.DataFrame], current_date: pd.Timestamp) -> List[Dict]:
        signals = []
        for stock_code, df in data.items():
            result = self.select_stock(df, current_date)
            if result['selected']:
                signals.append({
                    'stock_code': stock_code,
                    'date': current_date,
                    'price': result['current_price'],
                    'limit_up_price': result['limit_up_price'],
                    'turnover': result['turnover'],
                })
        return signals
