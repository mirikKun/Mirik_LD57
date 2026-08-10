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
        [Header("Single Wall")]
        [Tooltip("How far the wall rises above the path (min..max), multiplied by SceneryScale.")]
        public Vector2 WallHeightAboveRange = new Vector2(35f, 60f);
        [Tooltip("How far the wall continues below the path (min..max), multiplied by SceneryScale.")]
        public Vector2 WallDepthBelowRange = new Vector2(30f, 50f);
        [Tooltip("Wall slab thickness (min..max).")]
        public Vector2 WallThicknessRange = new Vector2(3f, 6f);
        [Tooltip("Wall lean in degrees; positive leans over the player (min..max).")]
        public Vector2 WallTiltRange = new Vector2(0f, 6f);
        [Tooltip("Random slab offset for a constructed, uneven wall face.")]
        public float WallRoughness = 1.2f;

        [Header("Protrusions")]
        [Range(0f, 1f)]
        [Tooltip("Chance a waypoint gets a rock instead of a beam.")]
        public float RockChance = 0.5f;
        [Tooltip("How far walkable protrusions stick out from the wall face (min..max).")]
        public Vector2 ProtrusionLengthRange = new Vector2(2.5f, 4.5f);
        [Tooltip("Decorative protrusions per path step, scaled by SceneryDensity.")]
        public float DecorPerStep = 2f;

        public override void BuildGeometry(LevelBuildContext ctx, List<PathPoint> path)
        {
            float sideSign = ctx.Chance(0.5f) ? -1f : 1f;
            if (sideSign < 0f)
                ctx.HasLeftWall = true;
            else
                ctx.HasRightWall = true;

            float heightAbove = ctx.Range(WallHeightAboveRange) * SceneryScale;
            float depthBelow = ctx.Range(WallDepthBelowRange) * SceneryScale;
            float tilt = ctx.Range(WallTiltRange);

            for (int i = 0; i < path.Count; i++)
            {
                Vector3 right = Vector3.Cross(Vector3.up, path[i].Forward).normalized;
                Vector3 toWall = right * sideSign;
                float offset = ctx.Range(ProtrusionLengthRange);
                Vector3 wallFace = path[i].Position + toWall * offset;

                // Extra candidates get their own protrusion offset from the wall face, so
                // some read as a farther, more exposed reach along the same wall.
                if (i > 0)
                {
                    bool allowBranch = i < path.Count - 1;
                    List<PathPoint> candidates = GenerateJumpCandidates(ctx, path[i - 1], path[i], allowBranch, Vector3.zero);
                    for (int c = 0; c < candidates.Count; c++)
                    {
                        bool isPrimary = c == 0;
                        Vector3 candRight = Vector3.Cross(Vector3.up, candidates[c].Forward).normalized;
                        Vector3 candToWall = candRight * sideSign;
                        float candOffset = isPrimary ? offset : ctx.Range(ProtrusionLengthRange);
                        Vector3 candWallFace = candidates[c].Position + candToWall * candOffset;
                        BuildProtrusion(ctx, candidates[c], candWallFace, candToWall, candOffset, i, c, isPrimary);
                    }
                }

                if (i < path.Count - 1)
                {
                    Vector3 mid = (path[i].Position + path[i + 1].Position) * 0.5f + toWall * offset;
                    float segmentLength = Vector3.Distance(path[i].Position, path[i + 1].Position) * 1.7f;
                    BuildWallSegment(ctx, mid, path[i].Forward, toWall, segmentLength, heightAbove, depthBelow, tilt, i);
                    if (ctx.GenerateDecorations)
                        BuildDecorProtrusions(ctx, mid, path[i].Forward, toWall, segmentLength, heightAbove, depthBelow);
                }
            }
        }

        private void BuildProtrusion(LevelBuildContext ctx, PathPoint point, Vector3 wallFace, Vector3 toWall, float length, int index, int candidateIndex, bool isPrimary)
        {
            float size = RollPlatformSize(ctx);

            if (ctx.Chance(RockChance))
            {
                // Irregular rock jammed into the wall, top roughly flat at path height.
                Vector3 rockSize = new Vector3(size, ctx.Range(1.5f, 3f), size * ctx.Range(0.8f, 1.3f));
                Quaternion rot = Quaternion.LookRotation(point.Forward) * Quaternion.Euler(ctx.Range(-7f, 7f), ctx.Range(0f, 360f), ctx.Range(-7f, 7f));
                LevelGeometry.CreateBox(
                    ctx.LevelElements, $"Rock_{index}_{candidateIndex}",
                    point.Position + Vector3.down * (rockSize.y * 0.45f) + toWall * (length * 0.3f),
                    rot, rockSize, ctx.Palette.StructureMaterial);
            }
            else
            {
                // Cantilever beam sticking straight out of the wall.
                float thickness = ctx.Range(0.6f, 1.2f);
                Vector3 beamCenter = point.Position + Vector3.down * (thickness * 0.5f) + toWall * (length * 0.5f - size * 0.25f);
                LevelGeometry.CreateBox(
                    ctx.LevelElements, $"CantileverBeam_{index}_{candidateIndex}",
                    beamCenter,
                    Quaternion.LookRotation(toWall),
                    new Vector3(size * 0.7f, thickness, length + size),
                    ctx.Palette.StructureMaterial);
            }

            RegisterWalkable(ctx, point.Position, index, isPrimary);
        }

        private void BuildWallSegment(LevelBuildContext ctx, Vector3 faceMid, Vector3 forward, Vector3 toWall, float length, float heightAbove, float depthBelow, float tilt, int index)
        {
            int slabCount = ctx.RangeInt(2, 5);
            float totalHeight = heightAbove + depthBelow;
            float slabHeight = totalHeight / slabCount;
            float bottom = faceMid.y - depthBelow;

            // Roll around the forward axis so the wall leans over the path.
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
                    new Vector3(thickness, slabHeight + 0.6f, length),
                    ctx.Palette.StructureMaterial);
            }
        }

        private void BuildDecorProtrusions(LevelBuildContext ctx, Vector3 faceMid, Vector3 forward, Vector3 toWall, float length, float heightAbove, float depthBelow)
        {
            int count = Mathf.RoundToInt(DecorPerStep * SceneryDensity);
            for (int i = 0; i < count; i++)
            {
                float y = ctx.Chance(0.5f) ? ctx.Range(5f, heightAbove * 0.8f) : -ctx.Range(3f, depthBelow * 0.8f);
                Vector3 pos = faceMid + forward * ctx.Range(-length * 0.4f, length * 0.4f) + Vector3.up * y - toWall * ctx.Range(0.5f, 2.5f);
                Vector3 size = new Vector3(ctx.Range(0.8f, 2.5f), ctx.Range(0.8f, 2.5f), ctx.Range(1.5f, 4f));
                LevelGeometry.CreateBox(
                    ctx.Decorations, "WallDecor",
                    pos,
                    Quaternion.LookRotation(-toWall) * Quaternion.Euler(ctx.Range(-10f, 10f), ctx.Range(-10f, 10f), ctx.Range(0f, 360f)),
                    size, ctx.Palette.SceneryMaterial);
            }
        }
    }
}
