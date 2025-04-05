using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation
{
    public class LocationsGenerator:MonoBehaviour
    {
        [SerializeField] private Location _startLocation;
        [SerializeField] private List<Location> _locationPrefabs;
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
                int randomIndex = Random.Range(0, _locationPrefabs.Count);
                Location chosenLocation = _locationPrefabs[randomIndex];
                Location newLocation = Instantiate(chosenLocation,
                    GetNextLocationPosition(root.position, chosenLocation), Quaternion.identity);
                _currentLocations.Add(newLocation);
                newLocation.LocationEntered+=OnLocationEntered;
            }
          
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