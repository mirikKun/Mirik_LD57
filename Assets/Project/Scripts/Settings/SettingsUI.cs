using System;
using Assets.Scripts.Player.Controller;
using Scripts.Settings.UserModels;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Settings
{
    public class SettingsUI:MonoBehaviour
    {
        [SerializeField] private Slider _mouseSensitivitySlider;
        [SerializeField] private CameraController _cameraController;

        [SerializeField] private float _sensitivityFrom = 5;
        [SerializeField] private float _sensitivityTo = 25;
        private SettingsModel _settingsModel;

        private void Start()
        {
            _settingsModel = SettingsModel.LoadModel();
            _cameraController.SetCameraSpeed(_settingsModel.MouseSensitivity);
            _mouseSensitivitySlider.onValueChanged.AddListener(SensitivitySliderChanged);
            _mouseSensitivitySlider.value = Mathf.InverseLerp(_sensitivityFrom, _sensitivityTo, _settingsModel.MouseSensitivity);
        }

        private void SensitivitySliderChanged(float value)
        {
            float sensitivity = Mathf.Lerp(_sensitivityFrom, _sensitivityTo, value);
            _cameraController.SetCameraSpeed(sensitivity);
            _settingsModel.MouseSensitivity = sensitivity;
        }
     
    }
}