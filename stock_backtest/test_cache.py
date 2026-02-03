from a_stock_backtest_optimized import AStockBacktest
import time
import os

DATA_DIR = r'D:\BaiduNetdiskDownload\沪深个股60分钟_按年汇总'

def test_cache_performance():
    print("测试数据缓存性能...")
    
    # 获取测试股票代码
    import zipfile
    stock_codes = []
    zip_path = os.path.join(DATA_DIR, '2015_60min.zip')
    if os.path.exists(zip_path):
        with zipfile.ZipFile(zip_path) as z:
            filenames = z.namelist()
            codes = [f.split('_')[0] for f in filenames if f.endswith('.csv')]
            stock_codes = list(set(codes))[:10]
    
    print(f"使用 {len(stock_codes)} 只股票进行测试...")
    print()
    
    # 第一次运行（无缓存）
    print("第一次运行（无缓存）:")
    start_time = time.time()
    
    backtest = AStockBacktest(
        data_dir=DATA_DIR,
        initial_capital=1000000,
        years=[2015, 2016]
    )
    
    results1 = backtest.run(stock_pool=stock_codes, verbose=False)
    
    elapsed1 = time.time() - start_time
    print(f"耗时: {elapsed1:.2f} 秒")
    print()
    
    # 第二次运行（有缓存）
    print("第二次运行（有缓存）:")
    start_time = time.time()
    
    backtest2 = AStockBacktest(
        data_dir=DATA_DIR,
        initial_capital=1000000,
        years=[2015, 2016]
    )
    
    results2 = backtest2.run(stock_pool=stock_codes, verbose=False)
    
    elapsed2 = time.time() - start_time
    print(f"耗时: {elapsed2:.2f} 秒")
    print()
    
    # 计算速度提升
    if elapsed1 > 0:
        speedup = elapsed1 / elapsed2
        print(f"速度提升: {speedup:.2f} 倍")
    
    print("\n测试完成！")
    print(f"缓存文件已保存到: data_cache/")

if __name__ == "__main__":
    test_cache_performance()
