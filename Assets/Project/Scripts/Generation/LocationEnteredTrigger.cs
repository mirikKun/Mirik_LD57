using System;
using UnityEngine;

namespace Project.Scripts.Generation
{
    public class LocationEnteredTrigger:MonoBehaviour
    {
        public event Action LocationEntered;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Trigger the event when the player enters the location
                LocationEntered?.Invoke();
            }
        }
    }
}