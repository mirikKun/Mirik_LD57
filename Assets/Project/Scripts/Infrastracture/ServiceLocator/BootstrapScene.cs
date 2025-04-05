using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Scripts.Infrastracture.ServiceLocator {
    [AddComponentMenu("ServiceLocator/ServiceLocator Scene")]
    public class BootstrapScene : Bootstrapper {
        private const string SceneName = "BootstrapScene";
        protected override void Bootstrap()
        {
            if (ServiceLocator.Global == null)
                //SceneManager.LoadScene(SceneName);
                SceneLoaderService.InstantLoad(SceneName);
            
            
            Container.ConfigureForScene();            
        }
    }
}