using System;
using System.Collections.Generic;
using Assets.Scripts.Player.PlayerStateMachine.States.AbstractStates;
using DG.Tweening;
using Project.Scripts.Utils.SerialisedTypes;
using Project.Scripts.Utils.SerialisedTypes.Attributes;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Player.Controller
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _textParticle;
        [SerializeField] private PlayerInventory _playerInventory;
        [SerializeField] private List<Slot> _abilitySlots;
        [Space] 
        [SerializeField] private Vector3 _startParticleOffset;
        [SerializeField] private Vector3 _particleOffset;
        [SerializeField] private float _particleDuration = 0.4f;
        [SerializeField] private Ease _easing = Ease.InOutCubic;

        private void Start()
        {
            ResetAbilitiesCount();
            _playerInventory.AbilitiesReseted += ResetAbilitiesCount;
            _playerInventory.AbilityAdded += OnAbilityAdded;
            _playerInventory.AbilitySpent += OnAbilitySpent;
        }

        private void ResetAbilitiesCount()
        {
            foreach (var abilitySlot in _abilitySlots)
            {
                abilitySlot.CountText.text = _playerInventory.GetAbilitiesCount(abilitySlot.Ability.Type).ToString();
                abilitySlot.Count = _playerInventory.GetAbilitiesCount(abilitySlot.Ability.Type);
            }
        }

        private void OnAbilityAdded(Type ability, int count)
        {
            Slot abilitySlot = GetSlot(ability);
            abilitySlot.Count += count;
            abilitySlot.CountText.text = abilitySlot.Count.ToString();

            PlayParticle(ability, $"+{count}");
        }

        private void OnAbilitySpent(Type ability)
        {
            Slot abilitySlot = GetSlot(ability);
            abilitySlot.Count -= 1;
            abilitySlot.CountText.text = abilitySlot.Count.ToString();
            PlayParticle(ability, "-1");
        }

        private Slot GetSlot(Type ability)
        {
            foreach (var abilitySlot in _abilitySlots)
            {
                if (abilitySlot.Ability.Type == ability)
                {
                    return abilitySlot;
                }
            }

            return _abilitySlots[0];
        }

        private void PlayParticle(Type ability, string text)
        {
            Slot abilitySlot = GetSlot(ability);
            if (abilitySlot.Ability.Type == ability)
            {
                var particle = Instantiate(_textParticle, abilitySlot.CountText.transform.position+_startParticleOffset, quaternion.identity,
                    abilitySlot.Root.transform);
                particle.text = text;
                particle.transform.DOMove(particle.transform.position + _particleOffset, _particleDuration)
                    .SetEase(_easing).OnComplete(() => Destroy(particle.gameObject));
                particle.DOFade(0, _particleDuration);
            }
        }


        [Serializable]
        public class Slot
        {
            [TypeFilter(typeof(ISpendableState))] [SerializeField]
            public SerializableType Ability;

            public GameObject Root;
            public TMP_Text CountText;
            public int Count;
        }
    }
}