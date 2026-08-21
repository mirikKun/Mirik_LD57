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
        [SerializeField] private DescentController _descentController;
        [SerializeField] private LocationsGenerator _locationsGenerator;
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Material _darknessMaterial;
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
            _anchors.Snap(playerPos);
            SyncVisual();

            _killSampler = new DarknessKillSampler(
                _anchors,
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
            _anchors.Snap(_playerController.transform.position);
            SyncVisual();
        }

        private void OnLocationEntered(Bounds locationBounds, Transform entryPlatform)
        {
            Vector3 playerPos = _playerController.transform.position;
            Vector3 pathEnd = entryPlatform != null ? entryPlatform.position : playerPos;
            _anchors.CoverPath(playerPos, pathEnd);
            SyncVisual();
        }

        public void SetPulseScale(float scale)
        {
            _anchors.SetPulseScale(scale);
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
            _anchors.Tick(GetFollowPosition(), Time.deltaTime);
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
    }
}
