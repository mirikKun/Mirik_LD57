using System;
using UnityEngine;

namespace Project.Scripts.Generation.Darkness
{
    [Serializable]
    public class DarknessVolumeField
    {
        [SerializeField] private int _resolution = 24;
        [SerializeField] private float _coverageDiameterFactor = 2.35f;
        [SerializeField] private float _chaseBlendRadiusFactor = 0.15f;
        [SerializeField] private float _noiseStrengthFactor = 0.045f;
        [SerializeField] private float _noiseScale = 0.028f;
        [SerializeField] private float _densitySmoothSpeed = 6f;

        private float _cellSize = 7.83f;
        private float _chaseBlendHeight = 12f;
        private float _noiseStrength = 3.6f;
        private float[] _density;
        private float[] _densityScratch;
        private Vector3 _origin;
        private int _resolutionCubed;
        private bool _hasDensity;
        private bool _domainNoiseEnabled = true;

        public int Resolution => _resolution;
        public float CellSize => _cellSize;
        public Vector3 Origin => _origin;
        public float[] Density => _density;

        public void ApplyWebProfile()
        {
            _resolution = 16;
            _coverageDiameterFactor = 2.2f;
            _noiseStrengthFactor = 0f;
            _domainNoiseEnabled = false;
            _density = null;
            _densityScratch = null;
            _hasDensity = false;
        }

        public void EnsureInitialized()
        {
            _resolutionCubed = _resolution * _resolution * _resolution;
            if (_density != null && _density.Length == _resolutionCubed)
                return;
            _density = new float[_resolutionCubed];
            _densityScratch = new float[_resolutionCubed];
            _hasDensity = false;
        }

        public void Rebuild(
            Vector3 center,
            DarknessClearAnchors anchors,
            DarknessChaseState chase,
            float deltaTime,
            bool instant = false)
        {
            EnsureInitialized();
            SyncCoverage(anchors.TargetClearRadius);

            float extent = _resolution * _cellSize;
            Vector3 rawOrigin = center - Vector3.one * (extent * 0.5f);
            Vector3 newOrigin = new Vector3(
                Mathf.Floor(rawOrigin.x / _cellSize) * _cellSize,
                Mathf.Floor(rawOrigin.y / _cellSize) * _cellSize,
                Mathf.Floor(rawOrigin.z / _cellSize) * _cellSize);

            if (_hasDensity && newOrigin != _origin)
                ShiftDensity(newOrigin);

            _origin = newOrigin;

            float blend = instant || !_hasDensity
                ? 1f
                : 1f - Mathf.Exp(-_densitySmoothSpeed * deltaTime);

            for (int z = 0; z < _resolution; z++)
            {
                for (int y = 0; y < _resolution; y++)
                {
                    for (int x = 0; x < _resolution; x++)
                    {
                        int index = Index(x, y, z);
                        float target = EvaluateDensity(CellCenter(x, y, z), anchors, chase);
                        _density[index] = blend >= 0.999f
                            ? target
                            : Mathf.Lerp(_density[index], target, blend);
                    }
                }
            }

            _hasDensity = true;
        }

        public float Sample(Vector3 worldPosition, DarknessClearAnchors anchors, DarknessChaseState chase)
        {
            return EvaluateDensity(worldPosition, anchors, chase);
        }

        public float GetDensity(int x, int y, int z) => _density[Index(x, y, z)];

        public Vector3 CellCenter(int x, int y, int z)
        {
            return _origin + new Vector3(
                (x + 0.5f) * _cellSize,
                (y + 0.5f) * _cellSize,
                (z + 0.5f) * _cellSize);
        }

        private void SyncCoverage(float clearRadius)
        {
            float safeRadius = Mathf.Max(1f, clearRadius);
            float newCellSize = safeRadius * _coverageDiameterFactor / _resolution;
            if (Mathf.Abs(newCellSize - _cellSize) > 0.001f)
            {
                _cellSize = newCellSize;
                _hasDensity = false;
            }

            _chaseBlendHeight = safeRadius * _chaseBlendRadiusFactor;
            _noiseStrength = _domainNoiseEnabled ? safeRadius * _noiseStrengthFactor : 0f;
        }

        private void ShiftDensity(Vector3 newOrigin)
        {
            int shiftX = Mathf.RoundToInt((newOrigin.x - _origin.x) / _cellSize);
            int shiftY = Mathf.RoundToInt((newOrigin.y - _origin.y) / _cellSize);
            int shiftZ = Mathf.RoundToInt((newOrigin.z - _origin.z) / _cellSize);

            for (int z = 0; z < _resolution; z++)
            {
                for (int y = 0; y < _resolution; y++)
                {
                    for (int x = 0; x < _resolution; x++)
                    {
                        int srcX = x + shiftX;
                        int srcY = y + shiftY;
                        int srcZ = z + shiftZ;
                        int dst = Index(x, y, z);
                        if (srcX < 0 || srcX >= _resolution ||
                            srcY < 0 || srcY >= _resolution ||
                            srcZ < 0 || srcZ >= _resolution)
                        {
                            _densityScratch[dst] = 1f;
                            continue;
                        }

                        _densityScratch[dst] = _density[Index(srcX, srcY, srcZ)];
                    }
                }
            }

            float[] swap = _density;
            _density = _densityScratch;
            _densityScratch = swap;
        }

        private float EvaluateDensity(Vector3 worldPosition, DarknessClearAnchors anchors, DarknessChaseState chase)
        {
            Vector3 samplePosition = worldPosition;
            if (_domainNoiseEnabled && _noiseStrength > 0.0001f)
                samplePosition += NoiseOffset(worldPosition) * _noiseStrength;

            float clear = anchors.SampleClear(samplePosition);
            float aboveChase = EdgeSmoothStep(
                chase.ChaseY - _chaseBlendHeight,
                chase.ChaseY + _chaseBlendHeight,
                samplePosition.y);

            return Mathf.Clamp01(1f - clear * aboveChase);
        }

        private static float EdgeSmoothStep(float edge0, float edge1, float x)
        {
            if (edge0 >= edge1)
                return x < edge0 ? 0f : 1f;
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private Vector3 NoiseOffset(Vector3 p)
        {
            float s = _noiseScale;
            float nx = Mathf.PerlinNoise(p.x * s + 11.2f, p.z * s + 4.7f) * 2f - 1f;
            float ny = Mathf.PerlinNoise(p.y * s + 2.3f, p.x * s + 8.1f) * 2f - 1f;
            float nz = Mathf.PerlinNoise(p.z * s + 5.9f, p.y * s + 1.4f) * 2f - 1f;
            return new Vector3(nx, ny, nz);
        }

        private int Index(int x, int y, int z) => x + _resolution * (y + _resolution * z);
    }
}
