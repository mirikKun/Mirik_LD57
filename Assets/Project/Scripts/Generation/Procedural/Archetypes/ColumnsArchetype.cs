using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// No walls, no ceiling: prefab columns rise out of the dark, optionally with
    /// a hanging upper piece. Extras branch sideways off the path.
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Levels/Archetypes/Columns", fileName = "ColumnsArchetype")]
    public class ColumnsArchetype : LevelArchetype
    {
        [Serializable]
        public class ColumnSettings
        {
            public float Weight = 1f;
            public ColumnProtrusion Prefab;
            public FloatRange WidthRange;
            public FloatRange LowerHeightRange;
            public FloatRange UpperHeightRange;
            [Range(0f, 1f)]
            public float HideUpperChance;
            [Tooltip("Gap between the lower top and the upper bottom, meters.")]
            public FloatRange GapRange;
            public FloatRange LowerOffsetRange;
            public FloatRange UpperOffsetRange;
            public Vector3Range RotationRange;
        }

        [Header("Platforms")]
        [Tooltip("Platform size removed at difficulty 1.")]
        public float DifficultyPlatformShrink = 1f;

        [Header("Columns")]
        public List<ColumnSettings> Columns = new List<ColumnSettings>
        {
            new ColumnSettings
            {
                Weight = 0.7f,
                WidthRange = new FloatRange(3f, 6f),
                LowerHeightRange = new FloatRange(45f, 80f),
                UpperHeightRange = new FloatRange(8f, 20f),
                HideUpperChance = 0.85f,
                GapRange = new FloatRange(2f, 6f),
                LowerOffsetRange = new FloatRange(-0.5f, 0.5f),
                UpperOffsetRange = new FloatRange(-1f, 1f),
                RotationRange = new Vector3Range(new Vector3(0f, 0f, 0f), new Vector3(0f, 360f, 0f)),
            },
            new ColumnSettings
            {
                Weight = 0.3f,
                WidthRange = new FloatRange(2.5f, 5f),
                LowerHeightRange = new FloatRange(20f, 40f),
                UpperHeightRange = new FloatRange(15f, 40f),
                HideUpperChance = 0.15f,
                GapRange = new FloatRange(3f, 10f),
                LowerOffsetRange = new FloatRange(-1f, 1f),
                UpperOffsetRange = new FloatRange(-2f, 4f),
                RotationRange = new Vector3Range(new Vector3(0f, 0f, 0f), new Vector3(0f, 360f, 0f)),
            },
        };

        [Header("Extras")]
        [Tooltip("Minimum 3D distance between any two landings, meters.")]
        public float MinRange = 4f;
        [Tooltip("Maximum 3D distance from an extra to the nearest path point, meters.")]
        public float MaxPossibleRange = 8f;
        [Tooltip("Lateral offset from the path, meters (min..max). Negative = left, positive = right.")]
        public FloatRange ExtraLateralRange = new FloatRange(-6f, 6f);

        public override void BuildGeometry(LevelBuildContext ctx, List<PathPoint> path)
        {
            for (int i = 1; i < path.Count; i++)
                BuildColumn(ctx, path[i], i, isPrimary: true);

            List<PathPoint> extras = WallExtraSampler.Sample(
                path, MinRange, MaxPossibleRange, ExtraLateralRange, ctx.Rng, offsetAlongRight: true);
            for (int e = 0; e < extras.Count; e++)
                BuildColumn(ctx, extras[e], e, isPrimary: false);
        }

        private void BuildColumn(LevelBuildContext ctx, PathPoint point, int index, bool isPrimary)
        {
            ColumnSettings settings = Columns[PickColumnIndex(ctx)];
            float shrink = DifficultyPlatformShrink * ctx.Difficulty;
            float width = Mathf.Max(MinPlatformSize, ctx.Range(settings.WidthRange) - shrink);
            Vector3 lowerSize = new Vector3(width, ctx.Range(settings.LowerHeightRange), width);
            Vector3 upperSize = new Vector3(width, ctx.Range(settings.UpperHeightRange), width);
            Vector3 euler = ctx.RangeEven(settings.RotationRange);

            ColumnProtrusion instance = Instantiate(settings.Prefab, ctx.LevelElements);
            instance.name = $"{settings.Prefab.name}_{index}";
            Transform tr = instance.transform;
            tr.localPosition = point.Position;
            tr.localRotation = Quaternion.Euler(euler);
            Vector3 walkableLocal = instance.Apply(
                lowerSize,
                upperSize,
                ctx.Range(settings.GapRange),
                ctx.Range(settings.LowerOffsetRange),
                ctx.Range(settings.UpperOffsetRange),
                ctx.Chance(settings.HideUpperChance),
                ctx.Palette.StructureMaterial,
                ctx.Palette.StructureMaterial);
            RegisterWalkable(ctx, tr.localPosition + tr.localRotation * walkableLocal, index, isPrimary);
        }

        private int PickColumnIndex(LevelBuildContext ctx)
        {
            float totalWeight = 0f;
            for (int i = 0; i < Columns.Count; i++)
                totalWeight += Mathf.Max(0f, Columns[i].Weight);

            float roll = (float)ctx.Rng.NextDouble() * totalWeight;
            for (int i = 0; i < Columns.Count; i++)
            {
                roll -= Mathf.Max(0f, Columns[i].Weight);
                if (roll <= 0f)
                    return i;
            }

            return 0;
        }
    }
}
