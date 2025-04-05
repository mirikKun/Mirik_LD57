using System;
using System.Collections.Generic;
using Assets.Scripts.General.StateMachine;
using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.StateConfigs;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerStateMachine
{
    public class PlayerStatesHolder:MonoBehaviour
    {
        [SerializeField] private List<BaseStateConfig> _stateConfigs;
        
        public List<BaseStateConfig> StateConfigs => _stateConfigs;
        public event Action OnStateConfigsChanged;
        public void AddConfig(BaseStateConfig config)
        {
            _stateConfigs.Add(config);
            OnStateConfigsChanged?.Invoke();
        }
        public List<StateConfiguration> GetStateConfigurations(PlayerController playerController)
        {
            List<StateConfiguration> stateConfigurations=new List<StateConfiguration>();
            foreach (var stateConfig in _stateConfigs)
            {
                stateConfigurations.AddRange(stateConfig.GetStateConfiguration(playerController));
            }

            return stateConfigurations;
        }

        public void RebuildStateMachine()
        {
            OnStateConfigsChanged?.Invoke();
        }
    }
}