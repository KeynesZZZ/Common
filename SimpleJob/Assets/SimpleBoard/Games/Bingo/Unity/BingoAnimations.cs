using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SimpleBoard.Games.Bingo.Unity
{
    /// <summary>
    /// Bingo 动画系统 - 管理 Bingo 游戏的动画效果
    /// </summary>
    public class BingoAnimations : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float _animationDuration = 0.3f;
        [SerializeField] private float _bounceScale = 1.2f;
        [SerializeField] private Ease _animationEase = Ease.OutBack;

        [Header("References")]
        [SerializeField] private TextMeshProUGUI _bingoText;
        [SerializeField] private TextMeshProUGUI _numberText;
        [SerializeField] private ParticleSystem _bingoParticles;
        [SerializeField] private AudioSource _audioSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip _bingoSound;

        /// <summary>
        /// 播放 Bingo 胜利动画
        /// </summary>
        public void PlayBingoAnimation()
        {
            // 播放音效
            PlaySound(_bingoSound);
            
            // 显示 Bingo 文本
            if (_bingoText != null)
            {
                _bingoText.gameObject.SetActive(true);
                _bingoText.rectTransform.localScale = Vector3.zero;
                
                // 弹跳动画
                _bingoText.rectTransform.DOScale(Vector3.one * 1.5f, _animationDuration * 2)
                    .SetEase(_animationEase)
                    .OnComplete(() =>
                    {
                        // 淡出动画
                        _bingoText.DOFade(0, _animationDuration)
                            .SetEase(Ease.InQuad)
                            .OnComplete(() =>
                            {
                                _bingoText.gameObject.SetActive(false);
                                _bingoText.alpha = 1;
                            });
                    });
            }
            
            // 播放粒子效果
            if (_bingoParticles != null)
            {
                _bingoParticles.Play();
            }
        }

        /// <summary>
        /// 播放数字呼叫动画
        /// </summary>
        /// <param name="number">呼叫的数字</param>
        public void PlayNumberCallAnimation(int number)
        {
            if (_numberText != null)
            {
                _numberText.text = number.ToString();
                _numberText.gameObject.SetActive(true);
                _numberText.rectTransform.localScale = Vector3.zero;
                
                // 弹跳动画
                _numberText.rectTransform.DOScale(Vector3.one * 1.2f, _animationDuration)
                    .SetEase(_animationEase)
                    .OnComplete(() =>
                    {
                        // 延迟后淡出
                        DOVirtual.DelayedCall(1f, () =>
                        {
                            _numberText.DOFade(0, _animationDuration)
                                .SetEase(Ease.InQuad)
                                .OnComplete(() =>
                                {
                                    _numberText.gameObject.SetActive(false);
                                    _numberText.alpha = 1;
                                });
                        });
                    });
            }
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="clip">音效剪辑</param>
        private void PlaySound(AudioClip clip)
        {
            if (clip != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// 停止所有动画
        /// </summary>
        public void StopAllAnimations()
        {
            DOTween.KillAll();
            
            // 停止粒子效果
            if (_bingoParticles != null)
            {
                _bingoParticles.Stop();
            }
        }
    }
}