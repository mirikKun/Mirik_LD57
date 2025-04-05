using UnityEngine;

namespace Assets.Scripts.General.ColliderLogic
{
    public interface IAttackTrigger
    {
        public float Damage { get; }
        public void Reset();
        public Vector3 GetPosition();
        public void AddHitProtected(ITriggerHittable hittable);
    }
}