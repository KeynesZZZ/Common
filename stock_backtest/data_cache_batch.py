import os
import pandas as pd
import pickle
from typing import Dict, List
import numpy as np
import tempfile

class BatchDataCache:
    def __init__(self, cache_dir: str = 'data_cache'):
        self.cache_dir = cache_dir
        self.batch_size = 100
        os.makedirs(self.cache_dir, exist_ok=True)
    
    def get_batch_cache_key(self, batch_id: int, years: List[int]) -> str:
        years_str = '_'.join(map(str, sorted(years)))
        return f"batch_{batch_id}_{years_str}.pkl"
    
    def get_batch_path(self, batch_id: int, years: List[int]) -> str:
        cache_key = self.get_batch_cache_key(batch_id, years)
        return os.path.join(self.cache_dir, cache_key)
    
    def get_stock_batch_id(self, stock_code: str) -> int:
        code_num = int(stock_code.lstrip('sh').lstrip('sz'))
        return code_num // self.batch_size
    
    def load_from_cache(self, stock_code: str, years: List[int]) -> pd.DataFrame:
        batch_id = self.get_stock_batch_id(stock_code)
        batch_path = self.get_batch_path(batch_id, years)
        
        if not os.path.exists(batch_path):
            return pd.DataFrame()
        
        try:
            with open(batch_path, 'rb') as f:
                batch_data = pickle.load(f)
            
            if stock_code in batch_data:
                return batch_data[stock_code]
            return pd.DataFrame()
        except Exception:
            return pd.DataFrame()
    
    def save_to_cache(self, stock_code: str, years: List[int], df: pd.DataFrame):
        if df.empty:
            return
        
        batch_id = self.get_stock_batch_id(stock_code)
        batch_path = self.get_batch_path(batch_id, years)
        
        batch_data = {}
        if os.path.exists(batch_path):
            try:
                with open(batch_path, 'rb') as f:
                    batch_data = pickle.load(f)
            except Exception:
                pass
        
        batch_data[stock_code] = df
        
        try:
            with open(batch_path, 'wb') as f:
                pickle.dump(batch_data, f, protocol=pickle.HIGHEST_PROTOCOL)
        except Exception:
            pass
    
    def is_cached(self, stock_code: str, years: List[int]) -> bool:
        batch_id = self.get_stock_batch_id(stock_code)
        batch_path = self.get_batch_path(batch_id, years)
        
        if not os.path.exists(batch_path):
            return False
        
        try:
            with open(batch_path, 'rb') as f:
                batch_data = pickle.load(f)
            return stock_code in batch_data
        except Exception:
            return False
    
    def clear_cache(self):
        for file in os.listdir(self.cache_dir):
            file_path = os.path.join(self.cache_dir, file)
            if os.path.isfile(file_path):
                os.remove(file_path)
        print(f"缓存已清空: {self.cache_dir}")
