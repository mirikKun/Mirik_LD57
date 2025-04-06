using Assets.Scripts.Player.Controller;
using Assets.Scripts.Player.PlayerStateMachine.States.AbstractStates;
using Project.Scripts.Utils.SerialisedTypes;
using Project.Scripts.Utils.SerialisedTypes.Attributes;
using UnityEngine;

namespace Scripts.LevelObjects
{
    public class PickUpAbility:MonoBehaviour
    {
        [TypeFilter(typeof(ISpendableState))] [SerializeField]
        private SerializableType _ability;

        [SerializeField] private int _amount;


        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PlayerInventory>(out var playerInventory))
            {
                playerInventory.AddAbility(_ability.Type,_amount);
                Destroy(gameObject);
            }
        }
    }
}