using System;
using UnityEngine;

namespace Assets.Scripts.General.ColliderLogic
{
    public class CounterTrigger : MonoBehaviour, ITriggerHittable
    {
        [SerializeField] private BodyHittable _bodyHittable;
        public event Action OnHitEvent;

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public void OnHit(IAttackTrigger attackTrigger)
        {
            // float attackBodyDistance = Vector3.Distance(attackTrigger.GetPosition(), _bodyHittable.GetPosition());
            // float attackCounterDistance = Vector3.Distance(attackTrigger.GetPosition(), GetPosition());
            //
            // if(attackBodyDistance<attackCounterDistance)
            //     return;
            
            
            Debug.Log($"Counter {gameObject.name} was hit by {attackTrigger.GetType()}");
            OnHitEvent?.Invoke();
            attackTrigger.AddHitProtected(_bodyHittable);
        }
    }
}