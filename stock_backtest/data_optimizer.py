import os
import pandas as pd
import pickle
import lz4.frame
from typing import Dict, List, Optional
import asyncio
import concurrent.futures

class DataOptimizer:
    def __init__(self, cache_dir: str = 'data_cache'):
        self.cache_dir = cache_dir
        self.memory_cache = {}
        self.batch_size = 100
        os.makedirs(self.cache_dir, exist_ok=True)
    
    def get_cache_key(self, stock_code: str, years: List[int]) -> str:
        years_str = '_'.join(map(str, sorted(years)))
        return f"{stock_code}_{years_str}.lz4"
    
    def get_cache_path(self, stock_code: str, years: List[int]) -> str:
        cache_key = self.get_cache_key(stock_code, years)
        return os.path.join(self.cache_dir, cache_key)
    
    def get_stock_batch_id(self, stock_code: str) -> int:
        code_num = int(stock_code.lstrip('sh').lstrip('sz'))
        return code_num // self.batch_size
    
    def load_from_memory_cache(self, stock_code: str, years: List[int]) -> Optional[pd.DataFrame]:
        cache_key = (stock_code, tuple(sorted(years)))
        return self.memory_cache.get(cache_key)
    
    def save_to_memory_cache(self, stock_code: str, years: List[int], df: pd.DataFrame):
        if df.empty:
            return
        cache_key = (stock_code, tuple(sorted(years)))
        self.memory_cache[cache_key] = df
    
    def is_cached(self, stock_code: str, years: List[int]) -> bool:
        if self.load_from_memory_cache(stock_code, years) is not None:
            return True
        return os.path.exists(self.get_cache_path(stock_code, years))
    
    def load_from_cache(self, stock_code: str, years: List[int]) -> pd.DataFrame:
        df = self.load_from_memory_cache(stock_code, years)
        if df is not None:
            return df
        
        cache_path = self.get_cache_path(stock_code, years)
        if not os.path.exists(cache_path):
            return pd.DataFrame()
        
        try:
            with lz4.frame.open(cache_path, 'rb') as f:
                data = f.read()
                df = pickle.loads(data)
            
            self.save_to_memory_cache(stock_code, years, df)
            return df
        except Exception:
            return pd.DataFrame()
    
    def save_to_cache(self, stock_code: str, years: List[int], df: pd.DataFrame):
        if df.empty:
            return
        
        self.save_to_memory_cache(stock_code, years, df)
        
        cache_path = self.get_cache_path(stock_code, years)
        try:
            data = pickle.dumps(df, protocol=pickle.HIGHEST_PROTOCOL)
            with lz4.frame.open(cache_path, 'wb') as f:
                f.write(data)
        except Exception:
            pass
    
    async def preload_cache(self, stock_codes: List[str], years: List[int]):
        """异步预加载缓存"""
        def preload_one(stock_code):
            cache_path = self.get_cache_path(stock_code, years)
            if os.path.exists(cache_path):
                try:
                    with lz4.frame.open(cache_path, 'rb') as f:
                        data = f.read()
                        df = pickle.loads(data)
                    self.save_to_memory_cache(stock_code, years, df)
                except Exception:
                    pass
        
        with concurrent.futures.ThreadPoolExecutor(max_workers=4) as executor:
            tasks = [executor.submit(preload_one, code) for code in stock_codes]
            for task in concurrent.futures.as_completed(tasks):
                pass
    
    def clear_cache(self):
        self.memory_cache.clear()
        for file in os.listdir(self.cache_dir):
            file_path = os.path.join(self.cache_dir, file)
            if os.path.isfile(file_path):
                os.remove(file_path)
        print(f"缓存已清空: {self.cache_dir}")
    
    def clear_memory_cache(self):
        self.memory_cache.clear()
        print("内存缓存已清空")
    
    def get_cache_stats(self):
        disk_size = 0
        for file in os.listdir(self.cache_dir):
            file_path = os.path.join(self.cache_dir, file)
            if os.path.isfile(file_path):
                disk_size += os.path.getsize(file_path)
        
        return {
            'memory_cache_size': len(self.memory_cache),
            'disk_cache_size': disk_size / 1024 / 1024,
            'disk_cache_files': len(os.listdir(self.cache_dir))
        }
