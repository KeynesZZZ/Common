using System;
using UnityEngine;

namespace BlockBlast.Core
{
    public class ComboManager
    {
        public int CurrentCombo { get; private set; }
        public int MaxCombo { get; private set; }
        public event EventHandler<int> ComboIncreased;
        public event EventHandler<int> ComboReset;

        private const int ComboResetDelay = 1500; // 1.5秒无消除则重置
        private float _lastComboTime;

        public ComboManager()
        {
            ResetCombo();
        }

        public void RegisterCombo(int lineCount)
        {
            if (lineCount > 0)
            {
                CurrentCombo++;
                if (CurrentCombo > MaxCombo)
                {
                    MaxCombo = CurrentCombo;
                }
                _lastComboTime = UnityEngine.Time.time;
                ComboIncreased?.Invoke(this, CurrentCombo);
            }
        }

        public void Update()
        {
            // 检查是否需要重置Combo
            if (CurrentCombo > 0 && UnityEngine.Time.time - _lastComboTime > ComboResetDelay / 1000f)
            {
                ResetCombo();
            }
        }

        public void ResetCombo()
        {
            if (CurrentCombo > 0)
            {
                ComboReset?.Invoke(this, CurrentCombo);
            }
            CurrentCombo = 0;
            _lastComboTime = UnityEngine.Time.time;
        }

        public int CalculateScore(int baseScore, int lineCount)
        {
            if (CurrentCombo == 0)
            {
                return baseScore * lineCount;
            }

            // Combo加成公式：baseScore * lineCount * (1 + 0.1 * (CurrentCombo - 1))
            float comboMultiplier = 1 + 0.1f * (CurrentCombo - 1);
            return Mathf.FloorToInt(baseScore * lineCount * comboMultiplier);
        }
    }
}
