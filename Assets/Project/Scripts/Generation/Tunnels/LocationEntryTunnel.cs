using Project.Scripts.Generation.Darkness;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    public class LocationEntryTunnel : MonoBehaviour
    {
        [SerializeField] private float _height = 10f;
        [SerializeField] private Transform _visual;
        [SerializeField] private Transform _bottomAnchor;
        [SerializeField] private float _visualRadius = 4.4f;
        [SerializeField] private DarknessFollowZone _followZone;
        [SerializeField] private float _followTriggerDiameter = 3.5f;

        public float Height => _height;
        public Transform BottomAnchor => _bottomAnchor;

        public void SetHeight(float height)
        {
            _height = Mathf.Max(0.1f, height);
            ApplyHeight();
        }

        private void Awake()
        {
            ApplyHeight();
        }

        private void OnValidate()
        {
            if (_visual == null || _bottomAnchor == null)
                return;

            ApplyHeight();
        }

        private void ApplyHeight()
        {
            _visual.localScale = new Vector3(_visualRadius, _height, _visualRadius);
            _bottomAnchor.localPosition = new Vector3(0f, -_height, 0f);
            _followZone = DarknessFollowZone.Ensure(transform, _followZone, _height, _followTriggerDiameter);
        }
    }
}
