using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States.AbstractStates;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class CrouchSlidingJumpState: IJumpState
    {
        private readonly PlayerController _controller;

        private readonly float _jumpPower;
        private readonly float _minJumpAngle;
        private bool _jumpKeyIsPressed;



        public CrouchSlidingJumpState(PlayerController controller, float jumpPower, float minJumpAngle)
        {
            _controller = controller;
            _jumpPower = jumpPower;
            _minJumpAngle = minJumpAngle;
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
            Vector3 moveDirection = _controller.CalculateMovementVelocity();
            Vector3 cameraForward = _controller.CameraTrY.transform.forward;
            
            Vector3 direction =moveDirection.sqrMagnitude>0?moveDirection.normalized:cameraForward;
            Vector3 horizontalDirection = (direction - VectorMath.ExtractDotVector(direction, _controller.Tr.up)).normalized;


            Vector3 axis = Vector3.Cross(_controller.Tr.up, horizontalDirection);
            Quaternion jumpRotation = Quaternion.AngleAxis(-_minJumpAngle, axis);

            Vector3 jumpDirection = jumpRotation * horizontalDirection;
            Vector3 newMomentum = jumpDirection * _jumpPower;

            _controller.SetMomentum(newMomentum);
            _jumpKeyIsPressed= false;
        }

        public bool SlidingToJump() => _jumpKeyIsPressed;
        public bool JumpToRising() => !_jumpKeyIsPressed;
        public bool JumpToFalling() => _controller.HitCeiling();
    }
}