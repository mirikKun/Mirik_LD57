using Scripts;
using UnityEngine;

namespace Assets.Scripts.Player.Controller {
    public class CameraController : MonoBehaviour {
        [Range(0f, 90f)] public float _upperVerticalLimit =75f;
        [Range(0f, 90f)] public float _lowerVerticalLimit = 76f;

        [SerializeField] private float _cameraSpeed = 15f;
        [SerializeField] private bool _smoothCameraRotation;
        [Range(1f, 50f)] public float _cameraSmoothingFactor = 25f;

        [SerializeField] private Transform _horizontalPivot;
        [SerializeField] private Transform _verticalPivot;
        [SerializeField] private InputReader _input;

        public Vector3 GetUpDirection() => _verticalPivot.up;
        public Vector3 GetFacingDirection () => _horizontalPivot.forward;

        public Transform CameraTrX=> _horizontalPivot;
        public Transform CameraTrY=> _verticalPivot;

        private float _currentXAngle;
        private float _currentYAngle;
        
        private float _angleYToRotate;

        public void Awake() {
            
            _currentXAngle = _verticalPivot.localRotation.eulerAngles.x;
            _currentYAngle = _horizontalPivot.localRotation.eulerAngles.y;
        }

        public void LateUpdate() {
            RotateCamera(_input.LookDirection.x, -_input.LookDirection.y);
            RotateAdditionalAngle();
        }

        private void RotateAdditionalAngle()
        {
            float deltaAngle = Mathf.Clamp(_angleYToRotate, -_cameraSpeed * Time.deltaTime*3, _cameraSpeed * Time.deltaTime*3);
            _currentYAngle+=deltaAngle;
            _horizontalPivot.localRotation = Quaternion.Euler(0, _currentYAngle, 0);
            _angleYToRotate -= deltaAngle;
        }
        public void SetCameraSpeed(float speed) {
            _cameraSpeed = speed;
        }

        public void RotateCamera(float horizontalInput, float verticalInput) {
            if (_smoothCameraRotation) {
                horizontalInput = Mathf.Lerp(0, horizontalInput, Time.deltaTime * _cameraSmoothingFactor);
                verticalInput = Mathf.Lerp(0, verticalInput, Time.deltaTime * _cameraSmoothingFactor);
            }

            _currentYAngle += horizontalInput * _cameraSpeed * Time.deltaTime;
            
            _currentXAngle += verticalInput * _cameraSpeed * Time.deltaTime;
            _currentXAngle = Mathf.Clamp(_currentXAngle, -_upperVerticalLimit, _lowerVerticalLimit);
            
            _horizontalPivot.localRotation = Quaternion.Euler(0, _currentYAngle, 0);
            _verticalPivot.localRotation = Quaternion.Euler(_currentXAngle, 0, 0);
        }

        public void RotateCameraHorizontal( float angle)
        {
            _angleYToRotate += angle;
        }
    }
}