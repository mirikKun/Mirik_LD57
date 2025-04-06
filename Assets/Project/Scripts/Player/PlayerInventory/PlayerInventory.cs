using System;
using System.Collections.Generic;
using Assets.Scripts.Player.PlayerStateMachine.States.AbstractStates;
using Project.Scripts.Utils.SerialisedTypes;
using Project.Scripts.Utils.SerialisedTypes.Attributes;
using UnityEngine;

namespace Assets.Scripts.Player.Controller
{
    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private List<StartInventoryAbility> _startInventory;
        private Dictionary<Type, int> _abilityStacks = new Dictionary<Type, int>();

        private Dictionary<Type, int> _tempSpentAbilityStacks = new Dictionary<Type, int>();

        public event Action AbilitiesReseted;
        public event Action<Type, int> AbilityAdded;
        public event Action<Type> AbilitySpent;

        private void Start()
        {
            foreach (var startAbility in _startInventory)
            {
                AddAbility(startAbility.Ability.Type, startAbility.Amount);
            }

            ResetTempAbilities();
        }

        public int GetAbilitiesCount(Type abilityType)
        {
            return _abilityStacks.GetValueOrDefault(abilityType, 0);
        }


        public void AddAbility(Type abilityType, int amount)
        {
            if (_abilityStacks.TryGetValue(abilityType, out var value))
            {
                _abilityStacks[abilityType] = value + amount;
            }
            else
            {
                _abilityStacks.Add(abilityType, amount);
            }

            AbilityAdded?.Invoke(abilityType, amount);
        }

        public bool HaveAbility(Type abilityType)
        {
            int total = _abilityStacks.GetValueOrDefault(abilityType, 0);
            int spent = _tempSpentAbilityStacks.GetValueOrDefault(abilityType, 0);
            if (total > 0 && total > spent)
            {
                return true;
            }

            return false;
        }

        public void TempSpendAbility(Type abilityType)
        {
            if (_abilityStacks.TryGetValue(abilityType, out var value) && value > 0)
            {
                if (_tempSpentAbilityStacks.TryGetValue(abilityType, out var tempValue) &&
                    _abilityStacks[abilityType] > tempValue)
                {
                    _tempSpentAbilityStacks[abilityType]+= 1;
                }
                else
                {
                    _tempSpentAbilityStacks.Add(abilityType, 1);
                }

                AbilitySpent?.Invoke(abilityType);
            }
        }

        public void ResetTempAbilities()
        {
            _tempSpentAbilityStacks = new Dictionary<Type, int>();
            AbilitiesReseted?.Invoke();
        }

        public void ApplyTempSpentAbilities()
        {
            foreach (var ability in _tempSpentAbilityStacks.Keys)
            {
                _abilityStacks[ability] -= _tempSpentAbilityStacks[ability];
            }

            ResetTempAbilities();
        }

        [Serializable]
        public class StartInventoryAbility
        {
            [TypeFilter(typeof(ISpendableState))] [SerializeField]
            public SerializableType Ability;

            [field: SerializeField] public int Amount { get; private set; }
        }
    }
}