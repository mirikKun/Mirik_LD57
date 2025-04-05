using Assets.Scripts.General;
using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using ImprovedTimers;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class GravityChangeState:IState
    {
        private readonly PlayerController _controller;
        private float _raycastDistance = 4.5f;
        private float _changingDuration = 0.5f;
        
        
        private Vector3 _gravityDirection;
        private Vector3 _lastGravityDirection;
        private Quaternion _startRotation;
        private Quaternion _changeRotation;
        
        private RaycastSensor _raycastSensor;
        private  CountdownTimer _gravityChangeTimer;
        private bool _actionKeyIsPressed;
        private float _angleTreashold = 0.01f;


        public GravityChangeState(PlayerController controller,float raycastDistance,float changingDuration)
        {
            _controller = controller;
            _raycastDistance = raycastDistance;
            _changingDuration = changingDuration;
            _gravityChangeTimer = new CountdownTimer(_changingDuration);
            _controller.Input.Action1 += HandleActionInput;

            _raycastSensor = new RaycastSensor(_controller.CameraTrY);
            _raycastSensor.castLength=(_raycastDistance);
            _raycastSensor.SetCastDirection(RaycastSensor.CastDirection.Forward);
        }
        public void Dispose()
        {
            _controller.Input.Action1 -= HandleActionInput;
        }

        private void HandleActionInput(bool isButtonPressed)
        {
            _actionKeyIsPressed = isButtonPressed;
        }

        public void OnEnter()
        {
            _controller.SetMomentum(Vector3.zero);
            _lastGravityDirection = _controller.Tr.up;
            _lastGravityDirection = _raycastSensor.GetNormal();
            _startRotation = _controller.Tr.rotation;
            _changeRotation=Quaternion.FromToRotation(_controller.Tr.up, _raycastSensor.GetNormal());
            _gravityChangeTimer.Start();
            _actionKeyIsPressed = false;

        }

        public void OnExit()
        {
            _controller.Tr.rotation = _changeRotation * _startRotation;
        }

        public void FixedUpdate()
        {
            _controller.Tr.rotation = Quaternion.Lerp(_changeRotation * _startRotation,_startRotation,_gravityChangeTimer.Progress);
        }
        
        public bool GroundedToGravityChange()=>_raycastSensor.CastAndCheck(_controller.CameraTrY.position)&&Vector3.Angle(_raycastSensor.GetNormal(),_controller.Tr.up)>_angleTreashold&&_actionKeyIsPressed;
        public bool GravityChangeToGrounded()=>_gravityChangeTimer.IsFinished&&_controller.IsGrounded();
        public bool GravityChangeToFalling() => _gravityChangeTimer.IsFinished;

    }
}