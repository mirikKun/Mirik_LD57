using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// One colossal wall. Protrusions stick out of its face; the player descends
    /// along them to the end of the wall. The wall extends far beyond the playable
    /// area so it feels endless.
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Levels/Archetypes/Single Wall", fileName = "SingleWallArchetype")]
    public class SingleWallArchetype : LevelArchetype
    {
        [Serializable]
        public class WallProtrusionSettings
        {
            public float Weight = 1f;
            [Tooltip("X = width along the wall, Y = height, Z = length perpendicular to the wall.")]
            public Vector3Range SizeRange;
            [Tooltip("0.5 = half in the wall, 1 = fully out and just touching.")]
            public FloatRange SinkRange;
            public Vector3Range RotationRange;
        }

        private struct RolledProtrusion
        {
            public int VariantIndex;
            public Vector3 Size;
            public float Sink;
            public Vector3 Euler;
        }

        [Header("Single Wall")]
        [Tooltip("How far the wall rises above the path (min..max).")]
        public FloatRange WallHeightAboveRange = new FloatRange(35f, 60f);
        [Tooltip("How far the wall continues below the path (min..max).")]
        public FloatRange WallDepthBelowRange = new FloatRange(30f, 50f);
        [Tooltip("Wall slab thickness (min..max).")]
        public FloatRange WallThicknessRange = new FloatRange(3f, 6f);
        [Tooltip("Wall lean in degrees; positive leans over the player (min..max).")]
        public FloatRange WallTiltRange = new FloatRange(0f, 3f);
        [Tooltip("Random slab offset for a constructed, uneven wall face.")]
        public float WallRoughness = 1.2f;
        [Tooltip("Distance from the path to the wall face, meters (min..max). Rolled once per level.")]
        public FloatRange WallFaceOffsetRange = new FloatRange(4f, 7f);

        [Header("Protrusions")]
        public List<WallProtrusionSettings> Protrusions = new List<WallProtrusionSettings>
        {
            new WallProtrusionSettings
            {
                Weight = 0.65f,
                SizeRange = new Vector3Range(new Vector3(5f, 3f, 10f), new Vector3(9f, 6f, 15f)),
                SinkRange = new FloatRange(0.7f, 0.9f),
                RotationRange = new Vector3Range(new Vector3(-7f, -20f, -7f), new Vector3(7f, 20f, 7f)),
            },
            new WallProtrusionSettings
            {
                Weight = 0.35f,
                SizeRange = new Vector3Range(new Vector3(2.5f, 0.6f, 2.5f), new Vector3(9f, 2.4f, 9f)),
                SinkRange = new FloatRange(0.6f, 0.75f),
                RotationRange = new Vector3Range(new Vector3(-4f, -8f, -4f), new Vector3(4f, 8f, 4f)),
            },
        };

        [Header("Wall Construction")]
        public float WallSegmentLengthMultiplier = 1.7f;
        public IntRange SlabCountRange = new IntRange(2, 4);
        public float SlabHeightOverlap = 0.6f;

        [Header("Extras")]
        [Tooltip("Minimum 3D distance between any two landings, meters.")]
        public float MinRange = 4f;
        [Tooltip("Maximum 3D distance from an extra to the nearest path point, meters.")]
        public float MaxPossibleRange = 8f;
        [Tooltip("Height offset from the interpolated path on the wall face (min..max).")]
        public FloatRange ExtraHeightOffsetRange = new FloatRange(-3f, 3f);

        public override void BuildGeometry(LevelBuildContext ctx, List<PathPoint> path)
        {
            float sideSign = ctx.Chance(0.5f) ? -1f : 1f;
            float heightAbove = ctx.Range(WallHeightAboveRange);
            float depthBelow = ctx.Range(WallDepthBelowRange);
            float tilt = ctx.Range(WallTiltRange);
            float wallOffset = ctx.Range(WallFaceOffsetRange);

            for (int i = 0; i < path.Count; i++)
            {
                Vector3 right = Vector3.Cross(Vector3.up, path[i].Forward).normalized;
                Vector3 toWall = right * sideSign;
                RolledProtrusion primary = RollProtrusion(ctx);

                if (i > 0)
                    BuildProtrusion(ctx, path[i], toWall, primary, wallOffset, i, isPrimary: true);

                if (i < path.Count - 1)
                {
                    Vector3 mid = (path[i].Position + path[i + 1].Position) * 0.5f + toWall * wallOffset;
                    float segmentLength = Vector3.Distance(path[i].Position, path[i + 1].Position) * WallSegmentLengthMultiplier;
                    BuildWallSegment(ctx, mid, path[i].Forward, toWall, segmentLength, heightAbove, depthBelow, tilt, i);
                }
            }

            List<PathPoint> extras = WallExtraSampler.Sample(
                path, MinRange, MaxPossibleRange, ExtraHeightOffsetRange, ctx.Rng);
            for (int e = 0; e < extras.Count; e++)
            {
                Vector3 extraRight = Vector3.Cross(Vector3.up, extras[e].Forward).normalized;
                Vector3 extraToWall = extraRight * sideSign;
                BuildProtrusion(ctx, extras[e], extraToWall, RollProtrusion(ctx), wallOffset, e, isPrimary: false);
            }
        }

        private RolledProtrusion RollProtrusion(LevelBuildContext ctx)
        {
            int variantIndex = PickProtrusionIndex(ctx);
            WallProtrusionSettings settings = Protrusions[variantIndex];
            return new RolledProtrusion
            {
                VariantIndex = variantIndex,
                Size = ctx.RangeEven(settings.SizeRange),
                Sink = ctx.Range(settings.SinkRange),
                Euler = ctx.RangeEven(settings.RotationRange),
            };
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

        private void BuildProtrusion(
            LevelBuildContext ctx,
            PathPoint point,
            Vector3 toWall,
            RolledProtrusion rolled,
            float wallOffset,
            int index,
            bool isPrimary)
        {
            Quaternion rotation = Quaternion.LookRotation(toWall) * Quaternion.Euler(rolled.Euler);
            Vector3 center = point.Position
                             + toWall * (wallOffset - rolled.Size.z * (rolled.Sink - 0.5f))
                             + Vector3.down * (rolled.Size.y * 0.5f);

            string objectName = $"Protrusion{rolled.VariantIndex + 1}_{index}";
            LevelGeometry.CreateBox(
                ctx.LevelElements, objectName,
                center, rotation, rolled.Size, ctx.Palette.StructureMaterial);

            RegisterWalkable(ctx, point.Position, index, isPrimary);
        }

        private void BuildWallSegment(LevelBuildContext ctx, Vector3 faceMid, Vector3 forward, Vector3 toWall, float length, float heightAbove, float depthBelow, float tilt, int index)
        {
            int slabCount = ctx.RangeInt(SlabCountRange);
            float totalHeight = heightAbove + depthBelow;
            float slabHeight = totalHeight / slabCount;
            float bottom = faceMid.y - depthBelow;

            float leanSign = Mathf.Sign(Vector3.Dot(toWall, Vector3.Cross(Vector3.up, forward)));
            Quaternion rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(0f, 0f, tilt * leanSign);

            for (int s = 0; s < slabCount; s++)
            {
                float thickness = ctx.Range(WallThicknessRange);
                float jitterAlong = ctx.Range(-WallRoughness, WallRoughness);
                float jitterOut = ctx.Range(0f, WallRoughness);
                float centerY = bottom + slabHeight * (s + 0.5f);

                Vector3 center = faceMid + toWall * (thickness * 0.5f + jitterOut) + forward * jitterAlong;
                center.y = centerY;

                LevelGeometry.CreateBox(
                    ctx.LevelElements, $"Wall_{index}_{s}",
                    center, rotation,
                    new Vector3(thickness, slabHeight + SlabHeightOverlap, length),
                    ctx.Palette.StructureMaterial);
            }
        }
    }
}
