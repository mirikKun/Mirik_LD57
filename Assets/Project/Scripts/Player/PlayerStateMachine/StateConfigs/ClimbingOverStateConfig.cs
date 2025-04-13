using System.Collections.Generic;
using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.StateConfigs
{
    [CreateAssetMenu(menuName = "State Configs/Climbing Over Config", fileName = "ClimbingOverStateConfig")]

    public class ClimbingOverStateConfig:BaseStateConfig
    {
        [field: SerializeField] public float ClimbingOverSpeed { get; private set; } = 7f;
        [field: SerializeField] public float HorizontalSpeedReduction { get; private set; } = 0.3f;

        public override List<StateConfiguration> GetStateConfiguration(PlayerController playerController)
        {
            List<StateConfiguration> jumpStateConfigurations = new List<StateConfiguration>()
            {
                GetClimbingOverConfiguration(playerController)
            };
            return jumpStateConfigurations;
        }
        
        private StateConfiguration GetClimbingOverConfiguration(PlayerController playerController)
        {
            var climbingOver = new ClimbingOverState(playerController,this);
            StateConfiguration configuration = new StateConfiguration
            {
                State = climbingOver,
                Transitions = new List<TransitionConfiguration>()
                {
                    TransitionConfiguration.GetConfiguration<FallingState,ClimbingOverState>(climbingOver.FallingToClimbingOver),
                    TransitionConfiguration.GetConfiguration<RisingState,ClimbingOverState>(climbingOver.RisingToClimbingOver),
                    TransitionConfiguration.GetConfiguration<GroundedState,ClimbingOverState>(climbingOver.GroundedToClimbingOver),
                    TransitionConfiguration.GetConfiguration<ClimbingOverState,RisingState>(climbingOver.ClimbingOverToRising),
                    TransitionConfiguration.GetConfiguration<ClimbingOverState,FallingState>(climbingOver.ClimbingOverToFalling),
                    
                }
            };
            return configuration;
        }
    }
}