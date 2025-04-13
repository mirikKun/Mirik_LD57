using System.Collections.Generic;
using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.StateConfigs
{
    [CreateAssetMenu(menuName = "State Configs/Wall Clinging State Config", fileName = "WallClingingStateConfig")]

    public class WallClingingConfig:BaseStateConfig

    {
        public override List<StateConfiguration> GetStateConfiguration(PlayerController playerController)
        {
            List<StateConfiguration> jumpStateConfigurations = new List<StateConfiguration>()
            {
                GetClingingConfiguration(playerController)
            };
            return jumpStateConfigurations;
        }
        private StateConfiguration GetClingingConfiguration(PlayerController playerController)
        {
            var clinging = new WallClingingState(playerController,this);
            StateConfiguration configuration = new StateConfiguration
            {
                State = clinging,
                Transitions = new List<TransitionConfiguration>()
                {
                    TransitionConfiguration.GetConfiguration<FallingState,WallClingingState>(clinging.FallingToClinging),
                    TransitionConfiguration.GetConfiguration<RisingState,WallClingingState>(clinging.RisingToClinging),
                    TransitionConfiguration.GetConfiguration<WallClingingState,PounceState>(clinging.ClingingToPounce),
                    TransitionConfiguration.GetConfiguration<WallClingingState,FallingState>(clinging.ClingingToFalling)
                }
            };
            return configuration;
        }
    }
}