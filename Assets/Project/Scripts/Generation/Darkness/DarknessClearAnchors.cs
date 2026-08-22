using System;
using UnityEngine;

namespace Project.Scripts.Generation.Darkness
{
    [Serializable]
    public class DarknessClearAnchors
    {
        [SerializeField] private float _radiusHorizontal = 140f;
        [SerializeField] private float _radiusVertical = 80f;
        [SerializeField] private float _maxRadiusScale = 1.25f;
        [SerializeField] private float _followMoveSpeed = 18f;
        [SerializeField] private float _recedeMoveSpeed = 55f;
        [SerializeField] private float _growSpeed = 8f;
        [SerializeField] private float _shrinkSpeed = 0.22f;
        [SerializeField] private float _recedeNormalized = 0.72f;

        private Vector3 _position;
        private float _radiusScale = 1f;
        private float _pulseScale = 1f;
        private bool _hasState;

        public Vector3 Position => _position;
        public float RadiusScale => _radiusScale;
        public float CurrentRadiusHorizontal => _radiusHorizontal * EffectiveScale;
        public float CurrentRadiusVertical => _radiusVertical * EffectiveScale;

        private float EffectiveScale => _radiusScale * _pulseScale;

        public void ApplyWebProfile()
        {
            _maxRadiusScale = 1.15f;
            _growSpeed = 6f;
            _hasState = false;
        }

        public void SetPulseScale(float scale)
        {
            _pulseScale = Mathf.Max(0.01f, scale);
        }

        public void Snap(Vector3 position)
        {
            _position = position;
            _radiusScale = 1f;
            _hasState = true;
        }

        public void CoverPoint(Vector3 position, float radiusScale)
        {
            _position = position;
            _radiusScale = Mathf.Clamp(Mathf.Max(_radiusScale, radiusScale), 0.01f, _maxRadiusScale);
            _hasState = true;
        }

        public void CoverPath(Vector3 from, Vector3 to)
        {
            Vector3 mid = Vector3.Lerp(from, to, 0.5f);
            Vector3 delta = to - from;
            float nx = Mathf.Abs(delta.x) * 0.5f / Mathf.Max(1f, _radiusHorizontal);
            float ny = Mathf.Abs(delta.y) * 0.5f / Mathf.Max(1f, _radiusVertical);
            float nz = Mathf.Abs(delta.z) * 0.5f / Mathf.Max(1f, _radiusHorizontal);
            float needed = Mathf.Sqrt(nx * nx + ny * ny + nz * nz) + 0.12f;
            CoverPoint(mid, needed);
        }

        public void Tick(Vector3 target, float deltaTime)
        {
            if (!_hasState)
            {
                Snap(target);
                return;
            }

            float normalized = NormalizedDistance(target);
            bool receding = normalized > _recedeNormalized;
            float moveSpeed = receding ? _recedeMoveSpeed : _followMoveSpeed;
            float moveBlend = 1f - Mathf.Exp(-moveSpeed * deltaTime);
            _position = Vector3.Lerp(_position, target, moveBlend);

            normalized = NormalizedDistance(target);
            float targetScale = 1f;
            if (normalized > 1f)
                targetScale = Mathf.Min(_maxRadiusScale, _radiusScale * normalized * 1.05f);
            else if (receding)
                targetScale = Mathf.Min(_maxRadiusScale, Mathf.Max(1f, _radiusScale));

            if (targetScale > _radiusScale)
            {
                _radiusScale = Mathf.MoveTowards(_radiusScale, targetScale, _growSpeed * deltaTime);
                return;
            }

            if (_radiusScale <= 1.0001f)
                return;

            float shrunk = Mathf.MoveTowards(_radiusScale, 1f, _shrinkSpeed * deltaTime);
            float previous = _radiusScale;
            _radiusScale = shrunk;
            if (NormalizedDistance(target) > 1f)
                _radiusScale = previous;
        }

        public float SampleClear(Vector3 worldPosition)
        {
            if (!_hasState)
                return 0f;

            float radiusX = _radiusHorizontal * EffectiveScale;
            float radiusY = _radiusVertical * EffectiveScale;
            if (radiusX <= 0.0001f || radiusY <= 0.0001f)
                return 0f;

            Vector3 delta = worldPosition - _position;
            float nx = delta.x / radiusX;
            float ny = delta.y / radiusY;
            float nz = delta.z / radiusX;
            float distSq = nx * nx + ny * ny + nz * nz;
            if (distSq >= 1f)
                return 0f;

            float t = 1f - Mathf.Sqrt(distSq);
            return t * t * (3f - 2f * t);
        }

        public float SampleDensity(Vector3 worldPosition)
        {
            var sampleClear = SampleClear(worldPosition);
            return 1f - sampleClear;
        }

        private float NormalizedDistance(Vector3 point)
        {
            float radiusX = Mathf.Max(1f, _radiusHorizontal * EffectiveScale);
            float radiusY = Mathf.Max(1f, _radiusVertical * EffectiveScale);
            Vector3 delta = point - _position;
            float nx = delta.x / radiusX;
            float ny = delta.y / radiusY;
            float nz = delta.z / radiusX;
            return Mathf.Sqrt(nx * nx + ny * ny + nz * nz);
        }
    }
}
