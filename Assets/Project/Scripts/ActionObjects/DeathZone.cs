using Assets.Scripts.General.ColliderLogic;
using Assets.Scripts.General.Health;
using UnityEngine;

namespace Scripts.ActionObjects
{
    public class DeathZone:MonoBehaviour,IAttackTrigger
    {
        [SerializeField] private int _damage = 20;
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IHealth health))
            {
                health.TakeDamage(_damage);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out IHealth health))
            {
                //hittable.;
            }
        }
    }
}