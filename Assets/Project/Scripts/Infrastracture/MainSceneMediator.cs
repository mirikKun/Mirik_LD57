using System;
using Project.Scripts.Generation;
using Scripts.UI.Animations;
using UnityEngine;

namespace Scripts.Infrastracture
{
    public class MainSceneMediator : MonoBehaviour
    {
        [SerializeField] private GameObject _mainMenu;
        [SerializeField] private GameObject _settingsMenu;

        [SerializeField] private GameObject _hud;
        [SerializeField] private GameObject _tutorial;
        [SerializeField] private GameObject _gameEnd;
        [SerializeField] private FadeAnimation _hudAnimation;

        [SerializeField] private InputReader _inputReader;
        

        private void Start()
        {
            _inputReader.Esc += OnEsc;
            ShowMainMenu();
            GameEndTrigger.OnGameEnded += ShowGameEndMenu;
        }

        private void OnDestroy()
        {
            GameEndTrigger.OnGameEnded -= ShowGameEndMenu;
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
            _tutorial.SetActive(false);
            _hudAnimation.InstantFadeIn();
            _inputReader.DisablePlayerActions();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        public void ShowGameEndMenu()
        {
            _mainMenu.SetActive(false);
            _gameEnd.SetActive(true);
            _settingsMenu.SetActive(false);
            _hud.SetActive(false);
            _tutorial.SetActive(false);
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
            _tutorial.SetActive(false);

        }
        public void ShowTutorialMenu()
        {
            _mainMenu.SetActive(false);
            _settingsMenu.SetActive(false);
            _hud.SetActive(false);
            _tutorial.SetActive(true);

        }
        public void ShowHud()
        {
            _inputReader.EnablePlayerActions();
            _mainMenu.SetActive(false);
            _settingsMenu.SetActive(false);
            _hud.SetActive(true);
            _tutorial.SetActive(false);

            _hudAnimation.FadeOut();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
    }
}