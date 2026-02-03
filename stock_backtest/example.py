from a_stock_backtest import AStockBacktest
import os

DATA_DIR = r'D:\BaiduNetdiskDownload\沪深个股60分钟_按年汇总'

def example_basic():
    print("=" * 80)
    print("示例1: 基本回测")
    print("=" * 80)
    
    backtest = AStockBacktest(
        data_dir=DATA_DIR,
        initial_capital=1000000
    )
    
    results = backtest.run()
    
    if results:
        backtest.plot_results(results, save_path='results_basic.png')

def example_custom_params():
    print("=" * 80)
    print("示例2: 自定义参数回测")
    print("=" * 80)
    
    backtest = AStockBacktest(
        data_dir=DATA_DIR,
        initial_capital=1000000,
        stop_loss_pct=-0.08,
        take_profit_trigger=0.12,
        take_profit_fallback=-0.04,
        max_holding_days=10,
        commission=0.0003,
        slippage=0.001
    )
    
    results = backtest.run()
    
    if results:
        backtest.plot_results(results, save_path='results_custom.png')

def example_stock_pool():
    print("=" * 80)
    print("示例3: 指定股票池回测")
    print("=" * 80)
    
    stock_codes = ['sh600000', 'sz000001', 'sz300750']
    
    backtest = AStockBacktest(
        data_dir=DATA_DIR,
        initial_capital=1000000
    )
    
    results = backtest.run(stock_pool=stock_codes)
    
    if results:
        backtest.plot_results(results, save_path='results_pool.png')

def example_strategy_comparison():
    print("=" * 80)
    print("示例4: 策略对比")
    print("=" * 80)
    
    strategies = [
        {
            'stop_loss_pct': -0.05,
            'take_profit_trigger': 0.10,
            'name': '止损5%-止盈10%'
        },
        {
            'stop_loss_pct': -0.08,
            'take_profit_trigger': 0.10,
            'name': '止损8%-止盈10%'
        },
        {
            'stop_loss_pct': -0.10,
            'take_profit_trigger': 0.10,
            'name': '止损10%-止盈10%'
        },
    ]
    
    backtest = AStockBacktest(
        data_dir=DATA_DIR,
        initial_capital=1000000
    )
    
    comparison_df = backtest.compare_strategies(strategies)
    
    if not comparison_df.empty:
        backtest.plot_comparison(comparison_df, save_path='comparison.png')

def example_full_market():
    print("=" * 80)
    print("示例5: 全市场回测")
    print("=" * 80)
    
    backtest = AStockBacktest(
        data_dir=DATA_DIR,
        initial_capital=1000000
    )
    
    print("注意: 全市场回测可能需要较长时间...")
    results = backtest.run()
    
    if results:
        backtest.plot_results(results, save_path='results_full_market.png')

if __name__ == "__main__":
    import sys
    
    if len(sys.argv) > 1:
        mode = sys.argv[1]
        
        if mode == 'basic':
            example_basic()
        elif mode == 'custom':
            example_custom_params()
        elif mode == 'pool':
            example_stock_pool()
        elif mode == 'compare':
            example_strategy_comparison()
        elif mode == 'full':
            example_full_market()
        else:
            print(f"未知模式: {mode}")
            print("可用模式: basic, custom, pool, compare, full")
    else:
        print("请指定运行模式:")
        print("  python example.py basic     - 基本回测")
        print("  python example.py custom    - 自定义参数回测")
        print("  python example.py pool      - 指定股票池回测")
        print("  python example.py compare   - 策略对比")
        print("  python example.py full      - 全市场回测")
