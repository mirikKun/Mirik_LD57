using UnityEngine;

namespace Scripts.Player.Inventory.General
{
    public interface IInventoryItem
    {
        string ID { get; }
        string Name { get; }
        string Description { get; }
        Sprite Icon { get; }
    }
}