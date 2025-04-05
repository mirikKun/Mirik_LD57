using Assets.Scripts.Player.Controller;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States.AbstractStates
{
    public abstract class BaseAirState: IJumpState
    {
        protected readonly PlayerController _controller;

        public BaseAirState(PlayerController controller) {
            this._controller = controller;
        }

        public virtual void FixedUpdate(){}
        public virtual void OnEnter() { }

        protected Vector3 AdjustHorizontalAirMomentum( Vector3 horizontalMomentum, Vector3 movementVelocity)
        {
            if (horizontalMomentum.magnitude > _controller.MovementSpeed)
            {
                if (VectorMath.GetDotProduct(movementVelocity, horizontalMomentum.normalized) > 0f)
                {
                    movementVelocity = VectorMath.RemoveDotVector(movementVelocity, horizontalMomentum.normalized);
                }

                horizontalMomentum += movementVelocity * (Time.deltaTime * _controller.AirControlRate * 0.25f);
            }
            else
            {
                horizontalMomentum += movementVelocity * (Time.deltaTime * _controller.AirControlRate);
                horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, _controller.MovementSpeed);
            }

            return horizontalMomentum;
        }
    }
}