using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using ImprovedTimers;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class WallRunningState : IState
    {
        private readonly PlayerController _controller;

        private readonly float _wallRunDuration;
        private readonly float _wallRunSpeed;
        private readonly float _minSpeedToStartWallRun;
        private readonly float _wallGravity;
        private readonly float _cameraAngle;
        private readonly CountdownTimer _wallRunTimer;
        private readonly float _maxVerticalSpeedToStartWallRun;
        private float _wallAngleMultiplier=3;

        private Vector3 _wallNormal;

        public WallRunningState(PlayerController controller, float wallRunDuration, float wallRunSpeed,
            float minSpeedToStartWallRun,float maxVerticalSpeedToStartWallRun, float wallGravity, float cameraAngle, float wallAngleMultiplier)
        {
            _controller = controller;
            _wallRunDuration = wallRunDuration;
            _wallRunSpeed = wallRunSpeed;
            _minSpeedToStartWallRun = minSpeedToStartWallRun;
            _maxVerticalSpeedToStartWallRun = maxVerticalSpeedToStartWallRun;
            _wallGravity = wallGravity;
            _cameraAngle = cameraAngle;
            _wallAngleMultiplier = wallAngleMultiplier;
            _wallRunTimer = new CountdownTimer(_wallRunDuration);
        }

        public void OnEnter()
        {
            _controller.OnGroundContactLost();
            _controller.WallDetector.SetWallAngleWithMultiplier(_wallAngleMultiplier);
            _wallRunTimer.Start();
            RotateCamera();
            _wallNormal = _controller.GetWallNormal();
        }

        public void FixedUpdate()
        {
            float wallRotation=SignedAngle(_wallNormal,_controller.GetWallNormal(),_controller.Tr.up);
            Vector3 horizontalCameraDirection = _controller.CameraTrX.forward -
                                                VectorMath.ExtractDotVector(_controller.CameraTrX.forward,
                                                    _controller.Tr.up);
            Vector3 wallRunDirection = Vector3.ProjectOnPlane(horizontalCameraDirection, _controller.GetWallNormal())
                .normalized;
            Vector3 velocity = wallRunDirection * _wallRunSpeed - _controller.GetWallNormal() * _wallGravity;
            
            _controller.CameraController.RotateCameraHorizontal(wallRotation);
            _wallNormal = _controller.GetWallNormal();

            _controller.SetVelocity(velocity);
            _controller.SetMomentum(velocity);
        }
        float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
        {
            float angle = Vector3.Angle(from, to);
    
            Vector3 cross = Vector3.Cross(from, to);
            float sign = Mathf.Sign(Vector3.Dot(axis, cross));
    
            return angle * sign;
        }

        public void OnExit()
        {
            ResetCameraAngle();
            _controller.WallDetector.SetWallAngleWithMultiplier(1);
        }

        private void RotateCamera()
        {
            float sign = Mathf.Sign( Vector3.Dot(_controller.GetWallNormal(), _controller.CameraTrX.right));
            //
            // float targetAngle = sign * _cameraAngle;
            // _controller.CameraViewTr.rotation = Quaternion.AngleAxis(-targetAngle, _controller.CameraTrX.forward)*_controller.CameraViewTr.rotation;
            
            _controller.PlayerEffects.CameraMovingEffects.SetWallRunTilt(-sign);
        }

        private void ResetCameraAngle()
        {
            //_controller.CameraViewTr.localRotation = Quaternion.identity;
            _controller.PlayerEffects.CameraMovingEffects.SetWallRunTilt(0);

        }

        private bool AlignWithInput()
        {
            Vector3 horizontalCameraDirection = _controller.CameraTrX.forward -
                                                VectorMath.ExtractDotVector(_controller.CameraTrX.forward,
                                                    _controller.Tr.up);
            Vector3 wallRunDirection = Vector3.ProjectOnPlane(horizontalCameraDirection, _controller.GetWallNormal())
                .normalized;
            Vector3 inputDirection = _controller.CalculateMovementDirection();
            return Vector3.Dot(wallRunDirection, inputDirection) > 0f;
        }
        
        
        public bool WallRunningToGround() => _controller.IsGrounded();
        public bool WallRunningToFalling() => _wallRunTimer.IsFinished || !_controller.HitSidewaysWall();

        public bool FallingToWallRunning() =>
            !_controller.IsGrounded() &&
            _controller.GetHorizontalMomentum().magnitude > _minSpeedToStartWallRun &&
            _controller.HitSidewaysWall() &&
            _controller.GetVerticalMomentum().magnitude < _maxVerticalSpeedToStartWallRun
            && !_controller.HaveStateInHistory<WallRunningState>(2) && AlignWithInput();

        public bool RisingToWallRunning() => (_controller.IsFalling() || _controller.HitCeiling()) &&
                                             _controller.GetHorizontalMomentum().magnitude > _minSpeedToStartWallRun &&
                                             _controller.HitSidewaysWall()&&AlignWithInput();
    }
}