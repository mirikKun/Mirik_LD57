using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States.AbstractStates;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class RisingState : BaseAirState
    {

        public RisingState(PlayerController controller) : base(controller)
        {
        }

        public override void OnEnter()
        {
            _controller.OnGroundContactLost();
        }

        public override void FixedUpdate()
        {
            Vector3 momentum = _controller.GetMomentum();
            Vector3 verticalMomentum = VectorMath.ExtractDotVector(momentum, _controller.Tr.up);
            Vector3 horizontalMomentum = momentum - verticalMomentum;
            verticalMomentum -= _controller.Tr.up * (_controller.Gravity * Time.fixedDeltaTime);

            horizontalMomentum =
                AdjustHorizontalAirMomentum(horizontalMomentum, _controller.CalculateMovementVelocity());


            float friction = _controller.AirFriction;
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * Time.deltaTime);
            momentum = horizontalMomentum + verticalMomentum;

            _controller.SetMomentum(momentum);
        }

        public bool RisingToGrounded() => _controller.IsGrounded()&&!_controller.IsGroundTooSteep()&&!_controller.IsRising();
        public bool GroundToSliding() =>  _controller.IsGrounded()&&_controller.IsGroundTooSteep();
        public bool RisingToFalling() => _controller.IsFalling()||_controller.HitCeiling();

    }
}