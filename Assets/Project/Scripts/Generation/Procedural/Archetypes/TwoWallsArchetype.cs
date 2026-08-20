using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// A canyon: two walls whose gap changes along the descent. Beams sit
    /// between them, biased toward the center or either wall.
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Levels/Archetypes/Two Walls", fileName = "TwoWallsArchetype")]
    public class TwoWallsArchetype : LevelArchetype
    {
        [Serializable]
        public class WallProtrusionSettings
        {
            public float Weight = 1f;
            [Tooltip("X = width along the path, Y = thickness, Z = length across the canyon.")]
            public Vector3Range SizeRange;
            [Tooltip("0 = canyon center, -1 = left wall face, 1 = right wall face.")]
            public FloatRange SideRange;
            public Vector3Range RotationRange;
        }

        private struct RolledProtrusion
        {
            public int VariantIndex;
            public Vector3 Size;
            public float Side;
            public Vector3 Euler;
        }

        [Header("Two Walls")]
        [Tooltip("Gap between the walls at the level start (min..max).")]
        public FloatRange StartGapRange = new FloatRange(9f, 14f);
        [Tooltip("Gap between the walls at the level end (min..max).")]
        public FloatRange EndGapRange = new FloatRange(22f, 36f);
        [Tooltip("How far the walls rise above the path (min..max).")]
        public FloatRange WallHeightAboveRange = new FloatRange(25f, 45f);
        [Tooltip("How far the walls continue below the path (min..max).")]
        public FloatRange WallDepthBelowRange = new FloatRange(20f, 35f);
        [Tooltip("Wall slab thickness (min..max).")]
        public FloatRange WallThicknessRange = new FloatRange(2.5f, 4.5f);
        [Tooltip("Wall lean in degrees; positive leans over the player (min..max).")]
        public FloatRange WallTiltRange = new FloatRange(0f, 3f);
        [Tooltip("Random slab offset for a constructed, uneven wall face.")]
        public float WallRoughness = 1f;

        [Header("Protrusions")]
        public List<WallProtrusionSettings> Protrusions = new List<WallProtrusionSettings>
        {
            new WallProtrusionSettings
            {
                Weight = 0.5f,
                SizeRange = new Vector3Range(new Vector3(3f, 0.8f, 8f), new Vector3(6f, 2f, 16f)),
                SideRange = new FloatRange(-0.25f, 0.25f),
                RotationRange = new Vector3Range(new Vector3(-4f, -8f, -4f), new Vector3(4f, 8f, 4f)),
            },
            new WallProtrusionSettings
            {
                Weight = 0.25f,
                SizeRange = new Vector3Range(new Vector3(3f, 1f, 6f), new Vector3(6f, 2.5f, 14f)),
                SideRange = new FloatRange(-1f, -0.4f),
                RotationRange = new Vector3Range(new Vector3(-4f, -8f, -4f), new Vector3(4f, 8f, 4f)),
            },
            new WallProtrusionSettings
            {
                Weight = 0.25f,
                SizeRange = new Vector3Range(new Vector3(3f, 1f, 6f), new Vector3(6f, 2.5f, 14f)),
                SideRange = new FloatRange(0.4f, 1f),
                RotationRange = new Vector3Range(new Vector3(-4f, -8f, -4f), new Vector3(4f, 8f, 4f)),
            },
        };

        [Header("Wall Construction")]
        public float WallSegmentLengthMultiplier = 1.6f;
        public float WallOverrun = 20f;
        public IntRange SlabCountRange = new IntRange(2, 3);
        public float SlabHeightOverlap = 0.5f;

        [Header("Extras")]
        [Tooltip("Minimum 3D distance between any two landings, meters.")]
        public float MinRange = 4f;
        [Tooltip("Maximum 3D distance from an extra to the nearest path point, meters.")]
        public float MaxPossibleRange = 8f;
        [Tooltip("Height offset from the interpolated path (min..max).")]
        public FloatRange ExtraHeightOffsetRange = new FloatRange(-3f, 3f);

        public override void BuildGeometry(LevelBuildContext ctx, List<PathPoint> path)
        {
            float startGap = ctx.Range(StartGapRange);
            float endGap = ctx.Range(EndGapRange);
            float heightAbove = ctx.Range(WallHeightAboveRange);
            float depthBelow = ctx.Range(WallDepthBelowRange);
            float tilt = ctx.Range(WallTiltRange);
            float overrunLength = WallOverrun * WallSegmentLengthMultiplier;

            Vector3 startForward = path[0].Forward;
            Vector3 startRight = Vector3.Cross(Vector3.up, startForward).normalized;
            Vector3 startMid = path[0].Position - startForward * (WallOverrun * 0.5f);
            float startHalfGap = startGap * 0.5f;
            BuildWallSegment(ctx, startMid, startForward, startRight, startHalfGap, overrunLength, heightAbove, depthBelow, tilt, -1f, -1);
            BuildWallSegment(ctx, startMid, startForward, startRight, startHalfGap, overrunLength, heightAbove, depthBelow, tilt, 1f, -1);

            for (int i = 0; i < path.Count; i++)
            {
                float t = (float)i / (path.Count - 1);
                float halfGap = Mathf.Lerp(startGap, endGap, t) * 0.5f;
                Vector3 right = Vector3.Cross(Vector3.up, path[i].Forward).normalized;

                if (i > 0)
                    BuildProtrusion(ctx, path[i], right, halfGap, RollProtrusion(ctx), i, isPrimary: true);

                if (i < path.Count - 1)
                {
                    Vector3 mid = (path[i].Position + path[i + 1].Position) * 0.5f;
                    float segmentLength = Vector3.Distance(path[i].Position, path[i + 1].Position) * WallSegmentLengthMultiplier;
                    BuildWallSegment(ctx, mid, path[i].Forward, right, halfGap, segmentLength, heightAbove, depthBelow, tilt, -1f, i);
                    BuildWallSegment(ctx, mid, path[i].Forward, right, halfGap, segmentLength, heightAbove, depthBelow, tilt, 1f, i);
                }
            }

            Vector3 endForward = path[path.Count - 1].Forward;
            Vector3 endRight = Vector3.Cross(Vector3.up, endForward).normalized;
            Vector3 endMid = path[path.Count - 1].Position + endForward * (WallOverrun * 0.5f);
            float endHalfGap = endGap * 0.5f;
            BuildWallSegment(ctx, endMid, endForward, endRight, endHalfGap, overrunLength, heightAbove, depthBelow, tilt, -1f, path.Count);
            BuildWallSegment(ctx, endMid, endForward, endRight, endHalfGap, overrunLength, heightAbove, depthBelow, tilt, 1f, path.Count);

            List<PathPoint> extras = WallExtraSampler.Sample(
                path, MinRange, MaxPossibleRange, ExtraHeightOffsetRange, ctx.Rng);
            for (int e = 0; e < extras.Count; e++)
            {
                float extraT = PathT(path, extras[e].Position);
                float extraHalfGap = Mathf.Lerp(startGap, endGap, extraT) * 0.5f;
                Vector3 extraRight = Vector3.Cross(Vector3.up, extras[e].Forward).normalized;
                BuildProtrusion(ctx, extras[e], extraRight, extraHalfGap, RollProtrusion(ctx), e, isPrimary: false);
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
                Side = ctx.Range(settings.SideRange),
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
            Vector3 right,
            float halfGap,
            RolledProtrusion rolled,
            int index,
            bool isPrimary)
        {
            float offset = rolled.Side * halfGap;
            Vector3 top = point.Position + right * offset;
            Quaternion rotation = Quaternion.LookRotation(right) * Quaternion.Euler(rolled.Euler);
            Vector3 center = top + Vector3.down * (rolled.Size.y * 0.5f);

            LevelGeometry.CreateBox(
                ctx.LevelElements, $"Protrusion{rolled.VariantIndex + 1}_{index}",
                center, rotation, rolled.Size, ctx.Palette.StructureMaterial);

            RegisterWalkable(ctx, top, index, isPrimary);
        }

        private void BuildWallSegment(
            LevelBuildContext ctx,
            Vector3 pathMid,
            Vector3 forward,
            Vector3 right,
            float halfGap,
            float length,
            float heightAbove,
            float depthBelow,
            float tilt,
            float sideSign,
            int index)
        {
            int slabCount = ctx.RangeInt(SlabCountRange);
            float totalHeight = heightAbove + depthBelow;
            float slabHeight = totalHeight / slabCount;
            float bottom = pathMid.y - depthBelow;

            Vector3 toWall = right * sideSign;
            float leanSign = Mathf.Sign(Vector3.Dot(toWall, Vector3.Cross(Vector3.up, forward)));
            Quaternion rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(0f, 0f, tilt * leanSign);

            for (int s = 0; s < slabCount; s++)
            {
                float thickness = ctx.Range(WallThicknessRange);
                float jitterAlong = ctx.Range(-WallRoughness, WallRoughness);
                float jitterOut = ctx.Range(0f, WallRoughness);
                float centerY = bottom + slabHeight * (s + 0.5f);

                Vector3 center = pathMid
                                 + toWall * (halfGap + thickness * 0.5f + jitterOut)
                                 + forward * jitterAlong;
                center.y = centerY;

                LevelGeometry.CreateBox(
                    ctx.LevelElements, $"Wall_{index}_{(sideSign < 0 ? "L" : "R")}{s}",
                    center, rotation,
                    new Vector3(thickness, slabHeight + SlabHeightOverlap, length),
                    ctx.Palette.StructureMaterial);
            }
        }

        private static float PathT(IReadOnlyList<PathPoint> path, Vector3 position)
        {
            float total = 0f;
            float closestAlong = 0f;
            float best = float.MaxValue;
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 a = path[i].Position;
                Vector3 ab = path[i + 1].Position - a;
                float length = ab.magnitude;
                float u = length > 0.0001f ? Mathf.Clamp01(Vector3.Dot(position - a, ab) / (length * length)) : 0f;
                float d = Vector3.SqrMagnitude(position - (a + ab * u));
                if (d < best)
                {
                    best = d;
                    closestAlong = total + length * u;
                }

                total += length;
            }

            return total > 0.0001f ? closestAlong / total : 0f;
        }
    }
}
