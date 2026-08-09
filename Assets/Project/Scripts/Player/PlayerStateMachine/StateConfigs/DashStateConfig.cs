using System.Collections.Generic;
using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.StateConfigs
{
    [CreateAssetMenu(menuName = "State Configs/Dash State Config", fileName = "DashStateConfig")]

    public class DashStateConfig:BaseStateConfig
    {
        [field: SerializeField] public float DashSpeed { get; private set; } = 20f;
        [field: SerializeField] public float DashExitSpeed { get; private set; } = 10f;
        [field: SerializeField] public float DashDuration { get; private set; } = 0.4f;
        
        [field: SerializeField] public float UpdatedFov { get; private set; } =77;

        [field: Space]
        [field: SerializeField] public float AfterDashHoveringDuration { get; private set; } = 0.67f;
        [field: SerializeField] public float AfterDashHoveringGravity { get; private set; } = 9f;
        [field: SerializeField] public float AfterDashHoveringSpeed { get; private set; } = 19f;


        public override List<StateConfiguration> GetStateConfiguration(PlayerController playerController)
        {
            List<StateConfiguration> jumpStateConfigurations = new List<StateConfiguration>()
            {
                GetDashConfiguration(playerController),
                GetAfterDashHoveringConfiguration(playerController)
            };
            return jumpStateConfigurations;

        }
        private StateConfiguration GetDashConfiguration(PlayerController playerController)
        {
            var dash = new DashState(playerController,this);
            StateConfiguration configuration = new StateConfiguration
            {
                State = dash,
                Transitions = new List<TransitionConfiguration>()
                {
                    TransitionConfiguration.GetConfiguration<RisingState,DashState>(dash.AirToToDash),
                    TransitionConfiguration.GetConfiguration<FallingState,DashState>(dash.AirToToDash),
                    TransitionConfiguration.GetConfiguration<DashState,AfterDashHoveringState>(dash.EndOfDash),
                    TransitionConfiguration.GetConfiguration<DashState,FallingState>(dash.DashToFalling),
                    TransitionConfiguration.GetConfiguration<GroundedState,DashState>(dash.GroundToDash),
                    TransitionConfiguration.GetConfiguration<WallClingingState,DashState>(dash.WallClingingToDash)
                }
            };
            return configuration;
        }

        private StateConfiguration GetAfterDashHoveringConfiguration(PlayerController playerController)
        {
            var hovering = new AfterDashHoveringState(playerController, this);
            StateConfiguration configuration = new StateConfiguration
            {
                State = hovering,
                Transitions = new List<TransitionConfiguration>()
                {
                    TransitionConfiguration.GetConfiguration<AfterDashHoveringState,RisingState>(hovering.HoveringToRising),
                    TransitionConfiguration.GetConfiguration<AfterDashHoveringState,FallingState>(hovering.HoveringToFalling),
                    TransitionConfiguration.GetConfiguration<AfterDashHoveringState,GroundedState>(hovering.HoveringToGrounded),
                }
            };
            return configuration;
        }
    }
}
