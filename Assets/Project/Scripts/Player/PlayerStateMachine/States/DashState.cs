using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.StateConfigs;
using Assets.Scripts.Player.PlayerStateMachine.States.AbstractStates;
using ImprovedTimers;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class DashState:IState
    {
        private readonly PlayerController _controller;
        private readonly DashStateConfig _dashStateConfig;
 


        private Vector3 _dashDirection;
        private readonly CountdownTimer _dashTimer;
        private bool _jumpKeyIsPressed;


        public DashState(PlayerController controller, DashStateConfig dashStateConfig)
        {
            _controller = controller;
            _dashStateConfig = dashStateConfig;
     
            _dashTimer = new CountdownTimer(dashStateConfig.DashDuration);
            _controller.Input.Dash += HandleKeyInput;


        }
        public void Dispose()
        {
            _controller.Input.Dash -= HandleKeyInput;
        }

        private void HandleKeyInput(bool isButtonPressed)
        {
            _jumpKeyIsPressed = isButtonPressed;
        }

        public void OnEnter()
        {
            _controller.OnGroundContactLost();
            
            _dashDirection = 
                _controller.CalculateMovementDirection().magnitude>0?
                Vector3.ProjectOnPlane(_controller.CalculateMovementDirection(), _controller.CameraTrY.up).normalized:
                _controller.CameraTrY.forward;
            _controller.SetMomentum(Vector3.zero);
            _dashTimer.Start();
            _jumpKeyIsPressed = false;
            
            _controller.PlayerEffects.CameraMovingEffects.SetFOV(_dashStateConfig.UpdatedFov);
        }

        public void OnExit()
        {
            _controller.SetMomentum(_dashDirection * _dashStateConfig.DashExitSpeed);
            _controller.PlayerEffects.CameraMovingEffects.ResetFOV();
        }

        public void FixedUpdate()
        {
            _controller.SetMomentum(_dashDirection * _dashStateConfig.DashSpeed);
            
            
        }

        public bool GroundToDash() => _jumpKeyIsPressed;

        public bool AirToToDash() => _jumpKeyIsPressed&&_controller.HaveStateBeforeStateInHistory<IGroundState,DashState>();
        public bool DashToRising() => (_dashTimer.IsFinished )&&_controller.IsRising();
        public bool DashToFalling() => (_dashTimer.IsFinished || _controller.HitCeiling());

        public bool WallClingingToDash() => _jumpKeyIsPressed;
    }
}