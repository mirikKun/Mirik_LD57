using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.StateConfigs;
using Assets.Scripts.Player.PlayerStateMachine.States.AbstractStates;
using ImprovedTimers;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States {
    public class JumpingState : BaseAirState {
 
        private readonly CountdownTimer _jumpTimer;
        private readonly JumpStateConfig _jumpStateConfig;

        private bool _jumpKeyIsPressed;
        private bool _jumpInputIsLocked;

        public JumpingState(PlayerController controller, JumpStateConfig jumpStateConfig) : base(controller)
        {
            _jumpStateConfig = jumpStateConfig;
            _jumpTimer = new CountdownTimer(jumpStateConfig.JumpDuration);
            _controller.Input.Jump += HandleJumpKeyInput;
        }
        public void Dispose()
        {
            _controller.Input.Jump -= HandleJumpKeyInput;
        }
        public override void OnEnter() {
            _controller.OnGroundContactLost();
            OnJumpStart();
        }

        public override void OnExit()
        {
            _jumpTimer.Stop();
        }

        public override void FixedUpdate() {
            Vector3 momentum = _controller.GetMomentum();

            Vector3 horizontalMomentum = momentum -VectorMath.ExtractDotVector(momentum, _controller.Tr.up);
            horizontalMomentum = AdjustHorizontalAirMomentum( horizontalMomentum, _controller.CalculateMovementVelocity());

            
            float friction = _controller.AirFriction;
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * Time.fixedDeltaTime);
            momentum = horizontalMomentum;
            
            momentum = VectorMath.RemoveDotVector(momentum, _controller.Tr.up);
            momentum += _controller.Tr.up * _jumpStateConfig.JumpSpeed;
            
            _controller.SetMomentum(momentum);
        }
        private void HandleJumpKeyInput(bool isButtonPressed)
        {
            if (_jumpKeyIsPressed && !isButtonPressed)
            {
                _jumpInputIsLocked = false;
            }

            _jumpKeyIsPressed = isButtonPressed;
        }

        public void OnJumpStart()
        {
            Vector3 momentum = _controller.GetMomentum();


            momentum += _controller.Tr.up * _jumpStateConfig.JumpSpeed;
            _jumpTimer.Start();
            _jumpInputIsLocked = true;

            _controller.SetMomentum(momentum);
        }
        
        public bool GroundedToJumping()=>(_jumpKeyIsPressed ) && !_jumpInputIsLocked;
        public bool JumpingToRising() => _jumpTimer.IsFinished || !_jumpKeyIsPressed;
        public bool JumpingToFalling() => _controller.HitCeiling();

    }
}
