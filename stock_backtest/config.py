import os

PROJECT_ROOT = os.path.dirname(os.path.abspath(__file__))
DATA_DIR = os.path.join(PROJECT_ROOT, 'data')
CACHE_DIR = os.path.join(DATA_DIR, 'cache')
RESULTS_DIR = os.path.join(PROJECT_ROOT, 'results')

os.makedirs(DATA_DIR, exist_ok=True)
os.makedirs(CACHE_DIR, exist_ok=True)
os.makedirs(RESULTS_DIR, exist_ok=True)

STRATEGY_CONFIG = {
    'name': '涨停短线策略',
    'description': '20天内1次涨停，非ST，涨停后三天收盘价在涨停价2/3以上，换手率5%-10%，量比>1.2',
    'max_holding_days': 7,
    'min_holding_days': 3,
    'stop_loss_pct': -0.05,
    'take_profit_trigger': 0.10,
    'take_profit_fallback': -0.03,
    'position_size': 0.15,
    'max_positions': 5,
}

BACKTEST_CONFIG = {
    'start_date': '2015-01-01',
    'end_date': '2024-12-31',
    'initial_capital': 1000000,
    'commission': 0.0003,
    'slippage': 0.001,
    'stock_pool_size': 100,
}

SELECTION_CONFIG = {
    'days_to_check': 20,
    'limit_up_pct': 0.099,
    'limit_down_pct': -0.099,
    'min_close_after_limit': 0.70,
    'min_turnover': 0.05,
    'max_turnover': 0.10,
    'min_volume_ratio': 1.2,
    'min_market_cap': 5000000000,
    'max_market_cap': 500000000000,
    'min_listed_days': 180,
}

RISK_FREE_RATE = 0.03
TRADING_DAYS_PER_YEAR = 252
