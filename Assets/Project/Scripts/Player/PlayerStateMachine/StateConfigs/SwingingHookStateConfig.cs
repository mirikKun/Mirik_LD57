using System.Collections.Generic;
using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.PlayerStateMachine.States;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.StateConfigs
{
    [CreateAssetMenu(menuName = "State Configs/Swinging Hook State Config", fileName = "SwingingHookStateConfig")]

    public class SwingingHookStateConfig:BaseStateConfig
    {
        [field: SerializeField] public float SwingingSpeed { get; private set; }= 10f;
        [field: SerializeField] public float GrapplingSpeed { get; private set; }= 10f;
        [field: SerializeField] public float PreparingDuration { get; private set; }= 0.25f;

        [field: SerializeField] public   float SwingingDuration{ get; private set; }=5;

        [field: SerializeField] public float SwingingMaxDistance { get; private set; }= 50f;
        [field: SerializeField] public float SwingingMinDistance { get; private set; }= 5f;
        [field: SerializeField] public float MaxSwingingSpeed { get; private set; }= 20f;
        [field: SerializeField] public float SwingingExitSpeedMultiplier{ get; private set; } = 1f;
        [field: SerializeField] public float StartSwingMomentum{ get; private set; } = 2f;
        [field: SerializeField] public AnimationCurve SwingingDirectionLerpCurve{ get; private set; }


        public override List<StateConfiguration> GetStateConfiguration(Player.Controller.PlayerController playerController)
        {
            List<StateConfiguration> swingingHookStateConfigurations = new List<StateConfiguration>()
            {
                GetGrapplingHookConfiguration(playerController)
            };
            return swingingHookStateConfigurations;
        }
        
        private StateConfiguration GetGrapplingHookConfiguration(Player.Controller.PlayerController playerController)
        {
            var swingingHook = new SwingingHookState(playerController,SwingingSpeed,GrapplingSpeed,PreparingDuration,SwingingDuration,SwingingMaxDistance,MaxSwingingSpeed,SwingingMinDistance,SwingingExitSpeedMultiplier,StartSwingMomentum,SwingingDirectionLerpCurve);
            StateConfiguration configuration = new StateConfiguration
            {
                State = swingingHook,
                Transitions = new List<TransitionConfiguration>()
                {
                    //TransitionConfiguration.GetConfiguration<DashState,GroundedState>(dash.DashToGround),
                    TransitionConfiguration.GetConfiguration<GroundedState, SwingingHookState>(swingingHook.GroundedToSwingingHook),
                    TransitionConfiguration.GetConfiguration<RisingState, SwingingHookState>(swingingHook.AirToSwingingHook),
                    TransitionConfiguration.GetConfiguration<FallingState, SwingingHookState>(swingingHook.AirToSwingingHook),
                    TransitionConfiguration.GetConfiguration<SwingingHookState, RisingState>(swingingHook.SwingingHookToRising),
                    TransitionConfiguration.GetConfiguration<SwingingHookState, FallingState>(swingingHook.SwingingHookToFalling),
            }};
            return configuration;
        }
    }
}