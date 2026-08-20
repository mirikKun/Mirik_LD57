using System;
using UnityEngine;

namespace Project.Scripts.Generation.Darkness
{
    [Serializable]
    public class DarknessChaseState
    {
        private float _chaseY;
        private bool _hasMaxY;
        private float _maxDarknessY;

        public float ChaseY => _chaseY;

        public void SetChaseY(float y) => _chaseY = y;

        public void Tick(float targetY)
        {
            _chaseY = ClampTargetY(targetY);
        }

        public void SealAt(float maxY)
        {
            _maxDarknessY = _hasMaxY ? Mathf.Min(_maxDarknessY, maxY) : maxY;
            _hasMaxY = true;
            _chaseY = maxY;
        }

        public void SnapBelow(float y)
        {
            if (_chaseY <= y)
                return;

            _chaseY = y;
        }

        public float ClampTargetY(float targetY)
        {
            if (_hasMaxY)
                return Mathf.Min(targetY, _maxDarknessY);
            return targetY;
        }
    }
}
