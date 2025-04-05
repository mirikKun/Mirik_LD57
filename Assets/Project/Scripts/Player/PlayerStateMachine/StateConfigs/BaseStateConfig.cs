using System.Collections.Generic;
using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine.StateConfigs
{
    public abstract class BaseStateConfig:ScriptableObject
    {
        public abstract List<StateConfiguration> GetStateConfiguration(PlayerController playerController);
        
    }
}