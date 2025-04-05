using System;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Player.Health
{
    public class HealthUI:MonoBehaviour
    {
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private Image _healthSlider;

        private void Start()
        {
            _playerHealth.HealthChanged += UpdateHealthUI;
        }

        private void OnDestroy()
        {
            _playerHealth.HealthChanged -= UpdateHealthUI;
        }

        private void UpdateHealthUI(float value)
        {
            _healthSlider.fillAmount = value;
        }
    }
}