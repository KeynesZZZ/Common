using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using TMPro;
using BingoGame.Core;
using BingoGame.Core.Models;
using BingoGame.GameModes.ClassicBingo;

namespace BingoGame.Views
{
    /// <summary>
    /// Bingo棋盘视图
    /// 负责显示Bingo棋盘的UI
    /// </summary>
    public class BingoBoardView : MonoBehaviour, IBingoBoardView
    {
        [Header("UI组件")]
        [SerializeField] private Transform boardContainer;
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private Color markedColor = Color.yellow;
        [SerializeField] private Color freeSpaceColor = Color.green;
        [SerializeField] private Color defaultColor = Color.white;

        private BingoCell[,] cellViews;
        private BingoBoard boardModel;

        /// <summary>
        /// 初始化棋盘视图
        /// </summary>
        /// <param name="board">Bingo棋盘模型</param>
        public async UniTask InitializeBoardAsync(BingoBoard board)
        {
            boardModel = board;
            cellViews = new BingoCell[GameBoard.BoardSize, GameBoard.BoardSize];

            await CreateBoardUIAsync();
            Debug.Log("Bingo棋盘视图初始化完成");
        }

        /// <summary>
        /// 创建棋盘UI
        /// </summary>
        private async UniTask CreateBoardUIAsync()
        {
            if (boardContainer == null)
            {
                Debug.LogError("棋盘容器未设置");
                return;
            }

            if (cellPrefab == null)
            {
                Debug.LogError("单元格预制体未设置");
                return;
            }

            for (int row = 0; row < GameBoard.BoardSize; row++)
            {
                for (int col = 0; col < GameBoard.BoardSize; col++)
                {
                    var cellObj = Instantiate(cellPrefab, boardContainer);
                    var cellView = cellObj.GetComponent<BingoCellView>();
                    
                    if (cellView != null)
                    {
                        var cellData = boardModel.GetCell(row, col) as BingoCell;
                        cellView.Initialize(cellData, row, col);
                        cellViews[row, col] = cellData;
                    }
                }
            }

            await UniTask.Yield();
        }

        /// <summary>
        /// 更新指定位置的单元格
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        public async UniTask UpdateCellAsync(int row, int col)
        {
            if (row < 0 || row >= GameBoard.BoardSize || col < 0 || col >= GameBoard.BoardSize)
            {
                Debug.LogWarning($"无效的单元格位置: ({row}, {col})");
                return;
            }

            var cellData = boardModel.GetCell(row, col) as BingoCell;
            var cellTransform = GetCellTransform(row, col);

            if (cellTransform != null)
            {
                var cellView = cellTransform.GetComponent<BingoCellView>();
                if (cellView != null)
                {
                    cellView.SetMarked(cellData.IsMarked);
                }
            }

            await UniTask.Yield();
        }

        /// <summary>
        /// 高亮指定位置的单元格
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        public async UniTask HighlightCellAsync(int row, int col)
        {
            var cellTransform = GetCellTransform(row, col);
            
            if (cellTransform != null)
            {
                var cellView = cellTransform.GetComponent<BingoCellView>();
                if (cellView != null)
                {
                    cellView.Highlight();
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            if (cellTransform != null)
            {
                var cellView = cellTransform.GetComponent<BingoCellView>();
                if (cellView != null)
                {
                    cellView.Unhighlight();
                }
            }
        }

        /// <summary>
        /// 重置棋盘视图
        /// </summary>
        public async UniTask ResetBoardAsync()
        {
            for (int row = 0; row < GameBoard.BoardSize; row++)
            {
                for (int col = 0; col < GameBoard.BoardSize; col++)
                {
                    var cellTransform = GetCellTransform(row, col);
                    
                    if (cellTransform != null)
                    {
                        var cellView = cellTransform.GetComponent<BingoCellView>();
                        if (cellView != null)
                        {
                            cellView.Reset();
                        }
                    }
                }
            }

            await UniTask.Yield();
            Debug.Log("Bingo棋盘视图已重置");
        }

        /// <summary>
        /// 获取指定位置的单元格Transform
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        /// <returns>单元格Transform</returns>
        public Transform GetCellTransform(int row, int col)
        {
            if (row < 0 || row >= GameBoard.BoardSize || col < 0 || col >= GameBoard.BoardSize)
            {
                return null;
            }

            if (boardContainer == null)
            {
                return null;
            }

            int index = row * GameBoard.BoardSize + col;
            
            if (index >= 0 && index < boardContainer.childCount)
            {
                return boardContainer.GetChild(index);
            }

            return null;
        }
    }

    /// <summary>
    /// Bingo单元格视图
    /// </summary>
    public class BingoCellView : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private TextMeshProUGUI numberText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button cellButton;
        [SerializeField] private Image markIndicator;

        [Header("颜色设置")]
        [SerializeField] private Color markedColor = Color.yellow;
        [SerializeField] private Color freeSpaceColor = Color.green;
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private Color highlightColor = Color.cyan;

        private BingoCell cellData;
        private int row;
        private int col;
        private bool isInteractable = true;

        /// <summary>
        /// 初始化单元格
        /// </summary>
        /// <param name="cell">单元格数据</param>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        public void Initialize(BingoCell cell, int row, int col)
        {
            this.cellData = cell;
            this.row = row;
            this.col = col;

            if (numberText != null)
            {
                if (cell.IsFreeSpace)
                {
                    numberText.text = "FREE";
                }
                else
                {
                    numberText.text = cell.Number.ToString();
                }
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = cell.IsFreeSpace ? freeSpaceColor : defaultColor;
            }

            if (markIndicator != null)
            {
                markIndicator.gameObject.SetActive(false);
            }

            if (cellButton != null)
            {
                cellButton.onClick.AddListener(OnCellClicked);
            }

            Reset();
        }

        /// <summary>
        /// 设置标记状态
        /// </summary>
        /// <param name="marked">是否标记</param>
        public void SetMarked(bool marked)
        {
            if (markIndicator != null)
            {
                markIndicator.gameObject.SetActive(marked);
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = marked ? markedColor : 
                    (cellData.IsFreeSpace ? freeSpaceColor : defaultColor);
            }
        }

        /// <summary>
        /// 设置可交互状态
        /// </summary>
        /// <param name="interactable">是否可交互</param>
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
            
            if (cellButton != null)
            {
                cellButton.interactable = interactable;
            }
        }

        /// <summary>
        /// 高亮单元格
        /// </summary>
        public void Highlight()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = highlightColor;
            }
        }

        /// <summary>
        /// 取消高亮
        /// </summary>
        public void Unhighlight()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = cellData.IsMarked ? markedColor : 
                    (cellData.IsFreeSpace ? freeSpaceColor : defaultColor);
            }
        }

        /// <summary>
        /// 重置单元格
        /// </summary>
        public void Reset()
        {
            SetMarked(false);
            SetInteractable(true);
        }

        /// <summary>
        /// 单元格点击事件
        /// </summary>
        private void OnCellClicked()
        {
            if (!isInteractable)
            {
                return;
            }

            Debug.Log($"点击单元格 ({row}, {col})");
            
            var position = new Vector2Int(row, col);
            Controllers.GameController.Instance?.HandlePlayerInputAsync(position).Forget();
        }
    }
}
