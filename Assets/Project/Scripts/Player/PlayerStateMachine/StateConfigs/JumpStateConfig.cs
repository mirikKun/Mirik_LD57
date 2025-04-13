using System.Collections.Generic;
using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.StateConfigs
{
    
    [CreateAssetMenu(menuName = "State Configs/Jump State Config", fileName = "JumpStateConfig")]
    public class JumpStateConfig:BaseStateConfig
    {
        [field: SerializeField] public float JumpSpeed { get; private set; } = 10f;
        [field: SerializeField] public float JumpDuration { get; private set; } = 0.2f;

        public override List<StateConfiguration> GetStateConfiguration(PlayerController playerController)
        {
            List<StateConfiguration> jumpStateConfigurations = new List<StateConfiguration>()
            {
                GetJumpingConfiguration(playerController)
            };
            return jumpStateConfigurations;
        }
        
        private StateConfiguration GetJumpingConfiguration(PlayerController playerController)
        {
            var jumping = new JumpingState(playerController,this);
            StateConfiguration configuration = new StateConfiguration
            {
                State = jumping,
                Transitions = new List<TransitionConfiguration>()
                {
                    TransitionConfiguration.GetConfiguration<GroundedState,JumpingState>(jumping.GroundedToJumping),
                    TransitionConfiguration.GetConfiguration<JumpingState,RisingState>(jumping.JumpingToRising),
                    TransitionConfiguration.GetConfiguration<JumpingState,FallingState>(jumping.JumpingToFalling)
                }
            };
            return configuration;
        }
    }
}