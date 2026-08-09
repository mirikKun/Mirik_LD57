using Unity.Cinemachine;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerEffects
{
    public class CameraMovingEffects : MonoBehaviour
    {
        [Header("Camera References")]
        [SerializeField] private CinemachineCamera _playerCamera;
        [SerializeField] private Transform _cameraHolder;
        [SerializeField] private float _cameraPositionReturnSpeed = 3f;

        [Header("Walking Shake Settings")]
        [SerializeField] private float _walkShakeIntensityVertical = 0.1f;
        [SerializeField] private float _walkShakeIntensityHorizontal = 0.06f;
        [SerializeField] private float _walkShakeIntensityForward = 0.03f;
        [SerializeField] private float _walkShakeCycleTime = 1.3f;
        [SerializeField] private AnimationCurve _walkShakeVerticalCurve = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(0.25f, 0.5f), new Keyframe(0.5f, 0f),
            new Keyframe(0.75f, -0.5f), new Keyframe(1f, 0f));
        [SerializeField] private AnimationCurve _walkShakeHorizontalCurve = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(0.25f, 0.5f), new Keyframe(0.5f, 0f),
            new Keyframe(0.75f, -0.5f), new Keyframe(1f, 0f));
        [SerializeField] private AnimationCurve _walkShakeForwardCurve = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(0.25f, 0.5f), new Keyframe(0.5f, 0f),
            new Keyframe(0.75f, -0.5f), new Keyframe(1f, 0f));
        [SerializeField] private bool _enableWalkShake = true;

        [Header("Fall Settings")]
        [SerializeField] private float _fallMaxHeight = 20f;
        [SerializeField] private float _heightMultiplier = 3f;
        [SerializeField] private float _fallShakeMaxDuration = 0.4f;
        [SerializeField] private float _fallShakeMaxPower = 0.04f;
        [SerializeField] private float _fallBounceMaxDuration = 1.3f;
        [SerializeField] private float _fallBounceMaxPower = -0.4f;
        [SerializeField] private float _fallBounceMaxRotation = 5f;
        [SerializeField] private AnimationCurve _fallBounceAnimationCurve = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(0.25f, 1f), new Keyframe(1f, 0f));
        [SerializeField] private bool _enableFallBounce = true;

        [Header("Camera Shake Settings")]
        [SerializeField] private float _defaultCameraShakePower = 0.01f;
        [SerializeField] private float _defaultCameraShakeDuration = 0.3f;
        [SerializeField] private bool _enableCameraShake = true;

        [Header("Wall Run Settings")]
        [SerializeField] private float _wallRunTiltAngle = 9f;
        [SerializeField] private float _wallRunTiltSpeed = 60f;
        [SerializeField] private bool _enableWallRunTilt = true;

        [Header("FOV Settings")]
        [SerializeField] private float _defaultFOV = 60f;
        [SerializeField] private float _maxFOV = 70f;
        [SerializeField] private float _fovChangeSpeed = 135f;
        [SerializeField] private bool _enableFOVChange = true;

        private float _initialCameraY;
        private float _initialCameraZ;
        private Vector3 _targetCameraPosition;

        private float _movingShakeElapsedTime;
        private float _cameraShakeElapsedTime;
        private float _fallBounceElapsedTime;

        private float _currentCameraShakePower;
        private float _currentCameraShakeDuration;

        private float _currentWallTiltAngle;
        private float _targetWallTiltAngle;

        private float _currentFOV;
        private float _targetFOV;
        private bool _isGrounded;

        private bool _cameraShakeActive;
        private bool _fallBounceActive;
        private float _fallHeight;

        private Vector3 DefaultCameraPosition => new Vector3(0f, _initialCameraY, _initialCameraZ);

        private void Start()
        {
            if (_cameraHolder == null)
                _cameraHolder = _playerCamera.transform;

            _initialCameraY = _cameraHolder.localPosition.y;
            _initialCameraZ = _cameraHolder.localPosition.z;

            _currentFOV = _defaultFOV;
            _targetFOV = _defaultFOV;
            _playerCamera.Lens.FieldOfView = _defaultFOV;

            _targetCameraPosition = DefaultCameraPosition;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            _targetCameraPosition = Vector3.Lerp(
                _targetCameraPosition, DefaultCameraPosition, deltaTime * _cameraPositionReturnSpeed);

            if (_enableWalkShake)
                ApplyWalkingShake(deltaTime);

            if (_enableWallRunTilt)
                ApplyWallRunTilt(deltaTime);

            if (_enableFOVChange)
                ApplyFOVChange(deltaTime);

            if (_enableCameraShake)
                ApplyCameraShakeEffect(deltaTime);

            if (_enableFallBounce)
                ApplyFallBounceEffect(deltaTime);

            _cameraHolder.localPosition = _targetCameraPosition;
        }

        public void SetGrounded(bool grounded)
        {
            _isGrounded = grounded;
        }

        public void StartFallEffect(float height)
        {
            const float minHeight = 0.3f;
            if (height < minHeight)
                return;

            _fallBounceActive = true;
            _fallHeight = height;
            _fallBounceElapsedTime = 0f;

            float fallHeightMultiplier = Mathf.Clamp(_fallHeight * _heightMultiplier / _fallMaxHeight, 0f, 1f);
            StartCameraShake(fallHeightMultiplier * _fallShakeMaxPower, fallHeightMultiplier * _fallShakeMaxDuration);
        }

        public void StartCameraShake(float intensity = -1f, float duration = -1f)
        {
            if (!_enableCameraShake)
                return;

            _currentCameraShakePower = intensity > 0f ? intensity : _defaultCameraShakePower;
            _currentCameraShakeDuration = duration > 0f ? duration : _defaultCameraShakeDuration;
            _cameraShakeElapsedTime = 0f;
            _cameraShakeActive = true;
        }

        private void ApplyWalkingShake(float deltaTime)
        {
            if (!_isGrounded)
            {
                _movingShakeElapsedTime = 0f;
                return;
            }

            _movingShakeElapsedTime += deltaTime;
            float normalizedTime = (_movingShakeElapsedTime % _walkShakeCycleTime) / _walkShakeCycleTime;

            float verticalOffset = _walkShakeVerticalCurve.Evaluate(normalizedTime) * _walkShakeIntensityVertical;
            float horizontalOffset = _walkShakeHorizontalCurve.Evaluate(normalizedTime) * _walkShakeIntensityHorizontal;
            float forwardOffset = _walkShakeForwardCurve.Evaluate(normalizedTime) * _walkShakeIntensityForward;

            _targetCameraPosition = DefaultCameraPosition + new Vector3(horizontalOffset, verticalOffset, forwardOffset);
        }

        private void ApplyCameraShakeEffect(float deltaTime)
        {
            if (!_cameraShakeActive)
                return;

            if (_cameraShakeElapsedTime > _currentCameraShakeDuration)
            {
                _cameraShakeElapsedTime = 0f;
                _cameraShakeActive = false;
                return;
            }

            _targetCameraPosition += Random.insideUnitSphere * _currentCameraShakePower;
            _cameraShakeElapsedTime += deltaTime;
        }

        private void ApplyFallBounceEffect(float deltaTime)
        {
            if (!_fallBounceActive)
                return;

            float fallHeightMultiplier = Mathf.Clamp(_fallHeight * _heightMultiplier / _fallMaxHeight, 0f, 1f);
            float bounceDuration = _fallBounceMaxDuration * fallHeightMultiplier;
            float bouncePower = _fallBounceMaxPower * fallHeightMultiplier;
            float bounceRotation = _fallBounceMaxRotation * fallHeightMultiplier;

            if (_fallBounceElapsedTime > bounceDuration)
            {
                _fallBounceActive = false;
                _fallBounceElapsedTime = 0f;
                return;
            }

            _fallBounceElapsedTime += deltaTime;

            float normalizedTime = _fallBounceElapsedTime / bounceDuration;
            float bounceOffset = _fallBounceAnimationCurve.Evaluate(normalizedTime) * bouncePower;
            float bounceRotationAngle = _fallBounceAnimationCurve.Evaluate(normalizedTime) * bounceRotation;

            _targetCameraPosition += new Vector3(0f, bounceOffset, 0f);
            Vector3 currentRotation = _cameraHolder.localEulerAngles;
            _cameraHolder.localEulerAngles = new Vector3(bounceRotationAngle, currentRotation.y, currentRotation.z);
        }

        private void ApplyWallRunTilt(float deltaTime)
        {
            _currentWallTiltAngle =
                Mathf.MoveTowards(_currentWallTiltAngle, _targetWallTiltAngle, _wallRunTiltSpeed * deltaTime);

            Vector3 currentRotation = _cameraHolder.localEulerAngles;
            _cameraHolder.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, _currentWallTiltAngle);
        }

        private void ApplyFOVChange(float deltaTime)
        {
            _currentFOV = Mathf.MoveTowards(_currentFOV, _targetFOV, _fovChangeSpeed * deltaTime);
            _playerCamera.Lens.FieldOfView = _currentFOV;
        }

        public void SetWallRunTilt(float side)
        {
            if (!_enableWallRunTilt)
                return;

            _targetWallTiltAngle = side * _wallRunTiltAngle;
        }

        public void StopWallRun()
        {
            _targetWallTiltAngle = 0f;
        }

        public void SetSpeedFOV(float normalizedSpeed)
        {
            if (!_enableFOVChange)
                return;

            normalizedSpeed = Mathf.Clamp01(normalizedSpeed);
            _targetFOV = Mathf.Lerp(_defaultFOV, _maxFOV, normalizedSpeed);
        }

        public void SetFOV(float newFOV)
        {
            if (!_enableFOVChange)
                return;

            _targetFOV = newFOV;
        }

        public void ResetFOV()
        {
            _targetFOV = _defaultFOV;
        }
    }
}
