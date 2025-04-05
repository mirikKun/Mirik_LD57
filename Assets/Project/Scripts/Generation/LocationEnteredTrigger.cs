using System;
using Assets.Scripts.Player.Controller;
using UnityEngine;

namespace Project.Scripts.Generation
{
    public class LocationEnteredTrigger:MonoBehaviour
    {
        public event Action LocationEntered;

        private void OnTriggerEnter(Collider other)
        {

            if (other.TryGetComponent<PlayerController>(out var playerController))
            {
                LocationEntered?.Invoke();
            }
        }
    }
}