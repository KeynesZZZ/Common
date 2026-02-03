import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from a_stock_backtest import AStockBacktest

DATA_DIR = r'D:\BaiduNetdiskDownload\沪深个股60分钟_按年汇总'

if __name__ == "__main__":
    print("开始全市场回测...")
    
    backtest = AStockBacktest(
        data_dir=DATA_DIR,
        initial_capital=1000000
    )
    
    results = backtest.run()
    
    if results:
        print("\n正在生成可视化图表...")
        backtest.plot_results(results, save_path='results_full_market.png')
        print("\n回测完成！")
        print(f"图表已保存到: results_full_market.png")
