using UnityEngine;

namespace Assets.Scripts.Player.PlayerEffects
{
    public class CameraMovingEffects : MonoBehaviour
    {
        [Header("Camera References")]
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Transform _cameraHolder;
    
    [Header("Walking Shake Settings")]
    [SerializeField] private float _walkShakeIntensityVertical = 0.1f;
    [SerializeField] private float _walkShakeIntensityHorizontal = 0.05f;
    [SerializeField] private float _walkShakeCycleTime = 1.0f;
    [SerializeField] private AnimationCurve _walkShakeVerticalCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(0.25f, 1f, 0f, 0f), new Keyframe(0.5f, 0f, 0f, 0f), new Keyframe(0.75f, -1f, 0f, 0f), new Keyframe(1f, 0f, 0f, 0f));
    [SerializeField] private AnimationCurve _walkShakeHorizontalCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f), new Keyframe(0.25f, 0.5f, 0f, 0f), new Keyframe(0.5f, 0f, 0f, 0f), new Keyframe(0.75f, -0.5f, 0f, 0f), new Keyframe(1f, 0f, 0f, 0f));
    [SerializeField] private bool _enableWalkShake = true;
    
    [Header("Wall Run Settings")]
    [SerializeField] private float _wallRunTiltAngle = 9;
    [SerializeField] private float _wallRunTiltSpeed = 60f;
    [SerializeField] private bool _enableWallRunTilt = true;
    
    [Header("FOV Settings")]
    [SerializeField] private float _defaultFOV = 60f;
    [SerializeField] private float _maxFOV = 70f;
    [SerializeField] private AnimationCurve _speedToFOVCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float _fovChangeSpeed = 135f;
    [SerializeField] private bool _enableFOVChange = true;
    
    // Private variables
    private float _initialCameraY;
    private float _shakeTime;
    private float _currentTiltAngle;
    private float _targetTiltAngle;
    private float _currentFOV;
    private float _targetFOV;
    private bool _isGrounded;
    
    private void Start()
    {
        // Initialize
        if (_playerCamera == null)
            _playerCamera = GetComponentInChildren<Camera>();
            
        if (_cameraHolder == null)
            _cameraHolder = _playerCamera.transform;
            
        // Store initial camera position
        _initialCameraY = _cameraHolder.localPosition.y;
        
        // Set initial FOV values
        _currentFOV = _defaultFOV;
        _targetFOV = _defaultFOV;
        _playerCamera.fieldOfView = _defaultFOV;
        

    }
    
    private void Update()
    {
        // Apply camera effects
        if (_enableWalkShake)
            ApplyWalkingShake();
            
        if (_enableWallRunTilt)
            ApplyWallRunTilt();
            
        if (_enableFOVChange)
            ApplyFOVChange();
    }
    
    /// <summary>
    /// Set the grounded state from external script
    /// </summary>
    public void SetGrounded(bool grounded)
    {
        _isGrounded = grounded;
    }
    
    /// <summary>
    /// Applies a walking bobbing effect to the camera when the player is moving on the ground
    /// </summary>
    private void ApplyWalkingShake()
    {
        if (!_isGrounded)
        {
            // Reset camera position when in air
            ResetCameraPosition();
            return;
        }
        
        // Only apply walking shake when player is moving (this should be connected to player movement)
        // For this example, we'll just assume the player is always walking when grounded
        if (_isGrounded)
        {
            // Increment shake time
            _shakeTime += Time.deltaTime;
            
            // Cycle the time value between 0 and walkShakeCycleTime
            float normalizedTime = (_shakeTime % _walkShakeCycleTime) / _walkShakeCycleTime;
            
            // Evaluate vertical movement from curve
            float verticalOffset = _walkShakeVerticalCurve.Evaluate(normalizedTime) * _walkShakeIntensityVertical;
            
            // Evaluate horizontal movement from curve (potentially offset from vertical for more natural movement)
            float horizontalOffset = _walkShakeHorizontalCurve.Evaluate(normalizedTime) * _walkShakeIntensityHorizontal;
            
        
            // Apply both offsets to camera position
            _cameraHolder.localPosition = new Vector3(
                horizontalOffset, 
                _initialCameraY + verticalOffset, 
                _cameraHolder.localPosition.z);
        }
    }
    
    /// <summary>
    /// Reset camera position to default
    /// </summary>
    private void ResetCameraPosition()
    {
        _cameraHolder.localPosition = new Vector3(0, _initialCameraY, _cameraHolder.localPosition.z);
    }
    
    /// <summary>
    /// Applies a smooth camera tilt when wall running
    /// </summary>
    private void ApplyWallRunTilt()
    {
        // Smoothly transition to target tilt angle using MoveTowards for more consistent movement
        _currentTiltAngle = Mathf.MoveTowards(_currentTiltAngle, _targetTiltAngle, _wallRunTiltSpeed * Time.deltaTime);
        
        // Apply tilt to camera
        Vector3 currentRotation = _cameraHolder.localEulerAngles;
        _cameraHolder.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, _currentTiltAngle);
    }
    
    /// <summary>
    /// Applies FOV changes based on player speed
    /// </summary>
    private void ApplyFOVChange()
    {
        // Smoothly transition FOV using MoveTowards for more consistent movement
        _currentFOV = Mathf.MoveTowards(_currentFOV, _targetFOV, _fovChangeSpeed * Time.deltaTime);
        _playerCamera.fieldOfView = _currentFOV;
    }
    
    /// <summary>
    /// Set wall run tilt angle. Positive values tilt right, negative values tilt left.
    /// </summary>
    /// <param name="side">Direction of tilt. Use 1 for right, -1 for left, or 0 to reset</param>
    public void SetWallRunTilt(float side)
    {
        if (!_enableWallRunTilt)
            return;
            
        _targetTiltAngle = side * _wallRunTiltAngle;
    }
    
    /// <summary>
    /// Stop wall running
    /// </summary>
    public void StopWallRun()
    {
        _targetTiltAngle = 0f;
    }
    
    /// <summary>
    /// Change FOV based on player speed
    /// </summary>
    /// <param name="normalizedSpeed">Player speed normalized between 0 and 1</param>
    public void SetSpeedFOV(float normalizedSpeed)
    {
        if (!_enableFOVChange)
            return;
            
        // Clamp speed between 0 and 1
        normalizedSpeed = Mathf.Clamp01(normalizedSpeed);
        
        // Get evaluated value from animation curve (range 0-1)
        float curveValue = _speedToFOVCurve.Evaluate(normalizedSpeed);
        
        // Calculate target FOV using the curve value
        float fovRange = _maxFOV - _defaultFOV;
        _targetFOV = _defaultFOV + (fovRange * curveValue);
    }
    
    /// <summary>
    /// Set target FOV directly
    /// </summary>
    /// <param name="newFOV">Target FOV value</param>
    public void SetFOV(float newFOV)
    {
        if (!_enableFOVChange)
            return;
            
        _targetFOV = newFOV;
    }
    
    /// <summary>
    /// Reset FOV to default value
    /// </summary>
    public void ResetFOV()
    {
        _targetFOV = _defaultFOV;
    }
    }
}