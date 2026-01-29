using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SimpleBoard.Core;
using SimpleJob.Runtime;

namespace BlockBlast
{
    /// <summary>
    /// Block Blast 游戏主控制器
    /// 集成 SimpleBoard 和 SimpleJob 框架
    /// </summary>
    public class BlockBlastGame : IDisposable
    {
        private BlockGameBoard _board;
        private ScoreManager _scoreManager;
        private JobsExecutor _jobExecutor;
        private GameConfig _config;
        private List<BlockData> _availableBlocks;
        private bool _isGameOver;
        private bool _isPaused;

        public BlockGameBoard Board => _board;
        public ScoreManager ScoreManager => _scoreManager;
        public GameConfig Config => _config;
        public IReadOnlyList<BlockData> AvailableBlocks => _availableBlocks;
        public bool IsGameOver => _isGameOver;
        public bool IsPaused => _isPaused;

        /// <summary>
        /// 当游戏开始时触发
        /// </summary>
        public event Action OnGameStarted;
        
        /// <summary>
        /// 当游戏结束时触发
        /// </summary>
        public event Action OnGameOver;
        
        /// <summary>
        /// 当方块被放置时触发
        /// </summary>
        public event Action<GridPosition, BlockData> OnBlockPlaced;
        
        /// <summary>
        /// 当行/列被消除时触发
        /// </summary>
        public event Action<List<int>, List<int>> OnLinesCleared;
        
        /// <summary>
        /// 当可用方块更新时触发
        /// </summary>
        public event Action OnAvailableBlocksUpdated;
        
        /// <summary>
        /// 当游戏暂停状态改变时触发
        /// </summary>
        public event Action<bool> OnPauseStateChanged;

        public BlockBlastGame(GameConfig config = null)
        {
            _config = config ?? GameConfig.Default;
            _board = new BlockGameBoard();
            _scoreManager = new ScoreManager(_config);
            _jobExecutor = new JobsExecutor();
            _availableBlocks = new List<BlockData>();
            
            // 订阅游戏板事件
            _board.OnBlockPlaced += (pos, block) => OnBlockPlaced?.Invoke(pos, block);
            _board.OnLinesCleared += OnLinesClearedInternal;
        }

        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartGame()
        {
            _isGameOver = false;
            _isPaused = false;
            
            // 初始化游戏板
            _board.Initialize(_config.BoardWidth, _config.BoardHeight);
            _scoreManager.ResetScore();
            GenerateNewBlocks();
            
            OnGameStarted?.Invoke();
        }

        /// <summary>
        /// 异步放置方块
        /// </summary>
        public async UniTask<bool> PlaceBlockAsync(BlockData block, GridPosition position)
        {
            if (_isGameOver || _isPaused)
                return false;

            if (!_availableBlocks.Contains(block))
                return false;

            // 创建作业序列
            var jobs = new List<IJob>();
            
            // 放置作业
            jobs.Add(new PlaceBlockJob(_board, block, position, 0));
            
            // 检查是否需要消除
            var (rowsToClear, colsToClear) = GetLinesToClear(block, position);
            if (rowsToClear.Count > 0 || colsToClear.Count > 0)
            {
                jobs.Add(new ClearLinesJob(_board, rowsToClear, colsToClear, 1));
                jobs.Add(new UpdateScoreJob(_scoreManager, rowsToClear.Count, colsToClear.Count, 2));
            }
            
            // 执行作业
            await _jobExecutor.ExecuteJobsAsync(jobs);
            
            // 更新游戏状态
            _availableBlocks.Remove(block);
            
            // 检查是否需要生成新方块
            if (_availableBlocks.Count == 0)
            {
                GenerateNewBlocks();
            }
            
            // 检查游戏是否结束
            CheckGameOver();
            
            return true;
        }

        /// <summary>
        /// 同步放置方块（用于简单场景）
        /// </summary>
        public bool PlaceBlock(BlockData block, GridPosition position)
        {
            if (_isGameOver || _isPaused)
                return false;

            if (!_availableBlocks.Contains(block))
                return false;

            if (_board.PlaceBlock(block, position))
            {
                _availableBlocks.Remove(block);
                
                if (_availableBlocks.Count == 0)
                {
                    GenerateNewBlocks();
                }
                
                CheckGameOver();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查是否可以放置指定方块
        /// </summary>
        public bool CanPlaceBlock(BlockData block, GridPosition position)
        {
            return _board.CanPlaceBlock(block, position);
        }

        /// <summary>
        /// 获取方块的有效放置位置
        /// </summary>
        public List<GridPosition> GetValidPlacements(BlockData block)
        {
            return _board.GetValidPlacements(block);
        }

        /// <summary>
        /// 暂停/继续游戏
        /// </summary>
        public void TogglePause()
        {
            _isPaused = !_isPaused;
            OnPauseStateChanged?.Invoke(_isPaused);
        }

        /// <summary>
        /// 重置游戏
        /// </summary>
        public void ResetGame()
        {
            StartGame();
        }

        /// <summary>
        /// 生成新的方块组
        /// </summary>
        private void GenerateNewBlocks()
        {
            _availableBlocks.Clear();
            
            for (int i = 0; i < _config.BlocksPerTurn; i++)
            {
                _availableBlocks.Add(BlockData.CreateRandom());
            }
            
            OnAvailableBlocksUpdated?.Invoke();
        }

        /// <summary>
        /// 获取需要消除的行和列
        /// </summary>
        private (List<int> rows, List<int> cols) GetLinesToClear(BlockData block, GridPosition origin)
        {
            var rows = new List<int>();
            var cols = new List<int>();
            
            // 临时放置方块来检查
            var positions = block.GetWorldPositions(origin);
            
            // 检查涉及的行
            var affectedRows = new HashSet<int>();
            var affectedCols = new HashSet<int>();
            
            foreach (var pos in positions)
            {
                affectedRows.Add(pos.RowIndex);
                affectedCols.Add(pos.ColumnIndex);
            }
            
            // 检查这些行/列是否已满
            foreach (var row in affectedRows)
            {
                bool isFull = true;
                for (int col = 0; col < _board.ColumnCount; col++)
                {
                    var checkPos = new GridPosition(row, col);
                    if (!_board.IsPositionOnBoard(checkPos))
                    {
                        isFull = false;
                        break;
                    }
                    
                    // 检查是否是被放置的方块或者是已有方块
                    bool isPlacedBlock = positions.Exists(p => p.RowIndex == row && p.ColumnIndex == col);
                    bool hasExistingBlock = _board[checkPos].HasBlock;
                    
                    if (!isPlacedBlock && !hasExistingBlock)
                    {
                        isFull = false;
                        break;
                    }
                }
                
                if (isFull)
                    rows.Add(row);
            }
            
            foreach (var col in affectedCols)
            {
                bool isFull = true;
                for (int row = 0; row < _board.RowCount; row++)
                {
                    var checkPos = new GridPosition(row, col);
                    if (!_board.IsPositionOnBoard(checkPos))
                    {
                        isFull = false;
                        break;
                    }
                    
                    bool isPlacedBlock = positions.Exists(p => p.RowIndex == row && p.ColumnIndex == col);
                    bool hasExistingBlock = _board[checkPos].HasBlock;
                    
                    if (!isPlacedBlock && !hasExistingBlock)
                    {
                        isFull = false;
                        break;
                    }
                }
                
                if (isFull)
                    cols.Add(col);
            }
            
            return (rows, cols);
        }

        /// <summary>
        /// 行/列消除的内部处理
        /// </summary>
        private void OnLinesClearedInternal(List<int> rows, List<int> cols)
        {
            _scoreManager.AddScore(rows.Count, cols.Count);
            OnLinesCleared?.Invoke(rows, cols);
        }

        /// <summary>
        /// 检查游戏是否结束
        /// </summary>
        private void CheckGameOver()
        {
            if (!_board.CanPlaceAnyBlock(_availableBlocks))
            {
                _isGameOver = true;
                OnGameOver?.Invoke();
            }
        }

        /// <summary>
        /// 跳过当前回合（放弃剩余方块）
        /// </summary>
        public void SkipTurn()
        {
            if (_isGameOver || _isPaused)
                return;

            GenerateNewBlocks();
            _scoreManager.ResetCombo();
            CheckGameOver();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _board?.Dispose();
            _board = null;
            
            OnGameStarted = null;
            OnGameOver = null;
            OnBlockPlaced = null;
            OnLinesCleared = null;
            OnAvailableBlocksUpdated = null;
            OnPauseStateChanged = null;
        }
    }
}
