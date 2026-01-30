using System.Collections;
using Cysharp.Threading.Tasks;
using SimpleBoard;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockBlast
{
    /// <summary>
    /// 游戏主UI控制器
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("游戏板")]
        [SerializeField] private GameBoardUI _gameBoardUI;
        [SerializeField] private BlockTrayUI _blockTrayUI;
        [SerializeField] private BlockBlastInputHandler _inputHandler;
        [Header("分数显示")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _highScoreText;
        [SerializeField] private TextMeshProUGUI _comboText;

        [Header("控制按钮")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _skipButton;

        [Header("游戏结束界面")]
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private TextMeshProUGUI _newRecordText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _mainMenuButton;

        [Header("设置")]
        [SerializeField] private GameConfig _gameConfig;

        private BlockBlastGame _game;
        private bool _isPaused;

        private void Awake()
        {
            InitializeGame();
            SetupUI();
        }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        private void InitializeGame()
        {
            if (_gameConfig == null)
                _gameConfig = GameConfig.Default;
            BlockGameBoardRenderer blockGameBoardRenderer = this.GetComponent<BlockGameBoardRenderer>();
            _game = new BlockBlastGame(_gameConfig,blockGameBoardRenderer);
            
            // 初始化UI组件
            _gameBoardUI.Initialize(_game);
            _blockTrayUI.Initialize(_game);
            _inputHandler.Initialize(_game);
            // 订阅游戏事件
    
            _game.ScoreManager.OnScoreChanged += OnScoreChanged;
            _game.ScoreManager.OnComboChanged += OnComboChanged;
            _game.ScoreManager.OnNewRecord += OnNewRecord;
        }

        /// <summary>
        /// 设置UI
        /// </summary>
        private void SetupUI()
        {
            // 按钮事件
            _startButton.onClick.AddListener(StartGame);
            _pauseButton.onClick.AddListener(TogglePause);
            _resetButton.onClick.AddListener(ResetGame);
            _skipButton.onClick.AddListener(SkipTurn);
            _restartButton.onClick.AddListener(RestartGame);
            _mainMenuButton.onClick.AddListener(ReturnToMainMenu);

            // 初始状态
            _gameOverPanel.SetActive(false);
            UpdateScoreDisplay();
            UpdateHighScoreDisplay();
            UpdateComboDisplay();
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        private void StartGame()
        {
            _game.StartAsync().Forget();
            _startButton.gameObject.SetActive(false);
            _pauseButton.gameObject.SetActive(true);
            _skipButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// 切换暂停状态
        /// </summary>
        private void TogglePause()
        {
            _isPaused = !_isPaused;
            _game.TogglePause();
            
            if (_isPaused)
            {
                _pauseButton.GetComponentInChildren<TextMeshProUGUI>().text = "继续";
                Time.timeScale = 0f;
            }
            else
            {
                _pauseButton.GetComponentInChildren<TextMeshProUGUI>().text = "暂停";
                Time.timeScale = 1f;
            }
        }

        /// <summary>
        /// 重置游戏
        /// </summary>
        private void ResetGame()
        {
            _game.ResetGame();
            _gameOverPanel.SetActive(false);
            _isPaused = false;
            Time.timeScale = 1f;
            _pauseButton.GetComponentInChildren<TextMeshProUGUI>().text = "暂停";
        }

        /// <summary>
        /// 跳过回合
        /// </summary>
        private void SkipTurn()
        {
            _game.SkipTurn();
        }

        /// <summary>
        /// 重新开始游戏
        /// </summary>
        private void RestartGame()
        {
            _gameOverPanel.SetActive(false);
            _game.ResetGame();
            _isPaused = false;
            Time.timeScale = 1f;
            _pauseButton.GetComponentInChildren<TextMeshProUGUI>().text = "暂停";
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        private void ReturnToMainMenu()
        {
            // 这里可以实现返回主菜单的逻辑
            // 暂时只是重新开始游戏
            RestartGame();
            _startButton.gameObject.SetActive(true);
            _pauseButton.gameObject.SetActive(false);
            _skipButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// 游戏开始事件
        /// </summary>
        private void OnGameStarted()
        {
            _gameOverPanel.SetActive(false);
            _isPaused = false;
            Time.timeScale = 1f;
            _pauseButton.GetComponentInChildren<TextMeshProUGUI>().text = "暂停";
        }

        /// <summary>
        /// 游戏结束事件
        /// </summary>
        private void OnGameOver()
        {
            StartCoroutine(ShowGameOverPanel());
        }

        /// <summary>
        /// 显示游戏结束面板
        /// </summary>
        private IEnumerator ShowGameOverPanel()
        {
            yield return new WaitForSeconds(0.5f); // 等待消除动画完成

            _gameOverPanel.SetActive(true);
            _finalScoreText.text = $"最终分数: {_game.ScoreManager.CurrentScore}";
            
            // 检查是否创造新纪录
            if (_game.ScoreManager.CurrentScore >= _game.ScoreManager.HighScore)
            {
                _newRecordText.gameObject.SetActive(true);
                _newRecordText.text = "新纪录!";
            }
            else
            {
                _newRecordText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 分数改变事件
        /// </summary>
        private void OnScoreChanged(int score)
        {
            UpdateScoreDisplay();
            StartCoroutine(ScoreAnimation());
        }

        /// <summary>
        /// 连击数改变事件
        /// </summary>
        private void OnComboChanged(int combo)
        {
            UpdateComboDisplay();
            if (combo > 0)
            {
                StartCoroutine(ComboAnimation());
            }
        }

        /// <summary>
        /// 新纪录事件
        /// </summary>
        private void OnNewRecord(int record)
        {
            UpdateHighScoreDisplay();
            StartCoroutine(NewRecordAnimation());
        }

        /// <summary>
        /// 更新分数显示
        /// </summary>
        private void UpdateScoreDisplay()
        {
            _scoreText.text = $"分数: {_game.ScoreManager.CurrentScore}";
        }

        /// <summary>
        /// 更新最高分显示
        /// </summary>
        private void UpdateHighScoreDisplay()
        {
            _highScoreText.text = $"最高分: {_game.ScoreManager.HighScore}";
        }

        /// <summary>
        /// 更新连击显示
        /// </summary>
        private void UpdateComboDisplay()
        {
            if (_game.ScoreManager.ComboCount > 0)
            {
                _comboText.gameObject.SetActive(true);
                _comboText.text = $"连击 x{_game.ScoreManager.ComboCount}";
            }
            else
            {
                _comboText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 分数动画
        /// </summary>
        private IEnumerator ScoreAnimation()
        {
            var originalScale = _scoreText.transform.localScale;
            var targetScale = originalScale * 1.2f;

            float elapsed = 0f;
            float duration = 0.2f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _scoreText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _scoreText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }

            _scoreText.transform.localScale = originalScale;
        }

        /// <summary>
        /// 连击动画
        /// </summary>
        private IEnumerator ComboAnimation()
        {
            var originalColor = _comboText.color;
            var targetColor = Color.yellow;

            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _comboText.color = Color.Lerp(originalColor, targetColor, t);
                yield return null;
            }

            _comboText.color = targetColor;
        }

        /// <summary>
        /// 新纪录动画
        /// </summary>
        private IEnumerator NewRecordAnimation()
        {
            var originalColor = _highScoreText.color;
            var targetColor = Color.green;

            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _highScoreText.color = Color.Lerp(originalColor, targetColor, t);
                yield return null;
            }

            // 闪烁效果
            for (int i = 0; i < 3; i++)
            {
                _highScoreText.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                _highScoreText.color = targetColor;
                yield return new WaitForSeconds(0.1f);
            }

            _highScoreText.color = originalColor;
        }
    }
}
