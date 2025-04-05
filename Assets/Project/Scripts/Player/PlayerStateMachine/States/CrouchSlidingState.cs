using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using ImprovedTimers;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class CrouchSlidingState : IState
    {
        private float _slideSpeed;
        private float _slideMaxSpeedDuration;
        private float _minSlideSpeed;
        private float _minSlideAngle;
        private float _colliderHeight;
        private float _slidingFriction;

        private readonly PlayerController _controller;
        private bool _crouchKeyIsPressed;
        private readonly CountdownTimer _slideMaxSpeedTimer;


        private Vector3 _slideDirection;
        private bool _firstTimerFinished;

        public CrouchSlidingState(PlayerController controller,  float slideSpeed,float slideMaxSpeedDuration,
            float minSlideSpeed, float minSlideAngle, float colliderHeight, float slidingFriction)
        {
            _controller = controller;
            _slideSpeed = slideSpeed;
            _slideMaxSpeedDuration = slideMaxSpeedDuration;
            _minSlideSpeed = minSlideSpeed;
            _minSlideAngle = minSlideAngle;
            _colliderHeight = colliderHeight;
            _slidingFriction = slidingFriction;
            _controller.Input.Crouch += HandleCrouchKeyInput;
            _slideMaxSpeedTimer = new CountdownTimer(slideMaxSpeedDuration);
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
            _controller.StartCrouching(_colliderHeight);
            Vector3 momentum = _controller.GetMomentum();
            _slideDirection = Vector3.ProjectOnPlane(momentum, _controller.GetGroundNormal()).normalized;
            _controller.SetMomentum(_slideDirection * _slideSpeed);
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
            if (onSlope && slideOppositeAngle < _minSlideAngle)
            {
                if(_firstTimerFinished||_slideMaxSpeedTimer.IsFinished)
                {
                    _firstTimerFinished = true;
                    _slideMaxSpeedTimer.Start();
                    momentum = Vector3.MoveTowards(momentum, slopeDirection * _slideSpeed,
                        _slidingFriction * Time.fixedDeltaTime);
                }
            }
            else
             if (_slideMaxSpeedTimer.IsFinished)
             {
                 _firstTimerFinished = true;

                momentum = Vector3.MoveTowards(momentum, Vector3.zero, _slidingFriction * Time.fixedDeltaTime);
            }
            
            _controller.SetMomentum(momentum);
        }

        public bool CrouchSlidingToGround() => (!_crouchKeyIsPressed&&_firstTimerFinished)|| (_controller.GetMomentum().magnitude < _minSlideSpeed && !_crouchKeyIsPressed) ;

        public bool CrouchSlidingToFalling() => !_controller.IsGrounded()&&_controller.IsFalling();
        public bool GroundedToCrouchSliding() => _crouchKeyIsPressed;
    }
}