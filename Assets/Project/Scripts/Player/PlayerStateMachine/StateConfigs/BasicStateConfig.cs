using System.Collections.Generic;
using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.StateConfigs
{
    [CreateAssetMenu(menuName = "State Configs/Basic State Config", fileName = "BasicStateConfig")]
    public class BasicStateConfig:BaseStateConfig
    {
        public override List<StateConfiguration> GetStateConfiguration(PlayerController playerController)
        {
            List<StateConfiguration> configurations = new List<StateConfiguration>();
            
            
            configurations.Add(GetGroundedConfiguration(playerController));
            configurations.Add(GetFallingConfiguration(playerController));
            configurations.Add(GetSlidingConfiguration(playerController));
            configurations.Add(GetRisingConfiguration(playerController));
            return configurations;
        }
        private StateConfiguration GetGroundedConfiguration(PlayerController playerController)
        {
            var grounded = new GroundedState(playerController);
            StateConfiguration configuration = new StateConfiguration
            {
                State = grounded,
                Transitions = new List<TransitionConfiguration>()
                {
                    TransitionConfiguration.GetConfiguration<GroundedState,RisingState>(grounded.GroundedToRising),
                    TransitionConfiguration.GetConfiguration<GroundedState,SlopeSlidingState>(grounded.GroundedToSliding),
                    TransitionConfiguration.GetConfiguration<GroundedState,FallingState>(grounded.GroundedToFalling)
                }
            };
            return configuration;
        }
        
        private StateConfiguration GetFallingConfiguration(PlayerController playerController)
        {
            var falling = new FallingState(playerController);
            StateConfiguration configuration = new StateConfiguration
            {
                State = falling,
                Transitions = new List<TransitionConfiguration>()
                {
                    TransitionConfiguration.GetConfiguration<FallingState,RisingState>(falling.FallingToRising),
                    TransitionConfiguration.GetConfiguration<FallingState,GroundedState>(falling.FallingToGrounded),
                    TransitionConfiguration.GetConfiguration<FallingState,SlopeSlidingState>(falling.FallingToSliding)
                }
            };
            return configuration;
        }
        
        private StateConfiguration GetSlidingConfiguration(PlayerController playerController)
        {
            var slopeSliding = new SlopeSlidingState(playerController);
            StateConfiguration configuration = new StateConfiguration
            {
                State = slopeSliding,
                Transitions = new List<TransitionConfiguration>()
                {
                    TransitionConfiguration.GetConfiguration<SlopeSlidingState,RisingState>(slopeSliding.SlidingToRising),
                    TransitionConfiguration.GetConfiguration<SlopeSlidingState,FallingState>(slopeSliding.SlidingToFalling),
                    TransitionConfiguration.GetConfiguration<SlopeSlidingState,GroundedState>(slopeSliding.SlidingToGround)
                }
            };
            return configuration;
        }        
        private StateConfiguration GetRisingConfiguration(PlayerController playerController)
        {
            var rising = new RisingState(playerController);
            StateConfiguration configuration = new StateConfiguration
            {
                State = rising,
                Transitions = new List<TransitionConfiguration>()
                {
                    TransitionConfiguration.GetConfiguration<RisingState,GroundedState>(rising.RisingToGrounded),
                    TransitionConfiguration.GetConfiguration<RisingState,SlopeSlidingState>(rising.GroundToSliding),
                    TransitionConfiguration.GetConfiguration<RisingState,FallingState>(rising.RisingToFalling)
                }
            };
            return configuration;
        }
    }
    
    
}