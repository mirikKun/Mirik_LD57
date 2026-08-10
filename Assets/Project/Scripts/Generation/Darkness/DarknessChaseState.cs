using System;
using UnityEngine;

namespace Project.Scripts.Generation.Darkness
{
    [Serializable]
    public class DarknessChaseState
    {
        [SerializeField] private float _acceleration = 2f;
        [SerializeField] private float _deceleration = 1.3f;
        [SerializeField] private float _impulseSpeedChange = -15f;
        [SerializeField] private AnimationCurve _speedCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        private float _currentSpeed;
        private float _chaseY;
        private bool _hasMaxY;
        private float _maxDarknessY;

        public float ChaseY => _chaseY;
        public float CurrentSpeed => _currentSpeed;
        public bool HasMaxY => _hasMaxY;
        public float MaxDarknessY => _maxDarknessY;

        public void SetChaseY(float y) => _chaseY = y;

        public void Tick(float targetY, float deltaTime)
        {
            int accelerationSign = (int)Mathf.Sign(targetY - _chaseY);
            int speedSign = (int)Mathf.Sign(_currentSpeed);
            _currentSpeed += _acceleration * accelerationSign * deltaTime;
            if (accelerationSign != speedSign)
                _currentSpeed /= _deceleration;

            float newY = _chaseY + _currentSpeed * deltaTime;
            if (_hasMaxY && newY > _maxDarknessY)
            {
                newY = _maxDarknessY;
                if (_currentSpeed > 0f)
                    _currentSpeed = 0f;
            }

            _chaseY = newY;
        }

        public void ApplyGroundedImpulse(float targetY, float offsetMagnitude)
        {
            float distanceFactor = offsetMagnitude > 0.0001f
                ? Mathf.Abs(targetY - _chaseY) / offsetMagnitude
                : 0f;
            _currentSpeed += _impulseSpeedChange * _speedCurve.Evaluate(distanceFactor);
        }

        public void SealAt(float maxY)
        {
            _maxDarknessY = _hasMaxY ? Mathf.Min(_maxDarknessY, maxY) : maxY;
            _hasMaxY = true;
            _chaseY = maxY;
            _currentSpeed = 0f;
        }

        public float ClampTargetY(float targetY)
        {
            if (_hasMaxY)
                return Mathf.Min(targetY, _maxDarknessY);
            return targetY;
        }
    }
}
