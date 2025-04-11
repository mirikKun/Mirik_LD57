using System;
using System.Collections.Generic;
using Project.Scripts.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project.Scripts.Generation
{
    public class LocationsGenerator:MonoBehaviour
    {
        [SerializeField] private Location _startLocation;
        [SerializeField] private List<Location> _easyLcationsPrefabs;
        [SerializeField] private List<Location> _hardLocationsPrefabs;

        [SerializeField] private Location _lastLocation;
        [SerializeField] private List<Location> _smallLocationPrefabs;
        private List<Location> _currentLocations = new List<Location>();
        private int _currentLocationIndex;
        public event Action<Vector3,Vector3> LocationEntered; 
        
        private void Start()
        {
            _currentLocations.Add(_startLocation);
            _easyLcationsPrefabs=_easyLcationsPrefabs.Shuffle();
            _hardLocationsPrefabs = _hardLocationsPrefabs.Shuffle();
            GenerateLocations(_startLocation);
        }

        private void GenerateLocations(Location fromLocation)
        {
            foreach (var root in fromLocation.LocationEndPoints)
            {
                Location chosenLocation;
                if (fromLocation.IsBigLocation)
                {
                    int randomIndex = Random.Range(0, _smallLocationPrefabs.Count);
                    chosenLocation = _smallLocationPrefabs[randomIndex];
                }else
                {
                    chosenLocation = GetNextBigLocation();
                    _currentLocationIndex++;
                }
                Location newLocation = Instantiate(chosenLocation,
                    GetNextLocationPosition(root.position, chosenLocation), Quaternion.Euler(0,Random.Range(0,360),0));
                _currentLocations.Add(newLocation);
                newLocation.LocationEntered+=OnLocationEntered;
            }
          
        }

        private Location GetNextBigLocation()
        {
            if (_currentLocationIndex < _easyLcationsPrefabs.Count)
            {
                return _easyLcationsPrefabs[_currentLocationIndex];
            }
            else if (_currentLocationIndex < _easyLcationsPrefabs.Count + _hardLocationsPrefabs.Count)
            {
                return _hardLocationsPrefabs[_currentLocationIndex-_easyLcationsPrefabs.Count];
            }
            else
            {
                return _lastLocation;
            }
        }
   
        public void GenerateNextLocation()
        {
            OnLocationEntered(_currentLocations[^1]);
        }

        
  
        private void OnLocationEntered(Location enteredLocation)
        {
            foreach (var location in _currentLocations)
            {
                if(location == enteredLocation)
                    continue;
                Destroy(location.gameObject);
            }
            _currentLocations.Clear();
            _currentLocations.Add(enteredLocation);
            GenerateLocations(enteredLocation);
            enteredLocation. CalculateBounds();
            enteredLocation.GetLocationSize(out Vector3 center, out Vector3 size);
            LocationEntered?.Invoke(center-size/2,size);
        }


        private Vector3 GetNextLocationPosition(Vector3 fromPosition, Location toLocation)
        {
            Vector3 toOffset = toLocation.LocationStartPoint.localPosition ;
            return fromPosition-toOffset;
        }
    
    }
}