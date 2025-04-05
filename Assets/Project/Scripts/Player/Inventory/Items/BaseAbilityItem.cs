using Assets.Scripts.Player.Inventory.General;
using Assets.Scripts.Player.PlayerStateMachine.StateConfigs;
using UnityEngine;

namespace Scripts.Player.Inventory.Items
{
    public class BaseAbilityItem: BaseItem, IAbilityItem
    {
        [SerializeField] private BaseStateConfig _stateConfig;
        public BaseStateConfig StateConfig=>_stateConfig;
    }
}