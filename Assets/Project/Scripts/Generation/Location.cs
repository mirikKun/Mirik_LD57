using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation
{
    public class Location : MonoBehaviour
    {
        [SerializeField] private Transform _locationStartPoint;
        [SerializeField] private LocationEnteredTrigger _locationEnterTrigger;
        [SerializeField] private bool _bigLocation;
        [Space] [SerializeField] private List<Transform> _locationEndPoints;

        [SerializeField] private Transform _levelElements;
        [SerializeField] private Transform _locationEntrance;
        [SerializeField] private Transform _locationExit;
        [SerializeField] private Transform _decorations;

        [SerializeField] private Bounds _worldBounds;
        [Space] [SerializeField] private bool _visualizeBounds = true;
        [SerializeField] private Color _boundsColor = Color.green;

        private bool _boundsLocked;

        public event Action<Location> LocationEntered;

        public Transform LocationStartPoint => _locationStartPoint;
        public List<Transform> LocationEndPoints => _locationEndPoints;
        public bool IsBigLocation => _bigLocation;
        public Bounds WorldBounds => _worldBounds;
        public Transform LevelElements => _levelElements;
        public Transform LocationEntrance => _locationEntrance;
        public Transform LocationExit => _locationExit;
        public Transform Decorations => _decorations;

        public void InitializeRuntime(
            Transform startPoint,
            List<Transform> endPoints,
            LocationEnteredTrigger enterTrigger,
            Transform levelElements,
            Transform locationEntrance,
            Transform locationExit,
            Transform decorations)
        {
            _locationStartPoint = startPoint;
            _locationEndPoints = new List<Transform>(endPoints);
            _locationEnterTrigger = enterTrigger;
            _levelElements = levelElements;
            _locationEntrance = locationEntrance;
            _locationExit = locationExit;
            _decorations = decorations;
        }

        public void SetWorldBounds(Bounds worldBounds)
        {
            _worldBounds = worldBounds;
            _boundsLocked = true;
        }

        private void Start()
        {
            if (_locationEnterTrigger != null)
                _locationEnterTrigger.LocationEntered += HandleLocationEntered;
        }

        private void OnDestroy()
        {
            if (_locationEnterTrigger != null)
                _locationEnterTrigger.LocationEntered -= HandleLocationEntered;
        }

        private void HandleLocationEntered()
        {
            LocationEntered?.Invoke(this);
        }

        [ContextMenu("CalculateBounds")]
        public void CalculateBounds()
        {
            if (_boundsLocked)
                return;

            _worldBounds = ComputeWorldBoundsFromGeometry();
        }

        private Bounds ComputeWorldBoundsFromGeometry()
        {
            Transform searchRoot = _levelElements != null ? _levelElements : transform;
            return ComputeBoundsUnder(searchRoot);
        }

        public static Bounds ComputeBoundsUnder(Transform root)
        {
            bool hasBounds = false;
            Bounds bounds = default;

            foreach (Collider collider in root.GetComponentsInChildren<Collider>())
            {
                if (!hasBounds)
                {
                    bounds = collider.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(collider.bounds);
                }
            }

            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
            {
                if (renderer is ParticleSystemRenderer)
                    continue;

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
                return new Bounds(root.position, Vector3.zero);

            return bounds;
        }

        private void OnDrawGizmos()
        {
            if (!_visualizeBounds || _worldBounds.size == Vector3.zero)
                return;

            Gizmos.color = _boundsColor;
            Gizmos.DrawWireCube(_worldBounds.center, _worldBounds.size);
            Gizmos.DrawSphere(_worldBounds.center, 0.05f);
        }
    }
}
