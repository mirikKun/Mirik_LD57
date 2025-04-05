
using System;

namespace Project.Scripts.Sounds
{
    public interface ISoundSettingsService
    {
        public bool MusicActive { get; }
        public bool SoundActive { get; }
        
        public void Init();
        
        public void SetMusicState(bool isActive);
        
        public  void SetSoundState(bool isActive);
        event Action SoundStateChanged;
    }
}