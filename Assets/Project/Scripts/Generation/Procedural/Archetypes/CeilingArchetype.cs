using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// Only a ceiling, no walls: a vast slab over the descent. Prefab protrusions
    /// hang from it; extras branch sideways under the slab.
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Levels/Archetypes/Ceiling", fileName = "CeilingArchetype")]
    public class CeilingArchetype : LevelArchetype
    {
        [Serializable]
        public class CeilingProtrusionSettings
        {
            public float Weight = 1f;
            public CeilingProtrusion Prefab;
            [Tooltip("X = width, Y = thickness, Z = depth.")]
            public Vector3Range PlatformSizeRange;
            [Tooltip("X/Z = column cross-section. Y is extra height added on top of clearance.")]
            public Vector3Range ColumnSizeRange;
        }

        [Header("Platforms")]
        [Tooltip("Platform size removed at difficulty 1.")]
        public float DifficultyPlatformShrink = 1f;

        [Header("Ceiling")]
        [Tooltip("Clearance between the path and the ceiling (min..max).")]
        public FloatRange ClearanceRange = new FloatRange(10f, 18f);
        [Tooltip("Ceiling slab width to each side of the path (min..max).")]
        public FloatRange CeilingHalfWidthRange = new FloatRange(18f, 30f);
        [Tooltip("Ceiling slab thickness (min..max).")]
        public FloatRange CeilingThicknessRange = new FloatRange(2f, 5f);
        public float CeilingSegmentLengthMultiplier = 1.5f;
        public float EntryOverheadMargin = 2f;

        [Header("Protrusions")]
        public List<CeilingProtrusionSettings> Protrusions = new List<CeilingProtrusionSettings>
        {
            new CeilingProtrusionSettings
            {
                Weight = 0.7f,
                PlatformSizeRange = new Vector3Range(new Vector3(5f, 0.5f, 5f), new Vector3(8f, 0.8f, 8f)),
                ColumnSizeRange = new Vector3Range(new Vector3(0.8f, 1f, 0.8f), new Vector3(1.8f, 1f, 1.8f)),
            },
            new CeilingProtrusionSettings
            {
                Weight = 0.3f,
                PlatformSizeRange = new Vector3Range(new Vector3(6f, 0.4f, 6f), new Vector3(10f, 1f, 10f)),
                ColumnSizeRange = new Vector3Range(new Vector3(1.2f, 1f, 1.2f), new Vector3(2.4f, 1f, 2.4f)),
            },
        };

        [Header("Extras")]
        [Tooltip("Minimum 3D distance between any two landings, meters.")]
        public float MinRange = 4f;
        [Tooltip("Maximum 3D distance from an extra to the nearest path point, meters.")]
        public float MaxPossibleRange = 8f;
        [Tooltip("Lateral offset from the path, meters (min..max). Negative = left, positive = right.")]
        public FloatRange ExtraLateralRange = new FloatRange(-6f, 6f);

        public override float RollEntryOverheadHeight(LevelBuildContext ctx)
            => ClearanceRange.y + CeilingThicknessRange.y + EntryOverheadMargin;

        public override void BuildGeometry(LevelBuildContext ctx, List<PathPoint> path)
        {
            float clearance = ctx.Range(ClearanceRange);
            float halfWidth = ctx.Range(CeilingHalfWidthRange);

            for (int i = 0; i < path.Count; i++)
            {
                Vector3 ceilingPoint = path[i].Position + Vector3.up * clearance;

                if (i > 0)
                    BuildProtrusion(ctx, path[i], ceilingPoint, i, isPrimary: true);

                if (i > 0 && i < path.Count - 1)
                {
                    Vector3 nextCeiling = path[i + 1].Position + Vector3.up * clearance;
                    BuildCeilingSegment(ctx, ceilingPoint, nextCeiling, halfWidth, i);
                }
            }

            List<PathPoint> extras = WallExtraSampler.Sample(
                path, MinRange, MaxPossibleRange, ExtraLateralRange, ctx.Rng, offsetAlongRight: true);
            for (int e = 0; e < extras.Count; e++)
            {
                Vector3 extraCeiling = extras[e].Position + Vector3.up * clearance;
                BuildProtrusion(ctx, extras[e], extraCeiling, e, isPrimary: false);
            }
        }

        private void BuildProtrusion(LevelBuildContext ctx, PathPoint point, Vector3 ceilingPoint, int index, bool isPrimary)
        {
            CeilingProtrusionSettings settings = Protrusions[PickProtrusionIndex(ctx)];
            Vector3 platformSize = ctx.RangeEven(settings.PlatformSizeRange);
            float shrink = DifficultyPlatformShrink * ctx.Difficulty;
            platformSize.x = Mathf.Max(MinPlatformSize, platformSize.x - shrink);
            platformSize.z = Mathf.Max(MinPlatformSize, platformSize.z - shrink);
            Vector3 columnSize = ctx.RangeEven(settings.ColumnSizeRange);
            float height = ceilingPoint.y - point.Position.y;

            CeilingProtrusion instance = Instantiate(settings.Prefab, ctx.LevelElements);
            instance.name = $"{settings.Prefab.name}_{index}";
            Transform tr = instance.transform;
            tr.localPosition = ceilingPoint;
            tr.localRotation = Quaternion.identity;
            Vector3 walkableLocal = instance.Apply(
                height, platformSize, columnSize,
                ctx.Palette.StructureMaterial, ctx.Palette.PlatformMaterial);
            RegisterWalkable(ctx, tr.localPosition + tr.localRotation * walkableLocal, index, isPrimary);
        }

        private int PickProtrusionIndex(LevelBuildContext ctx)
        {
            float totalWeight = 0f;
            for (int i = 0; i < Protrusions.Count; i++)
                totalWeight += Mathf.Max(0f, Protrusions[i].Weight);

            float roll = (float)ctx.Rng.NextDouble() * totalWeight;
            for (int i = 0; i < Protrusions.Count; i++)
            {
                roll -= Mathf.Max(0f, Protrusions[i].Weight);
                if (roll <= 0f)
                    return i;
            }

            return 0;
        }

        private void BuildCeilingSegment(LevelBuildContext ctx, Vector3 from, Vector3 to, float halfWidth, int index)
        {
            Vector3 mid = (from + to) * 0.5f;
            Vector3 delta = to - from;
            float length = delta.magnitude * CeilingSegmentLengthMultiplier;
            float thickness = ctx.Range(CeilingThicknessRange);

            Quaternion rotation = Quaternion.LookRotation(delta.normalized);
            LevelGeometry.CreateBox(
                ctx.LevelElements, $"Ceiling_{index}",
                mid + Vector3.up * (thickness * 0.5f),
                rotation,
                new Vector3(halfWidth * 2f, thickness, length),
                ctx.Palette.StructureMaterial);
        }
    }
}
