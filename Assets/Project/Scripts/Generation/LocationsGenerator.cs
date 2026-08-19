using System;
using System.Collections.Generic;
using Assets.Scripts.Player.Controller;
using Project.Scripts.Generation.Procedural;
using Project.Scripts.Infrastracture.GameLoop;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project.Scripts.Generation
{
    /// <summary>
    /// Sequence: basic tutorials (prefabs) -> N procedural levels -> advanced
    /// tutorials (prefabs) -> endless procedural levels with growing difficulty.
    /// Keeps the committed location plus two ahead so the next level's tunnel is
    /// already visible while playing the current one.
    /// </summary>
    public class LocationsGenerator : MonoBehaviour, IGameStartable
    {
        private const int LookaheadDepth = 2;
        private const float EntryPlatformRespawnOffset = 1f;

        [SerializeField] private Location _startLocation;
        [SerializeField] private LevelArchetype _testArchetype;
        [SerializeField] private bool _skipTutorial;
        [SerializeField] private List<Location> _basicTutorialLocations;
        [SerializeField] private List<Location> _advancedTutorialLocations;
        [SerializeField] private ProceduralLevelsConfig _proceduralConfig;

        [NonSerialized] private ProceduralLevelBuilder _proceduralBuilder;
        [NonSerialized] private readonly List<Location> _currentLocations = new List<Location>();
        [NonSerialized] private readonly Dictionary<Location, List<Location>> _childrenByLocation = new Dictionary<Location, List<Location>>();
        [NonSerialized] private int _currentLocationIndex;
        [NonSerialized] private bool _initialized;
        [NonSerialized] private bool _startLocationCommitted;
        [NonSerialized] private Location _lastProceduralLocation;
        public event Action<Bounds, Transform> LocationEntered;

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

            _proceduralBuilder = new ProceduralLevelBuilder(_proceduralConfig, _testArchetype);
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
            _lastProceduralLocation = _proceduralBuilder.BuildNext(fromPosition, Quaternion.Euler(0, Random.Range(0, 360), 0));
            return _lastProceduralLocation;
        }

        public bool CanRegenerateLastLocation => _lastProceduralLocation != null;

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
            Location parent = FindParent(old);
            int listIndex = _currentLocations.IndexOf(old);
            int childIndex = parent != null ? _childrenByLocation[parent].IndexOf(old) : -1;

            _currentLocations.Remove(old);
            _childrenByLocation.Remove(old);
            DestroyLocationObject(old);

            Location rebuilt = _proceduralBuilder.RebuildLast(newSeed);
            if (rebuilt == null)
                return;

            ApplyEditorHideFlags(rebuilt);
            _lastProceduralLocation = rebuilt;
            _currentLocations.Insert(listIndex, rebuilt);
            rebuilt.LocationEntered += OnLocationEntered;

            if (parent != null && childIndex >= 0)
                _childrenByLocation[parent][childIndex] = rebuilt;

            NotifyLocationBounds(rebuilt);
        }

        public void GenerateNextLocation()
        {
            if (!EnsureInitialized())
                return;

            if (!Application.isPlaying)
            {
                GenerateSingleEditorLocation();
                return;
            }

            OnLocationEntered(_currentLocations[^1], null);
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
        /// Called when a location's commit trigger fires - for procedural levels
        /// that's the tunnel mouth at its exit, entered mid-fall. Drops anything
        /// behind the committed location, keeps/creates two levels ahead so the
        /// next tunnel is already visible, and points respawn at the destination
        /// entry platform.
        /// </summary>
        private void OnLocationEntered(Location committedLocation, PlayerController player)
        {
            if (committedLocation == _startLocation)
                _startLocationCommitted = true;

            int commitIndex = _currentLocations.IndexOf(committedLocation);
            if (commitIndex < 0)
            {
                Debug.LogWarning("LocationsGenerator: committed location is not in the active list.");
                return;
            }

            for (int i = 0; i < commitIndex; i++)
                DestroyLocation(_currentLocations[i]);
            _currentLocations.RemoveRange(0, commitIndex);

            List<Location> immediateNext = EnsureDepth(committedLocation, LookaheadDepth);

            var keep = new HashSet<Location> { committedLocation };
            CollectDescendants(committedLocation, LookaheadDepth, keep);

            for (int i = _currentLocations.Count - 1; i >= 0; i--)
            {
                Location location = _currentLocations[i];
                if (keep.Contains(location))
                    continue;

                DestroyLocation(location);
                _currentLocations.RemoveAt(i);
            }

            foreach (Location next in immediateNext)
                NotifyLocationBounds(next);

            if (player != null)
                SetRespawnToEntryPlatforms(player, immediateNext);
        }

        private List<Location> EnsureDepth(Location parent, int depth)
        {
            if (depth <= 0)
                return new List<Location>();

            if (!_childrenByLocation.TryGetValue(parent, out List<Location> children))
            {
                children = new List<Location>();
                foreach (Transform endPoint in parent.LocationEndPoints)
                {
                    Location next = CreateNextLocation(endPoint.position);
                    _currentLocationIndex++;
                    _currentLocations.Add(next);
                    next.LocationEntered += OnLocationEntered;
                    children.Add(next);
                }

                _childrenByLocation[parent] = children;
            }

            if (depth == 1)
                return children;

            foreach (Location child in children)
                EnsureDepth(child, depth - 1);

            return children;
        }

        private void CollectDescendants(Location parent, int depth, HashSet<Location> keep)
        {
            if (depth <= 0 || !_childrenByLocation.TryGetValue(parent, out List<Location> children))
                return;

            foreach (Location child in children)
            {
                keep.Add(child);
                CollectDescendants(child, depth - 1, keep);
            }
        }

        private void SetRespawnToEntryPlatforms(PlayerController player, List<Location> destinations)
        {
            foreach (Location destination in destinations)
            {
                Transform entry = destination.EntryPlatformPoint;
                if (entry == null)
                    continue;

                player.PlayerRespawner.SetRespawnPosition(entry.position + Vector3.up * EntryPlatformRespawnOffset);
                return;
            }
        }

        private Location FindParent(Location child)
        {
            foreach (KeyValuePair<Location, List<Location>> pair in _childrenByLocation)
            {
                if (pair.Value.Contains(child))
                    return pair.Key;
            }

            return null;
        }

        private void GenerateSingleEditorLocation()
        {
            ClearGeneratedEditorLocations();
            _proceduralBuilder = new ProceduralLevelBuilder(_proceduralConfig, _testArchetype);

            Transform endPoint = _startLocation.LocationEndPoints[0];
            Location next = BuildProceduralLocation(endPoint.position);
            _currentLocations.Add(next);
            next.LocationEntered += OnLocationEntered;
            ApplyEditorHideFlags(next);
            _childrenByLocation[_startLocation] = new List<Location> { next };
            NotifyLocationBounds(next);
        }

        private void ClearGeneratedEditorLocations()
        {
            for (int i = _currentLocations.Count - 1; i >= 0; i--)
            {
                Location location = _currentLocations[i];
                if (location == _startLocation)
                    continue;

                _currentLocations.RemoveAt(i);
                DestroyLocation(location);
            }

            _childrenByLocation.Clear();
            _lastProceduralLocation = null;
            DestroyOrphanProceduralLevels();
        }

        private void DestroyOrphanProceduralLevels()
        {
            Location[] locations = FindObjectsByType<Location>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Location location in locations)
            {
                if (location == _startLocation)
                    continue;
                if (!location.gameObject.name.StartsWith("ProceduralLevel_"))
                    continue;

                DestroyLocationObject(location);
            }
        }

        private void DestroyLocation(Location location)
        {
            _childrenByLocation.Remove(location);
            DestroyLocationObject(location);
        }

        private void DestroyLocationObject(Location location)
        {
            location.LocationEntered -= OnLocationEntered;
            if (Application.isPlaying)
                Destroy(location.gameObject);
            else
                DestroyImmediate(location.gameObject);
        }

        private static void ApplyEditorHideFlags(Location location)
        {
            if (!Application.isPlaying)
                location.gameObject.hideFlags = HideFlags.DontSaveInEditor;
        }

        private void NotifyLocationBounds(Location location)
        {
            location.CalculateBounds();
            LocationEntered?.Invoke(location.WorldBounds, location.EntryPlatformPoint);
        }

        private Vector3 GetNextLocationPosition(Vector3 fromPosition, Location toLocation)
        {
            Vector3 toOffset = toLocation.LocationStartPoint.localPosition;
            return fromPosition - toOffset;
        }
    }
}
