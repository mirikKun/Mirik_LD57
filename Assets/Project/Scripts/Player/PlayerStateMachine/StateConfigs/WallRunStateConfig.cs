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
        [SerializeField]private  float _wallRunDuration=3;
        [SerializeField]private  float _wallRunSpeed=6;
        [SerializeField]private  float _minSpeedToStartWallRun=4;
        [SerializeField] private float _maxVerticalSpeedToStartWallRun=14;
        [SerializeField]private  float _wallGravity=5;
        [SerializeField]private float _cameraAngle=7;
        [SerializeField]private float _wallAngleMultiplier=3;

        [Space]
        [SerializeField]private  float _jumpForwardPower=12;
        [SerializeField]private  float _jumpUpPower=12;
        [SerializeField]private  float _jumpFromWallPower=8;
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
            var wallRunning = new WallRunningState(playerController,_wallRunDuration,_wallRunSpeed,_minSpeedToStartWallRun,_maxVerticalSpeedToStartWallRun,_wallGravity,_cameraAngle,_wallAngleMultiplier);
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
            var wallJumping = new WallRunJumpState(playerController,_jumpForwardPower,_jumpUpPower,_jumpFromWallPower);
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