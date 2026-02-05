using System.Collections;
using System.Collections.Generic;
using BlockBlast.Core;
using BlockBlast.UI;
using UnityEngine;

namespace BlockBlast.Managers
{
    /// <summary>
    /// 游戏管理器 - 控制游戏流程和状态
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Managers")]
        public BoardManager boardManager;
        public BlockGenerator blockGenerator;
        public ScoreManager scoreManager;
        public UIManager uiManager;

        [Header("Settings")]
        public float eliminationDelay = 0.3f;

        private BlockShape[] availableBlocks = new BlockShape[3];
        private bool[] usedBlocks = new bool[3];
        private GameState currentState = GameState.Idle;
        private bool hadElimination = false;

        public enum GameState
        {
            Idle,       // 空闲状态
            Dragging,  // 拖拽中
            Placing,   // 放置中
            Eliminating, // 消除中
            GameOver    // 游戏结束
        }

        public GameState CurrentState => currentState;
        public BlockShape[] AvailableBlocks => availableBlocks;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            StartNewGame();
        }

        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartNewGame()
        {
            scoreManager.Reset();
            boardManager.ClearBoard();
            uiManager.ClearBoard();
            blockGenerator.ResetCombo();

            GenerateNewBlocks();
            currentState = GameState.Idle;
            uiManager.ShowGameUI();
        }

        /// <summary>
        /// 生成新的三个方块
        /// </summary>
        private void GenerateNewBlocks()
        {
            byte[] board = boardManager.GetBoardState();
            availableBlocks = blockGenerator.GenerateBlocks(board, scoreManager.CurrentScore, hadElimination);

            for (int i = 0; i < 3; i++)
            {
                usedBlocks[i] = false;
            }

            uiManager.UpdateBlockPreviews(availableBlocks, usedBlocks);
        }

        /// <summary>
        /// 方块拖拽开始
        /// </summary>
        public void OnBlockDragStart(int blockIndex)
        {
            if (currentState != GameState.Idle || usedBlocks[blockIndex]) return;
            currentState = GameState.Dragging;
            uiManager.ShowPlacementHint(availableBlocks[blockIndex]);
        }

        /// <summary>
        /// 方块拖拽结束
        /// </summary>
        public void OnBlockDragEnd(int blockIndex, Vector2 screenPosition, bool placed)
        {
            currentState = GameState.Idle;
            uiManager.HidePlacementHint();

            if (placed)
            {
                usedBlocks[blockIndex] = true;
                uiManager.UpdateBlockPreviews(availableBlocks, usedBlocks);

                if (AreAllBlocksUsed())
                {
                    StartCoroutine(DelayedBlockGeneration());
                }
            }
        }

        /// <summary>
        /// 尝试放置方块
        /// </summary>
        public bool TryPlaceBlock(BlockShape block, Vector2 screenPosition)
        {
            Vector2Int boardPos = ScreenToBoardPosition(screenPosition);

            if (!boardManager.CanPlaceBlock(block, boardPos.x, boardPos.y))
                return false;

            currentState = GameState.Placing;
            boardManager.PlaceBlock(block, boardPos.x, boardPos.y);
            uiManager.PlaceBlockVisual(block, boardPos.x, boardPos.y);

            StartCoroutine(EliminationSequence());
            return true;
        }

        /// <summary>
        /// 消除序列协程
        /// </summary>
        private IEnumerator EliminationSequence()
        {
            currentState = GameState.Eliminating;

            yield return new WaitForSeconds(0.1f);

            var eliminationResult = boardManager.CheckElimination();

            if (eliminationResult.HasElimination)
            {
                hadElimination = true;
                yield return StartCoroutine(uiManager.PlayEliminationAnimation(eliminationResult));
                boardManager.Eliminate(eliminationResult);
                scoreManager.AddScore(eliminationResult.TotalLines);
            }
            else
            {
                hadElimination = false;
                scoreManager.AddScore(0);
            }

            yield return new WaitForSeconds(eliminationDelay);

            CheckGameOver();

            if (currentState != GameState.GameOver)
            {
                currentState = GameState.Idle;
            }
        }

        /// <summary>
        /// 延迟生成新方块
        /// </summary>
        private IEnumerator DelayedBlockGeneration()
        {
            yield return new WaitForSeconds(eliminationDelay + 0.2f);
            GenerateNewBlocks();
        }

        /// <summary>
        /// 检测游戏是否结束
        /// </summary>
        private void CheckGameOver()
        {
            if (boardManager.IsGameOver(availableBlocks))
            {
                currentState = GameState.GameOver;
                uiManager.ShowGameOverScreen(scoreManager.CurrentScore, scoreManager.HighScore);
            }
        }

        /// <summary>
        /// 检测所有方块是否已使用
        /// </summary>
        private bool AreAllBlocksUsed()
        {
            for (int i = 0; i < 3; i++)
            {
                if (!usedBlocks[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// 屏幕坐标转换为棋盘坐标
        /// </summary>
        private Vector2Int ScreenToBoardPosition(Vector2 screenPosition)
        {
            RectTransform boardRect = uiManager.GetBoardRectTransform();
            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                boardRect,
                screenPosition,
                null,
                out localPoint
            );

            float cellSize = boardRect.rect.width / BoardManager.BOARD_SIZE;
            int x = Mathf.FloorToInt((localPoint.x + boardRect.rect.width / 2) / cellSize);
            int y = Mathf.FloorToInt((localPoint.y + boardRect.rect.height / 2) / cellSize);

            return new Vector2Int(x, y);
        }

        /// <summary>
        /// 重新开始游戏
        /// </summary>
        public void RestartGame()
        {
            StartNewGame();
        }
    }
}
