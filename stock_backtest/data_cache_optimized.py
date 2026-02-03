import os
import pandas as pd
import pickle
from typing import Dict, List
import lz4.frame

class OptimizedDataCache:
    def __init__(self, cache_dir: str = 'data_cache'):
        self.cache_dir = cache_dir
        os.makedirs(self.cache_dir, exist_ok=True)
    
    def get_cache_key(self, stock_code: str, years: List[int]) -> str:
        years_str = '_'.join(map(str, sorted(years)))
        return f"{stock_code}_{years_str}.lz4"
    
    def get_cache_path(self, cache_key: str) -> str:
        return os.path.join(self.cache_dir, cache_key)
    
    def is_cached(self, stock_code: str, years: List[int]) -> bool:
        cache_key = self.get_cache_key(stock_code, years)
        cache_path = self.get_cache_path(cache_key)
        return os.path.exists(cache_path)
    
    def load_from_cache(self, stock_code: str, years: List[int]) -> pd.DataFrame:
        cache_key = self.get_cache_key(stock_code, years)
        cache_path = self.get_cache_path(cache_key)
        
        try:
            with lz4.frame.open(cache_path, 'rb') as f:
                data = f.read()
                df = pickle.loads(data)
            return df
        except Exception:
            return pd.DataFrame()
    
    def save_to_cache(self, stock_code: str, years: List[int], df: pd.DataFrame):
        if df.empty:
            return
        
        cache_key = self.get_cache_key(stock_code, years)
        cache_path = self.get_cache_path(cache_key)
        
        try:
            data = pickle.dumps(df, protocol=pickle.HIGHEST_PROTOCOL)
            with lz4.frame.open(cache_path, 'wb') as f:
                f.write(data)
        except Exception:
            pass
    
    def clear_cache(self):
        for file in os.listdir(self.cache_dir):
            file_path = os.path.join(self.cache_dir, file)
            if os.path.isfile(file_path):
                os.remove(file_path)
        print(f"缓存已清空: {self.cache_dir}")
