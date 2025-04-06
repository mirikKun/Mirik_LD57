using Assets.Scripts.General;
using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States.AbstractStates;
using ImprovedTimers;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class GravityChangeState:ISpendableState
    {
        private readonly PlayerController _controller;
        private float _raycastDistance = 4.5f;
        private float _changingDuration = 0.5f;
        private float _gravityChangeFullDuration = 5f;
        
        
        private Vector3 _gravityDirection;
        private Vector3 _lastGravityDirection;
        private Quaternion _startRotation;
        private Quaternion _changeRotation;
        
        private RaycastSensor _raycastSensor;
        private  CountdownTimer _gravityChangingTimer;
        private  CountdownTimer _gravityFullChangeTimer;
        private bool _actionKeyIsPressed;
        private float _angleTreashold = 0.01f;
        private bool _wrongGravity;


        public GravityChangeState(PlayerController controller,float raycastDistance,float changingDuration,float gravityChangeFullDuration)
        {
            _controller = controller;
            _raycastDistance = raycastDistance;
            _changingDuration = changingDuration;

            _gravityChangeFullDuration = gravityChangeFullDuration;
            _gravityChangingTimer = new CountdownTimer(_changingDuration);
            _gravityFullChangeTimer = new CountdownTimer(_gravityChangeFullDuration);
            _controller.Input.Action3 += HandleActionInput;

            _raycastSensor = new RaycastSensor(_controller.CameraTrY);
            _raycastSensor.castLength=(_raycastDistance);
            _raycastSensor.SetCastDirection(RaycastSensor.CastDirection.Forward);
        }
        public void Dispose()
        {
            _controller.Input.Action3 -= HandleActionInput;
        }

        private void HandleActionInput(bool isButtonPressed)
        {
            _actionKeyIsPressed = isButtonPressed;
        }

        public void OnEnter()
        {
       

            

            if (!_wrongGravity|| (_actionKeyIsPressed && HaveAbility))
            {
                _gravityFullChangeTimer.Start();
                _wrongGravity = true;
                _controller.PlayerInventory.TempSpendAbility(this.GetType());
                
                
                _controller.SetMomentum(Vector3.zero);
                _lastGravityDirection = _controller.Tr.up;
                _lastGravityDirection = _raycastSensor.GetNormal();
                _startRotation = _controller.Tr.rotation;
                _changeRotation=Quaternion.FromToRotation(_controller.Tr.up, _raycastSensor.GetNormal());
                _gravityChangingTimer.Start();
                _actionKeyIsPressed = false;
                
            }
            else
            {
                _wrongGravity = false;
                _controller.SetMomentum(Vector3.zero);
                _lastGravityDirection = _controller.Tr.up;
                _lastGravityDirection = Vector3.up;
                _startRotation = _controller.Tr.rotation;
                _changeRotation=Quaternion.FromToRotation(_controller.Tr.up, _lastGravityDirection);
                _gravityChangingTimer.Start();
            }
            
        }

        public void OnExit()
        {
            _controller.Tr.rotation = _changeRotation * _startRotation;
        }

        public void FixedUpdate()
        {
            _controller.Tr.rotation = Quaternion.Lerp(_changeRotation * _startRotation,_startRotation,_gravityChangingTimer.Progress);
        }
        private bool HaveAbility => _controller.PlayerInventory.HaveAbility(this.GetType());

        public bool GroundedToGravityChange()=>_raycastSensor.CastAndCheck(_controller.CameraTrY.position)&&Vector3.Angle(_raycastSensor.GetNormal(),_controller.Tr.up)>_angleTreashold&&_actionKeyIsPressed&&HaveAbility;

        public bool GravityChangeDurationEnded() => _wrongGravity && _gravityFullChangeTimer.IsFinished;
        public bool GravityChangeToGrounded()=>_gravityChangingTimer.IsFinished&&_controller.IsGrounded();
        public bool GravityChangeToFalling() => _gravityChangingTimer.IsFinished;

    }
}