using BingoShared.Models;
using BingoShared.Protocol;

namespace BingoClient.Events
{
    /// <summary>
    /// 客户端事件集合 - 定义游戏中所有的事件类型
    /// 使用发布-订阅模式实现组件间解耦
    /// </summary>
    public class ClientEvents
    {
        /// <summary>
        /// 游戏初始化事件 - 玩家成功加入房间后触发
        /// </summary>
        public class GameInitialized
        {
            public RoomData RoomData { get; set; }
        }

        /// <summary>
        /// 游戏开始事件 - 倒计时结束后触发
        /// </summary>
        public class GameStarted
        {
            public System.DateTime StartTime { get; set; }
        }

        /// <summary>
        /// 格子点击事件 - 玩家点击格子后触发
        /// 包含点击结果、道具信息、Bingo 状态等
        /// </summary>
        public class SlotClicked
        {
            public int BoardIndex { get; set; }
            public int SlotIndex { get; set; }
            public bool IsMarked { get; set; }
            public bool HasPowerUp { get; set; }
            public PowerUpResult PowerUpResult { get; set; }
            public bool IsBingo { get; set; }
            public System.Collections.Generic.List<WinLine> WinLines { get; set; }
            public int RemainingBingo { get; set; }
            public FeedbackData Feedback { get; set; }
        }

        /// <summary>
        /// 棋盘初始化事件 - 棋盘数据加载完成后触发
        /// </summary>
        public class BoardsInitialized
        {
            public System.Collections.Generic.List<BoardData> Boards { get; set; }
        }

        /// <summary>
        /// 格子标记事件 - 格子状态更新时触发
        /// </summary>
        public class SlotMarked
        {
            public int SlotIndex { get; set; }
            public bool IsMarked { get; set; }
        }

        /// <summary>
        /// Bingo 达成事件 - 玩家完成任意连线时触发
        /// </summary>
        public class BingoAchieved
        {
            public System.Collections.Generic.List<WinLine> WinLines { get; set; }
        }

        /// <summary>
        /// 数字呼叫事件 - 系统呼叫新的数字时触发
        /// </summary>
        public class NumberCalled
        {
            public int Number { get; set; }
            public System.Collections.Generic.List<int> CalledNumbers { get; set; }
        }

        /// <summary>
        /// 游戏结束事件 - 所有玩家完成游戏或达到结束条件时触发
        /// </summary>
        public class GameEnded
        {
            public System.Collections.Generic.List<PlayerResult> Results { get; set; }
        }

        /// <summary>
        /// 倒计时完成事件 - 游戏开始前的倒计时结束时触发
        /// </summary>
        public class CountdownComplete
        {
        }

        /// <summary>
        /// 道具激活事件 - 玩家触发道具效果时触发
        /// </summary>
        public class PowerUpActivated
        {
            public PowerUpResult PowerUpResult { get; set; }
        }
    }

    /// <summary>
    /// 客户端事件总线 - 实现发布-订阅模式的事件系统
    /// 用于解耦组件之间的依赖关系
    /// </summary>
    public class ClientEventBus
    {
        private readonly System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<System.Delegate>> _eventHandlers = new();

        public void Subscribe<T>(System.Action<T> handler)
        {
            var eventType = typeof(T);
            if (!_eventHandlers.ContainsKey(eventType))
                _eventHandlers[eventType] = new System.Collections.Generic.List<System.Delegate>();
            _eventHandlers[eventType].Add(handler);
        }

        public void Publish<T>(T eventData)
        {
            var eventType = typeof(T);
            if (_eventHandlers.ContainsKey(eventType))
            {
                foreach (var handler in _eventHandlers[eventType])
                {
                    ((System.Action<T>)handler)(eventData);
                }
            }
        }
    }
}