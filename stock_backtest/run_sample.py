import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from a_stock_backtest_optimized import AStockBacktest

DATA_DIR = r'D:\BaiduNetdiskDownload\沪深个股60分钟_按年汇总'

def run_sample():
    print("开始示例回测（30只股票）...")
    
    backtest = AStockBacktest(
        data_dir=DATA_DIR,
        initial_capital=1000000,
        years=[2015, 2016]
    )
    
    # 使用前30只股票作为样本
    import zipfile
    stock_codes = []
    zip_path = os.path.join(DATA_DIR, '2015_60min.zip')
    if os.path.exists(zip_path):
        with zipfile.ZipFile(zip_path) as z:
            filenames = z.namelist()
            codes = [f.split('_')[0] for f in filenames if f.endswith('.csv')]
            stock_codes = list(set(codes))[:30]
    
    print(f"\n使用 {len(stock_codes)} 只股票进行回测...")
    
    results = backtest.run(stock_pool=stock_codes)
    
    if results:
        print("\n回测完成！")
        backtest.plot_results(results, save_path='results_sample.png')
        print(f"图表已保存到: results_sample.png")

if __name__ == "__main__":
    import os
    run_sample()
