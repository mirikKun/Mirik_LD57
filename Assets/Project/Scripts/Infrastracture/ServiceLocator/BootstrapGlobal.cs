using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Infrastracture.ServiceLocator {
    [AddComponentMenu("ServiceLocator/ServiceLocator Global")]
    public class BootstrapGlobal : Bootstrapper {
        [SerializeField] private bool _dontDestroyOnLoad = true;
        [SerializeField] private List<BootstrapServiceInstaller> _serviceInstallers;
        
        private const string MenuSceneName = "MainScene";
        protected override void Bootstrap() {
            Container.ConfigureAsGlobal(_dontDestroyOnLoad);
            foreach (var installer in _serviceInstallers)
            {
                installer.Install();
            }
            SceneLoaderService.InstantLoad(MenuSceneName);
       
        }
    }
}