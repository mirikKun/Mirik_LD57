using System;
using System.Collections.Generic;
using Assets.Scripts.Player.Controller;
using Project.Scripts.Generation.Procedural;
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
        [SerializeField] private Transform _entryPlatformPoint;

        [SerializeField] private Bounds _worldBounds;
        [Space] [SerializeField] private bool _visualizeBounds = true;
        [SerializeField] private Color _boundsColor = Color.green;
        [SerializeField] private bool _visualizePath = true;
        [SerializeField] private Color _pathColor = Color.cyan;
        [SerializeField] private List<PathPoint> _path;

        private bool _boundsLocked;

        public event Action<Location, PlayerController> LocationEntered;

        public Transform LocationStartPoint => _locationStartPoint;
        public List<Transform> LocationEndPoints => _locationEndPoints;
        public bool IsBigLocation => _bigLocation;
        public Bounds WorldBounds => _worldBounds;
        public Transform LevelElements => _levelElements;
        public Transform LocationEntrance => _locationEntrance;
        public Transform LocationExit => _locationExit;
        public Transform Decorations => _decorations;
        public Transform EntryPlatformPoint => _entryPlatformPoint;

        public void InitializeRuntime(
            Transform startPoint,
            List<Transform> endPoints,
            LocationEnteredTrigger enterTrigger,
            Transform levelElements,
            Transform locationEntrance,
            Transform locationExit,
            Transform decorations,
            Transform entryPlatformPoint)
        {
            _locationStartPoint = startPoint;
            _locationEndPoints = new List<Transform>(endPoints);
            _locationEnterTrigger = enterTrigger;
            _levelElements = levelElements;
            _locationEntrance = locationEntrance;
            _locationExit = locationExit;
            _decorations = decorations;
            _entryPlatformPoint = entryPlatformPoint;
        }

        public void SetWorldBounds(Bounds worldBounds)
        {
            _worldBounds = worldBounds;
            _boundsLocked = true;
        }

        public void SetPath(List<PathPoint> path)
        {
            _path = path;
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

        private void HandleLocationEntered(PlayerController player)
        {
            LocationEntered?.Invoke(this, player);
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
            if (TryEncapsulateRenderers(root, out Bounds rendererBounds))
                return rendererBounds;

            if (TryEncapsulateColliders(root, out Bounds colliderBounds))
                return colliderBounds;

            return new Bounds(root.position, Vector3.zero);
        }

        private static bool TryEncapsulateRenderers(Transform root, out Bounds bounds)
        {
            bool hasBounds = false;
            bounds = default;

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

            return hasBounds;
        }

        private static bool TryEncapsulateColliders(Transform root, out Bounds bounds)
        {
            bool hasBounds = false;
            bounds = default;

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

            return hasBounds;
        }

        private void OnDrawGizmos()
        {
            if (!_visualizeBounds || _worldBounds.size == Vector3.zero)
                return;

            Gizmos.color = _boundsColor;
            Gizmos.DrawWireCube(_worldBounds.center, _worldBounds.size);
            Gizmos.DrawSphere(_worldBounds.center, 0.05f);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_visualizePath || _path == null || _path.Count == 0)
                return;

            Gizmos.color = _pathColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            for (int i = 0; i < _path.Count; i++)
            {
                PathPoint point = _path[i];
                Gizmos.DrawSphere(point.Position, 0.25f);
                Gizmos.DrawRay(point.Position, point.Forward * 1.5f);
                if (i > 0)
                    Gizmos.DrawLine(_path[i - 1].Position, point.Position);
            }

            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
