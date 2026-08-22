using Assets.Scripts.Player.Controller;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerEffects
{
    public class LampSway : MonoBehaviour
    {
        [SerializeField] private PlayerController _player;
        [SerializeField] private Transform _target;
        [SerializeField] private float _accelToAngle = 0.12f;
        [SerializeField] private float _verticalAccelToAngle = 0.08f;
        [SerializeField] private float _velocityToAngle = 0.35f;
        [SerializeField] private float _maxTilt = 16f;
        [SerializeField] private float _velocitySmoothTime = 0.05f;
        [SerializeField] private float _tiltSmoothTime = 0.12f;
        [SerializeField] private float _bobFrequency = 8f;
        [SerializeField] private float _bobAmplitude = 0.45f;
        [SerializeField] private float _lookToAngle = 0.08f;
        [SerializeField] private float _lookYawToAngle = 0.04f;
        [SerializeField] private float _lookSmoothTime = 0.04f;

        private Quaternion _restRotation;
        private Vector3 _smoothedLocalVelocity;
        private Vector3 _velocitySmoothRef;
        private Vector3 _prevSmoothedVelocity;
        private Vector3 _smoothedLook;
        private Vector3 _lookSmoothRef;
        private Quaternion _prevCameraRotation;
        private Vector3 _tilt;
        private Vector3 _tiltSmoothRef;
        private float _bobPhase;
        private bool _hasPreviousVelocity;
        private bool _hasPreviousCameraRotation;

        private void Awake()
        {
            _restRotation = _target.localRotation;
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f)
                return;

            Vector3 localVelocity = _target.parent.InverseTransformDirection(_player.GetVelocity());
            _smoothedLocalVelocity = Vector3.SmoothDamp(
                _smoothedLocalVelocity,
                localVelocity,
                ref _velocitySmoothRef,
                _velocitySmoothTime,
                Mathf.Infinity,
                dt);

            Vector3 acceleration = _hasPreviousVelocity
                ? (_smoothedLocalVelocity - _prevSmoothedVelocity) / dt
                : Vector3.zero;
            _prevSmoothedVelocity = _smoothedLocalVelocity;
            _hasPreviousVelocity = true;

            Vector3 targetTilt = new Vector3(
                acceleration.z * _accelToAngle - acceleration.y * _verticalAccelToAngle,
                0f,
                -acceleration.x * _accelToAngle);

            targetTilt += new Vector3(
                _smoothedLocalVelocity.z * _velocityToAngle,
                0f,
                -_smoothedLocalVelocity.x * _velocityToAngle);

            float horizontalSpeed = new Vector2(_smoothedLocalVelocity.x, _smoothedLocalVelocity.z).magnitude;
            _bobPhase += horizontalSpeed * _bobFrequency * dt;
            targetTilt.x += Mathf.Sin(_bobPhase) * horizontalSpeed * _bobAmplitude;
            targetTilt.z += Mathf.Cos(_bobPhase * 0.5f) * horizontalSpeed * _bobAmplitude * 0.5f;

            targetTilt += GetLookTilt(dt);

            targetTilt = Vector3.ClampMagnitude(targetTilt, _maxTilt);
            _tilt = Vector3.SmoothDamp(_tilt, targetTilt, ref _tiltSmoothRef, _tiltSmoothTime, Mathf.Infinity, dt);

            _target.localRotation = Quaternion.Euler(_tilt) * _restRotation;
        }

        private Vector3 GetLookTilt(float dt)
        {
            Quaternion cameraRotation = _player.CameraTrY.rotation;
            Vector3 worldAngular = Vector3.zero;
            if (_hasPreviousCameraRotation)
            {
                Quaternion delta = cameraRotation * Quaternion.Inverse(_prevCameraRotation);
                delta.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180f)
                    angle -= 360f;
                if (Mathf.Abs(angle) > 0.001f)
                    worldAngular = axis.normalized * (angle / dt);
            }

            _prevCameraRotation = cameraRotation;
            _hasPreviousCameraRotation = true;

            Vector3 localAngular = _target.parent.InverseTransformDirection(worldAngular);
            _smoothedLook = Vector3.SmoothDamp(
                _smoothedLook,
                localAngular,
                ref _lookSmoothRef,
                _lookSmoothTime,
                Mathf.Infinity,
                dt);

            return new Vector3(
                -_smoothedLook.x * _lookToAngle,
                -_smoothedLook.y * _lookYawToAngle,
                _smoothedLook.y * _lookToAngle);
        }
    }
}
