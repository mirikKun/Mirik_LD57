using System;
using System.Collections.Generic;
using Assets.Scripts.Player.Inventory.General;
using Scripts.Player.Inventory.Enums;
using Scripts.Player.Inventory.General;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Player.Inventory.UI
{
    public class InventoryUI : MonoBehaviour
    {
        //[SerializeField] private int inventorySize = 20;
        [SerializeField] private List<ActiveAbilitySlotUI> _activeAbilitySlots = new List<ActiveAbilitySlotUI>();
        [SerializeField] private List<InactiveAbilitySlotUI> _inactiveAbilitySlots = new List<InactiveAbilitySlotUI>();
        

        private IInventorySystem _inventorySystem;
        
        private InventorySlotUI _selectedSlot;


        private void Awake()
        {
            //_inventorySystem=
        }

        private void OnEnable()
        {
            SetupInventory(_inventorySystem.ActiveAbilities,_inventorySystem.InactiveAbilities);
            
        }

        private void Start()
        {
            foreach (var activeAbilitySlot in _activeAbilitySlots)
            {
                activeAbilitySlot.OnSlotSelected += OnSlotSelected;
            }
        }


        public void SetupInventory(List<IAbilityItem> activeAbilities, List<IAbilityItem> inactiveAbilities)
        {
            for (int i = 0; i < _activeAbilitySlots.Count; i++)
            {
                if (i < activeAbilities.Count)
                {
                    _activeAbilitySlots[i].SetSlot(activeAbilities[i]);
                }
                else
                {
                    _activeAbilitySlots[i].ClearSlot();
                }
            }

            for (int i = 0; i < _inactiveAbilitySlots.Count; i++)
            {
                if (i < inactiveAbilities.Count)
                {
                    _inactiveAbilitySlots[i].SetSlot(inactiveAbilities[i]);
                }
                else
                {
                    _inactiveAbilitySlots[i].ClearSlot();
                }
            }
        }
        
  
        public void OnSlotSelected(InventorySlotUI slot)
        {
            
            if (_selectedSlot == null||slot.SlotType!=_selectedSlot.SlotType)
            {
                _selectedSlot = slot;
            }
            else 
            {
                if (slot is InactiveSlotUI && _selectedSlot is InactiveSlotUI||
                    slot is ActiveAbilitySlotUI && _selectedSlot is ActiveAbilitySlotUI)
                {
                    _selectedSlot = slot;
                }
                else
                {
                    switch (slot.SlotType)
                    {
                        case SlotType.Equipment:
                            
                            break;
                        case SlotType.Ability:
                            ChangeActiveAbility(slot,_selectedSlot);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    IInventoryItem previousItem = slot.Item;

                    slot.SetSlot(_selectedSlot.Item);
                    _selectedSlot.SetSlot(previousItem);

                    _selectedSlot = null;
                }
                
            }
        }
        

       private void ChangeActiveAbility(InventorySlotUI inventorySlotUI1, InventorySlotUI inventorySlotUI2)
       {

           if (inventorySlotUI1 is ActiveAbilitySlotUI&&inventorySlotUI2 is InactiveAbilitySlotUI )
           {
               _inventorySystem.RemoveActiveAbility(inventorySlotUI1.Item as IAbilityItem);
                _inventorySystem.SetActiveAbility(inventorySlotUI2.Item as IAbilityItem);
               
           }
           else if (inventorySlotUI1 is InactiveAbilitySlotUI&&inventorySlotUI2 is ActiveAbilitySlotUI)
           {
               _inventorySystem.RemoveActiveAbility(inventorySlotUI2.Item as IAbilityItem);
               _inventorySystem.SetActiveAbility(inventorySlotUI1.Item as IAbilityItem);
           }
         
       }
    }
}