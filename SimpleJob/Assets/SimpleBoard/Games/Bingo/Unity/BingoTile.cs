using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SimpleBoard.Games.Bingo.Unity
{
    public class BingoTile : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private TextMeshProUGUI _numberText;
        [SerializeField] private GameObject _checkMark;

        [Header("Animation Settings")]
        [SerializeField] private float _scaleDuration = 0.2f;
        [SerializeField] private float _bounceScale = 1.2f;
        [SerializeField] private Ease _scaleEase = Ease.OutBack;

        [Header("Color Settings")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _markedColor = Color.green;
        [SerializeField] private Color _freeSpaceColor = Color.yellow;
        [SerializeField] private Color _textColor = Color.black;
        [SerializeField] private Color _markedTextColor = Color.white;

        // Game State
        private BingoSlotState _slot;
        private int _cardId = -1;
        private int _row = -1;
        private int _col = -1;
        private bool _isInitialized = false;

        // Events
        public event Action<int, int, int> OnTileClicked;

        public void Initialize(BingoSlotState slot, int row, int col, int cardId)
        {
            _slot = slot;
            _row = row;
            _col = col;
            _cardId = cardId;
            _isInitialized = true;

            SetupTile();
            UpdateTileAppearance();
        }

        private void SetupTile()
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _normalColor;
            }

            if (_numberText != null)
            {
                _numberText.text = _slot.GetDisplayText();
                _numberText.color = _textColor;
            }

            if (_checkMark != null)
            {
                _checkMark.SetActive(false);
            }

            // 设置点击事件
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnTileClick);
            }
        }

        private void UpdateTileAppearance()
        {
            if (!_isInitialized) return;

            if (_backgroundImage != null)
            {
                if (_slot.IsFreeSpace)
                {
                    _backgroundImage.color = _freeSpaceColor;
                }
                else if (_slot.IsMarked)
                {
                    _backgroundImage.color = _markedColor;
                }
                else
                {
                    _backgroundImage.color = _normalColor;
                }
            }

            if (_numberText != null)
            {
                _numberText.text = _slot.GetDisplayText();
                _numberText.color = _slot.IsMarked ? _markedTextColor : _textColor;
            }

            if (_checkMark != null)
            {
                _checkMark.SetActive(_slot.IsMarked);
            }
        }

        private void OnTileClick()
        {
            if (!_isInitialized) return;

            OnTileClicked?.Invoke(_cardId, _row, _col);

            // 播放点击动画
            PlayClickAnimation();
        }

        private void PlayClickAnimation()
        {
            if (!_isInitialized) return;

            // 缩放动画
            transform.DOScale(_bounceScale, _scaleDuration)
                .SetEase(_scaleEase)
                .OnComplete(() =>
                {
                    transform.DOScale(1f, _scaleDuration).SetEase(Ease.OutBack);
                });
        }

        public void MarkTile()
        {
            if (!_isInitialized || _slot.IsMarked) return;

            _slot.Mark();
            UpdateTileAppearance();

            // 播放标记动画
            transform.DOScale(1.3f, _scaleDuration * 1.5f)
                .SetEase(_scaleEase)
                .OnComplete(() =>
                {
                    transform.DOScale(1f, _scaleDuration * 1.5f).SetEase(Ease.OutBack);
                });
        }

        public void HighlightTile()
        {
            if (!_isInitialized) return;

            // 高亮动画
            transform.DOScale(1.1f, 0.2f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
                });
        }

        public void SetInteractable(bool interactable)
        {
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.interactable = interactable;
            }

            if (_backgroundImage != null)
            {
                var color = _backgroundImage.color;
                color.a = interactable ? 1f : 0.5f;
                _backgroundImage.color = color;
            }
        }

        public void ResetTile()
        {
            if (!_isInitialized) return;

            _slot.Reset();
            UpdateTileAppearance();

            // 重置动画
            transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        }

        public BingoSlotState GetSlot()
        {
            return _slot;
        }

        private void OnDestroy()
        {
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveListener(OnTileClick);
            }
        }
    }
}