using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class WallClingingState:IState
    {
        private readonly PlayerController _controller;
        private bool _jumpKeyIsPressed;

        public WallClingingState(PlayerController controller)
        {
            _controller = controller;
        
            _controller.Input.Jump += HandleJumpKeyInput;
        }
        public void Dispose()
        {
            _controller.Input.Jump -= HandleJumpKeyInput;
        }

        public void OnEnter()
        {
            _controller.OnGroundContactLost();
            _controller.SetMomentum(Vector3.zero);
            _controller.SetVelocity(Vector3.zero);
            _jumpKeyIsPressed= false;

        }

        public void OnExit()
        {
            _jumpKeyIsPressed= false;
        }

        private void HandleJumpKeyInput(bool isButtonPressed)
        {
            _jumpKeyIsPressed = isButtonPressed;
        }
        public bool FallingToClinging() => _jumpKeyIsPressed&&_controller.HitWallToTheFront();
        public bool RisingToClinging() => _jumpKeyIsPressed&&_controller.HitWallToTheFront();
        public bool ClingingToPounce() => _jumpKeyIsPressed&&!_controller.HitWallToTheFront();
        public bool ClingingToFalling() => _jumpKeyIsPressed;
    }
}