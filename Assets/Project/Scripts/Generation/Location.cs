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

        [SerializeField] private Vector3 _size;
        [SerializeField] private Vector3 _center;
        [Space] [SerializeField] private bool _visualizeBounds = true;
        [SerializeField] private Color _boundsColor = Color.green;

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

        public void GetLocationSize(out Vector3 center, out Vector3 size)
        {
            center = _center;
            size = _size;
        }

        [ContextMenu("CalculateBounds")]
        public void CalculateBounds()
        {
            Transform[] allChildren = GetComponentsInChildren<Transform>();

            if (allChildren.Length <= 1)
            {
                Debug.LogWarning("no childs");
                _size = Vector3.zero;
                _center = Vector3.zero;
                return;
            }

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);


            foreach (Transform child in allChildren)
            {
                if (child == transform)
                    continue;

                Vector3 pos = child.position;

                min.x = Mathf.Min(min.x, pos.x);
                min.y = Mathf.Min(min.y, pos.y);
                min.z = Mathf.Min(min.z, pos.z);

                max.x = Mathf.Max(max.x, pos.x);
                max.y = Mathf.Max(max.y, pos.y);
                max.z = Mathf.Max(max.z, pos.z);
            }

            _size = max - min;

            float maxSize = Mathf.Max(_size.x, _size.y, _size.z);
            _size = new Vector3(maxSize, maxSize, maxSize);

            _center = min + _size / 2f;
        }

        private Vector3 GetAvgPoint()
        {
            List<Vector3> points = new List<Vector3>();
            foreach (var point in _locationEndPoints)
            {
                points.Add(point.position);
            }

            points.Add(_locationStartPoint.position);

            Vector3 avg = Vector3.zero;
            foreach (var point in points)
            {
                avg += point;
            }

            avg /= points.Count;
            return avg;
        }

        private void OnDrawGizmos()
        {
            if (!_visualizeBounds)
                return;

            Gizmos.color = _boundsColor;

            Matrix4x4 originalMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(_center, _size);
            Gizmos.matrix = originalMatrix;

            Gizmos.DrawSphere(transform.TransformPoint(_center), 0.05f);
        }
    }
}