using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace BingoGame.Core.Models
{
    /// <summary>
    /// 游戏状态转换类
    /// </summary>
    public class GameStateTransition
    {
        public GameState fromState;
        public GameState toState;
        public System.Func<UniTask> onTransition;
    }

    /// <summary>
    /// 游戏状态机
    /// 管理游戏状态的转换
    /// </summary>
    public class GameStateMachine
    {
        private GameState currentState;
        private Dictionary<GameState, List<GameStateTransition>> transitions;

        /// <summary>
        /// 当前游戏状态
        /// </summary>
        public GameState CurrentState => currentState;

        /// <summary>
        /// 状态改变事件
        /// </summary>
        public event System.Action<GameState, GameState> OnStateChanged;

        public GameStateMachine()
        {
            currentState = GameState.None;
            transitions = new Dictionary<GameState, List<GameStateTransition>>();
            InitializeTransitions();
        }

        /// <summary>
        /// 初始化状态转换规则
        /// </summary>
        private void InitializeTransitions()
        {
            AddTransition(GameState.None, GameState.Initializing);
            AddTransition(GameState.Initializing, GameState.Ready);
            AddTransition(GameState.Ready, GameState.Playing);
            AddTransition(GameState.Playing, GameState.Paused);
            AddTransition(GameState.Paused, GameState.Playing);
            AddTransition(GameState.Playing, GameState.CheckingWin);
            AddTransition(GameState.CheckingWin, GameState.Playing);
            AddTransition(GameState.CheckingWin, GameState.Win);
            AddTransition(GameState.CheckingWin, GameState.Lose);
            AddTransition(GameState.Win, GameState.GameOver);
            AddTransition(GameState.Lose, GameState.GameOver);
            AddTransition(GameState.GameOver, GameState.Ready);
        }

        /// <summary>
        /// 添加状态转换规则
        /// </summary>
        private void AddTransition(GameState from, GameState to)
        {
            if (!transitions.ContainsKey(from))
            {
                transitions[from] = new List<GameStateTransition>();
            }
            transitions[from].Add(new GameStateTransition { fromState = from, toState = to });
        }

        /// <summary>
        /// 切换到新状态
        /// </summary>
        /// <param name="newState">新状态</param>
        public async UniTask ChangeStateAsync(GameState newState)
        {
            if (!CanTransitionTo(newState))
            {
                Debug.LogWarning($"无法从状态 {currentState} 切换到 {newState}");
                return;
            }

            var oldState = currentState;
            currentState = newState;

            Debug.Log($"游戏状态从 {oldState} 切换到 {newState}");
            OnStateChanged?.Invoke(oldState, newState);

            await UniTask.Yield();
        }

        /// <summary>
        /// 检查是否可以切换到指定状态
        /// </summary>
        /// <param name="state">目标状态</param>
        /// <returns>是否可以切换</returns>
        public bool CanTransitionTo(GameState state)
        {
            if (transitions.ContainsKey(currentState))
            {
                return transitions[currentState].Any(t => t.toState == state);
            }
            return false;
        }
    }
}
