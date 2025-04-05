using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Player.DescentContorller
{
    public class DescentUI:MonoBehaviour
    {
        [SerializeField] private DescentController _descentController;
        [SerializeField] private TMP_Text _maxDepthText;
        [SerializeField] private TMP_Text _currentDepthText;

        [SerializeField] private Image _depthStaminaBar;

        private void Start()
        {
            _descentController.DepthStaminaChanged += UpdateDepthStaminaBar;
            _descentController.DepthChanged += UpdateDepthText;
        }

        private void UpdateDepthText(float current, float max)
        {
            
            _maxDepthText.text = max.ToString("F1");
            _currentDepthText.text = current.ToString("F1");
        }

        private void UpdateDepthStaminaBar(float stamina)
        {
            _depthStaminaBar.fillAmount = stamina;
        }
       
    }
}