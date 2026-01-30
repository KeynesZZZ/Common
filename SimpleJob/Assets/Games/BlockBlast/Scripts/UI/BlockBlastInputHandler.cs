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

            var draggableBlock = GetDraggableBlockAtPosition(e.WorldPosition);
            if (draggableBlock != null && !draggableBlock.IsPlaced)
            {
                _currentDraggingBlock = draggableBlock;
                _isDragging = true;
               
                draggableBlock.OnBeginDrag(e.WorldPosition);
            }
        }

        /// <summary>
        /// 指针拖拽事件
        /// </summary>
        private void OnPointerDrag(object sender, PointerEventArgs e)
        {
            if (!_isDragging || _currentDraggingBlock == null)
                return;

            _currentDraggingBlock.OnDrag(e.WorldPosition);
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

         
            _currentDraggingBlock.OnEndDrag(e.WorldPosition);
            
            if (!placed)
            {
                _currentDraggingBlock.ResetBlock();
            }
            _currentDraggingBlock = null;
        }

        /// <summary>
        /// 获取指定位置的可拖拽方块
        /// </summary>
        private DraggableBlock GetDraggableBlockAtPosition(Vector2 worldPosition)
        {
            // 将世界坐标转换为屏幕坐标
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            
            // 使用射线检测获取点击的方块
            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
            };

            var raycastResults = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            foreach (var result in raycastResults)
            {
                // 检查当前对象及其父对象是否有DraggableBlock组件
                var draggableBlock = result.gameObject.GetComponent<DraggableBlock>();
                if (draggableBlock != null)
                {
                    return draggableBlock;
                }
                
                // 检查父对象
                draggableBlock = result.gameObject.GetComponentInParent<DraggableBlock>();
                if (draggableBlock != null)
                {
                    return draggableBlock;
                }
            }

            return null;
        }

        /// <summary>
        /// 从屏幕坐标获取可拖拽方块（用于直接传入屏幕坐标的情况）
        /// </summary>
        public DraggableBlock GetDraggableBlockFromScreenPosition(Vector2 screenPosition)
        {
            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = screenPosition
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
                
                draggableBlock = result.gameObject.GetComponentInParent<DraggableBlock>();
                if (draggableBlock != null)
                {
                    return draggableBlock;
                }
            }

            return null;
        }
    }
}
