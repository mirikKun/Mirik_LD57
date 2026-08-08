using System;
using System.Collections.Generic;
using Project.Scripts.Generation.Procedural;
using Project.Scripts.Infrastracture.GameLoop;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project.Scripts.Generation
{
    /// <summary>
    /// Sequence: basic tutorials (prefabs) -> N procedural levels -> advanced
    /// tutorials (prefabs) -> endless procedural levels with growing difficulty.
    /// </summary>
    public class LocationsGenerator : MonoBehaviour, IGameStartable
    {
        [SerializeField] private Location _startLocation;
        [SerializeField] private bool _skipTutorial;
        [SerializeField] private List<Location> _basicTutorialLocations;
        [SerializeField] private List<Location> _advancedTutorialLocations;
        [SerializeField] private ProceduralLevelsConfig _proceduralConfig;

        private ProceduralLevelBuilder _proceduralBuilder;
        private readonly List<Location> _currentLocations = new List<Location>();
        private int _currentLocationIndex;
        private bool _initialized;
        private bool _startLocationCommitted;
        private Location _lastProceduralLocation;
        public event Action<Bounds> LocationEntered;

        public void GameStart()
        {
            if (!EnsureInitialized())
                return;

            if (!_startLocationCommitted)
                GenerateNextLocation();
        }

        private bool EnsureInitialized()
        {
            if (_initialized)
                return true;

            if (_proceduralConfig == null)
                _proceduralConfig = Resources.Load<ProceduralLevelsConfig>("ProceduralLevels/ProceduralLevelsConfig");
            if (_proceduralConfig == null)
            {
                Debug.LogError("LocationsGenerator: ProceduralLevelsConfig not found. Assign it or place it at Resources/ProceduralLevels/ProceduralLevelsConfig.");
                return false;
            }

            if (_startLocation == null)
            {
                Debug.LogError("LocationsGenerator: Start Location is not assigned.");
                return false;
            }

            _proceduralBuilder = new ProceduralLevelBuilder(_proceduralConfig);
            _currentLocations.Add(_startLocation);
            _startLocation.LocationEntered += OnLocationEntered;
            _initialized = true;
            return true;
        }

        private Location CreateNextLocation(Vector3 fromPosition)
        {
            if (_skipTutorial)
                return BuildProceduralLocation(fromPosition);

            int index = _currentLocationIndex;

            if (index < _basicTutorialLocations.Count)
                return InstantiatePrefabLocation(_basicTutorialLocations[index], fromPosition);
            index -= _basicTutorialLocations.Count;

            if (index < _proceduralConfig.LevelsBeforeAdvancedTutorials)
                return BuildProceduralLocation(fromPosition);
            index -= _proceduralConfig.LevelsBeforeAdvancedTutorials;

            if (index < _advancedTutorialLocations.Count)
                return InstantiatePrefabLocation(_advancedTutorialLocations[index], fromPosition);

            return BuildProceduralLocation(fromPosition);
        }

        private Location InstantiatePrefabLocation(Location prefab, Vector3 fromPosition)
        {
            return Instantiate(prefab, GetNextLocationPosition(fromPosition, prefab), Quaternion.Euler(0, Random.Range(0, 360), 0));
        }

        private Location BuildProceduralLocation(Vector3 fromPosition)
        {
            // Procedural levels keep their start anchor at the root origin, so the
            // root goes exactly to the previous location's end point.
            _lastProceduralLocation = _proceduralBuilder.BuildNext(fromPosition, Quaternion.Euler(0, Random.Range(0, 360), 0));
            return _lastProceduralLocation;
        }

        /// <summary>
        /// Testing tool (editor inspector button): replaces the most recently generated
        /// procedural location in place, using the same archetype and anchor, with
        /// either the same seed or a new one.
        /// </summary>
        public void RegenerateLastLocation(bool newSeed)
        {
            if (!EnsureInitialized())
                return;

            if (_lastProceduralLocation == null || _currentLocations.Count == 0
                || _currentLocations[^1] != _lastProceduralLocation)
            {
                Debug.LogWarning("LocationsGenerator: the newest location is not procedural, nothing to regenerate.");
                return;
            }

            Location old = _lastProceduralLocation;
            old.LocationEntered -= OnLocationEntered;
            _currentLocations.Remove(old);
            Destroy(old.gameObject);

            Location rebuilt = _proceduralBuilder.RebuildLast(newSeed);
            if (rebuilt == null)
                return;

            _lastProceduralLocation = rebuilt;
            _currentLocations.Add(rebuilt);
            rebuilt.LocationEntered += OnLocationEntered;

            NotifyLocationBounds(rebuilt);
        }

        public void GenerateNextLocation()
        {
            if (!EnsureInitialized())
                return;

            if (_currentLocations.Count == 0)
            {
                Debug.LogWarning("LocationsGenerator: no current location to advance from.");
                return;
            }

            OnLocationEntered(_currentLocations[^1]);
        }

        public bool TryGetNearestLocationEnterPoint(Vector3 position, float maxDistance, out Vector3 enterPoint)
        {
            enterPoint = default;

            float minDistance = maxDistance;
            foreach (Location location in _currentLocations)
            {
                foreach (Transform point in location.LocationEndPoints)
                {
                    if (point.position.y > position.y)
                    {
                        continue;
                    }

                    float distance = Vector3.Distance(position, point.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        enterPoint = point.position;
                    }
                }
            }

            return enterPoint != default;
        }

        /// <summary>
        /// Called when a location's own commit trigger fires - for procedural levels
        /// that's the tunnel mouth at its exit, entered mid-fall, not its entry. At that
        /// moment: drop anything left over from before the committed location (it's now
        /// unreachable, sealed behind the tunnel's darkness barrier), and build whatever
        /// comes next, anchored at the committed location's end point(s). Darkness retiles
        /// around the newly built location, since that's where the player is headed.
        /// </summary>
        private void OnLocationEntered(Location committedLocation)
        {
            if (committedLocation == _startLocation)
                _startLocationCommitted = true;

            foreach (var location in _currentLocations)
            {
                if (location == committedLocation)
                    continue;
                Destroy(location.gameObject);
            }

            _currentLocations.Clear();
            _currentLocations.Add(committedLocation);

            foreach (var endPoint in committedLocation.LocationEndPoints)
            {
                Location next = CreateNextLocation(endPoint.position);
                _currentLocationIndex++;
                _currentLocations.Add(next);
                next.LocationEntered += OnLocationEntered;

                NotifyLocationBounds(next);
            }
        }

        private void NotifyLocationBounds(Location location)
        {
            location.CalculateBounds();
            LocationEntered?.Invoke(location.WorldBounds);
        }

        private Vector3 GetNextLocationPosition(Vector3 fromPosition, Location toLocation)
        {
            Vector3 toOffset = toLocation.LocationStartPoint.localPosition;
            return fromPosition - toOffset;
        }
    }
}
