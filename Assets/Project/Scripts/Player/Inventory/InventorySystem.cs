using System.Collections.Generic;
using Assets.Scripts.Player.Inventory.General;
using Scripts.Player.Inventory.General;

namespace Scripts.Player.Inventory
{
    public class InventorySystem : IInventorySystem
    {
        private List<IAbilityItem> _activeAbilityItems;
        private List<IAbilityItem> _inactiveAbilityItems;

        public List<IAbilityItem> ActiveAbilities => _activeAbilityItems;
        public List<IAbilityItem> InactiveAbilities=> _inactiveAbilityItems;

        public void SetupInventory(List<IAbilityItem> activeAbilities, List<IAbilityItem> inactiveAbilities)
        {
            _activeAbilityItems = activeAbilities;
            _inactiveAbilityItems = inactiveAbilities;
        }

        public void AddItem(IInventoryItem item)
        {
            if (item is IAbilityItem abilityItem)
            {
                _inactiveAbilityItems.Add(abilityItem);
            }
        }

        public void SetActiveAbility(IAbilityItem newActiveAbility)
        {
            if(newActiveAbility==null)
                return;
            _inactiveAbilityItems.Add(newActiveAbility);
            _activeAbilityItems.Remove(newActiveAbility);
        }

        public void RemoveActiveAbility(IAbilityItem newActiveAbility)
        {
            if(newActiveAbility==null)
                return;
            _inactiveAbilityItems.Add(newActiveAbility);
            _activeAbilityItems.Remove(newActiveAbility);
        }
    }
}