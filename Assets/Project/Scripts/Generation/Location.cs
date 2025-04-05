using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation
{
    public class Location:MonoBehaviour
    {

        [SerializeField] private Transform _locationStartPoint;
        [SerializeField] private List<Transform> _locationEndPoints;

        [SerializeField] private LocationEnteredTrigger _locationEnteredTrigger;
        public event Action<Location> LocationEntered;
        
        public Transform LocationStartPoint => _locationStartPoint;
        public List<Transform> LocationEndPoints => _locationEndPoints;

        private void Start()
        {
            if (_locationEnteredTrigger != null)
            {
                _locationEnteredTrigger.LocationEntered += HandleLocationEntered;
            }
        }

        private void OnDestroy()
        {
            if (_locationEnteredTrigger != null)
            {
                _locationEnteredTrigger.LocationEntered -= HandleLocationEntered;
            }
        }

        private void HandleLocationEntered()
        {
            LocationEntered?.Invoke(this);
        }
    }

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