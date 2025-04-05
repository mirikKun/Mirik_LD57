using System;

namespace Assets.Scripts.General.Health
{
    public interface IHealth
    {
        event Action<float> HealthChanged;
        float Current { get; set; }
        float Max { get; set; }
        void TakeDamage(float damage,bool respawn =true);
    }
}