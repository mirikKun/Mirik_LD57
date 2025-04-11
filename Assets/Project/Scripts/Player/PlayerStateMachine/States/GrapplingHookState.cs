using Assets.Scripts.General;
using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States.AbstractStates;
using ImprovedTimers;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.States
{
    public class GrapplingHookState:ISpendableState
    {
        
        private float _grappleSpeed = 10f;
        private float _grappleMaxDistance = 100f;
        private float _grappleMaxApproachableDistance = 50f;
        private float _grappleMinDistance = 5f;
        private float _grapplingExitSpeedMultiplier= 0.5f;
        
        private bool _adaptiveGrapple = true;
        private float _adaptiveGrappleOffset = 0.5f;
        private float _adaptiveGrappleLetGoDistance = 3f;
        
        private readonly PlayerController _controller;
        private RaycastSensor _raycastSensor;
        private  CountdownTimer _grapplingTimer;

        
        
        private Vector3 _grapplePoint;
        private Vector3 _grappleDirection;
        private bool _actionKeyIsPressed;


        public GrapplingHookState(PlayerController controller, float grappleSpeed, float grappleMaxDistance,float grappleMaxApproachableDistance, float grappleMinDistance,float grapplingExitSpeedMultiplier, bool adaptiveGrapple, float adaptiveGrappleOffset,float adaptiveGrappleLetGoDistance)
        {
            _controller = controller;
            _grappleSpeed = grappleSpeed;
            _grappleMaxDistance = grappleMaxDistance;
    
            _grappleMaxApproachableDistance = grappleMaxApproachableDistance;
            _grappleMinDistance = grappleMinDistance;
            _grapplingExitSpeedMultiplier = grapplingExitSpeedMultiplier;
            
            
            _adaptiveGrapple = adaptiveGrapple;
            _adaptiveGrappleOffset = adaptiveGrappleOffset;
            _adaptiveGrappleLetGoDistance = adaptiveGrappleLetGoDistance;

            _controller.Input.Action2 += HandleActionInput;

            
            _raycastSensor = new RaycastSensor(_controller.CameraTrY);
            _raycastSensor.castLength=(_grappleMaxDistance);
            _raycastSensor.SetCastDirection(RaycastSensor.CastDirection.Forward);
        }
        public void Dispose()
        {
            _controller.Input.Action2 -= HandleActionInput;
        }

        private void HandleActionInput(bool isButtonPressed)
        {
            _actionKeyIsPressed = isButtonPressed;
        }

        public  void OnEnter()
        {

            _controller.PlayerInventory.TempSpendAbility(this.GetType());
            _controller.OnGroundContactLost();
            OnGrapplingHookStart();
        }

        public void OnExit()
        {
            _controller.SetMomentum(_controller.GetMomentum()*_grapplingExitSpeedMultiplier);
            _controller.PlayerEffects.HookEffects.ClearGrappleLine();

        }

        private void OnGrapplingHookStart()
        {
            _grappleDirection = _controller.CameraTrY.forward;
            _grapplePoint = _raycastSensor.GetPosition();
            float distance = Vector3.Distance(_controller.Tr.position, _grapplePoint);

            float grappleDuration = (Mathf.Clamp(distance,_grappleMinDistance, _grappleMaxApproachableDistance )-_adaptiveGrappleLetGoDistance) / _grappleSpeed;
            _grapplingTimer = new CountdownTimer(grappleDuration);
            _grapplingTimer.Start();

            Vector3 momentum=_grappleDirection*_grappleSpeed;
            _controller.SetMomentum(momentum);
        }
        
        public void Update()
        {
            _controller.PlayerEffects.HookEffects.DrawGrappleLine(_raycastSensor.GetPosition(), 1);
        }

        private bool CanGrapple => _raycastSensor.CastAndCheck(_controller.CameraTrY.position) && _raycastSensor.GetDistance() > _grappleMinDistance;
        private bool HaveAbility => _controller.PlayerInventory.HaveAbility(this.GetType());


        public bool GroundedToGrappleHook()=>_actionKeyIsPressed&&CanGrapple&&HaveAbility;
        public bool AirToGrappleHook()=>_actionKeyIsPressed&&CanGrapple&&!_controller.HaveStateBeforeStateInHistory<GrapplingHookState,IGroundState>()&&HaveAbility;
        public bool GrappleHookToRising() => _grapplingTimer.IsFinished&&_controller.IsRising();
        public bool GrappleHookToFalling() => _grapplingTimer.IsFinished&&_controller.IsFalling();
    }
}