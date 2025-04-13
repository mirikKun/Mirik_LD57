using System.Collections.Generic;
using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.StateConfigs
{
    [CreateAssetMenu(menuName = "State Configs/Wall Run Config", fileName = "WallRunConfig")]

    public class WallRunStateConfig:BaseStateConfig
    {
        [field:SerializeField] public float WallRunDuration { get; private set; }=3;
        [field:SerializeField]public  float WallRunSpeed{ get; private set; }=6;
        [field:SerializeField]public  float MinSpeedToStartWallRun{ get; private set; }=4;
        [field:SerializeField] public float MaxVerticalSpeedToStartWallRun{ get; private set; }=14;
        [field:SerializeField]public  float WallGravity{ get; private set; }=5;
        [field:SerializeField]public float CameraAngle{ get; private set; }=7;
        [field:SerializeField]public float WallAngleMultiplier{ get; private set; }=1.6f;

        [field:Space]
        [field:SerializeField]public  float JumpForwardPower{ get; private set; }=12;
        [field:SerializeField]public  float JumpUpPower{ get; private set; }=12;
        [field:SerializeField]public  float JumpFromWallPower{ get; private set; }=8;
        public override List<StateConfiguration> GetStateConfiguration(PlayerController playerController)
        {
            List<StateConfiguration> jumpStateConfigurations = new List<StateConfiguration>()
            {
                GetWallRunningConfiguration(playerController),
                GetWallRunJumpConfiguration(playerController)
            };
            return jumpStateConfigurations;
        }
        
        
        
        private StateConfiguration GetWallRunningConfiguration(PlayerController playerController)
        {
            var wallRunning = new WallRunningState(playerController,this);
            StateConfiguration configuration = new StateConfiguration
            {
                State = wallRunning,
                Transitions = new List<TransitionConfiguration>()
                {
                    TransitionConfiguration.GetConfiguration<WallRunningState,GroundedState>(wallRunning.WallRunningToGround),
                    TransitionConfiguration.GetConfiguration<WallRunningState,FallingState>(wallRunning.WallRunningToFalling),
                    TransitionConfiguration.GetConfiguration<RisingState,WallRunningState>(wallRunning.RisingToWallRunning),
                    TransitionConfiguration.GetConfiguration<FallingState,WallRunningState>(wallRunning.FallingToWallRunning)
                }
            };
            return configuration;
        }
        
        private StateConfiguration GetWallRunJumpConfiguration(PlayerController playerController)
        {
            var wallJumping = new WallRunJumpState(playerController,this);
            StateConfiguration configuration = new StateConfiguration
            {
                State = wallJumping,
                Transitions = new List<TransitionConfiguration>()
                {
                    TransitionConfiguration.GetConfiguration<WallRunningState,WallRunJumpState>(wallJumping.WallRunningToWallRunJump),
                    TransitionConfiguration.GetConfiguration<WallRunJumpState,RisingState>(wallJumping.WallRunJumpToRising),
                    TransitionConfiguration.GetConfiguration<WallRunJumpState,FallingState>(wallJumping.WallRunJumpToFalling)
                }
            };
            return configuration;
        }
    }
}