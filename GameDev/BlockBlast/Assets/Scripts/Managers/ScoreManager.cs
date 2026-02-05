using UnityEngine;

namespace BlockBlast.Managers
{
    public class ScoreManager : MonoBehaviour
    {
        public int CurrentScore { get; private set; }
        public int HighScore { get; private set; }
        public int CurrentCombo { get; private set; }

        private readonly int[] baseScores = { 0, 100, 250, 450, 700, 1000, 1400, 1800, 2300 };

        public System.Action<int> OnScoreChanged;
        public System.Action<int> OnComboChanged;
        public System.Action<int> OnHighScoreChanged;

        private void Start()
        {
            HighScore = PlayerPrefs.GetInt("BlockBlast_HighScore", 0);
        }

        public int CalculateScore(int linesEliminated, int combo)
        {
            if (linesEliminated <= 0) return 0;

            int baseScore = linesEliminated < baseScores.Length
                ? baseScores[linesEliminated]
                : baseScores[baseScores.Length - 1];

            float comboMultiplier = 1f + combo * 0.2f;
            comboMultiplier = Mathf.Min(comboMultiplier, 2f);

            return Mathf.RoundToInt(baseScore * comboMultiplier);
        }

        public void AddScore(int linesEliminated)
        {
            if (linesEliminated > 0)
            {
                CurrentCombo++;
                int score = CalculateScore(linesEliminated, CurrentCombo);
                CurrentScore += score;

                if (CurrentScore > HighScore)
                {
                    HighScore = CurrentScore;
                    PlayerPrefs.SetInt("BlockBlast_HighScore", HighScore);
                    OnHighScoreChanged?.Invoke(HighScore);
                }

                OnScoreChanged?.Invoke(CurrentScore);
                OnComboChanged?.Invoke(CurrentCombo);
            }
            else
            {
                CurrentCombo = 0;
                OnComboChanged?.Invoke(0);
            }
        }

        public void Reset()
        {
            CurrentScore = 0;
            CurrentCombo = 0;
            OnScoreChanged?.Invoke(0);
            OnComboChanged?.Invoke(0);
        }
    }
}
