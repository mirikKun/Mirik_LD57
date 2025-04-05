using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States.AbstractStates;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class SlopeSlidingState : IGroundState {
        private readonly PlayerController _controller;

        public SlopeSlidingState(PlayerController controller) {
            this._controller = controller;
        }

        public void OnEnter() {
            _controller.OnGroundContactLost();
        }
        public void FixedUpdate() {
            
            
            Vector3 momentum = _controller.GetMomentum();
            Vector3 verticalMomentum = VectorMath.ExtractDotVector(momentum, _controller.Tr.up);
            Vector3 horizontalMomentum = momentum - verticalMomentum;
            verticalMomentum -= _controller.Tr.up * (_controller.Gravity * Time.fixedDeltaTime);
            horizontalMomentum = GetHorizontalMomentum(horizontalMomentum);
            
            
            float friction = _controller.AirFriction;
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * Time.deltaTime);
            momentum = horizontalMomentum + verticalMomentum;
            
            
            momentum = Vector3.ProjectOnPlane(momentum, _controller.GetGroundNormal());
            if (VectorMath.GetDotProduct(momentum, _controller.Tr.up) > 0f) {
                momentum = VectorMath.RemoveDotVector(momentum, _controller.Tr.up);
            }

            Vector3 slideDirection = Vector3.ProjectOnPlane(-_controller.Tr.up, _controller.GetGroundNormal()).normalized;
            momentum += slideDirection * (_controller.SlideGravity * Time.deltaTime);

            _controller.SetMomentum(momentum);
        }

        private Vector3 GetHorizontalMomentum( Vector3 horizontalMomentum) {
            Vector3 pointDownVector = Vector3.ProjectOnPlane(_controller.GetGroundNormal(), _controller.Tr.up).normalized;
            Vector3 movementVelocity = _controller.CalculateMovementVelocity();
            movementVelocity = VectorMath.RemoveDotVector(movementVelocity, pointDownVector);
            horizontalMomentum += movementVelocity * Time.fixedDeltaTime;
            return horizontalMomentum;
        }
        public bool SlidingToRising() => _controller.IsRising();
        public bool SlidingToFalling() => !_controller.IsGrounded();
        public bool SlidingToGround() => _controller.IsGrounded() && !_controller.IsGroundTooSteep();

    }
}