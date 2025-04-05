using System;
using Assets.Scripts.General.ColliderLogic;
using Assets.Scripts.General.Health;
using UnityEngine;

namespace Scripts.Player.Health
{
    public class PlayerHealth:BodyHittable, IHealth
    {

        [SerializeField]
        private float _health;
        public event Action HealthChanged;
        public float Current { get; set; }
        public float Max { get; set; }

        private void Start()
        {
            Current = _health;
            Max = _health;
        }

        public void TakeDamage(float damage)
        {
            Current -= damage;
            HealthChanged?.Invoke();
            Debug.Log("Damage taken");
        }
    }
}