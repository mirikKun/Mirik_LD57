using System;
using Scripts.UI.Animations;
using UnityEngine;

namespace Scripts.Infrastracture
{
    public class MainSceneMediator : MonoBehaviour
    {
        [SerializeField] private GameObject _mainMenu;
        [SerializeField] private GameObject _settingsMenu;

        [SerializeField] private GameObject _hud;
        [SerializeField] private FadeAnimation _hudAnimation;

        [SerializeField] private InputReader _inputReader;

        private void Start()
        {
            _inputReader.Esc += OnEsc;
            ShowMainMenu();
        }

        private void OnEsc()
        {
            if (_mainMenu.activeSelf)
            {
                ShowHud();
            }
            else
            {
                ShowMainMenu();
            }
        }

        public void ShowMainMenu()
        {
            _mainMenu.SetActive(true);
            _settingsMenu.SetActive(false);
            _hud.SetActive(false);
            _hudAnimation.InstantFadeIn();
            _inputReader.DisablePlayerActions();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ShowSettingsMenu()
        {
            _mainMenu.SetActive(false);
            _settingsMenu.SetActive(true);
            _hud.SetActive(false);
        }

        public void ShowHud()
        {
            _inputReader.EnablePlayerActions();
            _mainMenu.SetActive(false);
            _settingsMenu.SetActive(false);
            _hud.SetActive(true);
            _hudAnimation.FadeOut();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}