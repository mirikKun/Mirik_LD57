using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.StateConfigs;
using Assets.Scripts.Player.PlayerStateMachine.States.AbstractStates;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class PounceState : IJumpState
    {
        protected readonly PlayerController _controller;

        private readonly PounceStateConfig _pounceStateConfig;
   
        private bool _jumpKeyIsPressed;


        public PounceState(PlayerController controller, PounceStateConfig pounceStateConfig)
        {
            _controller = controller;
            _pounceStateConfig = pounceStateConfig;
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
            OnPounceStart();
        }

        private void OnPounceStart()
        {
            Vector3 cameraForward = _controller.CameraTrY.transform.forward;
            Vector3 horizontalDirection = (cameraForward - VectorMath.ExtractDotVector(cameraForward, _controller.Tr.up)).normalized;

            float cameraAngle = Vector3.Angle(_controller.Tr.up, cameraForward);
            float clampedAngle = Mathf.Max(90f - cameraAngle, _pounceStateConfig.PounceMinAngle); // Минимум 30 градусов вверх

            Vector3 axis = Vector3.Cross(_controller.Tr.up, horizontalDirection);
            Quaternion jumpRotation = Quaternion.AngleAxis(-clampedAngle, axis);

            Vector3 jumpDirection = jumpRotation * horizontalDirection;
            Vector3 momentum = jumpDirection * _pounceStateConfig.PouncePower;

            _controller.SetMomentum(momentum);
            _jumpKeyIsPressed= false;
        }

        public bool GroundedToPounce() => _jumpKeyIsPressed;
        public bool PounceToRising() => !_jumpKeyIsPressed;
        public bool PounceToFalling() => _controller.HitCeiling();
    }
}