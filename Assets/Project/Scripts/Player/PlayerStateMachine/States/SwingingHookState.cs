using Assets.Scripts.General;
using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.StateConfigs;
using Assets.Scripts.Player.PlayerStateMachine.States.AbstractStates;
using ImprovedTimers;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class SwingingHookState : ISpendableState
    {
        private readonly SwingingHookStateConfig _swingingHookStateConfig;
        private PlayerController _controller;
        private RaycastSensor _raycastSensor;
        private CountdownTimer _hookTimer;
        private CountdownTimer _preparingTimer;
        private Vector3 _swingingPoint;

        private float _distance;
        private bool _actionKeyIsPressed;
        private bool _preparingStarted;
        private bool _preparingFinished;

        private RaycastSensor RaycastSensor
        {
            get
            {
                if (_raycastSensor == null || _raycastSensor.HaveNull)
                {
                    _raycastSensor = new RaycastSensor(_controller.CameraTrY);
                    _raycastSensor.castLength = (_swingingHookStateConfig.SwingingMaxDistance);
                    _raycastSensor.SetCastDirection(RaycastSensor.CastDirection.Forward);
                }

                return _raycastSensor;
            }
        }


        public SwingingHookState(PlayerController controller, SwingingHookStateConfig swingingHookStateConfig)
        {
            _controller = controller;
            _swingingHookStateConfig = swingingHookStateConfig;
       

            _controller.Input.Action1 += HandleActionInput;

            _raycastSensor = new RaycastSensor(_controller.CameraTrY);
            _raycastSensor.castLength = (_swingingHookStateConfig.SwingingMaxDistance);
            _raycastSensor.SetCastDirection(RaycastSensor.CastDirection.Forward);


            _hookTimer = new CountdownTimer(_swingingHookStateConfig.SwingingDuration);
            _preparingTimer = new CountdownTimer(_swingingHookStateConfig.PreparingDuration);
        }

        public void Dispose()
        {
            _controller.Input.Action1 -= HandleActionInput;
        }

        private void HandleActionInput(bool isButtonPressed)
        {
            if (!HaveAbility && isButtonPressed)
            {
                return;
            }

            _actionKeyIsPressed = isButtonPressed;
            if (!_preparingStarted && _actionKeyIsPressed&&CanGrapple)
            {
                _swingingPoint = RaycastSensor.GetPosition();

                _preparingTimer.Start();
                _controller.PlayerEffects.HookEffects.StartLineDrawing(_swingingPoint, _preparingTimer);
                _preparingStarted = true;
            }

            if (_preparingStarted && !_actionKeyIsPressed)
            {
                _preparingTimer.Stop();
                _controller.PlayerEffects.HookEffects.ClearGrappleLine();
                _preparingStarted = false;
            }
        }

        public void OnEnter()
        {
            _controller.OnGroundContactLost();
            OnSwingingHookStart();

            _controller.PlayerInventory.TempSpendAbility(this.GetType());
        }

        public void OnExit()
        {
            _actionKeyIsPressed = false;
            float momentumMagnitude = _controller.GetMomentum().magnitude;

            _controller.SetMomentum(_controller.GetMomentum() * _swingingHookStateConfig.SwingingExitSpeedMultiplier);
            _preparingTimer.Stop();
            _controller.PlayerEffects.HookEffects.ClearGrappleLine();
        }

        private void OnSwingingHookStart()
        {
            _hookTimer.Start();


            Vector3 momentum = _controller.GetMomentum();
            Vector3 hookDirection = _swingingPoint - _controller.Tr.position;
            momentum = VectorMath.RemoveDotVector(momentum, -hookDirection);
            //momentum=Vector3.zero;
            momentum += hookDirection.normalized * _swingingHookStateConfig.StartSwingMomentum;
            momentum = Vector3.ClampMagnitude(momentum, _swingingHookStateConfig.MaxSwingingSpeed);
            _controller.SetMomentum(momentum);
        }

        public void Update()
        {
            _controller.PlayerEffects.HookEffects.DrawGrappleLine(_raycastSensor.GetPosition(), 1);
        }

        public void FixedUpdate()
        {
            Vector3 momentum = _controller.GetMomentum();
            float friction = _controller.AirFriction;
            momentum = Vector3.MoveTowards(momentum, Vector3.zero, friction * Time.fixedDeltaTime);
            Vector3 hookDirection = _swingingPoint - _controller.Tr.position;
            _distance = hookDirection.magnitude;

            Vector3 swingingDirectionOrbital =
                -GetPerpendicularInPlane(hookDirection, -_controller.CameraTrY.forward).normalized;
            Vector3 swingingDirectionFroward =
                VectorMath.RemoveDotVector(_controller.CameraTrY.forward, -hookDirection.normalized);
            float aligning =
                _swingingHookStateConfig.SwingingDirectionLerpCurve.Evaluate(VectorMath.GetDotProduct(_controller.CameraTrY.forward,
                    hookDirection));

            Vector3 swingingDirection = Vector3.Lerp(swingingDirectionFroward, swingingDirectionOrbital, aligning);
            float additionalSpeed = Mathf.Lerp(_swingingHookStateConfig.GrapplingSpeed, _swingingHookStateConfig.SwingingSpeed, aligning);
            momentum = AdjustMaxMomentum(momentum, swingingDirection.normalized, additionalSpeed);
            momentum = VectorMath.RemoveDotVector(momentum, -hookDirection.normalized);
            _controller.SetMomentum(momentum);
        }

        Vector3 GetPerpendicularInPlane(Vector3 a, Vector3 b)
        {
            Vector3 normal = Vector3.Cross(a, b);
            Vector3 vector = Vector3.Cross(normal, a).normalized;

            if (Vector3.Dot(vector, b) < 0)
            {
                vector = -vector;
            }

            return vector;
        }

        private Vector3 AdjustMaxMomentum(Vector3 momentum, Vector3 addingMomentum, float speed)
        {
            if (momentum.magnitude > _swingingHookStateConfig.MaxSwingingSpeed)
            {
                if (VectorMath.GetDotProduct(addingMomentum, momentum.normalized) > 0f)
                {
                    addingMomentum = VectorMath.RemoveDotVector(addingMomentum, momentum.normalized).normalized;
                }

                momentum += speed * (Time.fixedDeltaTime) * addingMomentum;
            }
            else
            {
                momentum += speed * (Time.fixedDeltaTime) * addingMomentum;
            }

            momentum = Vector3.ClampMagnitude(momentum, _swingingHookStateConfig.MaxSwingingSpeed);

            return momentum;
        }

        private bool CanGrapple =>
            RaycastSensor.CastAndCheck(_controller.CameraTrY.position) &&
            RaycastSensor.GetDistance() > _swingingHookStateConfig.SwingingMinDistance;

        private bool HaveAbility => _controller.PlayerInventory.HaveAbility(this.GetType());

        public bool GroundedToSwingingHook() =>
            _preparingTimer.IsFinished && _preparingStarted && _actionKeyIsPressed && HaveAbility;

        public bool AirToSwingingHook() => _preparingTimer.IsFinished && _preparingStarted && _actionKeyIsPressed &&
                                           !_controller
                                               .HaveStateBeforeStateInHistory<GrapplingHookState, IGroundState>() &&
                                           HaveAbility;

        public bool SwingingHookToRising() =>
            (!_actionKeyIsPressed || _hookTimer.IsFinished || _distance <= _swingingHookStateConfig.SwingingMinDistance) &&
            _controller.IsRising();

        public bool SwingingHookToFalling() =>
            (!_actionKeyIsPressed || _hookTimer.IsFinished || _distance <= _swingingHookStateConfig.SwingingMinDistance) &&
            _controller.IsFalling();
    }
}