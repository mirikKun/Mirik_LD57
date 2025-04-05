using Project.Scripts.Extensions;
using UnityEngine;

namespace Project.Scripts.Infrastracture.ServiceLocator {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ServiceLocator))]
    public abstract class Bootstrapper : MonoBehaviour {
        private ServiceLocator _container;
        public ServiceLocator Container => _container.OrNull() ?? (_container = GetComponent<ServiceLocator>());

        private bool _hasBeenBootstrapped;

        private void Awake() => BootstrapOnDemand();
        
        public void BootstrapOnDemand() {
            if (_hasBeenBootstrapped) return;
            _hasBeenBootstrapped = true;
            Bootstrap();
        }
        
        protected abstract void Bootstrap();
    }
}
