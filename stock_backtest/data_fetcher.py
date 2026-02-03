import akshare as ak
import pandas as pd
import numpy as np
from datetime import datetime, timedelta
from typing import List, Dict
import os
import pickle
from config import CACHE_DIR, DATA_DIR

class DataFetcher:
    def __init__(self):
        self.cache_enabled = True

    def _get_cache_path(self, cache_key: str) -> str:
        return os.path.join(CACHE_DIR, f"{cache_key}.pkl")

    def _load_from_cache(self, cache_key: str):
        if not self.cache_enabled:
            return None
        cache_path = self._get_cache_path(cache_key)
        if os.path.exists(cache_path):
            try:
                with open(cache_path, 'rb') as f:
                    data = pickle.load(f)
                return data
            except:
                return None
        return None

    def _save_to_cache(self, cache_key: str, data):
        if not self.cache_enabled:
            return
        cache_path = self._get_cache_path(cache_key)
        try:
            with open(cache_path, 'wb') as f:
                pickle.dump(data, f)
        except:
            pass

    def get_stock_list(self) -> pd.DataFrame:
        cache_key = "stock_list"
        cached = self._load_from_cache(cache_key)
        if cached is not None:
            return cached

        try:
            stock_list = ak.stock_zh_a_spot_em()
            stock_list = stock_list[['代码', '名称', '总市值', '流通市值', '上市日期']]
            stock_list.columns = ['stock_code', 'stock_name', 'total_market_cap', 'circulating_market_cap', 'list_date']
            stock_list = stock_list[~stock_list['stock_code'].str.startswith(('ST', '*ST', 'SST', 'S*ST'))]
            stock_list['stock_code'] = stock_list['stock_code'].str.zfill(6)
            self._save_to_cache(cache_key, stock_list)
            return stock_list
        except Exception as e:
            print(f"获取股票列表失败: {e}")
            return pd.DataFrame()

    def get_stock_history(self, stock_code: str, start_date: str, end_date: str) -> pd.DataFrame:
        cache_key = f"history_{stock_code}_{start_date}_{end_date}"
        cached = self._load_from_cache(cache_key)
        if cached is not None:
            return cached

        try:
            df = ak.stock_zh_a_hist(symbol=stock_code, period="daily", start_date=start_date, end_date=end_date, adjust="qfq")
            if df.empty:
                return pd.DataFrame()
            
            df['date'] = pd.to_datetime(df['日期'])
            df['stock_code'] = stock_code
            df = df.sort_values('date')
            
            self._save_to_cache(cache_key, df)
            return df
        except Exception as e:
            print(f"获取股票{stock_code}历史数据失败: {e}")
            return pd.DataFrame()

    def get_index_history(self, index_code: str = '000001', start_date: str = '2015-01-01', end_date: str = '2024-12-31') -> pd.DataFrame:
        cache_key = f"index_{index_code}_{start_date}_{end_date}"
        cached = self._load_from_cache(cache_key)
        if cached is not None:
            return cached

        try:
            df = ak.index_zh_a_hist(symbol=index_code, period="daily", start_date=start_date, end_date=end_date)
            if df.empty:
                return pd.DataFrame()
            
            df['date'] = pd.to_datetime(df['日期'])
            df = df.sort_values('date')
            
            self._save_to_cache(cache_key, df)
            return df
        except Exception as e:
            print(f"获取指数{index_code}历史数据失败: {e}")
            return pd.DataFrame()

    def get_stock_pool(self, min_market_cap: float, max_market_cap: float, min_listed_days: int, pool_size: int = 100) -> List[str]:
        stock_list = self.get_stock_list()
        if stock_list.empty:
            return []
        
        stock_list['list_date'] = pd.to_datetime(stock_list['list_date'])
        today = pd.Timestamp.now()
        stock_list['listed_days'] = (today - stock_list['list_date']).dt.days
        
        filtered = stock_list[
            (stock_list['total_market_cap'] >= min_market_cap) &
            (stock_list['total_market_cap'] <= max_market_cap) &
            (stock_list['listed_days'] >= min_listed_days)
        ]
        
        if len(filtered) > pool_size:
            filtered = filtered.sample(n=pool_size, random_state=42)
        
        return filtered['stock_code'].tolist()

    def get_multiple_stocks_history(self, stock_codes: List[str], start_date: str, end_date: str) -> Dict[str, pd.DataFrame]:
        result = {}
        for i, code in enumerate(stock_codes):
            print(f"正在获取股票 {i+1}/{len(stock_codes)}: {code}")
            df = self.get_stock_history(code, start_date, end_date)
            if not df.empty:
                result[code] = df
        return result
