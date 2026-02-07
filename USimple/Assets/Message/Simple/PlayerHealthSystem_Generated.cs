using System;
namespace EventBus.Simple
{
    public partial class PlayerHealthSystem
    {
        private struct OnDamage_Wrapper : EventBus.Core.IEventListener<EventBus.Simple.DamageEvent>,IEquatable<OnDamage_Wrapper>
        {
            public PlayerHealthSystem Target;
            public void OnEvent(ref EventBus.Simple.DamageEvent e) => Target.OnDamage(ref e);
            public bool Equals(OnDamage_Wrapper other) => Target == other.Target;
        }

        private OnDamage_Wrapper OnDamage_handler;

        private struct OnHeal_Wrapper : EventBus.Core.IEventListener<EventBus.Simple.HealEvent>,IEquatable<OnHeal_Wrapper>
        {
            public PlayerHealthSystem Target;
            public void OnEvent(ref EventBus.Simple.HealEvent e) => Target.OnHeal(ref e);
            public bool Equals(OnHeal_Wrapper other) => Target == other.Target;
        }

        private OnHeal_Wrapper OnHeal_handler;

        private void Generated_RegisterAll()
        {
            OnDamage_handler = new OnDamage_Wrapper { Target = this };
            EventBus.Core.EventBus<EventBus.Simple.DamageEvent>.Register(OnDamage_handler);
            OnHeal_handler = new OnHeal_Wrapper { Target = this };
            EventBus.Core.EventBus<EventBus.Simple.HealEvent>.Register(OnHeal_handler);
        }

        private void Generated_UnregisterAll()
        {
            EventBus.Core.EventBus<EventBus.Simple.DamageEvent>.Unregister(OnDamage_handler);
            EventBus.Core.EventBus<EventBus.Simple.HealEvent>.Unregister(OnHeal_handler);
        }
    }
}
