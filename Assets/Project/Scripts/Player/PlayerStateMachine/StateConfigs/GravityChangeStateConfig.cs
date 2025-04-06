using System.Collections.Generic;
using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.StateConfigs
{
    [CreateAssetMenu(menuName = "State Configs/Gravity Change State Config", fileName = "GravityChangeStateConfig")]

    public class GravityChangeStateConfig : BaseStateConfig
    {
        [field: SerializeField] public float RaycastDistance { get; private set; } = 4.5f;
        [field: SerializeField] public float ChangingDuration { get; private set; } = 0.5f;
        [field: SerializeField] public float GravityChangeFullDuration { get; private set; } = 5.5f;

        public override List<StateConfiguration> GetStateConfiguration(PlayerController playerController)
        {
            List<StateConfiguration> jumpStateConfigurations = new List<StateConfiguration>()
            {
                GetPounceConfiguration(playerController)
            };
            return jumpStateConfigurations;
        }

        private StateConfiguration GetPounceConfiguration(PlayerController playerController)
        {
            var gravityChange = new GravityChangeState(playerController, RaycastDistance, ChangingDuration,GravityChangeFullDuration);
            StateConfiguration configuration = new StateConfiguration
            {
                State = gravityChange,
                Transitions = new List<TransitionConfiguration>()
                {
                    TransitionConfiguration.GetConfiguration<GroundedState, GravityChangeState>(gravityChange.GroundedToGravityChange),
                    
                    TransitionConfiguration.GetConfiguration<GroundedState, GravityChangeState>(gravityChange.GravityChangeDurationEnded),
                    TransitionConfiguration.GetConfiguration<RisingState, GravityChangeState>(gravityChange.GravityChangeDurationEnded),
                    TransitionConfiguration.GetConfiguration<FallingState, GravityChangeState>(gravityChange.GravityChangeDurationEnded),
                    TransitionConfiguration.GetConfiguration<SlopeSlidingState, GravityChangeState>(gravityChange.GravityChangeDurationEnded),
                    
                    TransitionConfiguration.GetConfiguration<GravityChangeState, GroundedState>(gravityChange.GravityChangeToGrounded),
                    TransitionConfiguration.GetConfiguration<GravityChangeState, FallingState>(gravityChange.GravityChangeToFalling)
                }
            };
            return configuration;
        }
    }
}