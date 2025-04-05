using UnityEngine;

namespace Assets.Scripts.Player.Controller {
    public class CeilingDetector : MonoBehaviour {
        [SerializeField] private float _ceilingAngleLimit = 10f;
        [SerializeField] private bool _isInDebugMode;
        private readonly float _debugDrawDuration = 2.0f;
        private bool _ceilingWasHit;
        
        private void OnCollisionEnter(Collision collision) => CheckForContact(collision);
        private void OnCollisionStay(Collision collision) => CheckForContact(collision);

        private void CheckForContact(Collision collision) {
            if (collision.contacts.Length == 0) return;
            
            float angle = Vector3.Angle(-transform.up, collision.contacts[0].normal);

            if (angle < _ceilingAngleLimit) {
                _ceilingWasHit = true;
            }

            if (_isInDebugMode) {
                Debug.DrawRay(collision.contacts[0].point, collision.contacts[0].normal, Color.red, _debugDrawDuration);
            }
        }
        
        public bool HitCeiling() => _ceilingWasHit;
        public void Reset() => _ceilingWasHit = false;
    }
}
