using System;
using Project.Scripts.Sounds.Constants;
using Project.Scripts.Sounds.UserModels;

namespace Project.Scripts.Sounds
{
    class SoundSettingsService : ISoundSettingsService
    {
        private readonly ISoundSystem _soundSystem;
        private readonly SoundUserModel _soundUserModel;
        public event Action SoundStateChanged;
        

        public SoundSettingsService(ISoundSystem soundSystem)
        {
            _soundSystem = soundSystem;
            _soundUserModel = SoundUserModel.LoadModel();
            Init();
        }

        public void Init()
        {
            SetSoundState(_soundUserModel.SoundEnabled);
            SetMusicState(_soundUserModel.MusicEnabled);
        }

        public bool MusicActive => _soundUserModel.MusicEnabled;
        public bool SoundActive => _soundUserModel.SoundEnabled;

        public void SetMusicState(bool isActive)
        {
            _soundUserModel.MusicEnabled = isActive;
            
            _soundSystem.InvokeEvent(isActive ? SoundConstants.TurnOnMusic : SoundConstants.TurnOffMusic);
            SoundStateChanged?.Invoke();
        }

        public void SetSoundState(bool isActive)
        {
            _soundUserModel.SoundEnabled = isActive;
            
            _soundSystem.InvokeEvent(isActive ? SoundConstants.TurnOnSounds : SoundConstants.TurnOffSounds);
            SoundStateChanged?.Invoke();
        }
    }
}