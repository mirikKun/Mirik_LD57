using System;
using Scripts.Player.Inventory.Enums;
using Scripts.Player.Inventory.General;
using UnityEngine;

namespace Scripts.Player.Inventory.UI
{
    public class ActiveAbilitySlotUI:ActiveSlotUI
    {


        public override SlotType SlotType => SlotType.Ability;

        public override void SetSlot(IInventoryItem item)
        {
            Item = item;
            _itemImage.sprite = item.Icon;
        }
   
        public override void ClearSlot()
        {
           
            Item = null;
            _itemImage.sprite = null;
        }

        public override void HighlightSlot()
        {
        }

        public override void UnhighlightSlot()
        {
        }
    }
}