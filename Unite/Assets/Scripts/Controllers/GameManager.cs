using UnityEngine;
using Cysharp.Threading.Tasks;
using BingoGame.Core;
using BingoGame.Services;
using BingoGame.Utilities;
using BingoGame.Views;

namespace BingoGame.Controllers
{
    /// <summary>
    /// 游戏管理器
    /// 负责初始化游戏和注册服务
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("游戏设置")]
        [SerializeField] private GameMode defaultGameMode = GameMode.ClassicBingo;
        [SerializeField] private GameObject animationServicePrefab;
        [SerializeField] private GameObject bingoBoardViewPrefab;

        private void Awake()
        {
            InitializeServices();
        }

        private void Start()
        {
            InitializeGameAsync(defaultGameMode).Forget();
        }

        /// <summary>
        /// 初始化所有服务
        /// </summary>
        private void InitializeServices()
        {
            Debug.Log("初始化服务...");

            if (animationServicePrefab != null)
            {
                var animationService = Instantiate(animationServicePrefab);
                ServiceLocator.RegisterService<IAnimationService>(animationService.GetComponent<IAnimationService>());
            }

            if (bingoBoardViewPrefab != null)
            {
                var bingoBoardView = Instantiate(bingoBoardViewPrefab);
                ServiceLocator.RegisterService<IBingoBoardView>(bingoBoardView.GetComponent<IBingoBoardView>());
            }

            Debug.Log("服务初始化完成");
        }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        /// <param name="mode">游戏模式</param>
        private async UniTask InitializeGameAsync(GameMode mode)
        {
            Debug.Log($"初始化游戏，模式: {mode}");

            await GameController.Instance.InitializeGameAsync(mode);
            await GameController.Instance.StartGameAsync();

            Debug.Log("游戏初始化完成");
        }

        private void OnDestroy()
        {
            ServiceLocator.ClearAllServices();
        }
    }
}
