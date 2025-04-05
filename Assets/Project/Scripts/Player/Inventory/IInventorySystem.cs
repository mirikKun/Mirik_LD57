using System.Collections.Generic;
using Assets.Scripts.Player.Inventory.General;
using Scripts.Player.Inventory.General;

namespace Scripts.Player.Inventory
{
    public interface IInventorySystem
    {
        
        public List<IAbilityItem> ActiveAbilities { get; }
        public List<IAbilityItem> InactiveAbilities { get; }
        public void SetupInventory(List<IAbilityItem> activeAbilities, List<IAbilityItem> inactiveAbilities);
        
        public void AddItem(IInventoryItem item);
        
        public void SetActiveAbility(IAbilityItem newActiveAbility);
        public void RemoveActiveAbility(IAbilityItem newActiveAbility);
    }
}