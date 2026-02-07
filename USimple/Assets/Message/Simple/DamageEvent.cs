using EventBus.Core;

namespace EventBus.Simple
{
    public struct DamageEvent : IEvent
    {
        public string PlayerId;
        public int Damage;
        public string HitPoint;
    }

    public struct HealEvent : IEvent
    {
        public string PlayerId;
        public int HealAmount;
    }
}