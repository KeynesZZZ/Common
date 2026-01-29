using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlockBlast
{
    /// <summary>
    /// 可拖拽方块组件
    /// </summary>
    public class DraggableBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Image _image;
        [SerializeField] private CanvasGroup _canvasGroup;

        private BlockData _blockData;
        private Transform _originalParent;
        private Vector2 _originalPosition;
        private bool _isDragging;
        private bool _isPlaced;

        public BlockData BlockData => _blockData;
        public bool IsPlaced => _isPlaced;

        /// <summary>
        /// 当开始拖拽时触发
        /// </summary>
        public event Action<DraggableBlock> OnDragStarted;
        
        /// <summary>
        /// 当拖拽结束时触发
        /// </summary>
        public event Action<DraggableBlock, Vector2> OnDragEnded;
        
        /// <summary>
        /// 当拖拽中触发
        /// </summary>
        public event Action<DraggableBlock, Vector2> OnDragging;

        private void Awake()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            if (_image == null)
                _image = GetComponent<Image>();
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// 初始化方块
        /// </summary>
        public void Initialize(BlockData blockData, float cellSize, float cellSpacing)
        {
            _blockData = blockData;
            _isPlaced = false;

            // 创建方块形状
            CreateBlockShape(cellSize, cellSpacing);
        }

        /// <summary>
        /// 创建方块形状
        /// </summary>
        private void CreateBlockShape(float cellSize, float cellSpacing)
        {
            // 清除旧的内容
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            // 计算方块的尺寸
            int minRow = int.MaxValue, minCol = int.MaxValue;
            int maxRow = int.MinValue, maxCol = int.MinValue;

            foreach (var cell in _blockData.Cells)
            {
                minRow = Mathf.Min(minRow, cell.RowIndex);
                minCol = Mathf.Min(minCol, cell.ColumnIndex);
                maxRow = Mathf.Max(maxRow, cell.RowIndex);
                maxCol = Mathf.Max(maxCol, cell.ColumnIndex);
            }

            int width = maxCol - minCol + 1;
            int height = maxRow - minRow + 1;

            // 设置容器大小
            float totalWidth = width * cellSize + (width - 1) * cellSpacing;
            float totalHeight = height * cellSize + (height - 1) * cellSpacing;
            _rectTransform.sizeDelta = new Vector2(totalWidth, totalHeight);

            // 创建每个单元格
            foreach (var cell in _blockData.Cells)
            {
                var cellObject = new GameObject("Cell", typeof(RectTransform), typeof(Image));
                var cellRect = cellObject.GetComponent<RectTransform>();
                var cellImage = cellObject.GetComponent<Image>();

                cellRect.SetParent(_rectTransform);
                cellRect.localScale = Vector3.one;

                // 计算位置
                float x = (cell.ColumnIndex - minCol) * (cellSize + cellSpacing);
                float y = -(cell.RowIndex - minRow) * (cellSize + cellSpacing);

                cellRect.anchoredPosition = new Vector2(x, y);
                cellRect.sizeDelta = new Vector2(cellSize, cellSize);

                cellImage.color = _blockData.Color;
                cellImage.sprite = _image.sprite;
            }

            // 隐藏主Image，因为我们创建了子单元格
            _image.enabled = false;
        }

        /// <summary>
        /// 标记为已放置
        /// </summary>
        public void MarkAsPlaced()
        {
            _isPlaced = true;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 重置方块状态
        /// </summary>
        public void ResetBlock()
        {
            _isPlaced = false;
            transform.SetParent(_originalParent);
            _rectTransform.anchoredPosition = _originalPosition;
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isPlaced)
                return;

            _isDragging = true;
            _originalParent = transform.parent;
            _originalPosition = _rectTransform.anchoredPosition;

            // 将方块移到最上层
            transform.SetParent(transform.root);
            _canvasGroup.alpha = 0.8f;
            _canvasGroup.blocksRaycasts = false;

            OnDragStarted?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;

            _rectTransform.position = eventData.position;
            OnDragging?.Invoke(this, eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging)
                return;

            _isDragging = false;
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;

            OnDragEnded?.Invoke(this, eventData.position);
        }
    }
}
