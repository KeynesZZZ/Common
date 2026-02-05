using System.Collections;
using System.Collections.Generic;
using BlockBlast.Core;
using BlockBlast.UI;
using UnityEngine;

namespace BlockBlast.Managers
{
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
            Idle,
            Dragging,
            Placing,
            Eliminating,
            GameOver
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

        public void OnBlockDragStart(int blockIndex)
        {
            if (currentState != GameState.Idle || usedBlocks[blockIndex]) return;
            currentState = GameState.Dragging;
            uiManager.ShowPlacementHint(availableBlocks[blockIndex]);
        }

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

        private IEnumerator DelayedBlockGeneration()
        {
            yield return new WaitForSeconds(eliminationDelay + 0.2f);
            GenerateNewBlocks();
        }

        private void CheckGameOver()
        {
            if (boardManager.IsGameOver(availableBlocks))
            {
                currentState = GameState.GameOver;
                uiManager.ShowGameOverScreen(scoreManager.CurrentScore, scoreManager.HighScore);
            }
        }

        private bool AreAllBlocksUsed()
        {
            for (int i = 0; i < 3; i++)
            {
                if (!usedBlocks[i]) return false;
            }
            return true;
        }

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

        public void RestartGame()
        {
            StartNewGame();
        }
    }
}
