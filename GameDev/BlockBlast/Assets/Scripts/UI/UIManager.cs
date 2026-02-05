using System.Collections;
using System.Collections.Generic;
using BlockBlast.Core;
using BlockBlast.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockBlast.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Board UI")]
        public RectTransform boardRect;
        public GameObject cellPrefab;
        public GameObject blockCellPrefab;
        public Transform boardCellsParent;
        public Transform placedBlocksParent;

        [Header("Block Previews")]
        public RectTransform[] previewRects;
        public GameObject blockPreviewPrefab;

        [Header("Score UI")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI highScoreText;
        public TextMeshProUGUI comboText;
        public GameObject comboDisplay;

        [Header("Game Over UI")]
        public GameObject gameOverPanel;
        public TextMeshProUGUI finalScoreText;
        public Button restartButton;

        [Header("Animation Settings")]
        public float eliminationAnimDuration = 0.3f;
        public float blockPlaceDuration = 0.2f;
        public Ease blockPlaceEase = Ease.OutBack;

        private List<GameObject> boardCells = new List<GameObject>(64);
        private List<GameObject> placedBlocks = new List<GameObject>();
        private GameObject[,] cellObjects = new GameObject[8, 8];
        private GameObject[] previewBlockObjects = new GameObject[3];
        private Canvas canvas;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            InitializeBoard();

            if (restartButton != null)
                restartButton.onClick.AddListener(() => GameManager.Instance.RestartGame());
        }

        private void Start()
        {
            ScoreManager scoreManager = GameManager.Instance.scoreManager;
            scoreManager.OnScoreChanged += UpdateScoreDisplay;
            scoreManager.OnComboChanged += UpdateComboDisplay;
            scoreManager.OnHighScoreChanged += UpdateHighScoreDisplay;

            UpdateScoreDisplay(0);
            UpdateHighScoreDisplay(scoreManager.HighScore);
            comboDisplay.SetActive(false);
        }

        private void InitializeBoard()
        {
            float cellSize = boardRect.rect.width / BoardManager.BOARD_SIZE;

            for (int y = 0; y < BoardManager.BOARD_SIZE; y++)
            {
                for (int x = 0; x < BoardManager.BOARD_SIZE; x++)
                {
                    GameObject cell = Instantiate(cellPrefab, boardCellsParent);
                    RectTransform cellRect = cell.GetComponent<RectTransform>();
                    cellRect.sizeDelta = new Vector2(cellSize, cellSize);
                    cellRect.anchoredPosition = new Vector2(
                        x * cellSize - boardRect.rect.width / 2 + cellSize / 2,
                        y * cellSize - boardRect.rect.height / 2 + cellSize / 2
                    );

                    boardCells.Add(cell);
                    cellObjects[x, y] = cell;
                }
            }
        }

        public void ClearBoard()
        {
            foreach (var block in placedBlocks)
            {
                Destroy(block);
            }
            placedBlocks.Clear();

            for (int y = 0; y < BoardManager.BOARD_SIZE; y++)
            {
                for (int x = 0; x < BoardManager.BOARD_SIZE; x++)
                {
                    Image cellImage = cellObjects[x, y].GetComponent<Image>();
                    cellImage.color = Color.white;
                }
            }
        }

        public void UpdateBlockPreviews(BlockShape[] blocks, bool[] used)
        {
            for (int i = 0; i < 3; i++)
            {
                if (previewBlockObjects[i] != null)
                {
                    Destroy(previewBlockObjects[i]);
                }

                if (used[i]) continue;

                GameObject previewObj = CreateBlockVisual(blocks[i], previewRects[i]);
                previewBlockObjects[i] = previewObj;

                BlockDragHandler dragHandler = previewObj.AddComponent<BlockDragHandler>();
                dragHandler.Initialize(i, blocks[i], canvas);
            }
        }

        private GameObject CreateBlockVisual(BlockShape shape, RectTransform parent)
        {
            GameObject blockObj = new GameObject("BlockVisual");
            blockObj.transform.SetParent(parent, false);

            RectTransform blockRect = blockObj.AddComponent<RectTransform>();
            blockRect.anchorMin = Vector2.zero;
            blockRect.anchorMax = Vector2.one;
            blockRect.offsetMin = Vector2.zero;
            blockRect.offsetMax = Vector2.zero;

            CanvasGroup canvasGroup = blockObj.AddComponent<CanvasGroup>();

            float cellSize = Mathf.Min(parent.rect.width / shape.width, parent.rect.height / shape.height) * 0.8f;
            float offsetX = (parent.rect.width - shape.width * cellSize) / 2;
            float offsetY = (parent.rect.height - shape.height * cellSize) / 2;

            for (int y = 0; y < shape.height; y++)
            {
                for (int x = 0; x < shape.width; x++)
                {
                    if (!shape.IsCellOccupied(x, y)) continue;

                    GameObject cell = Instantiate(blockCellPrefab, blockObj.transform);
                    RectTransform cellRect = cell.GetComponent<RectTransform>();
                    Image cellImage = cell.GetComponent<Image>();

                    cellRect.sizeDelta = new Vector2(cellSize, cellSize);
                    cellRect.anchoredPosition = new Vector2(
                        offsetX + x * cellSize,
                        offsetY + (shape.height - 1 - y) * cellSize
                    );
                    cellImage.color = shape.color;
                }
            }

            return blockObj;
        }

        public void PlaceBlockVisual(BlockShape shape, int boardX, int boardY)
        {
            GameObject blockObj = new GameObject("PlacedBlock");
            blockObj.transform.SetParent(placedBlocksParent, false);

            RectTransform blockRect = blockObj.AddComponent<RectTransform>();
            float cellSize = boardRect.rect.width / BoardManager.BOARD_SIZE;

            for (int y = 0; y < shape.height; y++)
            {
                for (int x = 0; x < shape.width; x++)
                {
                    if (!shape.IsCellOccupied(x, y)) continue;

                    int targetX = boardX + x;
                    int targetY = boardY + y;

                    GameObject cell = Instantiate(blockCellPrefab, blockObj.transform);
                    RectTransform cellRect = cell.GetComponent<RectTransform>();
                    Image cellImage = cell.GetComponent<Image>();

                    cellRect.sizeDelta = new Vector2(cellSize, cellSize);
                    cellRect.anchoredPosition = new Vector2(
                        targetX * cellSize - boardRect.rect.width / 2 + cellSize / 2,
                        targetY * cellSize - boardRect.rect.height / 2 + cellSize / 2
                    );
                    cellImage.color = shape.color;

                    cellRect.localScale = Vector3.zero;
                    cellRect.DOScale(1f, blockPlaceDuration).SetEase(blockPlaceEase);
                }
            }

            placedBlocks.Add(blockObj);
        }

        public IEnumerator PlayEliminationAnimation(EliminationResult result)
        {
            List<Tween> activeTweens = new List<Tween>();

            foreach (int row in result.rows)
            {
                for (int x = 0; x < BoardManager.BOARD_SIZE; x++)
                {
                    GameObject cell = cellObjects[x, row];
                    Image cellImage = cell.GetComponent<Image>();

                    Tween tween = cellImage.DOColor(Color.white, 0.1f)
                        .SetLoops(2, LoopType.Yoyo)
                        .OnComplete(() => cellImage.color = Color.white);
                    activeTweens.Add(tween);
                }
            }

            foreach (int col in result.columns)
            {
                for (int y = 0; y < BoardManager.BOARD_SIZE; y++)
                {
                    GameObject cell = cellObjects[col, y];
                    Image cellImage = cell.GetComponent<Image>();

                    Tween tween = cellImage.DOColor(Color.white, 0.1f)
                        .SetLoops(2, LoopType.Yoyo)
                        .OnComplete(() => cellImage.color = Color.white);
                    activeTweens.Add(tween);
                }
            }

            yield return new WaitForSeconds(0.2f);

            foreach (var block in placedBlocks)
            {
                RectTransform blockRect = block.GetComponent<RectTransform>();
                blockRect.DOScale(0f, eliminationAnimDuration).SetEase(Ease.InBack);
            }

            yield return new WaitForSeconds(eliminationAnimDuration);

            foreach (var block in placedBlocks)
            {
                Destroy(block);
            }
            placedBlocks.Clear();
        }

        public void ShowPlacementHint(BlockShape shape)
        {
            // 可以在这里添加放置提示效果
        }

        public void HidePlacementHint()
        {
            // 隐藏放置提示
        }

        public void UpdateScoreDisplay(int score)
        {
            scoreText.text = score.ToString("N0");
            scoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 1, 0.5f);
        }

        public void UpdateHighScoreDisplay(int highScore)
        {
            highScoreText.text = $"BEST: {highScore:N0}";
        }

        public void UpdateComboDisplay(int combo)
        {
            if (combo > 1)
            {
                comboDisplay.SetActive(true);
                comboText.text = $"x{combo}";
                comboText.transform.DOPunchScale(Vector3.one * 0.5f, 0.3f, 1, 0.5f);
            }
            else
            {
                comboDisplay.SetActive(false);
            }
        }

        public void ShowGameOverScreen(int finalScore, int highScore)
        {
            gameOverPanel.SetActive(true);
            finalScoreText.text = $"Score: {finalScore:N0}\nBest: {highScore:N0}";
        }

        public void ShowGameUI()
        {
            gameOverPanel.SetActive(false);
        }

        public RectTransform GetBoardRectTransform()
        {
            return boardRect;
        }
    }
}
