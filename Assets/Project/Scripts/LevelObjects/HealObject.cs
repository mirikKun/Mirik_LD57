using System;
using Assets.Scripts.General.Health;
using Scripts.Player.Health;
using UnityEngine;

namespace Scripts.LevelObjects
{
    public class HealObject:MonoBehaviour
    {
        [SerializeField] private int _healAmount;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IHealth>(out var health))
            {
                health.AddHealth(_healAmount);
                Destroy(gameObject);

            }
        }
    }
}