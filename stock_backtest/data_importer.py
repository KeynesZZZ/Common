import os
import zipfile
import pandas as pd
import numpy as np
from multiprocessing import Pool, cpu_count
from data_db_cache import DatabaseCache

DATA_DIR = r'D:\BaiduNetdiskDownload\沪深个股60分钟_按年汇总'

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
                        df = pd.read_csv(data, encoding='gbk')
                    except UnicodeDecodeError:
                        try:
                            df = pd.read_csv(data, encoding='utf-8')
                        except UnicodeDecodeError:
                            df = pd.read_csv(data, encoding='gb18030')
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
        print()
        
        stock_codes = self.get_all_stock_codes()
        total = len(stock_codes)
        print(f"发现 {total} 只股票")
        print()
        
        # 检查已缓存的股票
        cached_count = 0
        for stock_code in stock_codes:
            if self.db_cache.is_cached(stock_code, years):
                cached_count += 1
        
        print(f"已缓存 {cached_count} 只股票，需要导入 {total - cached_count} 只股票")
        print()
        
        # 过滤未缓存的股票
        to_import = [code for code in stock_codes if not self.db_cache.is_cached(code, years)]
        
        if not to_import:
            print("所有股票数据已缓存，无需导入")
            return
        
        # 并行处理
        print("开始并行导入数据...")
        print()
        
        batch_size = 100
        processed = 0
        success = 0
        
        with Pool(min(cpu_count(), 8)) as pool:
            tasks = [(code, years) for code in to_import]
            results = pool.starmap(self.process_single_stock, tasks)
        
        for stock_code, status in results:
            processed += 1
            if status:
                success += 1
            
            if processed % 100 == 0:
                print(f"已处理: {processed}/{len(to_import)}, 成功: {success}")
        
        print()
        print(f"导入完成！")
        print(f"总处理: {len(to_import)}, 成功: {success}")
        
        # 显示数据库统计信息
        stats = self.db_cache.get_cache_stats()
        print(f"\n数据库统计:")
        print(f"记录数: {stats['records']}")
        print(f"数据库大小: {stats['size_mb']:.2f} MB")
    
    def update_data(self, years: list = None):
        """更新股票数据"""
        if years is None:
            years = list(range(2015, 2025))
        
        print(f"开始更新股票数据（年份: {years}）...")
        
        # 清理数据库
        self.db_cache.clear_cache()
        print("数据库已清空，开始重新导入...")
        print()
        
        self.import_all_data(years)
    
    def check_status(self):
        """检查导入状态"""
        print("检查导入状态...")
        
        stock_codes = self.get_all_stock_codes()
        total = len(stock_codes)
        
        cached_count = 0
        for stock_code in stock_codes:
            if self.db_cache.is_cached(stock_code, list(range(2015, 2025))):
                cached_count += 1
        
        print(f"总股票数: {total}")
        print(f"已缓存: {cached_count}")
        print(f"未缓存: {total - cached_count}")
        
        stats = self.db_cache.get_cache_stats()
        print(f"\n数据库统计:")
        print(f"记录数: {stats['records']}")
        print(f"数据库大小: {stats['size_mb']:.2f} MB")

if __name__ == "__main__":
    importer = DataImporter()
    
    print("=" * 80)
    print("A股历史数据导入工具")
    print("=" * 80)
    print("1. 导入所有股票数据")
    print("2. 更新股票数据")
    print("3. 检查导入状态")
    print("=" * 80)
    
    choice = input("请选择操作: ")
    
    if choice == '1':
        importer.import_all_data()
    elif choice == '2':
        importer.update_data()
    elif choice == '3':
        importer.check_status()
    else:
        print("无效选择")
