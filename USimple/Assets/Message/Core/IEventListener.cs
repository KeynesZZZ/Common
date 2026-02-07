namespace EventBus.Core
{
    public interface IEventListener<TEvent> where TEvent : struct, IEvent
    {
        void OnEvent(ref TEvent eventData);
    }
}