using UnityEngine;

namespace Assets.Scripts.General
{
    public class SphereCastSensor
    {
        public float castLength = 1f;
        public float castRadius = 0.5f;
        public LayerMask layermask = 255;

        private Vector3 _origin = Vector3.zero;
        private readonly Transform _tr;
        private CastDirection _castDirection;
        private RaycastHit _hitInfo;

        public enum CastDirection { Forward, Right, Up, Backward, Left, Down }

        public SphereCastSensor(Transform playerTransform)
        {
            _tr = playerTransform;
        }

        public void Cast()
        {
            Vector3 worldOrigin = _tr.TransformPoint(_origin);
            Vector3 worldDirection = GetCastDirection();
            Physics.SphereCast(worldOrigin, castRadius, worldDirection, out _hitInfo, castLength, layermask,
                QueryTriggerInteraction.Ignore);
        }

        public bool HasDetectedHit() => _hitInfo.collider != null;

        public float GetDistance()
        {
            Vector3 worldOrigin = _tr.TransformPoint(_origin);
            return Vector3.Dot(_hitInfo.point - worldOrigin, GetCastDirection());
        }

        public Vector3 GetNormal() => _hitInfo.normal;
        public Vector3 GetPosition() => _hitInfo.point;
        public Collider GetCollider() => _hitInfo.collider;
        public Transform GetTransform() => _hitInfo.transform;

        public void SetCastDirection(CastDirection direction) => _castDirection = direction;
        public void SetCastOrigin(Vector3 pos) => _origin = _tr.InverseTransformPoint(pos);

        private Vector3 GetCastDirection()
        {
            return _castDirection switch
            {
                CastDirection.Forward => _tr.forward,
                CastDirection.Right => _tr.right,
                CastDirection.Up => _tr.up,
                CastDirection.Backward => -_tr.forward,
                CastDirection.Left => -_tr.right,
                CastDirection.Down => -_tr.up,
                _ => Vector3.one
            };
        }

        public void DrawDebug()
        {
            if (!HasDetectedHit()) return;

            Debug.DrawRay(_hitInfo.point, _hitInfo.normal, Color.red, Time.deltaTime);

            float markerSize = 0.2f;
            Debug.DrawLine(_hitInfo.point + Vector3.up * markerSize, _hitInfo.point - Vector3.up * markerSize, Color.green,
                Time.deltaTime);
            Debug.DrawLine(_hitInfo.point + Vector3.right * markerSize, _hitInfo.point - Vector3.right * markerSize,
                Color.green, Time.deltaTime);
            Debug.DrawLine(_hitInfo.point + Vector3.forward * markerSize, _hitInfo.point - Vector3.forward * markerSize,
                Color.green, Time.deltaTime);
        }
    }
}
