using UnityEngine;
using UnityEngine.UI;
using BingoClient.Models;

namespace BingoClient.Services
{
    public class FeedbackService : MonoBehaviour
    {
        [SerializeField] private GameObject _feedbackPrefab;
        [SerializeField] private Transform _feedbackContainer;
        [SerializeField] private ParticleSystem _daubEffect;
        [SerializeField] private AudioClip _perfectSound;
        [SerializeField] private AudioClip _greatSound;
        [SerializeField] private AudioClip _missSound;

        private AudioSource _audioSource;
        private ComboSystem _comboSystem;
        private AnimationService _animationService;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _comboSystem = GetComponent<ComboSystem>();
            _animationService = GetComponent<AnimationService>();
        }

        public void ShowFeedback(FeedbackData feedback)
        {
            switch (feedback.Type)
            {
                case FeedbackType.Perfect:
                    ShowPerfectFeedback(feedback.Position);
                    break;
                case FeedbackType.Great:
                    ShowGreatFeedback(feedback.Position);
                    break;
                case FeedbackType.Miss:
                    ShowMissFeedback(feedback.Position);
                    break;
                case FeedbackType.Bingo:
                    ShowBingoFeedback();
                    break;
            }
        }

        private void ShowPerfectFeedback(Vector3 position)
        {
            var feedback = Instantiate(_feedbackPrefab, position, Quaternion.identity, _feedbackContainer);
            var text = feedback.GetComponent<TextMeshProUGUI>();
            text.text = "Perfect!";
            text.color = Color.yellow;

            _daubEffect.transform.position = position;
            _daubEffect.Play();

            _audioSource.PlayOneShot(_perfectSound);

            _comboSystem?.AddCombo();

            feedback.transform.DOScale(Vector3.one * 1.5f, 0.2f).SetEase(Ease.OutBack);
            feedback.transform.DOMoveY(position.y + 50f, 0.5f).SetEase(Ease.OutQuad);
            feedback.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).SetDelay(0.3f);
            Destroy(feedback, 1f);
        }

        private void ShowGreatFeedback(Vector3 position)
        {
            var feedback = Instantiate(_feedbackPrefab, position, Quaternion.identity, _feedbackContainer);
            var text = feedback.GetComponent<TextMeshProUGUI>();
            text.text = "Great!";
            text.color = Color.green;

            _daubEffect.transform.position = position;
            _daubEffect.Play();

            _audioSource.PlayOneShot(_greatSound);

            _comboSystem?.AddCombo();

            feedback.transform.DOScale(Vector3.one * 1.3f, 0.2f).SetEase(Ease.OutBack);
            feedback.transform.DOMoveY(position.y + 40f, 0.5f).SetEase(Ease.OutQuad);
            feedback.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).SetDelay(0.3f);
            Destroy(feedback, 1f);
        }

        private void ShowMissFeedback(Vector3 position)
        {
            var feedback = Instantiate(_feedbackPrefab, position, Quaternion.identity, _feedbackContainer);
            var text = feedback.GetComponent<TextMeshProUGUI>();
            text.text = "Miss!";
            text.color = Color.red;

            _audioSource.PlayOneShot(_missSound);

            _comboSystem?.ResetCombo();

            var slot = position.GetComponent<Views.SlotView>();
            slot?.Shake();

            feedback.transform.DOScale(Vector3.one * 1.2f, 0.1f).SetEase(Ease.OutBack);
            feedback.GetComponent<CanvasGroup>().DOFade(0f, 0.3f);
            Destroy(feedback, 0.5f);
        }

        private void ShowBingoFeedback()
        {
            var feedback = Instantiate(_feedbackPrefab, _feedbackContainer);
            var text = feedback.GetComponent<TextMeshProUGUI>();
            text.text = "BINGO!";
            text.color = Color.magenta;
            text.fontSize = 72;

            feedback.transform.DOScale(Vector3.one * 2f, 0.5f).SetEase(Ease.OutElastic);
            feedback.transform.DORotate(Vector3.forward * 360f, 0.5f).SetEase(Ease.OutBack);
            Destroy(feedback, 2f);
        }
    }

    public class ComboSystem : MonoBehaviour
    {
        [SerializeField] private Slider _energyBar;
        [SerializeField] private int _maxCombo = 10;
        [SerializeField] private float _comboTimeout = 3f;

        private int _currentCombo;
        private float _lastComboTime;

        public void AddCombo()
        {
            _currentCombo = Mathf.Min(_currentCombo + 1, _maxCombo);
            _lastComboTime = Time.time;
            UpdateEnergyBar();
        }

        public void ResetCombo()
        {
            _currentCombo = 0;
            UpdateEnergyBar();
        }

        private void Update()
        {
            if (_currentCombo > 0 && Time.time - _lastComboTime > _comboTimeout)
            {
                ResetCombo();
            }
        }

        private void UpdateEnergyBar()
        {
            _energyBar.DOValue((float)_currentCombo / _maxCombo, 0.3f).SetEase(Ease.OutQuad);
        }
    }
}