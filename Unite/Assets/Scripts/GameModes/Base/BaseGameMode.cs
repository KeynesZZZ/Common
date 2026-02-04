using UnityEngine;
using Cysharp.Threading.Tasks;
using BingoGame.Core;
using BingoGame.Core.Models;

namespace BingoGame.GameModes
{
    /// <summary>
    /// 游戏模式抽象基类
    /// 实现模板方法模式，定义玩法框架
    /// </summary>
    public abstract class BaseGameMode : IGameMode
    {
        /// <summary>
        /// 游戏棋盘
        /// </summary>
        protected GameBoard board;

        /// <summary>
        /// 棋盘视图模型
        /// </summary>
        protected GameBoardViewModel boardViewModel;

        /// <summary>
        /// 游戏状态机
        /// </summary>
        protected GameStateMachine stateMachine;

        /// <summary>
        /// 玩法类型（子类实现）
        /// </summary>
        public abstract GameMode ModeType { get; }

        /// <summary>
        /// 玩法名称（子类实现）
        /// </summary>
        public abstract string ModeName { get; }

        /// <summary>
        /// 玩法描述（子类实现）
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// 初始化游戏模式
        /// </summary>
        public virtual async UniTask InitializeAsync()
        {
            Debug.Log($"初始化游戏模式: {ModeName}");

            board = CreateBoard();
            boardViewModel = CreateBoardViewModel(board);
            stateMachine = new GameStateMachine();

            await boardViewModel.InitializeAsync();

            Debug.Log($"{ModeName} 初始化完成");
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public virtual async UniTask StartGameAsync()
        {
            Debug.Log($"开始游戏: {ModeName}");
            await stateMachine.ChangeStateAsync(GameState.Playing);
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public virtual async UniTask PauseGameAsync()
        {
            Debug.Log($"暂停游戏: {ModeName}");
            await stateMachine.ChangeStateAsync(GameState.Paused);
        }

        /// <summary>
        /// 恢复游戏
        /// </summary>
        public virtual async UniTask ResumeGameAsync()
        {
            Debug.Log($"恢复游戏: {ModeName}");
            await stateMachine.ChangeStateAsync(GameState.Playing);
        }

        /// <summary>
        /// 结束游戏
        /// </summary>
        public virtual async UniTask EndGameAsync()
        {
            Debug.Log($"结束游戏: {ModeName}");
            await stateMachine.ChangeStateAsync(GameState.GameOver);
        }

        /// <summary>
        /// 重置游戏
        /// </summary>
        public virtual async UniTask ResetGameAsync()
        {
            Debug.Log($"重置游戏: {ModeName}");
            board.Reset();
            await boardViewModel.ResetBoardAsync();
            await stateMachine.ChangeStateAsync(GameState.Ready);
        }

        /// <summary>
        /// 检查胜利条件
        /// 默认实现：检查所有单元格是否都已完成
        /// </summary>
        /// <returns>是否胜利</returns>
        public virtual bool CheckWinCondition()
        {
            return board.IsAllCompleted();
        }

        /// <summary>
        /// 处理玩家输入
        /// </summary>
        /// <param name="position">点击位置</param>
        public virtual async UniTask HandlePlayerInputAsync(Vector2Int position)
        {
            if (stateMachine.CurrentState == GameState.Playing)
            {
                await boardViewModel.InteractCellAsync(position.x, position.y);

                if (CheckWinCondition())
                {
                    await EndGameAsync();
                }
            }
            else
            {
                Debug.LogWarning($"当前状态 {stateMachine.CurrentState} 不允许交互");
            }
        }

        /// <summary>
        /// 创建棋盘（子类实现）
        /// </summary>
        /// <returns>棋盘对象</returns>
        protected abstract GameBoard CreateBoard();

        /// <summary>
        /// 创建棋盘视图模型（子类实现）
        /// </summary>
        /// <param name="board">棋盘对象</param>
        /// <returns>视图模型对象</returns>
        protected abstract GameBoardViewModel CreateBoardViewModel(GameBoard board);
    }
}
