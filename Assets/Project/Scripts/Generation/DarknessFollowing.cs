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
        [SerializeField] private float _tunnelPathSpacing = 18f;
        [SerializeField] private float _tunnelPathWeight = 1f;
        [SerializeField] private float _tunnelPathRadiusScale = 0.85f;
        [SerializeField] private DescentController _descentController;
        [SerializeField] private LocationsGenerator _locationsGenerator;
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Material _darknessMaterial;
        [SerializeField] private DarknessChaseState _chase = new DarknessChaseState();
        [SerializeField] private DarknessClearAnchors _anchors = new DarknessClearAnchors();
        [SerializeField] private DarknessVolumeField _field = new DarknessVolumeField();
        [SerializeField] private float _fieldCenterYSmoothTime = 0.12f;
        [SerializeField] private int _fieldRebuildInterval = 2;
        [SerializeField] private int _meshRebuildInterval = 3;
        [SerializeField] private float _isoLevel = 0.5f;
        [SerializeField] private bool _killEnabled = true;
        [SerializeField] private float _killThreshold = 0.5f;
        [SerializeField] private float _velocityLookAhead = 1.5f;
        [SerializeField] private int _killDamage = 20;

        private DarknessMeshBuilder _meshBuilder;
        private DarknessKillSampler _killSampler;
        private int _fieldFrameCounter;
        private int _meshFrameCounter;
        private float _accumulatedFieldDeltaTime;
        private Vector3 _smoothedFieldCenter;
        private float _fieldCenterYVelocity;
        private bool _hasSmoothedFieldCenter;

        public void GameStart()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _anchors.ApplyWebProfile();
            _field.ApplyWebProfile();
            _fieldRebuildInterval = 4;
            _meshRebuildInterval = 6;
            _velocityLookAhead = 2f;
#endif
            _meshBuilder = new DarknessMeshBuilder();
            _meshFilter.sharedMesh = _meshBuilder.Mesh;
            _meshRenderer.sharedMaterial = _darknessMaterial;
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
            _meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            _meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            _anchors.EnsureInitialized();
            _field.EnsureInitialized();
            Vector3 playerPos = _playerController.transform.position;
            _chase.SetChaseY(playerPos.y + _offset.y);
            _anchors.AddGroundedAnchor(playerPos);
            _smoothedFieldCenter = playerPos;
            _hasSmoothedFieldCenter = true;
            _field.Rebuild(playerPos, _anchors, _chase, 0f, true);
            _meshBuilder.Build(_field, _meshFilter.transform, _isoLevel);

            _killSampler = new DarknessKillSampler(
                _field,
                _anchors,
                _chase,
                _playerHealth,
                _playerController.transform,
                _playerController,
                _killThreshold,
                _velocityLookAhead,
                _killDamage);

            _descentController.Grounded += OnCharacterGrounded;
            _locationsGenerator.LocationEntered += OnLocationEntered;
            _playerController.PlayerRespawner.Respawned += OnPlayerRespawned;
        }

        private void OnDestroy()
        {
            _descentController.Grounded -= OnCharacterGrounded;
            _locationsGenerator.LocationEntered -= OnLocationEntered;
            _playerController.PlayerRespawner.Respawned -= OnPlayerRespawned;
        }

        private void OnPlayerRespawned()
        {
            Vector3 position = _playerController.transform.position;
            _chase.SnapBelow(position.y - _deathClearDrop);
            _anchors.AddGroundedAnchor(position);
            ForceFieldRebuild(position);
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

            AddClearPath(
                playerPos,
                pathEnd,
                _tunnelPathSpacing,
                _tunnelPathWeight,
                _tunnelPathRadiusScale);
        }

        public void SnapClearAt(Vector3 position)
        {
            _anchors.SnapClear(position);
            ForceFieldRebuild(position);
        }

        public void AddClearBurst(Vector3 position, float weight = -1f, float radiusScale = -1f)
        {
            float burstWeight = weight > 0f ? weight : 1f;
            float burstScale = radiusScale > 0f ? radiusScale : 1f;
            _anchors.AddClearBurst(position, burstWeight, burstScale);
            ForceFieldRebuild(position);
        }

        public void AddClearPath(
            Vector3 from,
            Vector3 to,
            float spacing = -1f,
            float weight = -1f,
            float radiusScale = -1f)
        {
            _anchors.AddClearPath(from, to, spacing, weight, radiusScale);
            ForceFieldRebuild(Vector3.Lerp(from, to, 0.5f));
        }

        private void ForceFieldRebuild(Vector3 center)
        {
            _smoothedFieldCenter = center;
            _fieldCenterYVelocity = 0f;
            _hasSmoothedFieldCenter = true;
            _field.Rebuild(center, _anchors, _chase, 0f, true);
            _meshBuilder.Build(_field, _meshFilter.transform, _isoLevel);
            _fieldFrameCounter = 0;
            _meshFrameCounter = 0;
            _accumulatedFieldDeltaTime = 0f;
        }

        public void GameUpdate()
        {
            float deltaTime = Time.deltaTime;
            Vector3 targetPosition = GetTargetPosition();
            _chase.Tick(targetPosition.y, deltaTime);

            _anchors.TickDecay(deltaTime);
            _anchors.TickGrowth(deltaTime);
            UpdateGroundFollow();

            Vector3 playerPos = _playerController.transform.position;
            if (!_hasSmoothedFieldCenter)
            {
                _smoothedFieldCenter = playerPos;
                _hasSmoothedFieldCenter = true;
            }
            else
            {
                float smoothedY = Mathf.SmoothDamp(
                    _smoothedFieldCenter.y,
                    playerPos.y,
                    ref _fieldCenterYVelocity,
                    _fieldCenterYSmoothTime);
                _smoothedFieldCenter = new Vector3(playerPos.x, smoothedY, playerPos.z);
            }

            _accumulatedFieldDeltaTime += deltaTime;
            _fieldFrameCounter++;
            if (_fieldFrameCounter >= _fieldRebuildInterval)
            {
                _field.Rebuild(
                    _smoothedFieldCenter,
                    _anchors,
                    _chase,
                    _accumulatedFieldDeltaTime);
                _fieldFrameCounter = 0;
                _accumulatedFieldDeltaTime = 0f;
            }

            _meshFrameCounter++;
            if (_meshFrameCounter >= _meshRebuildInterval)
            {
                _meshFrameCounter = 0;
                _meshBuilder.Build(_field, _meshFilter.transform, _isoLevel);
            }

            if (_killEnabled)
                _killSampler.Tick();
        }

        private void UpdateGroundFollow()
        {
            bool grounded = Vector3.Distance(
                _descentController.LastGroundPosition,
                _playerController.transform.position) < 0.75f;

            if (!grounded)
            {
                _anchors.ReleaseFollow();
                return;
            }

            _anchors.TickFollow(_descentController.LastGroundPosition, Time.deltaTime);
        }

        private void OnCharacterGrounded()
        {
            Vector3 targetPosition = GetTargetPosition();
            _chase.ApplyGroundedImpulse(targetPosition.y, _offset.magnitude);
            _anchors.AddGrowingGroundedAnchor(_descentController.LastGroundPosition);
        }

        private Vector3 GetTargetPosition()
        {
            Vector3 target;
            if (_locationsGenerator.TryGetNearestLocationEnterPoint(
                    _descentController.LastGroundPosition,
                    _offset.magnitude,
                    out Vector3 locationEnter))
                target = locationEnter + _newLocationOffset;
            else
                target = new Vector3(0f, _descentController.LastGroundPosition.y, 0f) + _offset;

            target.y = _chase.ClampTargetY(target.y);
            return target;
        }
    }
}
