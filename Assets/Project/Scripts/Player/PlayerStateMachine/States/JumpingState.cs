using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States.AbstractStates;
using ImprovedTimers;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States {
    public class JumpingState : BaseAirState {
        private readonly float _jumpDuration;
        private readonly float _jumpSpeed;
        private readonly CountdownTimer _jumpTimer;


        private bool _jumpKeyIsPressed; // Tracks whether the jump key is currently being held down by the player
        private bool _jumpKeyWasPressed; // Indicates if the jump key was pressed since the last reset, used to detect jump initiation
        private bool _jumpKeyWasLetGo; // Indicates if the jump key was released since it was last pressed, used to detect when to stop jumping
        private bool _jumpInputIsLocked; // Prevents jump initiation when true, used to ensure only one jump action per press
        
        public JumpingState(PlayerController controller, float jumpDuration, float jumpSpeed) : base(controller)
        {
            _jumpDuration = jumpDuration;
            _jumpSpeed = jumpSpeed;
            _jumpTimer = new CountdownTimer(jumpDuration);
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

        public void OnExit()
        {
            _jumpInputIsLocked = false;
            ResetJumpKeys();
        }

        public override void FixedUpdate() {
            
            
            Vector3 momentum = _controller.GetMomentum();
            Vector3 horizontalMomentum = momentum -VectorMath.ExtractDotVector(momentum, _controller.Tr.up);
            horizontalMomentum = AdjustHorizontalAirMomentum( horizontalMomentum, _controller.CalculateMovementVelocity());

            
            float friction = _controller.AirFriction;
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * Time.fixedDeltaTime);
            momentum = horizontalMomentum;
            
            momentum = VectorMath.RemoveDotVector(momentum, _controller.Tr.up);
            momentum += _controller.Tr.up * _jumpSpeed;
            
            _controller.SetMomentum(momentum);

            ResetJumpKeys();
        }
        private void HandleJumpKeyInput(bool isButtonPressed)
        {
            if (!_jumpKeyIsPressed && isButtonPressed)
            {
                _jumpKeyWasPressed = true;
            }

            if (_jumpKeyIsPressed && !isButtonPressed)
            {
                _jumpKeyWasLetGo = true;
                _jumpInputIsLocked = false;
            }

            _jumpKeyIsPressed = isButtonPressed;
        }
        private void ResetJumpKeys()
        {
            _jumpKeyWasLetGo = false;
            _jumpKeyWasPressed = false;
        }

        public void OnJumpStart()
        {
            Vector3 momentum = _controller.GetMomentum();


            momentum += _controller.Tr.up * _jumpSpeed;
            _jumpTimer.Start();
            _jumpInputIsLocked = true;

            _controller.SetMomentum(momentum);
        }
        
        public bool GroundedToJumping()=>(_jumpKeyIsPressed ) && !_jumpInputIsLocked;
        public bool JumpingToRising() => _jumpTimer.IsFinished || _jumpKeyWasLetGo;
        public bool JumpingToFalling() => _controller.HitCeiling();

    }
}