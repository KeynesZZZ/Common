
using EventBus.Core;
using System.Diagnostics;
using UnityEngine;

namespace EventBus.Simple
{
    public partial class PlayerHealthSystem : MonoBehaviour
    {
        public int CurrentHP { get; private set; } = 100;
        public string PlayerId { get; private set; } = "Player1";

        public PlayerHealthSystem()
        {
            // __Generated_RegisterAll();
        }

        [SubscribeAttribute]
        public void OnDamage(ref DamageEvent eventData)
        {
            if (eventData.PlayerId == PlayerId)
            {
                CurrentHP -= eventData.Damage;
                UnityEngine.Debug.Log($"玩家 {PlayerId} 受到 {eventData.Damage} 点伤害！击中点: {eventData.HitPoint}，剩余血量: {CurrentHP}");
            }
        }

        [SubscribeAttribute]
        public void OnHeal(ref HealEvent eventData)
        {
            if (eventData.PlayerId == PlayerId)
            {
                CurrentHP += eventData.HealAmount;
                UnityEngine.Debug.Log($"玩家 {PlayerId} 恢复 {eventData.HealAmount} 点生命值，当前血量: {CurrentHP}");
            }
        }


        public void OnEnable()
        {
            Generated_RegisterAll();
            var de = new DamageEvent()
            {
                PlayerId = "Player1",
                Damage = 10,
            };
            EventBus.Core.EventBus<DamageEvent>.Dispatch(ref de);
        }

        public void OnDisable()
        {
            Generated_UnregisterAll();
        }


        public void Update()
        {
            if (Input.GetKey(KeyCode.T))
            {
              
            }
        }
     
    }
}