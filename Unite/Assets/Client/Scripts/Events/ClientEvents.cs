using BingoShared.Models;
using BingoShared.Protocol;

namespace BingoClient.Events
{
    public class ClientEvents
    {
        public class GameInitialized
        {
            public RoomData RoomData { get; set; }
        }

        public class GameStarted
        {
            public System.DateTime StartTime { get; set; }
        }

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

        public class BoardsInitialized
        {
            public System.Collections.Generic.List<BoardData> Boards { get; set; }
        }

        public class SlotMarked
        {
            public int SlotIndex { get; set; }
            public bool IsMarked { get; set; }
        }

        public class BingoAchieved
        {
            public System.Collections.Generic.List<WinLine> WinLines { get; set; }
        }

        public class NumberCalled
        {
            public int Number { get; set; }
            public System.Collections.Generic.List<int> CalledNumbers { get; set; }
        }

        public class GameEnded
        {
            public System.Collections.Generic.List<PlayerResult> Results { get; set; }
        }

        public class CountdownComplete
        {
        }

        public class PowerUpActivated
        {
            public PowerUpResult PowerUpResult { get; set; }
        }
    }

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