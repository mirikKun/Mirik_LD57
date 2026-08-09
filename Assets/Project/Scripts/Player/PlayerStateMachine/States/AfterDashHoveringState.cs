using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.StateConfigs;
using ImprovedTimers;
using Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class AfterDashHoveringState : IState
    {
        private readonly PlayerController _controller;
        private readonly DashStateConfig _config;
        private readonly CountdownTimer _hoveringTimer;

        public AfterDashHoveringState(PlayerController controller, DashStateConfig config)
        {
            _controller = controller;
            _config = config;
            _hoveringTimer = new CountdownTimer(_config.AfterDashHoveringDuration);
        }

        public void OnEnter()
        {
            _hoveringTimer.Start();
        }

        public void FixedUpdate()
        {
            Vector3 momentum = _controller.GetMomentum();
            Vector3 verticalMomentum = VectorMath.ExtractDotVector(momentum, _controller.Tr.up);
            Vector3 horizontalMomentum = momentum - verticalMomentum;

            verticalMomentum -= _controller.Tr.up * (_config.AfterDashHoveringGravity * Time.fixedDeltaTime);

            horizontalMomentum += _controller.CalculateMovementDirection() *
                                 (_config.AfterDashHoveringSpeed * Time.fixedDeltaTime);
            horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, _controller.MovementSpeed);

            momentum = horizontalMomentum + verticalMomentum;
            _controller.SetMomentum(momentum);
        }

        public bool HoveringToRising() => _hoveringTimer.IsFinished && _controller.IsRising();
        public bool HoveringToFalling() => _hoveringTimer.IsFinished || _controller.HitCeiling();
        public bool HoveringToGrounded() => _controller.IsGrounded();
    }
}
