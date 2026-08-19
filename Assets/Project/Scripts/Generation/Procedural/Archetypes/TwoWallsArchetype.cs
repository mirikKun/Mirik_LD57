using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// A canyon: two walls that slowly widen apart, with beams spanning between
    /// them. The player descends by jumping beam to beam.
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Levels/Archetypes/Two Walls", fileName = "TwoWallsArchetype")]
    public class TwoWallsArchetype : LevelArchetype
    {
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
        [Tooltip("Random slab offset for a constructed, uneven wall face.")]
        public float WallRoughness = 1f;

        [Header("Beams")]
        [Tooltip("Walkable beam width, i.e. the surface you land on (min..max).")]
        public FloatRange BeamWidthRange = new FloatRange(1.2f, 2.4f);
        [Tooltip("Walkable beam vertical thickness (min..max).")]
        public FloatRange BeamThicknessRange = new FloatRange(0.6f, 1.4f);
        public float BeamLengthOverhang = 3f;

        [Header("Wall Construction")]
        public float WallSegmentLengthMultiplier = 1.6f;
        public IntRange SlabCountRange = new IntRange(2, 3);
        public float SlabHeightOverlap = 0.5f;

        public override void BuildGeometry(LevelBuildContext ctx, List<PathPoint> path)
        {
            float startGap = ctx.Range(StartGapRange);
            float endGap = ctx.Range(EndGapRange);
            float heightAbove = ctx.Range(WallHeightAboveRange);
            float depthBelow = ctx.Range(WallDepthBelowRange);

            for (int i = 0; i < path.Count; i++)
            {
                float t = (float)i / (path.Count - 1);
                float gap = Mathf.Lerp(startGap, endGap, t);
                Vector3 right = Vector3.Cross(Vector3.up, path[i].Forward).normalized;

                if (i > 0)
                {
                    float beamWidth = ctx.Range(BeamWidthRange);
                    float beamThickness = ctx.Range(BeamThicknessRange);
                    LevelGeometry.CreateBox(
                        ctx.LevelElements, $"Beam_{i}",
                        path[i].Position + Vector3.down * (beamThickness * 0.5f),
                        Quaternion.LookRotation(path[i].Forward),
                        new Vector3(gap + BeamLengthOverhang, beamThickness, beamWidth),
                        ctx.Palette.StructureMaterial);
                    RegisterWalkable(ctx, path[i].Position, i);
                }

                if (i < path.Count - 1)
                {
                    Vector3 mid = (path[i].Position + path[i + 1].Position) * 0.5f;
                    Vector3 segmentForward = path[i].Forward;
                    float segmentLength = Vector3.Distance(path[i].Position, path[i + 1].Position) * WallSegmentLengthMultiplier;

                    BuildWallSegment(ctx, mid, segmentForward, right, gap * 0.5f, segmentLength, heightAbove, depthBelow, sideSign: -1f, i);
                    BuildWallSegment(ctx, mid, segmentForward, right, gap * 0.5f, segmentLength, heightAbove, depthBelow, sideSign: 1f, i);
                }
            }
        }

        private void BuildWallSegment(LevelBuildContext ctx, Vector3 pathMid, Vector3 forward, Vector3 right, float halfGap, float length, float heightAbove, float depthBelow, float sideSign, int index)
        {
            int slabCount = ctx.RangeInt(SlabCountRange);
            float totalHeight = heightAbove + depthBelow;
            float slabHeight = totalHeight / slabCount;
            float bottom = pathMid.y - depthBelow;

            for (int s = 0; s < slabCount; s++)
            {
                float thickness = ctx.Range(WallThicknessRange);
                float jitterAlong = ctx.Range(-WallRoughness, WallRoughness);
                float jitterOut = ctx.Range(0f, WallRoughness);
                float centerY = bottom + slabHeight * (s + 0.5f);

                Vector3 center = pathMid
                                 + right * sideSign * (halfGap + thickness * 0.5f + jitterOut)
                                 + forward * jitterAlong;
                center.y = centerY;

                LevelGeometry.CreateBox(
                    ctx.LevelElements, $"Wall_{index}_{(sideSign < 0 ? "L" : "R")}{s}",
                    center,
                    Quaternion.LookRotation(forward),
                    new Vector3(thickness, slabHeight + SlabHeightOverlap, length),
                    ctx.Palette.StructureMaterial);
            }
        }
    }
}
