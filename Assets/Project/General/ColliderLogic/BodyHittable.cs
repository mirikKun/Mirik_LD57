using UnityEngine;

namespace Assets.Scripts.General.ColliderLogic
{
    public class BodyHittable:MonoBehaviour,ITriggerHittable
    {
        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public void OnHit(IAttackTrigger attackTrigger)
        {
            Debug.Log($"Body {gameObject.name} was hit by {attackTrigger.GetType()}");
        }
    }
}