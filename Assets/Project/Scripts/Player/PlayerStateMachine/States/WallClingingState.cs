using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.StateConfigs;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class WallClingingState:IState
    {
        private readonly WallClingingConfig _wallClingingConfig;
        private readonly PlayerController _controller;
        private bool _jumpKeyIsPressed;

        public WallClingingState(PlayerController controller,WallClingingConfig wallClingingConfig)
        {
            
            _controller = controller;
            _wallClingingConfig = wallClingingConfig;
        
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