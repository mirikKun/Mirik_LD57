using System;

namespace Assets.Scripts.General.Health
{
    public interface IHealth
    {
        event Action<float> HealthChanged;
        int Current { get; set; }
        int Max { get; set; }
        void TakeDamage(int damage,bool respawn =true);
        void AddHealth(int healAmount);
    }
}