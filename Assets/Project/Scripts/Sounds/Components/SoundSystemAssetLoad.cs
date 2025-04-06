using Project.Scripts.Infrastracture.ServiceLocator;
using Project.Scripts.Sounds.AudioConfigsBase;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Scripts.Sounds.Components
{
    public class SoundSystemAssetLoad : MonoBehaviour
    {
        [SerializeField]
        private SoundSystemAsset _soundSystemAsset;

        [SerializeField]
        private bool _loadOnAwake = true;

        [SerializeField]
        private UnityEvent _onAssetLoaded;

        [SerializeField]
        private bool _unloadOnDestroy = true;

        private ISoundSystem _soundSystem;

  

        private void Awake()
        {
            if(ServiceLocator.Global==null)
                return;
            _soundSystem = ServiceLocator.Global.Get<ISoundSystem>();
            
            if (_loadOnAwake)
                LoadSoundAsset();
        }

        private void OnDestroy()
        {
            if (_soundSystem!=null&&_unloadOnDestroy)
                UnloadSoundAsset();
        }
        
        public async void LoadSoundAsset()
        {
             _soundSystem.LoadSoundAsset(_soundSystemAsset);
            _onAssetLoaded.Invoke();
        }
        
        public void UnloadSoundAsset()
        {
            _soundSystem.UnloadSoundAsset(_soundSystemAsset);
        }
    }
}