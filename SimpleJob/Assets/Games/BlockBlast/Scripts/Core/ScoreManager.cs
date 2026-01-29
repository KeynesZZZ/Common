using System;
using UnityEngine;

namespace BlockBlast
{
    /// <summary>
    /// 分数管理类
    /// </summary>
    public class ScoreManager
    {
        private int _currentScore;
        private int _highScore;
        private int _comboCount;
        private GameConfig _config;

        public int CurrentScore => _currentScore;
        public int HighScore => _highScore;
        public int ComboCount => _comboCount;

        /// <summary>
        /// 当分数更新时触发
        /// </summary>
        public event Action<int> OnScoreChanged;
        
        /// <summary>
        /// 当连击数更新时触发
        /// </summary>
        public event Action<int> OnComboChanged;
        
        /// <summary>
        /// 当创造新纪录时触发
        /// </summary>
        public event Action<int> OnNewRecord;

        public ScoreManager(GameConfig config)
        {
            _config = config;
            _currentScore = 0;
            _comboCount = 0;
            LoadHighScore();
        }

        /// <summary>
        /// 计算消除得分
        /// </summary>
        public int CalculateScore(int rowsCleared, int colsCleared)
        {
            int score = 0;

            // 基础分数
            score += rowsCleared * _config.BaseScore;
            score += colsCleared * _config.BaseScore;

            // 同时消除行和列的奖励
            if (rowsCleared > 0 && colsCleared > 0)
            {
                score += _config.ComboBonus;
            }

            // 连击倍数
            if (_comboCount > 0)
            {
                float multiplier = 1 + (_comboCount * _config.ComboMultiplier);
                score = Mathf.RoundToInt(score * multiplier);
            }

            return score;
        }

        /// <summary>
        /// 添加分数
        /// </summary>
        public void AddScore(int rowsCleared, int colsCleared)
        {
            int score = CalculateScore(rowsCleared, colsCleared);
            
            if (score > 0)
            {
                _currentScore += score;
                _comboCount++;
                
                OnScoreChanged?.Invoke(_currentScore);
                OnComboChanged?.Invoke(_comboCount);

                // 检查是否创造新纪录
                if (_currentScore > _highScore)
                {
                    _highScore = _currentScore;
                    SaveHighScore();
                    OnNewRecord?.Invoke(_highScore);
                }
            }
            else
            {
                // 没有消除，重置连击
                ResetCombo();
            }
        }

        /// <summary>
        /// 重置连击
        /// </summary>
        public void ResetCombo()
        {
            if (_comboCount > 0)
            {
                _comboCount = 0;
                OnComboChanged?.Invoke(_comboCount);
            }
        }

        /// <summary>
        /// 重置分数
        /// </summary>
        public void ResetScore()
        {
            _currentScore = 0;
            _comboCount = 0;
            OnScoreChanged?.Invoke(_currentScore);
            OnComboChanged?.Invoke(_comboCount);
        }

        /// <summary>
        /// 保存最高分
        /// </summary>
        private void SaveHighScore()
        {
            PlayerPrefs.SetInt("BlockBlast_HighScore", _highScore);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 加载最高分
        /// </summary>
        private void LoadHighScore()
        {
            _highScore = PlayerPrefs.GetInt("BlockBlast_HighScore", 0);
        }
    }
}
