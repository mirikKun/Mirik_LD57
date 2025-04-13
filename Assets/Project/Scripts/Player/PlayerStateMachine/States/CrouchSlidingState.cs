using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.StateConfigs;
using ImprovedTimers;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class CrouchSlidingState : IState
    {


        private readonly PlayerController _controller;
        private readonly CrouchSlidingStateConfig _crouchSlidingStateConfig;
        private bool _crouchKeyIsPressed;
        private readonly CountdownTimer _slideMaxSpeedTimer;


        private Vector3 _slideDirection;
        private bool _firstTimerFinished;

        public CrouchSlidingState(PlayerController controller,  CrouchSlidingStateConfig crouchSlidingStateConfig)
        {
            _controller = controller;
            _crouchSlidingStateConfig = crouchSlidingStateConfig;
         
            _controller.Input.Crouch += HandleCrouchKeyInput;
            _slideMaxSpeedTimer = new CountdownTimer(crouchSlidingStateConfig.SlideMaxSpeedDuration);
        }
        public void Dispose()
        {
            _controller.Input.Crouch -= HandleCrouchKeyInput;
        }

        private void HandleCrouchKeyInput(bool isButtonPressed)
        {
            _crouchKeyIsPressed = isButtonPressed;
        }

        public void OnEnter()
        {
            _slideMaxSpeedTimer.Start();
            _controller.StartCrouching(_crouchSlidingStateConfig.ColliderHeight);
            Vector3 momentum = _controller.GetMomentum();
            _slideDirection = Vector3.ProjectOnPlane(momentum, _controller.GetGroundNormal()).normalized;
            _controller.SetMomentum(_slideDirection * _crouchSlidingStateConfig.SlideSpeed);
            _firstTimerFinished = false;
        }

        public void OnExit()
        {
            _controller.StopCrouching();
        }


        public void FixedUpdate()
        {
            Vector3 momentum = _controller.GetMomentum();
            Vector3 verticalMomentum = VectorMath.ExtractDotVector(momentum, _controller.Tr.up);
            Vector3 horizontalMomentum = momentum - verticalMomentum;
            verticalMomentum -= _controller.Tr.up * (_controller.Gravity * Time.fixedDeltaTime);
            momentum = horizontalMomentum + verticalMomentum;


            momentum = Vector3.ProjectOnPlane(momentum, _controller.GetGroundNormal());
            if (VectorMath.GetDotProduct(momentum, _controller.Tr.up) > 0f)
            {
                momentum = VectorMath.RemoveDotVector(momentum, _controller.Tr.up);
            }

            Vector3 slopeDirection = Vector3.ProjectOnPlane(-_controller.Tr.up, _controller.GetGroundNormal())
                .normalized;
            float slideOppositeAngle = Vector3.Angle(slopeDirection, momentum.normalized);
            bool onSlope = Vector3.Angle(_controller.Tr.up, _controller.GetGroundNormal()) > 0.1f;
            if (onSlope && slideOppositeAngle < _crouchSlidingStateConfig.MinSlideAngle)
            {
                if(_firstTimerFinished||_slideMaxSpeedTimer.IsFinished)
                {
                    _firstTimerFinished = true;
                    _slideMaxSpeedTimer.Start();
                    momentum = Vector3.MoveTowards(momentum, slopeDirection * _crouchSlidingStateConfig.SlideSpeed,
                        _crouchSlidingStateConfig.SlidingFriction * Time.fixedDeltaTime);
                }
            }
            else
             if (_slideMaxSpeedTimer.IsFinished)
             {
                 _firstTimerFinished = true;

                momentum = Vector3.MoveTowards(momentum, Vector3.zero, _crouchSlidingStateConfig.SlidingFriction * Time.fixedDeltaTime);
            }
            
            _controller.SetMomentum(momentum);
        }

        public bool CrouchSlidingToGround() => (!_crouchKeyIsPressed&&_firstTimerFinished)|| (_controller.GetMomentum().magnitude < _crouchSlidingStateConfig.MinSlideSpeed && !_crouchKeyIsPressed) ;

        public bool CrouchSlidingToFalling() => !_controller.IsGrounded()&&_controller.IsFalling();
        public bool GroundedToCrouchSliding() => _crouchKeyIsPressed;
    }
}