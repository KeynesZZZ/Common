using UnityEngine;
using Cysharp.Threading.Tasks;
using BingoGame.Core;
using BingoGame.GameModes;
using BingoGame.Events;

namespace BingoGame.Controllers
{
    /// <summary>
    /// 游戏控制器
    /// 管理游戏的整体流程
    /// </summary>
    public class GameController : MonoBehaviour
    {
        private static GameController instance;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static GameController Instance => instance;

        /// <summary>
        /// 当前游戏模式
        /// </summary>
        public IGameMode CurrentGameMode { get; private set; }

        /// <summary>
        /// 当前游戏状态
        /// </summary>
        public GameState CurrentState => CurrentGameMode != null ? 
            (CurrentGameMode as BaseGameMode)?.stateMachine?.CurrentState ?? GameState.None : 
            GameState.None;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        /// <param name="mode">游戏模式</param>
        public async UniTask InitializeGameAsync(GameMode mode)
        {
            Debug.Log($"初始化游戏，模式: {mode}");

            CurrentGameMode = GameModeFactory.CreateGameMode(mode);

            if (CurrentGameMode == null)
            {
                Debug.LogError($"无法创建游戏模式: {mode}");
                return;
            }

            await CurrentGameMode.InitializeAsync();

            Debug.Log($"游戏初始化完成: {CurrentGameMode.ModeName}");
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public async UniTask StartGameAsync()
        {
            if (CurrentGameMode == null)
            {
                Debug.LogWarning("当前没有游戏模式");
                return;
            }

            await CurrentGameMode.StartGameAsync();
            GameEvents.NotifyGameStateChanged(GameState.Ready, GameState.Playing);
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public async UniTask PauseGameAsync()
        {
            if (CurrentGameMode == null)
            {
                Debug.LogWarning("当前没有游戏模式");
                return;
            }

            await CurrentGameMode.PauseGameAsync();
            GameEvents.NotifyGameStateChanged(GameState.Playing, GameState.Paused);
        }

        /// <summary>
        /// 恢复游戏
        /// </summary>
        public async UniTask ResumeGameAsync()
        {
            if (CurrentGameMode == null)
            {
                Debug.LogWarning("当前没有游戏模式");
                return;
            }

            await CurrentGameMode.ResumeGameAsync();
            GameEvents.NotifyGameStateChanged(GameState.Paused, GameState.Playing);
        }

        /// <summary>
        /// 结束游戏
        /// </summary>
        public async UniTask EndGameAsync()
        {
            if (CurrentGameMode == null)
            {
                Debug.LogWarning("当前没有游戏模式");
                return;
            }

            await CurrentGameMode.EndGameAsync();
        }

        /// <summary>
        /// 重置游戏
        /// </summary>
        public async UniTask ResetGameAsync()
        {
            if (CurrentGameMode == null)
            {
                Debug.LogWarning("当前没有游戏模式");
                return;
            }

            await CurrentGameMode.ResetGameAsync();
            GameEvents.NotifyGameReset();
        }

        /// <summary>
        /// 处理玩家输入
        /// </summary>
        /// <param name="position">点击位置</param>
        public async UniTask HandlePlayerInputAsync(Vector2Int position)
        {
            if (CurrentGameMode == null)
            {
                Debug.LogWarning("当前没有游戏模式");
                return;
            }

            await CurrentGameMode.HandlePlayerInputAsync(position);
        }

        /// <summary>
        /// 检查胜利条件
        /// </summary>
        /// <returns>是否胜利</returns>
        public bool CheckWinCondition()
        {
            if (CurrentGameMode == null)
            {
                Debug.LogWarning("当前没有游戏模式");
                return false;
            }

            return CurrentGameMode.CheckWinCondition();
        }
    }

    /// <summary>
    /// 游戏模式工厂
    /// </summary>
    public class GameModeFactory
    {
        /// <summary>
        /// 创建游戏模式
        /// </summary>
        /// <param name="mode">游戏模式类型</param>
        /// <returns>游戏模式实例</returns>
        public static IGameMode CreateGameMode(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.ClassicBingo:
                    return new ClassicBingo.ClassicBingoGame();
                case GameMode.HarvestBingo:
                    return new HarvestBingo.HarvestBingoGame();
                default:
                    Debug.LogError($"未知的游戏模式: {mode}");
                    return null;
            }
        }
    }
}
