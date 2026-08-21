using Assets.Scripts.Player.Controller;
using Project.Scripts.Generation.Darkness;
using Project.Scripts.Infrastracture.GameLoop;
using Scripts.Player.DescentContorller;
using Scripts.Player.Health;
using UnityEngine;

namespace Project.Scripts.Generation
{
    public class DarknessFollowing : MonoBehaviour, IGameStartable, IGameUpdatable
    {
        [SerializeField] private Vector3 _offset;
        [SerializeField] private Vector3 _newLocationOffset;
        [SerializeField] private float _tunnelEntryDarknessDrop = 5f;
        [SerializeField] private float _deathClearDrop = 8f;
        [SerializeField] private DescentController _descentController;
        [SerializeField] private LocationsGenerator _locationsGenerator;
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Material _darknessMaterial;
        [SerializeField] private DarknessChaseState _chase = new DarknessChaseState();
        [SerializeField] private DarknessClearAnchors _anchors = new DarknessClearAnchors();
        [SerializeField] private bool _killEnabled = true;
        [SerializeField] private float _killThreshold = 0.5f;
        [SerializeField] private float _velocityLookAhead = 1.5f;
        [SerializeField] private int _killDamage = 20;

        private DarknessKillSampler _killSampler;

        public void GameStart()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _anchors.ApplyWebProfile();
            _velocityLookAhead = 2f;
#endif
            _meshRenderer.sharedMaterial = _darknessMaterial;
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
            _meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            _meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            Vector3 playerPos = _playerController.transform.position;
            _chase.SetChaseY(playerPos.y + _offset.y);
            _anchors.Snap(playerPos);
            SyncVisual();

            _killSampler = new DarknessKillSampler(
                _anchors,
                _chase,
                _playerHealth,
                _playerController.transform,
                _playerController,
                _killThreshold,
                _velocityLookAhead,
                _killDamage);

            _locationsGenerator.LocationEntered += OnLocationEntered;
            _playerController.PlayerRespawner.Respawned += OnPlayerRespawned;
        }

        private void OnDestroy()
        {
            _locationsGenerator.LocationEntered -= OnLocationEntered;
            _playerController.PlayerRespawner.Respawned -= OnPlayerRespawned;
        }

        private void OnPlayerRespawned()
        {
            Vector3 position = _playerController.transform.position;
            _chase.SnapBelow(position.y - _deathClearDrop);
            _anchors.Snap(position);
            SyncVisual();
        }

        private void OnLocationEntered(Bounds locationBounds, Transform entryPlatform)
        {
            Vector3 playerPos = _playerController.transform.position;
            Vector3 pathEnd = entryPlatform != null ? entryPlatform.position : playerPos;
            if (entryPlatform != null)
            {
                float sealY = entryPlatform.position.y - _tunnelEntryDarknessDrop;
                _chase.SealAt(sealY);
            }

            _anchors.CoverPath(playerPos, pathEnd);
            SyncVisual();
        }

        public void SnapClearAt(Vector3 position)
        {
            _anchors.Snap(position);
            SyncVisual();
        }

        public void AddClearBurst(Vector3 position, float weight = -1f, float radiusScale = -1f)
        {
            float burstScale = radiusScale > 0f ? radiusScale : 1f;
            _anchors.CoverPoint(position, burstScale);
            SyncVisual();
        }

        public void AddClearPath(
            Vector3 from,
            Vector3 to,
            float spacing = -1f,
            float weight = -1f,
            float radiusScale = -1f)
        {
            _anchors.CoverPath(from, to);
            SyncVisual();
        }

        public void GameUpdate()
        {
            Vector3 followPosition = GetFollowPosition();
            _chase.Tick(GetTargetY(followPosition));
            _anchors.Tick(followPosition, Time.deltaTime);
            SyncVisual();

            if (_killEnabled)
                _killSampler.Tick();
        }

        private void SyncVisual()
        {
            transform.position = _anchors.Position;
            transform.localScale = new Vector3(
                _anchors.CurrentRadiusHorizontal,
                _anchors.CurrentRadiusVertical,
                _anchors.CurrentRadiusHorizontal);
        }

        private Vector3 GetFollowPosition()
        {
            if (_descentController.InDarknessFollowZone)
                return _playerController.transform.position;
            return _descentController.LastGroundPosition;
        }

        private float GetTargetY(Vector3 followPosition)
        {
            float targetY;
            if (_locationsGenerator.TryGetNearestLocationEnterPoint(
                    followPosition,
                    _offset.magnitude,
                    out Vector3 locationEnter))
                targetY = locationEnter.y + _newLocationOffset.y;
            else
                targetY = followPosition.y + _offset.y;

            return _chase.ClampTargetY(targetY);
        }
    }
}
