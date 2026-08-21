using System;
using Project.Scripts.Generation.Darkness;
using Scripts.LevelObjects;
using UnityEngine;
using LocationEnteredTrigger = Project.Scripts.Generation.LocationEnteredTrigger;

namespace Project.Scripts.Generation.Procedural
{
    public class LocationExitTunnel : MonoBehaviour
    {
        [SerializeField] private float _height = 3f;
        [SerializeField] private float _sealBarrierHeight = 8f;
        [SerializeField] private Transform _visual;
        [SerializeField] private Transform _endPoint;
        [SerializeField] private Transform _sealBarrier;
        [SerializeField] private LocationEnteredTrigger _enterTrigger;
        [SerializeField] private float _visualRadius = 4.4f;
        [SerializeField] private DarknessFollowZone _followZone;
        [SerializeField] private float _followTriggerDiameter = 3.5f;

        public float Height => _height;
        public Transform EndPoint => _endPoint;
        public LocationEnteredTrigger EnterTrigger => _enterTrigger;

        public void SetHeight(float height)
        {
            _height = Mathf.Max(0.1f, height);
            ApplyHeight();
        }

        private void Awake()
        {
            ApplyHeight();
        }

        public void Configure(float sealBarrierHeight, GameObject deathFloor)
        {
            _sealBarrierHeight = sealBarrierHeight;
            ApplyHeight();
            _sealBarrier.gameObject.SetActive(false);
            _enterTrigger.InitializeRuntime(
                new[] { _sealBarrier.gameObject },
                Array.Empty<LightFader>(),
                respawnOffset: -1f,
                setRespawnOnTrigger: true);

            if (deathFloor != null)
                _enterTrigger.LocationEntered += _ => deathFloor.SetActive(false);
        }

        private void OnValidate()
        {
            if (_visual == null || _endPoint == null || _sealBarrier == null)
                return;

            ApplyHeight();
        }

        private void ApplyHeight()
        {
            _visual.localScale = new Vector3(_visualRadius, _height, _visualRadius);
            _endPoint.localPosition = new Vector3(0f, -_height, 0f);
            _sealBarrier.localPosition = new Vector3(0f, _sealBarrierHeight, 0f);
            _followZone = DarknessFollowZone.Ensure(transform, _followZone, _height, _followTriggerDiameter);
        }
    }
}
