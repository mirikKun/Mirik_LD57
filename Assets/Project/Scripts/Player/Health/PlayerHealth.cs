using System;
using Assets.Scripts.General.ColliderLogic;
using Assets.Scripts.General.Health;
using Assets.Scripts.Player.Controller;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Player.Health
{
    public class PlayerHealth:BodyHittable, IHealth
    {

        [SerializeField]
        private int _health=100;

        [SerializeField]private PlayerController _playerController;
        public event Action<float> HealthChanged;
        public int Current { get; set; }
        public int Max { get; set; }

        public void Setup(PlayerController playerController)
        {
            _playerController=playerController;
        }
        private void Start()
        {
            Current = _health;
            Max = _health;
        }

        public void TakeDamage(int damage,bool respawn =true)
        {
            Current -= damage;
            HealthChanged?.Invoke((float)Current/Max);

            if (Current <= 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            if(respawn)
                _playerController.PlayerRespawner.Respawn();
            Debug.Log("Damage taken" + damage);
        }

        public void AddHealth(int healAmount)
        {
            healAmount = Mathf.Clamp(healAmount, 1, Max);
            Current += healAmount;

            Current = Mathf.Clamp(Current, Current, Max);
            HealthChanged?.Invoke((float)Current/Max);

        }
    }
}