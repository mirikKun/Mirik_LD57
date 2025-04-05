using System;
using Scripts.Player.Inventory.Enums;
using Scripts.Player.Inventory.General;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scripts.Player.Inventory.UI
{
    public abstract class InventorySlotUI : MonoBehaviour,IPointerDownHandler
    {
        [SerializeField]protected Image _itemImage;
        [SerializeField] protected bool _active;


        protected SelectionType _selectionType;
        


        public event Action<InventorySlotUI> OnSlotSelected;
        public bool IsActive => _active;


        public abstract SlotType SlotType { get; }
        public  IInventoryItem Item { get; protected set; }
        public abstract void SetSlot(IInventoryItem item);

        public abstract void ClearSlot();
        public abstract void HighlightSlot();
        public abstract void UnhighlightSlot();
        public virtual void  OnPointerDown(PointerEventData eventData)
        {
            OnSlotSelected?.Invoke(this);
        }
    }
}