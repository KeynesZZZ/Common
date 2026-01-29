using System;
using SimpleBoard.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BlockBlast
{
    /// <summary>
    /// Block Blast 输入处理器
    /// 集成 SimpleBoard.CanvasInputSystem
    /// </summary>
    public class BlockBlastInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasInputSystem _inputSystem;
        [SerializeField] private GameBoardUI _gameBoardUI;
        [SerializeField] private BlockTrayUI _blockTrayUI;

        private BlockBlastGame _game;
        private DraggableBlock _currentDraggingBlock;
        private bool _isDragging;

        /// <summary>
        /// 当方块开始拖拽时触发
        /// </summary>
        public event Action<DraggableBlock> OnBlockDragStarted;
        
        /// <summary>
        /// 当方块拖拽中时触发
        /// </summary>
        public event Action<DraggableBlock, Vector2> OnBlockDragging;
        
        /// <summary>
        /// 当方块结束拖拽时触发
        /// </summary>
        public event Action<DraggableBlock, Vector2, bool> OnBlockDragEnded;

        public void Initialize(BlockBlastGame game)
        {
            _game = game;
            
            if (_inputSystem == null)
            {
                _inputSystem = GetComponent<CanvasInputSystem>();
                if (_inputSystem == null)
                {
                    _inputSystem = gameObject.AddComponent<CanvasInputSystem>();
                }
            }

            // 订阅输入事件
            _inputSystem.PointerDown += OnPointerDown;
            _inputSystem.PointerDrag += OnPointerDrag;
            _inputSystem.PointerUp += OnPointerUp;
        }

        private void OnDestroy()
        {
            if (_inputSystem != null)
            {
                _inputSystem.PointerDown -= OnPointerDown;
                _inputSystem.PointerDrag -= OnPointerDrag;
                _inputSystem.PointerUp -= OnPointerUp;
            }
        }

        /// <summary>
        /// 指针按下事件
        /// </summary>
        private void OnPointerDown(object sender, PointerEventArgs e)
        {
            if (_game.IsGameOver || _game.IsPaused)
                return;

            // 检测是否点击了可拖拽方块
            var draggableBlock = GetDraggableBlockAtPosition(e.WorldPosition);
            if (draggableBlock != null && !draggableBlock.IsPlaced)
            {
                _currentDraggingBlock = draggableBlock;
                _isDragging = true;
                
                // 开始拖拽
                draggableBlock.OnBeginDrag(null);
                
                // 显示有效放置位置
                if (_game.Config.ShowPlacementPreview)
                {
                    var validPositions = _game.GetValidPlacements(draggableBlock.BlockData);
                    _gameBoardUI.HighlightValidPlacements(draggableBlock.BlockData, validPositions);
                }
                
                OnBlockDragStarted?.Invoke(draggableBlock);
            }
        }

        /// <summary>
        /// 指针拖拽事件
        /// </summary>
        private void OnPointerDrag(object sender, PointerEventArgs e)
        {
            if (!_isDragging || _currentDraggingBlock == null)
                return;

            // 更新拖拽位置
            _currentDraggingBlock.OnDrag(null);
            
            OnBlockDragging?.Invoke(_currentDraggingBlock, e.WorldPosition);
        }

        /// <summary>
        /// 指针抬起事件
        /// </summary>
        private void OnPointerUp(object sender, PointerEventArgs e)
        {
            if (!_isDragging || _currentDraggingBlock == null)
                return;

            _isDragging = false;
            _gameBoardUI.ClearHighlights();

            // 尝试放置方块
            bool placed = false;
            if (_gameBoardUI.GetGridPositionFromPointer(e.WorldPosition, out var gridPosition))
            {
                if (_game.PlaceBlock(_currentDraggingBlock.BlockData, gridPosition))
                {
                    placed = true;
                    _currentDraggingBlock.MarkAsPlaced();
                }
            }

            // 结束拖拽
            _currentDraggingBlock.OnEndDrag(null);
            
            if (!placed)
            {
                _currentDraggingBlock.ResetBlock();
            }

            OnBlockDragEnded?.Invoke(_currentDraggingBlock, e.WorldPosition, placed);
            _currentDraggingBlock = null;
        }

        /// <summary>
        /// 获取指定位置的可拖拽方块
        /// </summary>
        private DraggableBlock GetDraggableBlockAtPosition(Vector2 worldPosition)
        {
            // 使用射线检测获取点击的方块
            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = worldPosition
            };

            var raycastResults = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            foreach (var result in raycastResults)
            {
                var draggableBlock = result.gameObject.GetComponent<DraggableBlock>();
                if (draggableBlock != null)
                {
                    return draggableBlock;
                }
            }

            return null;
        }
    }
}
