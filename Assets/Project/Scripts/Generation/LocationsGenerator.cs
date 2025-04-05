using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation
{
    public class LocationsGenerator:MonoBehaviour
    {
        [SerializeField] private Location _startLocation;
        [SerializeField] private List<Location> _bigLocationPrefabs;
        [SerializeField] private List<Location> _smallLocationPrefabs;
        private List<Location> _currentLocations = new List<Location>();
        
        private void Start()
        {
            _currentLocations.Add(_startLocation);
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
                    int randomIndex = Random.Range(0, _bigLocationPrefabs.Count);
                    chosenLocation = _bigLocationPrefabs[randomIndex];
                }
                Location newLocation = Instantiate(chosenLocation,
                    GetNextLocationPosition(root.position, chosenLocation), Quaternion.Euler(0,Random.Range(0,360),0));
                _currentLocations.Add(newLocation);
                newLocation.LocationEntered+=OnLocationEntered;
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
        }


        private Vector3 GetNextLocationPosition(Vector3 fromPosition, Location toLocation)
        {
            Vector3 toOffset = toLocation.LocationStartPoint.localPosition ;
            return fromPosition-toOffset;
        }
    
    }
}