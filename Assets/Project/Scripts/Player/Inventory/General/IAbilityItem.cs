using Assets.Scripts.Player.PlayerStateMachine.StateConfigs;
using Scripts.Player.Inventory.General;

namespace Assets.Scripts.Player.Inventory.General
{
    public interface IAbilityItem:IInventoryItem
    {
        public BaseStateConfig StateConfig { get;  }
    }
}