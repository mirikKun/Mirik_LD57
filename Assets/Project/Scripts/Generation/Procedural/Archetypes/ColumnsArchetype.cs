using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// No walls, no ceiling: enormous columns rising out of the dark and monoliths
    /// hanging from above. Walkable tops and side ledges dictate the way deeper
    /// and downward; oversized decorative monoliths fill the distance.
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Levels/Archetypes/Columns", fileName = "ColumnsArchetype")]
    public class ColumnsArchetype : LevelArchetype
    {
        [Header("Playable Columns")]
        [Range(0f, 1f)]
        [Tooltip("Chance a waypoint is a hanging monolith with a side ledge instead of a standing column top.")]
        public float HangingChance = 0.35f;
        [Tooltip("Standing column diameter (min..max).")]
        public Vector2 ColumnDiameterRange = new Vector2(2.5f, 5f);
        [Tooltip("How far standing columns extend below their top (min..max), multiplied by SceneryScale.")]
        public Vector2 ColumnDepthRange = new Vector2(40f, 70f);
        [Tooltip("Hanging monolith cross-section (min..max).")]
        public Vector2 MonolithWidthRange = new Vector2(3f, 7f);
        [Tooltip("How far hanging monoliths extend above the path (min..max), multiplied by SceneryScale.")]
        public Vector2 MonolithHeightRange = new Vector2(35f, 60f);

        [Header("Scenery Monoliths")]
        [Tooltip("Decorative monoliths/columns per path step, scaled by SceneryDensity.")]
        public float SceneryPerStep = 1.5f;
        [Tooltip("Lateral distance band for decorative monoliths (min..max).")]
        public Vector2 SceneryDistanceRange = new Vector2(14f, 45f);
        [Tooltip("Decorative monolith cross-section (min..max), multiplied by SceneryScale.")]
        public Vector2 SceneryWidthRange = new Vector2(4f, 14f);

        public override void BuildGeometry(LevelBuildContext ctx, List<PathPoint> path)
        {
            // No walls or ceiling of its own; EnclosingShellBuilder pushes all three far away.
            for (int i = 0; i < path.Count; i++)
            {
                if (i > 0)
                {
                    bool allowBranch = i < path.Count - 1;
                    List<PathPoint> candidates = GenerateJumpCandidates(ctx, path[i - 1], path[i], allowBranch);
                    for (int c = 0; c < candidates.Count; c++)
                    {
                        bool isPrimary = c == 0;
                        // Roll independently per candidate so a cluster can mix standing/hanging.
                        if (ctx.Chance(HangingChance))
                            BuildHangingMonolith(ctx, candidates[c], i, c, isPrimary);
                        else
                            BuildStandingColumn(ctx, candidates[c], i, c, isPrimary);
                    }
                }

                BuildSceneryAround(ctx, path, i);
            }
        }

        private void BuildStandingColumn(LevelBuildContext ctx, PathPoint point, int index, int candidateIndex, bool isPrimary)
        {
            float diameter = Mathf.Max(RollPlatformSize(ctx), ctx.Range(ColumnDiameterRange));
            float depth = ctx.Range(ColumnDepthRange) * SceneryScale;

            LevelGeometry.CreateCylinder(
                ctx.LevelElements, $"Column_{index}_{candidateIndex}",
                point.Position + Vector3.down * (depth * 0.5f),
                Quaternion.identity,
                diameter, depth, ctx.Palette.StructureMaterial);

            // Slightly wider capital so the landing surface is generous.
            LevelGeometry.CreateCylinder(
                ctx.LevelElements, $"ColumnCap_{index}_{candidateIndex}",
                point.Position + Vector3.down * 0.4f,
                Quaternion.identity,
                diameter * 1.25f, 0.8f, ctx.Palette.PlatformMaterial);

            RegisterWalkable(ctx, point.Position, index, isPrimary);
        }

        private void BuildHangingMonolith(LevelBuildContext ctx, PathPoint point, int index, int candidateIndex, bool isPrimary)
        {
            float width = ctx.Range(MonolithWidthRange);
            float height = ctx.Range(MonolithHeightRange) * SceneryScale;
            Vector3 right = Vector3.Cross(Vector3.up, point.Forward).normalized;
            float ledgeSize = RollPlatformSize(ctx);

            // Monolith body hangs beside the path; its ledge is the walkable spot.
            float sideSign = ctx.Chance(0.5f) ? -1f : 1f;
            Vector3 bodyCenter = point.Position
                                 + right * sideSign * (width * 0.5f + ledgeSize * 0.35f)
                                 + Vector3.up * (height * 0.5f - 2f);

            LevelGeometry.CreateBox(
                ctx.LevelElements, $"Monolith_{index}_{candidateIndex}",
                bodyCenter,
                Quaternion.LookRotation(point.Forward) * Quaternion.Euler(0f, ctx.Range(-8f, 8f), 0f),
                new Vector3(width, height, width * ctx.Range(0.9f, 1.6f)),
                ctx.Palette.StructureMaterial);

            // Side ledge at path height.
            LevelGeometry.CreateBox(
                ctx.LevelElements, $"MonolithLedge_{index}_{candidateIndex}",
                point.Position + Vector3.down * 0.35f,
                Quaternion.LookRotation(point.Forward),
                new Vector3(ledgeSize * 1.4f, 0.7f, ledgeSize),
                ctx.Palette.PlatformMaterial);

            RegisterWalkable(ctx, point.Position, index, isPrimary);
        }

        private void BuildSceneryAround(LevelBuildContext ctx, List<PathPoint> path, int index)
        {
            int count = Mathf.RoundToInt(SceneryPerStep * SceneryDensity);
            for (int i = 0; i < count; i++)
            {
                Vector3 anchor = ctx.OnRing(path[index].Position, SceneryDistanceRange.x, SceneryDistanceRange.y);

                // Never let scenery crowd the playable line.
                if (DistanceToPath(path, anchor) < SceneryDistanceRange.x * 0.8f)
                    continue;

                float width = ctx.Range(SceneryWidthRange) * SceneryScale;
                float height = ctx.Range(50f, 120f) * SceneryScale;
                bool hanging = ctx.Chance(0.45f);
                // Hanging: bottom edge floats above the path. Standing: top edge ends near path level.
                float yCenter = hanging
                    ? ctx.Range(5f, 25f) + height * 0.5f
                    : ctx.Range(-20f, 5f) - height * 0.5f;

                LevelGeometry.CreateBox(
                    ctx.LevelElements, hanging ? "SceneryMonolith" : "SceneryColumn",
                    anchor + Vector3.up * yCenter,
                    Quaternion.Euler(0f, ctx.Range(0f, 360f), 0f),
                    new Vector3(width, height, width * ctx.Range(0.8f, 1.4f)),
                    ctx.Palette.SceneryMaterial,
                    withCollider: true);
            }
        }

        private static float DistanceToPath(List<PathPoint> path, Vector3 position)
        {
            float min = float.MaxValue;
            foreach (PathPoint p in path)
            {
                Vector2 a = new Vector2(p.Position.x, p.Position.z);
                Vector2 b = new Vector2(position.x, position.z);
                min = Mathf.Min(min, Vector2.Distance(a, b));
            }
            return min;
        }
    }
}
