

using Project.Scripts.Saving;

namespace Project.Scripts.Sounds.UserModels
{
    public class SoundUserModel : BaseSaveModel<SoundUserModel>
    {
        private bool _soundEnabled;
        private bool _musicEnabled;

        public SoundUserModel()
        {
            _soundEnabled = true;
            _musicEnabled = true;

            Save();
        }

        public SoundUserModel(bool soundEnabled, bool musicEnabled)
        {
            _soundEnabled = soundEnabled;
            _musicEnabled = musicEnabled;
        }

        public bool SoundEnabled
        {
            get => _soundEnabled;
            set
            {
                _soundEnabled = value;
                Save();
            }
        }
        
        public bool MusicEnabled
        {
            get => _musicEnabled;
            set
            {
                _musicEnabled = value;
                Save();
            }
        }
    }
}