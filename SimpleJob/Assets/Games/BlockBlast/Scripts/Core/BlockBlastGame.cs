using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleBoard;
using SimpleBoard.Core;
using SimpleBoard.Interfaces;
using SimpleJob.Runtime;
using UnityEngine;

namespace BlockBlast
{
    /// <summary>
    /// Block Blast 游戏主控制器
    /// 集成 SimpleBoard 和 SimpleJob 框架
    /// </summary>
    public class BlockBlastGame :BaseGame<IBlockGridSlot> 
    {
        private ScoreManager _scoreManager;
        private JobsExecutor _jobExecutor;
        private GameConfig _config;
        private List<BlockData> _availableBlocks;
        private bool _isGameOver;
        private bool _isPaused;
        

      

        public ScoreManager ScoreManager => _scoreManager;
        public GameConfig Config => _config;
        public IReadOnlyList<BlockData> AvailableBlocks => _availableBlocks;
        public bool IsGameOver => _isGameOver;
        public bool IsPaused => _isPaused;


        public event Action OnGameStartedEvent;
        public event Action OnGameClosedEvent;
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

        public BlockBlastGame(GameConfig config,IGameBoardDataProvider<IBlockGridSlot> gameBoardDataProvider, IGameBoardSolver<IBlockGridSlot> gameBoardSolver = null,  ISolvedSequencesConsumer<IBlockGridSlot>[] solvedSequencesConsumers = null) : base(gameBoardSolver, gameBoardDataProvider, solvedSequencesConsumers)
        {
            _config = config ?? GameConfig.Default;
            _scoreManager = new ScoreManager(_config);
            _jobExecutor = new JobsExecutor();
        }

        public async UniTask StartAsync(CancellationToken cancellationToken = default)
        {
            StartGame();
        }
        public async UniTask StopAsync(CancellationToken cancellationToken = default)
        {
            await UniTask.WaitForEndOfFrame();
            StopGame();
        }

        protected override void OnGameStarted()
        {
            _isGameOver = false;
            _isPaused = false;
            InitGameLevel(1);
            _scoreManager.ResetScore();
            GenerateNewBlocks();
        }

        protected override void OnGameStopped()
        {
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
            jobs.Add(new PlaceBlockJob(this, block, position, 0));
            
            // 检查是否需要消除
            var (rowsToClear, colsToClear) = GetLinesToClear(block, position);
            if (rowsToClear.Count > 0 || colsToClear.Count > 0)
            {
                jobs.Add(new ClearLinesJob(this, rowsToClear, colsToClear, 1));
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
        /// 放置方块
        /// </summary>
        public bool PlaceBlock(BlockData block, GridPosition origin)
        {
            if (!CanPlaceBlock(block, origin))
                return false;

            var positions = block.GetWorldPositions(origin);
            int blockType = (int)block.Shape;
            
            foreach (var pos in positions)
            {
                var slot = GameBoard[pos];
                slot.SetBlock(blockType, block.Color);
            }

            OnBlockPlaced?.Invoke(origin, block);
            return true;
        }

        
        /// <summary>
        /// 检查方块是否可以放置在指定位置
        /// </summary>
        private bool CanPlaceBlock(BlockData block, GridPosition origin)
        {
            var positions = block.GetWorldPositions(origin);
            foreach (var pos in positions)
            {
                if (!IsEmpty(pos))
                    return false;
            }
            return true;
        }

        
        /// <summary>
        /// 检查位置是否为空
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsEmpty(GridPosition position)
        {
            if (!GameBoard.IsPositionOnBoard(position))
                return false;
            return !GameBoard[position].HasBlock;
        }
        
        /// <summary>
        /// 获取方块的有效放置位置
        /// </summary>
        public List<GridPosition> GetValidPlacements(BlockData block)
        {
            var validPositions = new List<GridPosition>();
            for (int row = 0; row < GameBoard.RowCount; row++)
            {
                for (int col = 0; col < GameBoard.ColumnCount; col++)
                {
                    var pos = new GridPosition(row, col);
                    if (CanPlaceBlock(block, pos))
                        validPositions.Add(pos);
                }
            }

            return validPositions;
        }
        
        /// <summary>
        /// 检查是否可以放置任意方块
        /// </summary>
        public bool CanPlaceAnyBlock(IReadOnlyList<BlockData> blocks)
        {
            foreach (var block in blocks)
            {
                for (int row = 0; row <  GameBoard.RowCount; row++)
                {
                    for (int col = 0; col < GameBoard.ColumnCount; col++)
                    {
                        var pos = new GridPosition(row, col);
                        if (CanPlaceBlock(block, pos))
                            return true;
                    }
                }
            }
            return false;
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
                for (int col = 0; col < GameBoard.ColumnCount; col++)
                {
                    var checkPos = new GridPosition(row, col);
                    if (!GameBoard.IsPositionOnBoard(checkPos))
                    {
                        isFull = false;
                        break;
                    }
                    
                    // 检查是否是被放置的方块或者是已有方块
                    bool isPlacedBlock = positions.Exists(p => p.RowIndex == row && p.ColumnIndex == col);
                    bool hasExistingBlock = GameBoard[checkPos].HasBlock;
                    
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
                for (int row = 0; row < GameBoard.RowCount; row++)
                {
                    var checkPos = new GridPosition(row, col);
                    if (!GameBoard.IsPositionOnBoard(checkPos))
                    {
                        isFull = false;
                        break;
                    }
                    
                    bool isPlacedBlock = positions.Exists(p => p.RowIndex == row && p.ColumnIndex == col);
                    bool hasExistingBlock = GameBoard[checkPos].HasBlock;
                    
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
            
            OnBlockPlaced = null;
            OnLinesCleared = null;
            OnAvailableBlocksUpdated = null;
            OnPauseStateChanged = null;
        }

      
    }
}
