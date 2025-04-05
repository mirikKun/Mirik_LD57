using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class ClimbingOverState:IState
    {
        private readonly PlayerController _controller;
        private readonly float _climbingSpeed =7f;
        private readonly float _horizontalSpeedReduction =0.3f;

        public ClimbingOverState(PlayerController controller,float climbingSpeed,float horizontalSpeedReduction) {
            this._controller = controller;
            _climbingSpeed = climbingSpeed;
            _horizontalSpeedReduction = horizontalSpeedReduction;
        }

        public  void OnEnter()
        {
            _controller.OnGroundContactLost();
            _controller.SetMomentum(Vector3.zero);
        }
        public  void FixedUpdate()
        {
            Vector3 velocity = _controller.CalculateMovementVelocity()*_horizontalSpeedReduction;
            Vector3 verticalVelocity = VectorMath.ExtractDotVector(velocity, _controller.Tr.up);
            Vector3 horizontalMomentum = velocity - verticalVelocity;
            verticalVelocity += _controller.Tr.up * _climbingSpeed;

            velocity = horizontalMomentum + verticalVelocity;
            _controller.SetVelocity(velocity);
        }

        public void OnExit()
        {
            // Vector3 momentum = _controller.GetMomentum();
            //
            //
            // momentum += _controller.Tr.up * _climbingSpeed;
            //
            // _controller.SetMomentum(momentum);
        }

        private bool SameDirection()
        {
            Vector3 wallNormal = -_controller.GetWallNormal();
            Vector3 inputVelocity = _controller.CalculateMovementVelocity().normalized;
            return Vector3.Dot(wallNormal, inputVelocity) > 0;
        }
        
        public bool FallingToClimbingOver() => _controller.HitForwardBottomWall()&&SameDirection();
        public bool RisingToClimbingOver() => _controller.IsFalling() && _controller.HitForwardBottomWall()&&SameDirection();
        public bool GroundedToClimbingOver() => _controller.HitForwardBottomWall()&&SameDirection();
        public bool ClimbingOverToRising() => !_controller.HitForwardBottomWall() || !SameDirection();
        public bool ClimbingOverToFalling() => _controller.HitCeiling();

    }
}