using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// A canyon: two walls that slowly widen apart, with beams spanning between
    /// them. The player descends by jumping beam to beam. Criss-cross scenery
    /// beams above and below sell the megastructure scale.
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Levels/Archetypes/Two Walls", fileName = "TwoWallsArchetype")]
    public class TwoWallsArchetype : LevelArchetype
    {
        [Header("Two Walls")]
        [Tooltip("Gap between the walls at the level start (min..max).")]
        public Vector2 StartGapRange = new Vector2(9f, 14f);
        [Tooltip("Gap between the walls at the level end (min..max).")]
        public Vector2 EndGapRange = new Vector2(22f, 36f);
        [Tooltip("How far the walls rise above the path (min..max), multiplied by SceneryScale.")]
        public Vector2 WallHeightAboveRange = new Vector2(25f, 45f);
        [Tooltip("How far the walls continue below the path (min..max).")]
        public Vector2 WallDepthBelowRange = new Vector2(20f, 35f);
        [Tooltip("Wall slab thickness (min..max).")]
        public Vector2 WallThicknessRange = new Vector2(2.5f, 4.5f);
        [Tooltip("Random slab offset for a constructed, uneven wall face.")]
        public float WallRoughness = 1f;

        [Header("Beams")]
        [Tooltip("Walkable beam width, i.e. the surface you land on (min..max).")]
        public Vector2 BeamWidthRange = new Vector2(1.2f, 2.4f);
        [Tooltip("Walkable beam vertical thickness (min..max).")]
        public Vector2 BeamThicknessRange = new Vector2(0.6f, 1.4f);
        [Tooltip("Average decorative criss-cross beams per path step.")]
        public float CrissCrossPerStep = 1.2f;

        public override void BuildGeometry(LevelBuildContext ctx, List<PathPoint> path)
        {
            ctx.HasLeftWall = true;
            ctx.HasRightWall = true;

            float startGap = ctx.Range(StartGapRange);
            float endGap = ctx.Range(EndGapRange);
            float heightAbove = ctx.Range(WallHeightAboveRange) * SceneryScale;
            float depthBelow = ctx.Range(WallDepthBelowRange);

            for (int i = 0; i < path.Count; i++)
            {
                float t = (float)i / (path.Count - 1);
                float gap = Mathf.Lerp(startGap, endGap, t);
                Vector3 right = Vector3.Cross(Vector3.up, path[i].Forward).normalized;

                // Walkable beam(s) spanning the canyon (skip the entry platform point). Extra
                // candidates sit farther/closer along the corridor - a beam spans the full gap
                // width already, so sideways offset doesn't apply here, only distance.
                if (i > 0)
                {
                    bool allowBranch = i < path.Count - 1;
                    List<PathPoint> candidates = GenerateJumpCandidates(ctx, path[i - 1], path[i], allowBranch, Vector3.zero);
                    for (int c = 0; c < candidates.Count; c++)
                    {
                        bool isPrimary = c == 0;
                        float beamWidth = ctx.Range(BeamWidthRange) * (isPrimary ? 1f : 0.8f);
                        float beamThickness = ctx.Range(BeamThicknessRange);
                        LevelGeometry.CreateBox(
                            ctx.Root, $"Beam_{i}_{c}",
                            candidates[c].Position + Vector3.down * (beamThickness * 0.5f),
                            Quaternion.LookRotation(candidates[c].Forward),
                            new Vector3(gap + 3f, beamThickness, beamWidth),
                            ctx.Palette.StructureMaterial);
                        RegisterWalkable(ctx, candidates[c].Position, i, isPrimary);
                    }
                }

                // Wall segments on both sides of this stretch.
                if (i < path.Count - 1)
                {
                    Vector3 mid = (path[i].Position + path[i + 1].Position) * 0.5f;
                    Vector3 segmentForward = path[i].Forward;
                    float segmentLength = Vector3.Distance(path[i].Position, path[i + 1].Position) * 1.6f;

                    BuildWallSegment(ctx, mid, segmentForward, right, gap * 0.5f, segmentLength, heightAbove, depthBelow, sideSign: -1f, i);
                    BuildWallSegment(ctx, mid, segmentForward, right, gap * 0.5f, segmentLength, heightAbove, depthBelow, sideSign: 1f, i);

                    BuildCrissCross(ctx, mid, right, gap, heightAbove, depthBelow);
                }
            }
        }

        private void BuildWallSegment(LevelBuildContext ctx, Vector3 pathMid, Vector3 forward, Vector3 right, float halfGap, float length, float heightAbove, float depthBelow, float sideSign, int index)
        {
            // Two or three stacked slabs with jitter so the wall reads as built, not extruded.
            int slabCount = ctx.RangeInt(2, 4);
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
                    ctx.Root, $"Wall_{index}_{(sideSign < 0 ? "L" : "R")}{s}",
                    center,
                    Quaternion.LookRotation(forward),
                    new Vector3(thickness, slabHeight + 0.5f, length),
                    ctx.Palette.StructureMaterial);
            }
        }

        private void BuildCrissCross(LevelBuildContext ctx, Vector3 pathMid, Vector3 right, float gap, float heightAbove, float depthBelow)
        {
            int count = Mathf.RoundToInt(CrissCrossPerStep * SceneryDensity);
            if (count <= 0 && ctx.Chance(CrissCrossPerStep * SceneryDensity))
                count = 1;

            for (int i = 0; i < count; i++)
            {
                bool above = ctx.Chance(0.6f);
                float yOffset = above ? ctx.Range(7f, heightAbove * 0.9f) : -ctx.Range(4f, depthBelow * 0.9f);
                float yTilt = ctx.Range(-6f, 6f);

                Vector3 a = pathMid - right * gap * 0.5f + Vector3.up * yOffset;
                Vector3 b = pathMid + right * gap * 0.5f + Vector3.up * (yOffset + yTilt);
                Vector3 direction = b - a;

                float thickness = ctx.Range(0.5f, 1.3f);

                LevelGeometry.CreateBox(
                    ctx.Root, "CrissCrossBeam",
                    (a + b) * 0.5f,
                    Quaternion.FromToRotation(Vector3.right, direction.normalized),
                    new Vector3(direction.magnitude + 2f, thickness, thickness),
                    ctx.Palette.SceneryMaterial);
            }
        }
    }
}
