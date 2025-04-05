using Project.Scripts.Sounds;
using Project.Scripts.Sounds.AudioConfigsBase;
using UnityEngine;

namespace Project.Scripts.Infrastracture.ServiceLocator
{
    public class BootstrapServiceInstaller: MonoBehaviour,IServiceInstaller,ICoroutineRunner
    {

        [SerializeField] private SoundSystemAsset _globalSoundSystemAsset;
        public void Install()
        {
            SoundSystem soundSystem = new SoundSystem();
            ServiceLocator.Global.Register<ISoundSystem>(soundSystem);
            ServiceLocator.Global.Register<ISoundSettingsService>(new SoundSettingsService(soundSystem));

            ServiceLocator.Global.Register<ISceneLoaderService>(new SceneLoaderService(this));
            
            soundSystem.LoadSoundAsset(_globalSoundSystemAsset);
        }
    }
}