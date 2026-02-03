import os
import sqlite3
import pandas as pd
import pickle
import io
from typing import Dict, List

class DatabaseCache:
    def __init__(self, db_path: str = 'stock_data.db'):
        self.db_path = db_path
        self._init_db()
    
    def _init_db(self):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
        CREATE TABLE IF NOT EXISTS stock_data (
            stock_code TEXT,
            years TEXT,
            data BLOB,
            timestamp INTEGER,
            PRIMARY KEY (stock_code, years)
        )''')
        
        conn.commit()
        conn.close()
    
    def get_years_key(self, years: List[int]) -> str:
        return '_'.join(map(str, sorted(years)))
    
    def is_cached(self, stock_code: str, years: List[int]) -> bool:
        years_key = self.get_years_key(years)
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute(
            "SELECT 1 FROM stock_data WHERE stock_code = ? AND years = ?",
            (stock_code, years_key)
        )
        
        exists = cursor.fetchone() is not None
        conn.close()
        return exists
    
    def load_from_cache(self, stock_code: str, years: List[int]) -> pd.DataFrame:
        years_key = self.get_years_key(years)
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute(
            "SELECT data FROM stock_data WHERE stock_code = ? AND years = ?",
            (stock_code, years_key)
        )
        
        row = cursor.fetchone()
        conn.close()
        
        if not row:
            return pd.DataFrame()
        
        try:
            df = pickle.loads(row[0])
            return df
        except Exception:
            return pd.DataFrame()
    
    def save_to_cache(self, stock_code: str, years: List[int], df: pd.DataFrame):
        if df.empty:
            return
        
        years_key = self.get_years_key(years)
        data = pickle.dumps(df, protocol=pickle.HIGHEST_PROTOCOL)
        timestamp = int(pd.Timestamp.now().timestamp())
        
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute(
            "INSERT OR REPLACE INTO stock_data (stock_code, years, data, timestamp) VALUES (?, ?, ?, ?)",
            (stock_code, years_key, data, timestamp)
        )
        
        conn.commit()
        conn.close()
    
    def clear_cache(self):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute("DELETE FROM stock_data")
        conn.commit()
        conn.close()
        
        print(f"数据库缓存已清空: {self.db_path}")
    
    def get_cache_stats(self):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        cursor.execute("SELECT COUNT(*) FROM stock_data")
        count = cursor.fetchone()[0]
        
        cursor.execute("SELECT SUM(LENGTH(data)) FROM stock_data")
        size = cursor.fetchone()[0] or 0
        
        conn.close()
        
        return {
            'records': count,
            'size_mb': size / 1024 / 1024
        }
    
    def batch_save(self, stock_data: Dict[str, pd.DataFrame], years: List[int]):
        conn = sqlite3.connect(self.db_path)
        cursor = conn.cursor()
        
        for stock_code, df in stock_data.items():
            if df.empty:
                continue
            
            years_key = self.get_years_key(years)
            data = pickle.dumps(df, protocol=pickle.HIGHEST_PROTOCOL)
            timestamp = int(pd.Timestamp.now().timestamp())
            
            cursor.execute(
                "INSERT OR REPLACE INTO stock_data (stock_code, years, data, timestamp) VALUES (?, ?, ?, ?)",
                (stock_code, years_key, data, timestamp)
            )
        
        conn.commit()
        conn.close()
    
    def batch_load(self, stock_codes: List[str], years: List[int]) -> Dict[str, pd.DataFrame]:
        years_key = self.get_years_key(years)
        conn = sqlite3.connect(self.db_path)
        conn.row_factory = sqlite3.Row
        cursor = conn.cursor()
        
        placeholders = ','.join(['?'] * len(stock_codes))
        cursor.execute(
            f"SELECT stock_code, data FROM stock_data WHERE stock_code IN ({placeholders}) AND years = ?",
            stock_codes + [years_key]
        )
        
        results = {}
        for row in cursor.fetchall():
            try:
                df = pickle.loads(row['data'])
                results[row['stock_code']] = df
            except Exception:
                pass
        
        conn.close()
        return results
