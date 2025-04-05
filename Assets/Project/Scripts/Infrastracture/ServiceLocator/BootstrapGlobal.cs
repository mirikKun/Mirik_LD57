using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Infrastracture.ServiceLocator {
    [AddComponentMenu("ServiceLocator/ServiceLocator Global")]
    public class BootstrapGlobal : Bootstrapper {
        [SerializeField] private bool _dontDestroyOnLoad = true;
        [SerializeField] private List<BootstrapServiceInstaller> _serviceInstallers;
        
        private const string MenuSceneName = "MainMenuScene";
        protected override void Bootstrap() {
            Container.ConfigureAsGlobal(_dontDestroyOnLoad);
            foreach (var installer in _serviceInstallers)
            {
                installer.Install();
            }
            
            if(Container.TryGet( out ISceneLoaderService sceneLoaderService))
            {
                sceneLoaderService.LoadScene(MenuSceneName);
            }
            else
            {
                Debug.LogError("SceneLoaderService not found");
            }
        }
    }
}