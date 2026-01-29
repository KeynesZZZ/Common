using System.Collections.Generic;
using SimpleBoard.Core;
using UnityEngine;

namespace BlockBlast
{
    /// <summary>
    /// 方块托盘UI - 显示和管理可用方块
    /// </summary>
    public class BlockTrayUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _trayContainer;
        [SerializeField] private DraggableBlock _blockPrefab;
        [SerializeField] private GameBoardUI _gameBoardUI;

        [Header("Settings")]
        [SerializeField] private float _cellSize = 40f;
        [SerializeField] private float _cellSpacing = 4f;
        [SerializeField] private float _blockSpacing = 100f;

        private BlockBlastGame _game;
        private List<DraggableBlock> _draggableBlocks;
        private DraggableBlock _currentDraggingBlock;

        public void Initialize(BlockBlastGame game)
        {
            _game = game;
            _draggableBlocks = new List<DraggableBlock>();

            // 订阅事件
            _game.OnAvailableBlocksUpdated += OnAvailableBlocksUpdated;
            _game.OnGameStarted += OnGameStarted;
        }

        private void OnDestroy()
        {
            if (_game != null)
            {
                _game.OnAvailableBlocksUpdated -= OnAvailableBlocksUpdated;
                _game.OnGameStarted -= OnGameStarted;
            }
        }

        /// <summary>
        /// 更新方块显示
        /// </summary>
        private void OnAvailableBlocksUpdated()
        {
            ClearBlocks();
            CreateBlocks();
        }

        /// <summary>
        /// 游戏开始事件
        /// </summary>
        private void OnGameStarted()
        {
            ClearBlocks();
            CreateBlocks();
        }

        /// <summary>
        /// 清除所有方块
        /// </summary>
        private void ClearBlocks()
        {
            foreach (var block in _draggableBlocks)
            {
                if (block != null)
                {
                    block.OnDragStarted -= OnBlockDragStarted;
                    block.OnDragEnded -= OnBlockDragEnded;
                    block.OnDragging -= OnBlockDragging;
                    Destroy(block.gameObject);
                }
            }
            _draggableBlocks.Clear();
        }

        /// <summary>
        /// 创建方块
        /// </summary>
        private void CreateBlocks()
        {
            var availableBlocks = _game.AvailableBlocks;
            float startX = -(availableBlocks.Count - 1) * _blockSpacing / 2f;

            for (int i = 0; i < availableBlocks.Count; i++)
            {
                var blockData = availableBlocks[i];
                var draggableBlock = Instantiate(_blockPrefab, _trayContainer);

                // 初始化方块
                draggableBlock.Initialize(blockData, _cellSize, _cellSpacing);

                // 设置位置
                var rectTransform = draggableBlock.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(startX + i * _blockSpacing, 0);

                // 订阅拖拽事件
                draggableBlock.OnDragStarted += OnBlockDragStarted;
                draggableBlock.OnDragEnded += OnBlockDragEnded;
                draggableBlock.OnDragging += OnBlockDragging;

                _draggableBlocks.Add(draggableBlock);
            }
        }

        /// <summary>
        /// 方块开始拖拽
        /// </summary>
        private void OnBlockDragStarted(DraggableBlock block)
        {
            _currentDraggingBlock = block;

            // 显示有效放置位置
            if (_game.Config.ShowPlacementPreview)
            {
                var validPositions = _game.GetValidPlacements(block.BlockData);
                _gameBoardUI.HighlightValidPlacements(block.BlockData, validPositions);
            }
        }

        /// <summary>
        /// 方块拖拽中
        /// </summary>
        private void OnBlockDragging(DraggableBlock block, Vector2 position)
        {
            // 可以在这里添加实时预览效果
        }

        /// <summary>
        /// 方块结束拖拽
        /// </summary>
        private void OnBlockDragEnded(DraggableBlock block, Vector2 position)
        {
            _currentDraggingBlock = null;
            _gameBoardUI.ClearHighlights();

            // 尝试放置方块
            if (_gameBoardUI.GetGridPositionFromPointer(position, out GridPosition gridPosition))
            {
                if (_game.PlaceBlock(block.BlockData, gridPosition))
                {
                    // 放置成功
                    block.MarkAsPlaced();
                    _draggableBlocks.Remove(block);

                    // 检查是否需要生成新方块
                    if (_draggableBlocks.Count == 0)
                    {
                        // 延迟生成新方块，等待消除动画
                        Invoke(nameof(CreateBlocks), 0.5f);
                    }
                }
                else
                {
                    // 放置失败，返回原位
                    block.ResetBlock();
                }
            }
            else
            {
                // 放置在游戏板外，返回原位
                block.ResetBlock();
            }
        }
    }
}
