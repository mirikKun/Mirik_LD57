using System;
using Assets.Scripts.General.ColliderLogic;
using Assets.Scripts.General.Health;
using Assets.Scripts.Player.Controller;
using UnityEngine;

namespace Scripts.Player.Health
{
    public class PlayerHealth:BodyHittable, IHealth
    {

        [SerializeField]
        private float _health;

        [SerializeField]private PlayerController _playerController;
        public event Action<float> HealthChanged;
        public float Current { get; set; }
        public float Max { get; set; }

        public void Setup(PlayerController playerController)
        {
            _playerController=playerController;
        }
        private void Start()
        {
            Current = _health;
            Max = _health;
        }

        public void TakeDamage(float damage,bool respawn =true)
        {
            Current -= damage;
            HealthChanged?.Invoke(Current/Max);
            
            if(respawn)
                _playerController.PlayerRespawner.Respawn();
            Debug.Log("Damage taken");
        }
    }
}