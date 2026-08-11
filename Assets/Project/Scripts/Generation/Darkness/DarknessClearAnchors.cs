using System;
using UnityEngine;

namespace Project.Scripts.Generation.Darkness
{
    [Serializable]
    public class DarknessClearAnchors
    {
        [SerializeField] private int _maxAnchors = 48;
        [SerializeField] private float _radiusHorizontal = 140f;
        [SerializeField] private float _radiusVertical = 80f;
        [SerializeField] private float _anchorBoostWeight = 1f;
        [SerializeField] private float _trailWeight = 0.45f;
        [SerializeField] private float _decayPerSecond = 0.1f;
        [SerializeField] private float _minWeight = 0.05f;
        [SerializeField] private float _mergeDistance = 14f;
        [SerializeField] private float _growSpeed = 3.5f;
        [SerializeField] private float _landingStartWeight = 0.2f;
        [SerializeField] private float _landingStartRadiusFactor = 0.3f;
        [SerializeField] private float _followMoveSpeed = 32f;
        [SerializeField] private float _trailSpacing = 20f;
        [SerializeField] private float _trailRadiusFactor = 0.75f;

        private Anchor[] _anchors;
        private int _count;
        private int _followIndex = -1;
        private Vector3 _lastTrailDropPosition;
        private bool _hasTrailDropPosition;

        public float TargetClearRadius => Mathf.Max(_radiusHorizontal, _radiusVertical);
        public float RadiusHorizontal => _radiusHorizontal;
        public float RadiusVertical => _radiusVertical;
        public float TrailSpacing => _trailSpacing;
        public float TrailWeight => _trailWeight;
        public float TrailRadiusFactor => _trailRadiusFactor;

        public void ApplyWebProfile()
        {
            _maxAnchors = 28;
            _anchors = null;
            _count = 0;
            _followIndex = -1;
            _hasTrailDropPosition = false;
        }

        private struct Anchor
        {
            public Vector3 Position;
            public float Weight;
            public float TargetWeight;
            public float RadiusScale;
            public float TargetRadiusScale;
            public bool IsFollow;
        }

        public void EnsureInitialized()
        {
            if (_anchors != null && _anchors.Length == _maxAnchors)
                return;
            _anchors = new Anchor[_maxAnchors];
            _count = 0;
            _followIndex = -1;
        }

        public void AddGroundedAnchor(Vector3 position)
        {
            BeginFollow(position, true);
        }

        public void AddGrowingGroundedAnchor(Vector3 position)
        {
            BeginFollow(position, false);
        }

        public void SnapClear(Vector3 position)
        {
            AddOrBoost(position, _anchorBoostWeight, _anchorBoostWeight, 1f, 1f, false);
        }

        public void AddClearBurst(Vector3 position, float weight, float radiusScale)
        {
            float clampedWeight = Mathf.Max(weight, _minWeight);
            float clampedScale = Mathf.Max(0.01f, radiusScale);
            AddOrBoost(position, clampedWeight, clampedWeight, clampedScale, clampedScale, false);
        }

        public void AddClearPath(
            Vector3 from,
            Vector3 to,
            float spacing = -1f,
            float weight = -1f,
            float radiusScale = -1f)
        {
            EnsureInitialized();

            float step = spacing > 0.01f ? spacing : _trailSpacing;
            float pathWeight = weight > 0f ? weight : _anchorBoostWeight;
            float pathScale = radiusScale > 0f ? radiusScale : 1f;

            Vector3 delta = to - from;
            float length = delta.magnitude;
            if (length <= 0.01f)
            {
                AddClearBurst(from, pathWeight, pathScale);
                return;
            }

            int segments = Mathf.Max(1, Mathf.CeilToInt(length / step));
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                AddClearBurst(Vector3.Lerp(from, to, t), pathWeight, pathScale);
            }
        }

        public void TickFollow(Vector3 position, float deltaTime)
        {
            EnsureInitialized();
            if (_followIndex < 0 || _followIndex >= _count || !_anchors[_followIndex].IsFollow)
                BeginFollow(position, true);

            Anchor follow = _anchors[_followIndex];
            Vector3 previous = follow.Position;
            float moveBlend = 1f - Mathf.Exp(-_followMoveSpeed * deltaTime);
            follow.Position = Vector3.Lerp(follow.Position, position, moveBlend);
            follow.TargetWeight = Mathf.Max(follow.TargetWeight, _anchorBoostWeight);
            follow.TargetRadiusScale = Mathf.Max(follow.TargetRadiusScale, 1f);
            follow.IsFollow = true;
            _anchors[_followIndex] = follow;

            if (!_hasTrailDropPosition)
            {
                _lastTrailDropPosition = previous;
                _hasTrailDropPosition = true;
            }

            if (Vector3.Distance(follow.Position, _lastTrailDropPosition) >= _trailSpacing)
            {
                AddTrailAnchor(_lastTrailDropPosition);
                _lastTrailDropPosition = follow.Position;
            }
        }

        public void ReleaseFollow()
        {
            if (_followIndex >= 0 && _followIndex < _count)
            {
                Anchor follow = _anchors[_followIndex];
                follow.IsFollow = false;
                _anchors[_followIndex] = follow;
            }

            _followIndex = -1;
            _hasTrailDropPosition = false;
        }

        public void TickGrowth(float deltaTime)
        {
            EnsureInitialized();
            float blend = 1f - Mathf.Exp(-_growSpeed * deltaTime);
            for (int i = 0; i < _count; i++)
            {
                Anchor anchor = _anchors[i];
                anchor.Weight = Mathf.Lerp(anchor.Weight, anchor.TargetWeight, blend);
                anchor.RadiusScale = Mathf.Lerp(anchor.RadiusScale, anchor.TargetRadiusScale, blend);
                _anchors[i] = anchor;
            }
        }

        public void TickDecay(float deltaTime)
        {
            EnsureInitialized();
            float decay = _decayPerSecond * deltaTime;
            int write = 0;
            int newFollow = -1;
            for (int i = 0; i < _count; i++)
            {
                Anchor anchor = _anchors[i];
                if (!anchor.IsFollow)
                {
                    anchor.Weight -= decay;
                    anchor.TargetWeight -= decay;
                }

                if (anchor.TargetWeight < _minWeight && anchor.Weight < _minWeight)
                    continue;
                if (anchor.TargetWeight < _minWeight)
                    anchor.TargetWeight = _minWeight;

                if (anchor.IsFollow)
                    newFollow = write;
                _anchors[write++] = anchor;
            }

            _count = write;
            _followIndex = newFollow;
        }

        public float SampleClear(Vector3 worldPosition)
        {
            EnsureInitialized();
            if (_count == 0)
                return 0f;

            float clear = 0f;
            for (int i = 0; i < _count; i++)
            {
                float scale = _anchors[i].RadiusScale;
                if (scale <= 0.0001f)
                    continue;

                float radiusX = _radiusHorizontal * scale;
                float radiusY = _radiusVertical * scale;
                if (radiusX <= 0.0001f || radiusY <= 0.0001f)
                    continue;

                Vector3 delta = worldPosition - _anchors[i].Position;
                float nx = delta.x / radiusX;
                float ny = delta.y / radiusY;
                float nz = delta.z / radiusX;
                float normalizedDistSq = nx * nx + ny * ny + nz * nz;
                if (normalizedDistSq > 1f)
                    continue;

                float t = 1f - Mathf.Sqrt(normalizedDistSq);
                float falloff = t * t * (3f - 2f * t);
                clear = Mathf.Max(clear, falloff * Mathf.Clamp01(_anchors[i].Weight));
            }

            return Mathf.Clamp01(clear);
        }

        private void BeginFollow(Vector3 position, bool fullStrength)
        {
            EnsureInitialized();
            if (_followIndex >= 0 && _followIndex < _count && _anchors[_followIndex].IsFollow)
            {
                Anchor existing = _anchors[_followIndex];
                existing.Position = position;
                if (fullStrength)
                {
                    existing.Weight = Mathf.Max(existing.Weight, _anchorBoostWeight);
                    existing.TargetWeight = _anchorBoostWeight;
                    existing.RadiusScale = Mathf.Max(existing.RadiusScale, 1f);
                    existing.TargetRadiusScale = 1f;
                }

                _anchors[_followIndex] = existing;
                _lastTrailDropPosition = position;
                _hasTrailDropPosition = true;
                return;
            }

            for (int i = 0; i < _count; i++)
            {
                Anchor anchor = _anchors[i];
                anchor.IsFollow = false;
                _anchors[i] = anchor;
            }

            float startWeight = fullStrength ? _anchorBoostWeight : _landingStartWeight;
            float startScale = fullStrength ? 1f : _landingStartRadiusFactor;
            AddOrBoost(position, startWeight, _anchorBoostWeight, startScale, 1f, true);
            _lastTrailDropPosition = position;
            _hasTrailDropPosition = true;
        }

        private void AddTrailAnchor(Vector3 position)
        {
            AddOrBoost(
                position,
                _trailWeight,
                _trailWeight,
                _trailRadiusFactor,
                _trailRadiusFactor,
                false);
        }

        private void AddOrBoost(
            Vector3 position,
            float startWeight,
            float targetWeight,
            float startRadiusScale,
            float targetRadiusScale,
            bool asFollow)
        {
            EnsureInitialized();

            if (!asFollow)
            {
                for (int i = 0; i < _count; i++)
                {
                    if (_anchors[i].IsFollow)
                        continue;
                    if (Vector3.SqrMagnitude(_anchors[i].Position - position) > _mergeDistance * _mergeDistance)
                        continue;

                    Anchor merged = _anchors[i];
                    merged.Position = Vector3.Lerp(merged.Position, position, 0.35f);
                    merged.TargetWeight = Mathf.Min(2f, Mathf.Max(merged.TargetWeight, targetWeight));
                    merged.TargetRadiusScale = Mathf.Max(merged.TargetRadiusScale, targetRadiusScale);
                    if (merged.Weight < startWeight)
                        merged.Weight = startWeight;
                    _anchors[i] = merged;
                    return;
                }
            }

            Anchor created = new Anchor
            {
                Position = position,
                Weight = startWeight,
                TargetWeight = targetWeight,
                RadiusScale = startRadiusScale,
                TargetRadiusScale = targetRadiusScale,
                IsFollow = asFollow
            };

            if (_count < _maxAnchors)
            {
                if (asFollow)
                    _followIndex = _count;
                _anchors[_count++] = created;
                return;
            }

            int weakest = 0;
            float weakestWeight = float.MaxValue;
            for (int i = 0; i < _count; i++)
            {
                if (_anchors[i].IsFollow)
                    continue;
                if (_anchors[i].TargetWeight < weakestWeight)
                {
                    weakestWeight = _anchors[i].TargetWeight;
                    weakest = i;
                }
            }

            if (_anchors[weakest].IsFollow)
                return;

            _anchors[weakest] = created;
            if (asFollow)
                _followIndex = weakest;
        }
    }
}
