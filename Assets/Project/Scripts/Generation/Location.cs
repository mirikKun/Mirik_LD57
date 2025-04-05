using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation
{
    public class Location:MonoBehaviour
    {

        [SerializeField] private Transform _locationStartPoint;
        [SerializeField] private LocationEnteredTrigger _locationEnterTrigger;
        [SerializeField] private bool _bigLocation;
        [Space]
        [SerializeField] private List<Transform> _locationEndPoints;

        public event Action<Location> LocationEntered;
        
        public Transform LocationStartPoint => _locationStartPoint;
        public List<Transform> LocationEndPoints => _locationEndPoints;
        
        public bool IsBigLocation => _bigLocation;

        private void Start()
        {
            if (_locationEnterTrigger != null)
            {
                _locationEnterTrigger.LocationEntered += HandleLocationEntered;
            }
        }

        private void OnDestroy()
        {
            if (_locationEnterTrigger != null)
            {
                _locationEnterTrigger.LocationEntered -= HandleLocationEntered;
            }
        }

        private void HandleLocationEntered()
        {
            LocationEntered?.Invoke(this);
        }
    }
}