using System.Collections.Generic;
using SimpleBoard.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BlockBlast
{
    /// <summary>
    /// 游戏板UI控制器
    /// </summary>
    public class GameBoardUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _boardContainer;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private GameObject _blockCellPrefab;

        [Header("Settings")]
        [SerializeField] private float _cellSize = 50f;
        [SerializeField] private float _cellSpacing = 5f;

        private BlockBlastGame _game;
        private RectTransform[,] _cellUIs;
        private Dictionary<GridPosition, GameObject> _placedBlocks;

        public void Initialize(BlockBlastGame game)
        {
            _game = game;
            _placedBlocks = new Dictionary<GridPosition, GameObject>();
            
            CreateBoardGrid();
            
            // 订阅事件
            _game.OnBlockPlaced += OnBlockPlaced;
            _game.OnLinesCleared += OnLinesCleared;
            _game.OnGameStartedEvent += OnGameStarted;
        }

        private void OnDestroy()
        {
            if (_game != null)
            {
                _game.OnBlockPlaced -= OnBlockPlaced;
                _game.OnLinesCleared -= OnLinesCleared;
                _game.OnGameStartedEvent -= OnGameStarted;
            }
        }

        /// <summary>
        /// 创建游戏板网格
        /// </summary>
        private void CreateBoardGrid()
        {
            int width = _game.Config.BoardWidth;
            int height = _game.Config.BoardHeight;
            
            _cellUIs = new RectTransform[height, width];
            
            float totalWidth = width * _cellSize + (width - 1) * _cellSpacing;
            float totalHeight = height * _cellSize + (height - 1) * _cellSpacing;
            
            _boardContainer.sizeDelta = new Vector2(totalWidth, totalHeight);

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    var cell = Instantiate(_cellPrefab, _boardContainer);
                    var rectTransform = cell.GetComponent<RectTransform>();
                    
                    float x = col * (_cellSize + _cellSpacing);
                    float y = -(row * (_cellSize + _cellSpacing));
                    
                    rectTransform.anchoredPosition = new Vector2(x, y);
                    rectTransform.sizeDelta = new Vector2(_cellSize, _cellSize);
                    
                    _cellUIs[row, col] = rectTransform;
                }
            }
        }

        /// <summary>
        /// 获取网格位置对应的世界坐标
        /// </summary>
        public Vector2 GetCellWorldPosition(GridPosition position)
        {
            if (position.RowIndex < 0 || position.RowIndex >= _cellUIs.GetLength(0) ||
                position.ColumnIndex < 0 || position.ColumnIndex >= _cellUIs.GetLength(1))
            {
                return Vector2.zero;
            }

            var cell = _cellUIs[position.RowIndex, position.ColumnIndex];
            return cell.position;
        }

        /// <summary>
        /// 获取鼠标位置对应的网格坐标
        /// </summary>
        public bool GetGridPositionFromPointer(Vector3 pointerPosition, out GridPosition gridPosition)
        {
            gridPosition = new GridPosition(0, 0);
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(pointerPosition);
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _boardContainer, pointerPosition, null, out Vector2 localPoint);

            float totalWidth = _game.Config.BoardWidth * (_cellSize + _cellSpacing);
            float totalHeight = _game.Config.BoardHeight * (_cellSize + _cellSpacing);

            if (localPoint.x < 0 || localPoint.x > totalWidth ||
                localPoint.y > 0 || localPoint.y < -totalHeight)
            {
                return false;
            }

            int col = Mathf.FloorToInt(localPoint.x / (_cellSize + _cellSpacing));
            int row = Mathf.FloorToInt(-localPoint.y / (_cellSize + _cellSpacing));

            col = Mathf.Clamp(col, 0, _game.Config.BoardWidth - 1);
            row = Mathf.Clamp(row, 0, _game.Config.BoardHeight - 1);

            gridPosition = new GridPosition(row, col);
            return true;
        }

        /// <summary>
        /// 高亮有效放置位置
        /// </summary>
        public void HighlightValidPlacements(BlockData block, List<GridPosition> validPositions)
        {
            ClearHighlights();

            foreach (var position in validPositions)
            {
                var cells = block.GetWorldPositions(position);
                foreach (var cell in cells)
                {
                    if (cell.RowIndex >= 0 && cell.RowIndex < _cellUIs.GetLength(0) &&
                        cell.ColumnIndex >= 0 && cell.ColumnIndex < _cellUIs.GetLength(1))
                    {
                        var cellUI = _cellUIs[cell.RowIndex, cell.ColumnIndex];
                        var image = cellUI.GetComponent<Image>();
                        if (image != null)
                        {
                            image.color = new Color(0.5f, 1f, 0.5f, 0.5f);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 清除高亮
        /// </summary>
        public void ClearHighlights()
        {
            for (int row = 0; row < _cellUIs.GetLength(0); row++)
            {
                for (int col = 0; col < _cellUIs.GetLength(1); col++)
                {
                    var cellUI = _cellUIs[row, col];
                    var image = cellUI.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = Color.white;
                    }
                }
            }
        }

        /// <summary>
        /// 方块放置事件处理
        /// </summary>
        private void OnBlockPlaced(GridPosition position, BlockData block)
        {
            var cells = block.GetWorldPositions(position);
            
            foreach (var cell in cells)
            {
                var blockCell = Instantiate(_blockCellPrefab, _boardContainer);
                var rectTransform = blockCell.GetComponent<RectTransform>();
                var image = blockCell.GetComponent<Image>();
                
                float x = cell.ColumnIndex * (_cellSize + _cellSpacing);
                float y = -(cell.RowIndex * (_cellSize + _cellSpacing));
                
                rectTransform.anchoredPosition = new Vector2(x, y);
                rectTransform.sizeDelta = new Vector2(_cellSize, _cellSize);
                
                if (image != null)
                {
                    image.color = block.Color;
                }

                _placedBlocks[cell] = blockCell;
            }
        }

        /// <summary>
        /// 行/列消除事件处理
        /// </summary>
        private void OnLinesCleared(List<int> rows, List<int> cols)
        {
            // 移除被消除的方块
            var toRemove = new List<GridPosition>();
            
            foreach (var kvp in _placedBlocks)
            {
                var pos = kvp.Key;
                if (rows.Contains(pos.RowIndex) || cols.Contains(pos.ColumnIndex))
                {
                    toRemove.Add(pos);
                }
            }

            foreach (var pos in toRemove)
            {
                if (_placedBlocks.TryGetValue(pos, out GameObject block))
                {
                    Destroy(block);
                    _placedBlocks.Remove(pos);
                }
            }
        }

        /// <summary>
        /// 游戏开始事件处理
        /// </summary>
        private void OnGameStarted()
        {
            // 清除所有已放置的方块
            foreach (var kvp in _placedBlocks)
            {
                Destroy(kvp.Value);
            }
            _placedBlocks.Clear();
            
            ClearHighlights();
        }
    }
}
