using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BingoShared.Models;

namespace BingoClient.Views
{
    public class PowerUpBarView : MonoBehaviour
    {
        [SerializeField] private Transform _powerUpContainer;
        [SerializeField] private GameObject _powerUpButtonPrefab;
        [SerializeField] private Slider _energySlider;

        private List<PowerUpButton> _powerUpButtons = new();
        private float _currentEnergy;

        public void AddPowerUp(PowerUpType type)
        {
            var powerUpButton = Instantiate(_powerUpButtonPrefab, _powerUpContainer)
                .GetComponent<PowerUpButton>();
            powerUpButton.Initialize(type);
            powerUpButton.OnClicked += OnPowerUpClicked;
            _powerUpButtons.Add(powerUpButton);
        }

        private void OnPowerUpClicked(PowerUpButton button)
        {
            if (_currentEnergy >= button.RequiredEnergy)
            {
                _currentEnergy -= button.RequiredEnergy;
                UpdateEnergyBar();
                button.Activate();
            }
        }

        public void AddEnergy(float amount)
        {
            _currentEnergy = Mathf.Min(_currentEnergy + amount, 1f);
            UpdateEnergyBar();
        }

        private void UpdateEnergyBar()
        {
            _energySlider.value = _currentEnergy;
        }
    }

    public class PowerUpButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _energyText;

        private PowerUpType _powerUpType;

        public event Action<PowerUpButton> OnClicked;
        public float RequiredEnergy { get; private set; }

        public void Initialize(PowerUpType type)
        {
            _powerUpType = type;
            RequiredEnergy = GetRequiredEnergy(type);
            _energyText.text = RequiredEnergy.ToString();
            _button.onClick.AddListener(OnButtonClick);
        }

        private float GetRequiredEnergy(PowerUpType type)
        {
            return type switch
            {
                PowerUpType.DoublePayout => 0.5f,
                PowerUpType.DoubleDaub => 0.3f,
                PowerUpType.Box => 0.8f,
                PowerUpType.Coin => 0.2f,
                _ => 0.5f
            };
        }

        private void OnButtonClick()
        {
            OnClicked?.Invoke(this);
        }

        public void Activate()
        {
            _button.interactable = false;
            _icon.DOColor(Color.gray, 0.3f);
        }
    }
}