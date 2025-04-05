using UnityEngine;

namespace Assets.Scripts.General.ColliderLogic
{
    public interface ITriggerHittable
    {
        public Vector3 GetPosition();
        public void OnHit(IAttackTrigger attackTrigger);
    }
}