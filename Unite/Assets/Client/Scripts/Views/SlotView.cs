using UnityEngine;
using UnityEngine.UI;
using BingoClient.Models;

namespace BingoClient.Views
{
    public class SlotView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _numberText;
        [SerializeField] private Image _background;
        [SerializeField] private Image _powerUpIcon;
        [SerializeField] private Image _numberBallIcon;

        private SlotData _slotData;
        private int _boardIndex;
        private int _slotIndex;

        public event Action<SlotView> OnClicked;

        public int BoardIndex => _boardIndex;
        public int SlotIndex => _slotIndex;

        public void Initialize(SlotData slotData)
        {
            _slotData = slotData;
            _numberText.text = slotData.Number.ToString();
            _button.onClick.AddListener(OnButtonClick);

            if (slotData.HasPowerUp && slotData.PowerUp != null)
            {
                ShowPowerUp(slotData.PowerUp.Type);
            }
        }

        private void OnButtonClick()
        {
            OnClicked?.Invoke(this);
        }

        public void SetMarked(bool isMarked)
        {
            _background.color = isMarked ? Color.green : Color.white;
            _numberBallIcon.gameObject.SetActive(!isMarked);
        }

        public void HighlightWin()
        {
            _background.color = Color.yellow;
        }

        public void ShowPowerUp(PowerUpType type)
        {
            _powerUpIcon.gameObject.SetActive(true);
        }

        public void Shake()
        {
            transform.DOShake(0.5f, 0.1f, 10, 90, false);
        }
    }
}