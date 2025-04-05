using System.Collections.Generic;
using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.StateConfigs
{
    [CreateAssetMenu(menuName = "State Configs/Pounce State Config", fileName = "PounceStateConfig")]

    public class PounceStateConfig:BaseStateConfig
    {
        [field: SerializeField] public float PouncePower { get; private set; } = 21f;
        [field: SerializeField] public float PounceMinAngle { get; private set; } = 30f;

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
            var pounce = new PounceState(playerController,PouncePower,PounceMinAngle);
            StateConfiguration configuration = new StateConfiguration
            {
                State = pounce,
                Transitions = new List<TransitionConfiguration>()
                {
                    TransitionConfiguration.GetConfiguration<GroundedState,PounceState>(pounce.GroundedToPounce),
                    TransitionConfiguration.GetConfiguration<PounceState,RisingState>(pounce.PounceToRising),
                    TransitionConfiguration.GetConfiguration<PounceState,FallingState>(pounce.PounceToFalling)
                }
            };
            return configuration;
        }
    }
}