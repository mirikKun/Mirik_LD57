using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// Only a ceiling, no walls: a vast slab tilted slightly downward. Stalactites
    /// capped with platforms and hanging mushrooms grow from it; the player jumps
    /// between them following the ceiling into the depth.
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Levels/Archetypes/Ceiling", fileName = "CeilingArchetype")]
    public class CeilingArchetype : LevelArchetype
    {
        [Header("Ceiling")]
        [Tooltip("Clearance between the path and the ceiling (min..max).")]
        public Vector2 ClearanceRange = new Vector2(10f, 18f);
        [Tooltip("Ceiling slab width to each side of the path (min..max), multiplied by SceneryScale.")]
        public Vector2 CeilingHalfWidthRange = new Vector2(18f, 30f);
        [Tooltip("Ceiling slab thickness (min..max).")]
        public Vector2 CeilingThicknessRange = new Vector2(2f, 5f);

        [Header("Hanging Growth")]
        [Range(0f, 1f)]
        [Tooltip("Chance a waypoint grows a mushroom instead of a stalactite.")]
        public float MushroomChance = 0.35f;
        [Tooltip("Stalactite/stem diameter (min..max).")]
        public Vector2 StemDiameterRange = new Vector2(0.8f, 1.8f);
        [Tooltip("Mushroom cap thickness (min..max).")]
        public Vector2 MushroomCapThicknessRange = new Vector2(0.5f, 1f);
        [Tooltip("Decorative stalactites per path step, scaled by SceneryDensity.")]
        public float DecorStalactitesPerStep = 2.5f;

        /// <summary>
        /// The incoming shaft must pierce below the ceiling slab; use the worst-case
        /// clearance/thickness rolls plus a margin so the exit is always in open air.
        /// </summary>
        public override float RollEntryOverheadHeight(LevelBuildContext ctx)
            => ClearanceRange.y + CeilingThicknessRange.y + 2f;

        public override void BuildGeometry(LevelBuildContext ctx, List<PathPoint> path)
        {
            ctx.HasCeiling = true;

            float clearance = ctx.Range(ClearanceRange);
            float halfWidth = ctx.Range(CeilingHalfWidthRange) * SceneryScale;

            for (int i = 0; i < path.Count; i++)
            {
                Vector3 ceilingPoint = path[i].Position + Vector3.up * clearance;

                // Extra candidates spread sideways under the ceiling slab (well within
                // its half-width), so branching reads as "which growth to jump to".
                if (i > 0)
                {
                    bool allowBranch = i < path.Count - 1;
                    List<PathPoint> candidates = GenerateJumpCandidates(ctx, path[i - 1], path[i], allowBranch);
                    for (int c = 0; c < candidates.Count; c++)
                    {
                        bool isPrimary = c == 0;
                        Vector3 candCeiling = candidates[c].Position + Vector3.up * clearance;
                        BuildHangingPlatform(ctx, candidates[c], candCeiling, i, c, isPrimary);
                    }
                }

                // Segment 0 is skipped: the entry shaft descends right through that spot,
                // and a ceiling slab there would cross the shaft interior and block the fall.
                if (i > 0 && i < path.Count - 1)
                {
                    Vector3 nextCeiling = path[i + 1].Position + Vector3.up * clearance;
                    BuildCeilingSegment(ctx, ceilingPoint, nextCeiling, path[i].Forward, halfWidth, i);
                    if (ctx.GenerateDecorations)
                        BuildDecorStalactites(ctx, ceilingPoint, nextCeiling, path[i].Forward, halfWidth);
                }
            }
        }

        private void BuildHangingPlatform(LevelBuildContext ctx, PathPoint point, Vector3 ceilingPoint, int index, int candidateIndex, bool isPrimary)
        {
            float platformSize = RollPlatformSize(ctx);
            float stemDiameter = ctx.Range(StemDiameterRange);
            float hangLength = ceilingPoint.y - point.Position.y;

            if (ctx.Chance(MushroomChance))
            {
                // Mushroom: thin stem from the ceiling, wide flat cap at the bottom.
                float capThickness = ctx.Range(MushroomCapThicknessRange);
                LevelGeometry.CreateCylinder(
                    ctx.LevelElements, $"MushroomStem_{index}_{candidateIndex}",
                    point.Position + Vector3.up * (hangLength * 0.5f + capThickness),
                    Quaternion.identity,
                    stemDiameter * 0.6f, hangLength, ctx.Palette.StructureMaterial);
                LevelGeometry.CreateCylinder(
                    ctx.LevelElements, $"MushroomCap_{index}_{candidateIndex}",
                    point.Position + Vector3.down * (capThickness * 0.5f),
                    Quaternion.identity,
                    platformSize * 1.2f, capThickness, ctx.Palette.AccentMaterial);
            }
            else
            {
                // Stalactite: stacked shrinking sections, platform cap at the tip.
                int sections = ctx.RangeInt(2, 4);
                float sectionHeight = (hangLength - 0.6f) / sections;
                for (int s = 0; s < sections; s++)
                {
                    float t = (float)s / sections;
                    float diameter = Mathf.Lerp(stemDiameter * 2.2f, stemDiameter, t);
                    float centerY = ceilingPoint.y - sectionHeight * (s + 0.5f);
                    LevelGeometry.CreateBox(
                        ctx.LevelElements, $"Stalactite_{index}_{candidateIndex}_{s}",
                        new Vector3(point.Position.x, centerY, point.Position.z),
                        Quaternion.Euler(0f, ctx.Range(0f, 90f), 0f),
                        new Vector3(diameter, sectionHeight + 0.3f, diameter),
                        ctx.Palette.StructureMaterial);
                }

                LevelGeometry.CreateBox(
                    ctx.LevelElements, $"StalactitePlatform_{index}_{candidateIndex}",
                    point.Position + Vector3.down * 0.3f,
                    Quaternion.Euler(0f, ctx.Range(0f, 360f), 0f),
                    new Vector3(platformSize, 0.6f, platformSize),
                    ctx.Palette.PlatformMaterial);
            }

            RegisterWalkable(ctx, point.Position, index, isPrimary);
        }

        private void BuildCeilingSegment(LevelBuildContext ctx, Vector3 from, Vector3 to, Vector3 forward, float halfWidth, int index)
        {
            Vector3 mid = (from + to) * 0.5f;
            Vector3 delta = to - from;
            float length = delta.magnitude * 1.5f;
            float thickness = ctx.Range(CeilingThicknessRange);

            Quaternion rotation = Quaternion.LookRotation(delta.normalized);
            LevelGeometry.CreateBox(
                ctx.LevelElements, $"Ceiling_{index}",
                mid + Vector3.up * (thickness * 0.5f),
                rotation,
                new Vector3(halfWidth * 2f, thickness, length),
                ctx.Palette.StructureMaterial);
        }

        private void BuildDecorStalactites(LevelBuildContext ctx, Vector3 from, Vector3 to, Vector3 forward, float halfWidth)
        {
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            int count = Mathf.RoundToInt(DecorStalactitesPerStep * SceneryDensity);
            for (int i = 0; i < count; i++)
            {
                Vector3 anchor = Vector3.Lerp(from, to, ctx.Range(0f, 1f))
                                 + right * ctx.Range(-halfWidth * 0.9f, halfWidth * 0.9f);
                // Keep decorations away from the walkable line.
                if (Mathf.Abs(Vector3.Dot(anchor - from, right)) < 3.5f)
                    continue;

                float length = ctx.Range(2f, 14f) * SceneryScale;
                float diameter = ctx.Range(0.6f, 2.4f);
                LevelGeometry.CreateBox(
                    ctx.Decorations, "DecorStalactite",
                    anchor + Vector3.down * (length * 0.5f),
                    Quaternion.Euler(ctx.Range(-4f, 4f), ctx.Range(0f, 360f), ctx.Range(-4f, 4f)),
                    new Vector3(diameter, length, diameter),
                    ctx.Palette.SceneryMaterial);
            }
        }
    }
}
