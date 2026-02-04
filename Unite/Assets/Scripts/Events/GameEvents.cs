using UnityEngine;
using BingoGame.Core;
using BingoGame.Core.Models;

namespace BingoGame.Events
{
    /// <summary>
    /// 游戏事件类
    /// 定义游戏相关的事件
    /// </summary>
    public static class GameEvents
    {
        /// <summary>
        /// 游戏状态改变事件
        /// </summary>
        public static event System.Action<GameState, GameState> OnGameStateChanged;

        /// <summary>
        /// 数字呼叫事件
        /// </summary>
        public static event System<int> OnNumberCalled;

        /// <summary>
        /// 单元格标记事件
        /// </summary>
        public static event System.Action<Vector2Int> OnCellMarked;

        /// <summary>
        /// 胜利检查事件
        /// </summary>
        public static event System.Action<bool> OnWinChecked;

        /// <summary>
        /// 游戏重置事件
        /// </summary>
        public static event System.Action OnGameReset;

        /// <summary>
        /// 道具效果激活事件
        /// </summary>
        public static event System.Action<ActiveItemEffect> OnItemEffectActivated;

        /// <summary>
        /// 通知游戏状态改变
        /// </summary>
        /// <param name="oldState">旧状态</param>
        /// <param name="newState">新状态</param>
        public static void NotifyGameStateChanged(GameState oldState, GameState newState)
        {
            OnGameStateChanged?.Invoke(oldState, newState);
        }

        /// <summary>
        /// 通知数字呼叫
        /// </summary>
        /// <param name="number">呼叫的数字</param>
        public static void NotifyNumberCalled(int number)
        {
            OnNumberCalled?.Invoke(number);
        }

        /// <summary>
        /// 通知单元格标记
        /// </summary>
        /// <param name="position">单元格位置</param>
        public static void NotifyCellMarked(Vector2Int position)
        {
            OnCellMarked?.Invoke(position);
        }

        /// <summary>
        /// 通知胜利检查结果
        /// </summary>
        /// <param name="isWin">是否胜利</param>
        public static void NotifyWinChecked(bool isWin)
        {
            OnWinChecked?.Invoke(isWin);
        }

        /// <summary>
        /// 通知游戏重置
        /// </summary>
        public static void NotifyGameReset()
        {
            OnGameReset?.Invoke();
        }

        /// <summary>
        /// 通知道具效果激活
        /// </summary>
        /// <param name="effect">道具效果</param>
        public static void NotifyItemEffectActivated(ActiveItemEffect effect)
        {
            OnItemEffectActivated?.Invoke(effect);
        }
    }

    /// <summary>
    /// 活跃道具效果类
    /// </summary>
    public class ActiveItemEffect
    {
        /// <summary>
        /// 道具ID
        /// </summary>
        public string itemId;

        /// <summary>
        /// 玩家ID
        /// </summary>
        public string playerId;

        /// <summary>
        /// 开始时间
        /// </summary>
        public float startTime;

        /// <summary>
        /// 持续时间
        /// </summary>
        public float duration;

        /// <summary>
        /// 参数字典
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> parameters;
    }
}
