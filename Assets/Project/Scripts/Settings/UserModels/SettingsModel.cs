using Project.Scripts.Saving;

namespace Scripts.Settings.UserModels
{
    public class SettingsModel: BaseSaveModel<SettingsModel>
    {
        private float _mouseSensitivity;
        
        public SettingsModel()
        {
            _mouseSensitivity = 15;

            Save();
        }
        
        public float MouseSensitivity
        {
            get => _mouseSensitivity;
            set
            {
                _mouseSensitivity = value;
                Save();
            }
        }
    }
}