using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.StateConfigs;
using ImprovedTimers;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class WallRunningState : IState
    {
        private readonly PlayerController _controller;
        private readonly WallRunStateConfig _wallRunStateConfig;


        private readonly CountdownTimer _wallRunTimer;

        private Vector3 _wallNormal;

        public WallRunningState(PlayerController controller, WallRunStateConfig wallRunStateConfig)
        {
            _controller = controller;
            _wallRunStateConfig = wallRunStateConfig;
         
            _wallRunTimer = new CountdownTimer(_wallRunStateConfig.WallRunDuration);
        }

        public void OnEnter()
        {
            _controller.OnGroundContactLost();
            _controller.WallDetector.SetWallAngleWithMultiplier(_wallRunStateConfig.WallAngleMultiplier);
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
            Vector3 velocity = wallRunDirection * _wallRunStateConfig.WallRunSpeed - _controller.GetWallNormal() * _wallRunStateConfig.WallGravity;
            
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
            Vector3 inputDirection = (_controller.CalculateMovementDirection() - _controller.GetWallNormal()).normalized;
            return Vector3.Dot(wallRunDirection, inputDirection) > 0.6f;
        }
        
        
        public bool WallRunningToGround() => _controller.IsGrounded();
        public bool WallRunningToFalling() => _wallRunTimer.IsFinished || !_controller.HitSidewaysWall();

        public bool FallingToWallRunning() =>
            !_controller.IsGrounded() &&
            _controller.GetHorizontalMomentum().magnitude > _wallRunStateConfig.MinSpeedToStartWallRun &&
            _controller.HitSidewaysWall() &&
            _controller.GetVerticalMomentum().magnitude < _wallRunStateConfig.MaxVerticalSpeedToStartWallRun
            && !_controller.HaveStateInHistory<WallRunningState>(2) && AlignWithInput();

        public bool RisingToWallRunning() => (_controller.IsFalling() || _controller.HitCeiling()) &&
                                             _controller.GetHorizontalMomentum().magnitude > _wallRunStateConfig.MinSpeedToStartWallRun &&
                                             _controller.HitSidewaysWall()&&AlignWithInput();
    }
}