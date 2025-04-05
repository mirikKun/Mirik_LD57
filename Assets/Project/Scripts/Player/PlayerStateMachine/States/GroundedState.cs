using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States.AbstractStates;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class GroundedState : IGroundState {
        private readonly PlayerController _controller;
        private bool _moving;

        public GroundedState(PlayerController controller) {
            this._controller = controller;
        }

        public void OnEnter() {
            _controller.OnGroundContactRegained();
            _moving = false;
        }
        public void OnExit() {
            _controller.PlayerEffects.CameraMovingEffects.SetGrounded(false);

        }
        public void FixedUpdate() {
            
            Vector3 momentum = _controller.GetMomentum();
            Vector3 verticalMomentum = VectorMath.ExtractDotVector(momentum, _controller.Tr.up);
            Vector3 horizontalMomentum = momentum - verticalMomentum;
            verticalMomentum -= _controller.Tr.up * (_controller.Gravity * Time.fixedDeltaTime);
            
            
            if (VectorMath.GetDotProduct(verticalMomentum, _controller.Tr.up) < 0f)
            {
                verticalMomentum = Vector3.zero;
            }
            float friction = _controller.GroundFriction;
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * Time.fixedDeltaTime);

            Vector3 velocity = _controller.CalculateMovementVelocity();
            horizontalMomentum =velocity.sqrMagnitude>0? velocity:horizontalMomentum;
            
            momentum = horizontalMomentum + verticalMomentum;

            _controller.SetMomentum(momentum);
            //_controller.SetVelocity(velocity);


        }
        public void Update() {
            bool oldMoving = _moving;
            _moving = _controller.CalculateMovementVelocity().sqrMagnitude > 0;
            if(_moving !=oldMoving)
            {
                Debug.Log("Shaking to start");

                _controller.PlayerEffects.CameraMovingEffects.SetGrounded(_moving);
            }
            
        }

        public bool GroundedToRising()=> _controller.IsRising();
        public bool GroundedToSliding() => _controller.IsRising() && _controller.IsGroundTooSteep();
        public bool GroundedToFalling() => !_controller.IsGrounded();
        
    }
}