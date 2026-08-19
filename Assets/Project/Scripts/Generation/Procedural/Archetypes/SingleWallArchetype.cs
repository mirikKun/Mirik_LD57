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
        public struct WallProtrusionSettings
        {
            [Tooltip("X = width along the wall, Y = height, Z = length perpendicular to the wall.")]
            public Vector3Range SizeRange;
            [Tooltip("0.5 = half in the wall, 1 = fully out and just touching.")]
            public FloatRange SinkRange;
            public Vector3Range RotationRange;
        }

        private struct RolledProtrusion
        {
            public bool IsFirst;
            public Vector3 Size;
            public float Sink;
            public Vector3 Euler;
            public float WallOffset;
        }

        [Header("Single Wall")]
        [Tooltip("How far the wall rises above the path (min..max).")]
        public FloatRange WallHeightAboveRange = new FloatRange(35f, 60f);
        [Tooltip("How far the wall continues below the path (min..max).")]
        public FloatRange WallDepthBelowRange = new FloatRange(30f, 50f);
        [Tooltip("Wall slab thickness (min..max).")]
        public FloatRange WallThicknessRange = new FloatRange(3f, 6f);
        [Tooltip("Wall lean in degrees; positive leans over the player (min..max).")]
        public FloatRange WallTiltRange = new FloatRange(0f, 6f);
        [Tooltip("Random slab offset for a constructed, uneven wall face.")]
        public float WallRoughness = 1.2f;

        [Header("Protrusions")]
        [Range(0f, 1f)]
        [Tooltip("Chance a waypoint uses Protrusion1 instead of Protrusion2.")]
        public float Protrusion1Chance = 0.5f;
        public WallProtrusionSettings Protrusion1 = new WallProtrusionSettings
        {
            SizeRange = new Vector3Range(new Vector3(2.5f, 0.6f, 2.5f), new Vector3(4.5f, 1.5f, 4.5f)),
            SinkRange = new FloatRange(0.5f, 1f),
            RotationRange = new Vector3Range(new Vector3(-7f, -15f, -7f), new Vector3(7f, 15f, 7f)),
        };
        public WallProtrusionSettings Protrusion2 = new WallProtrusionSettings
        {
            SizeRange = new Vector3Range(new Vector3(2.5f, 0.6f, 2.5f), new Vector3(4.5f, 1.5f, 4.5f)),
            SinkRange = new FloatRange(0.5f, 1f),
            RotationRange = new Vector3Range(new Vector3(-7f, -15f, -7f), new Vector3(7f, 15f, 7f)),
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

            for (int i = 0; i < path.Count; i++)
            {
                Vector3 right = Vector3.Cross(Vector3.up, path[i].Forward).normalized;
                Vector3 toWall = right * sideSign;
                RolledProtrusion primary = RollProtrusion(ctx);

                if (i > 0)
                    BuildProtrusion(ctx, path[i], toWall, primary, i, isPrimary: true);

                if (i < path.Count - 1)
                {
                    Vector3 mid = (path[i].Position + path[i + 1].Position) * 0.5f + toWall * primary.WallOffset;
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
                BuildProtrusion(ctx, extras[e], extraToWall, RollProtrusion(ctx), e, isPrimary: false);
            }
        }

        private RolledProtrusion RollProtrusion(LevelBuildContext ctx)
        {
            bool isFirst = ctx.Chance(Protrusion1Chance);
            WallProtrusionSettings settings = isFirst ? Protrusion1 : Protrusion2;
            Vector3 size = ctx.RangeEven(settings.SizeRange);
            float sink = ctx.Range(settings.SinkRange);
            return new RolledProtrusion
            {
                IsFirst = isFirst,
                Size = size,
                Sink = sink,
                Euler = ctx.RangeEven(settings.RotationRange),
                WallOffset = size.z * sink * 0.5f,
            };
        }

        private void BuildProtrusion(
            LevelBuildContext ctx,
            PathPoint point,
            Vector3 toWall,
            RolledProtrusion rolled,
            int index,
            bool isPrimary)
        {
            Quaternion rotation = Quaternion.LookRotation(toWall) * Quaternion.Euler(rolled.Euler);
            Vector3 center = point.Position
                             + toWall * (rolled.Size.z * 0.5f * (1f - rolled.Sink))
                             + Vector3.down * (rolled.Size.y * 0.5f);

            string objectName = rolled.IsFirst
                ? $"Protrusion1_{index}"
                : $"Protrusion2_{index}";
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
