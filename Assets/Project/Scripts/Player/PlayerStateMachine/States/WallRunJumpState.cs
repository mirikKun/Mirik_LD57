using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.StateConfigs;
using Assets.Scripts.Player.PlayerStateMachine.States.AbstractStates;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class WallRunJumpState:IJumpState
    {
        
        protected readonly PlayerController _controller;
        private readonly WallRunStateConfig _wallRunStateConfig;
        
        private bool _jumpKeyIsPressed;

        public WallRunJumpState(PlayerController controller, WallRunStateConfig wallRunStateConfig)
        {
            _controller = controller;
            _wallRunStateConfig = wallRunStateConfig;
       
            _controller.Input.Jump += HandleJumpKeyInput;
        }
        public void Dispose()
        {
            _controller.Input.Jump -= HandleJumpKeyInput;
        }

        private void HandleJumpKeyInput(bool isButtonPressed)
        {
            _jumpKeyIsPressed = isButtonPressed;
        }
        public void OnEnter()
        {
            _controller.OnGroundContactLost();
            OnJumpStart();
        }

        private void OnJumpStart()
        {
            Vector3 wallNormal = _controller.GetWallNormal();
            Vector3 movingDirection = _controller.GetMomentum().normalized;
            Vector3 upDirection = _controller.Tr.up;
            
            Vector3 momentum = wallNormal * _wallRunStateConfig.JumpFromWallPower + movingDirection * _wallRunStateConfig.JumpForwardPower + upDirection * _wallRunStateConfig.JumpUpPower;

            _controller.SetMomentum(momentum);
            _jumpKeyIsPressed= false;        
        }
        public bool WallRunningToWallRunJump() => _jumpKeyIsPressed;
        public bool WallRunJumpToRising() => !_jumpKeyIsPressed;
        public bool WallRunJumpToFalling() => _controller.HitCeiling();

    }
}