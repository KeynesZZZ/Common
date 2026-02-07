using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EventBus.Core
{

    // 内部路由接口，解决 EventBus 无法直接管理泛型 Container 的问题
    internal interface IEventRouter<TEvent> where TEvent : struct, IEvent
    {
        void Dispatch(ref TEvent eventData);
    }



    /// <summary>
    /// 具体的特化存储容器：针对每一个具体的 (事件, 处理器) 类型组合。
    /// 实现 IEventRouter 接口是为了让 EventBus 能够以非泛型方式管理这些特化容器。
    /// </summary>
    public sealed class EventContainer<TEvent, THandler> : IEventRouter<TEvent>
        where TEvent : struct, IEvent
        where THandler : struct, IEventListener<TEvent>, IEquatable<THandler>
    {
        public static readonly EventContainer<TEvent, THandler> Instance = new EventContainer<TEvent, THandler>();

        private THandler[] _handlers = new THandler[16];
        private int _count = 0;
        private readonly object _lock = new object();

        private EventContainer() { }

        public void Register(THandler handler)
        {
            lock (_lock)
            {
                for (int i = 0; i < _count; i++)
                {
                    if (_handlers[i].Equals(handler)) return;
                }

                if (_count >= _handlers.Length)
                {
                    Array.Resize(ref _handlers, _handlers.Length * 2);
                }
                _handlers[_count++] = handler;
            }
        }

        public void Unregister(THandler handler)
        {
            lock (_lock)
            {
                for (int i = 0; i < _count; i++)
                {
                    if (_handlers[i].Equals(handler))
                    {
                        _handlers[i] = _handlers[_count - 1];
                        _handlers[_count - 1] = default;
                        _count--;
                        return;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispatch(ref TEvent eventData)
        {
            // 快照引用，保证派发时的线程安全
            THandler[] currentHandlers = _handlers;
            int currentCount = _count;

            for (int i = 0; i < currentCount; i++)
            {
                currentHandlers[i].OnEvent(ref eventData);
            }
        }
    }


    public static class EventBus<TEvent> where TEvent : struct, IEvent
    {
        private static List<IEventRouter<TEvent>> _routers = new List<IEventRouter<TEvent>>();
        private static readonly object _lock = new object();

        public static void Register<THandler>(THandler handler)
            where THandler : struct, IEventListener<TEvent>, IEquatable<THandler>
        {
            lock (_lock)
            {
                var container = EventContainer<TEvent, THandler>.Instance;
                if (!_routers.Contains(container))
                {
                    _routers.Add(container);
                }
                container.Register(handler);
            }
        }

        public static void Unregister<THandler>(THandler handler)
            where THandler : struct, IEventListener<TEvent>, IEquatable<THandler>
        {
            lock (_lock)
            {
                EventContainer<TEvent, THandler>.Instance.Unregister(handler);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Dispatch(ref TEvent eventData)
        {
            // 避免在派发时遍历 List 产生枚举器 GC
            for (int i = 0; i < _routers.Count; i++)
            {
                _routers[i].Dispatch(ref eventData);
            }
        }
    }



}