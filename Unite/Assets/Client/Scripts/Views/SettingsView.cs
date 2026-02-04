using UnityEngine;
using UnityEngine.UI;

namespace BingoClient.Views
{
    public class SettingsView : MonoBehaviour
    {
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Toggle _soundToggle;
        [SerializeField] private Button _closeButton;

        private void Awake()
        {
            _closeButton.onClick.AddListener(OnCloseClicked);
            _settingsPanel.SetActive(false);
        }

        public void Show()
        {
            _settingsPanel.SetActive(true);
            _settingsPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        private void OnCloseClicked()
        {
            _settingsPanel.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => _settingsPanel.SetActive(false));
        }
    }
}