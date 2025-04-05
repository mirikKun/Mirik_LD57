using Scripts.Player.Inventory.Enums;
using Scripts.Player.Inventory.General;
using UnityEngine;

namespace Scripts.Player.Inventory.UI
{
    public class InactiveAbilitySlotUI:InactiveSlotUI
    {
        public override SlotType SlotType => SlotType.Ability;

        public override void SetSlot(IInventoryItem item)
        {
            Item = item;
            _itemImage.sprite = item.Icon;
            _itemImage.color = Color.white;

        }

        public override void ClearSlot()
        {
            Item = null;
            _itemImage.sprite = null;
            _itemImage.color = new Color(1, 1, 1, 0);
        }

        public override void HighlightSlot()
        {
        }

        public override void UnhighlightSlot()
        {
        }
    }
}