using Project.Scripts.Infrastracture.ServiceLocator;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Sounds.Components
{
    public class SoundsSetting:MonoBehaviour
    {
        [SerializeField] private Toggle _musicSwitch;
        [SerializeField] private Toggle _soundSwitch;

            
        private bool _soundActive = true;
        private bool _musicActive = true;
        private ISoundSettingsService _soundSettingsService;


        private void Start()
        {
            _soundSettingsService = ServiceLocator.Global.Get<ISoundSettingsService>();

            _soundActive = _soundSettingsService.SoundActive;
            _soundSwitch.isOn=(_soundActive);
            _soundSwitch.onValueChanged.AddListener(SwitchSound);
            
            _musicActive = _soundSettingsService.MusicActive;
            _musicSwitch.isOn=(_musicActive);
            _musicSwitch.onValueChanged.AddListener(SwitchMusic);
        }

        public void SwitchMusic(bool _)
        {
            _musicActive = !_musicActive;
            _soundSettingsService.SetMusicState(_musicActive);

        }

        public void SwitchSound(bool _)
        {
            _soundActive = !_soundActive;
            _soundSettingsService.SetSoundState(_soundActive);
        }

    
    }
}