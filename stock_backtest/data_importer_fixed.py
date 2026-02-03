import os
import zipfile
import pandas as pd
import numpy as np
import logging
import io
from multiprocessing import Pool, cpu_count
from data_db_cache import DatabaseCache

DATA_DIR = r'D:\BaiduNetdiskDownload\沪深个股60分钟_按年汇总'

# 配置日志
logging.basicConfig(
    filename='import_log.txt',
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s'
)

class DataImporter:
    def __init__(self, data_dir: str = DATA_DIR):
        self.data_dir = data_dir
        self.db_cache = DatabaseCache('stock_data.db')
    
    def get_all_stock_codes(self) -> list:
        """获取所有股票代码"""
        all_codes = set()
        
        for year in range(2015, 2025):
            zip_path = os.path.join(self.data_dir, f'{year}_60min.zip')
            if os.path.exists(zip_path):
                with zipfile.ZipFile(zip_path) as z:
                    filenames = z.namelist()
                    codes = [f.split('_')[0] for f in filenames if f.endswith('.csv')]
                    all_codes.update(codes)
        
        return list(all_codes)
    
    def process_single_stock(self, stock_code: str, years: list):
        """处理单个股票的历史数据"""
        try:
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
                return stock_code, False
            
            df = pd.concat(all_data, ignore_index=True)
            df = df.sort_values('时间').reset_index(drop=True)
            
            daily_df = self._convert_to_daily(df)
            
            if not daily_df.empty:
                self.db_cache.save_to_cache(stock_code, years, daily_df)
                return stock_code, True
            return stock_code, False
        except Exception as e:
            logging.error(f"处理股票 {stock_code} 时出错: {e}")
            return stock_code, False
    
    def _convert_to_daily(self, df: pd.DataFrame) -> pd.DataFrame:
        """将60分钟数据转换为日线数据"""
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
    
    def import_all_data(self, years: list = None):
        """导入所有股票数据"""
        if years is None:
            years = list(range(2015, 2025))
        
        print(f"开始导入所有股票数据（年份: {years}）...")
        logging.info(f"开始导入所有股票数据（年份: {years}）")
        print()
        
        stock_codes = self.get_all_stock_codes()
        total = len(stock_codes)
        print(f"发现 {total} 只股票")
        logging.info(f"发现 {total} 只股票")
        print()
        
        # 检查已缓存的股票
        cached_count = 0
        for stock_code in stock_codes:
            if self.db_cache.is_cached(stock_code, years):
                cached_count += 1
        
        print(f"已缓存 {cached_count} 只股票，需要导入 {total - cached_count} 只股票")
        logging.info(f"已缓存 {cached_count} 只股票，需要导入 {total - cached_count} 只股票")
        print()
        
        # 过滤未缓存的股票
        to_import = [code for code in stock_codes if not self.db_cache.is_cached(code, years)]
        
        if not to_import:
            print("所有股票数据已缓存，无需导入")
            logging.info("所有股票数据已缓存，无需导入")
            return
        
        # 分批处理
        batch_size = 50
        total_batches = (len(to_import) + batch_size - 1) // batch_size
        
        print(f"开始分批导入数据，共 {total_batches} 批")
        logging.info(f"开始分批导入数据，共 {total_batches} 批")
        print()
        
        processed = 0
        success = 0
        
        for batch_idx in range(total_batches):
            start = batch_idx * batch_size
            end = min((batch_idx + 1) * batch_size, len(to_import))
            batch_codes = to_import[start:end]
            
            print(f"处理第 {batch_idx + 1}/{total_batches} 批: {start}-{end}")
            logging.info(f"处理第 {batch_idx + 1}/{total_batches} 批: {start}-{end}")
            
            with Pool(min(cpu_count(), 4)) as pool:
                tasks = [(code, years) for code in batch_codes]
                results = pool.starmap(self.process_single_stock, tasks)
            
            for stock_code, status in results:
                processed += 1
                if status:
                    success += 1
            
            print(f"第 {batch_idx + 1} 批完成: 成功 {sum(1 for _, s in results if s)}/{len(results)}")
        
        print()
        print(f"导入完成！")
        print(f"总处理: {len(to_import)}, 成功: {success}")
        
        logging.info(f"导入完成！总处理: {len(to_import)}, 成功: {success}")
        
        # 显示数据库统计信息
        stats = self.db_cache.get_cache_stats()
        print(f"\n数据库统计:")
        print(f"记录数: {stats['records']}")
        print(f"数据库大小: {stats['size_mb']:.2f} MB")
        
        logging.info(f"数据库统计: 记录数={stats['records']}, 大小={stats['size_mb']:.2f} MB")

if __name__ == "__main__":
    print("开始导入所有A股历史数据...")
    print()
    
    importer = DataImporter()
    importer.import_all_data()
    
    print()
    print("导入完成！")
