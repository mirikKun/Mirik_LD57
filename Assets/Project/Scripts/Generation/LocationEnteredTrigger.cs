using System;
using Assets.Scripts.Player.Controller;
using Scripts.LevelObjects;
using UnityEngine;

namespace Project.Scripts.Generation
{
    public class LocationEnteredTrigger:MonoBehaviour
    {
        [SerializeField] private float _respawnOffset = -2;
        [SerializeField] private GameObject[] _objectsToAppear;
        [SerializeField] private LightFader[] _lightsToFade;
        
        private bool _triggered;
        public event Action LocationEntered;

        private void OnTriggerEnter(Collider other)
        {
            if(_triggered)
                return;

            if (other.TryGetComponent<PlayerController>(out var playerController))
            {
                playerController.PlayerRespawner.SetRespawnPosition(transform.position + Vector3.up * _respawnOffset);
                playerController.DescentController.ReplenishFullStamina();

                LocationEntered?.Invoke();
                foreach (var obj in _objectsToAppear)
                {
                    obj.SetActive(true);
                }
                foreach (var light in _lightsToFade)
                {
                    light.FadeIn();
                }
                _triggered = true;
            }
        }
    }
}