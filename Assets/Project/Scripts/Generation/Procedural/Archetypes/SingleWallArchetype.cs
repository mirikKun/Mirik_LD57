using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// One colossal wall. Rocks and cantilever beams protrude from its face;
    /// the player descends along them to the end of the wall. The wall extends
    /// far beyond the playable area so it feels endless.
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Levels/Archetypes/Single Wall", fileName = "SingleWallArchetype")]
    public class SingleWallArchetype : LevelArchetype
    {
        [Header("Platforms")]
        [Tooltip("Width/depth of walkable protrusions (min..max).")]
        public FloatRange PlatformSizeRange = new FloatRange(2.5f, 4.5f);
        [Tooltip("Platform size removed at difficulty 1.")]
        public float DifficultyPlatformShrink = 1f;

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
        [Tooltip("Chance a waypoint gets a rock instead of a beam.")]
        public float RockChance = 0.5f;
        [Tooltip("How far walkable protrusions stick out from the wall face (min..max).")]
        public FloatRange ProtrusionLengthRange = new FloatRange(2.5f, 4.5f);
        public FloatRange RockHeightRange = new FloatRange(1.5f, 3f);
        public FloatRange RockDepthScaleRange = new FloatRange(0.8f, 1.3f);
        public FloatRange RockTiltRange = new FloatRange(-7f, 7f);
        public float RockEmbedDownScale = 0.45f;
        public float RockEmbedIntoWallScale = 0.3f;
        public FloatRange BeamThicknessRange = new FloatRange(0.6f, 1.2f);
        public float BeamWidthScale = 0.7f;
        public float BeamInsetScale = 0.25f;

        [Header("Wall Construction")]
        public float WallSegmentLengthMultiplier = 1.7f;
        public IntRange SlabCountRange = new IntRange(2, 4);
        public float SlabHeightOverlap = 0.6f;

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
                float offset = ctx.Range(ProtrusionLengthRange);

                if (i > 0)
                {
                    bool allowBranch = i < path.Count - 1;
                    List<PathPoint> candidates = GenerateJumpCandidates(ctx, path[i - 1], path[i], allowBranch);
                    for (int c = 0; c < candidates.Count; c++)
                    {
                        bool isPrimary = c == 0;
                        Vector3 candRight = Vector3.Cross(Vector3.up, candidates[c].Forward).normalized;
                        Vector3 candToWall = candRight * sideSign;
                        float candOffset = isPrimary ? offset : ctx.Range(ProtrusionLengthRange);
                        BuildProtrusion(ctx, candidates[c], candToWall, candOffset, i, c, isPrimary);
                    }
                }

                if (i < path.Count - 1)
                {
                    Vector3 mid = (path[i].Position + path[i + 1].Position) * 0.5f + toWall * offset;
                    float segmentLength = Vector3.Distance(path[i].Position, path[i + 1].Position) * WallSegmentLengthMultiplier;
                    BuildWallSegment(ctx, mid, path[i].Forward, toWall, segmentLength, heightAbove, depthBelow, tilt, i);
                }
            }
        }

        private void BuildProtrusion(LevelBuildContext ctx, PathPoint point, Vector3 toWall, float length, int index, int candidateIndex, bool isPrimary)
        {
            float size = RollPlatformSize(ctx, PlatformSizeRange, DifficultyPlatformShrink);

            if (ctx.Chance(RockChance))
            {
                Vector3 rockSize = new Vector3(size, ctx.Range(RockHeightRange), size * ctx.Range(RockDepthScaleRange));
                Quaternion rot = Quaternion.LookRotation(point.Forward) * Quaternion.Euler(ctx.Range(RockTiltRange), ctx.Range(0f, 360f), ctx.Range(RockTiltRange));
                LevelGeometry.CreateBox(
                    ctx.LevelElements, $"Rock_{index}_{candidateIndex}",
                    point.Position + Vector3.down * (rockSize.y * RockEmbedDownScale) + toWall * (length * RockEmbedIntoWallScale),
                    rot, rockSize, ctx.Palette.StructureMaterial);
            }
            else
            {
                float thickness = ctx.Range(BeamThicknessRange);
                Vector3 beamCenter = point.Position + Vector3.down * (thickness * 0.5f) + toWall * (length * 0.5f - size * BeamInsetScale);
                LevelGeometry.CreateBox(
                    ctx.LevelElements, $"CantileverBeam_{index}_{candidateIndex}",
                    beamCenter,
                    Quaternion.LookRotation(toWall),
                    new Vector3(size * BeamWidthScale, thickness, length + size),
                    ctx.Palette.StructureMaterial);
            }

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
