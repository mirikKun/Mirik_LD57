using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.General.ColliderLogic
{
    public class SimpleAttackTrigger:MonoBehaviour,IAttackTrigger
    {
        private List<ITriggerHittable> _hitObjects=new List<ITriggerHittable>();
        private List<ITriggerHittable> _hitProtectedObjects=new List<ITriggerHittable>();
        private float _damage;
        public float Damage => _damage;

        
        
        public void SetDamage(float damage)
        {
            _damage = damage;
        }
        public void Reset()
        {
            _hitObjects.Clear();
            _hitProtectedObjects.Clear();
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.TryGetComponent(out ITriggerHittable hittable)&&!_hitObjects.Contains(hittable)
               &&!_hitProtectedObjects.Contains(hittable))
            {
                _hitObjects.Add(hittable);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(other.TryGetComponent(out ITriggerHittable hittable)&&_hitObjects.Contains(hittable)&&!_hitProtectedObjects.Contains(hittable))
            {
              
                hittable.OnHit(this);
            }
        }


        public void AddHitProtected(ITriggerHittable hittable)
        {
            _hitProtectedObjects.Add(hittable);
        }
    }
}