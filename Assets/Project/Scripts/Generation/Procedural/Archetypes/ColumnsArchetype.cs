using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// No walls, no ceiling: enormous columns rising out of the dark and monoliths
    /// hanging from above. Walkable tops and side ledges dictate the way deeper
    /// and downward.
    /// </summary>
    [CreateAssetMenu(menuName = "Procedural Levels/Archetypes/Columns", fileName = "ColumnsArchetype")]
    public class ColumnsArchetype : LevelArchetype
    {
        [Header("Platforms")]
        [Tooltip("Width of walkable column caps and monolith ledges (min..max).")]
        public FloatRange PlatformSizeRange = new FloatRange(2.5f, 4.5f);
        [Tooltip("Platform size removed at difficulty 1.")]
        public float DifficultyPlatformShrink = 1f;

        [Header("Playable Columns")]
        [Range(0f, 1f)]
        [Tooltip("Chance a waypoint is a hanging monolith with a side ledge instead of a standing column top.")]
        public float HangingChance = 0.35f;
        [Tooltip("Standing column diameter (min..max).")]
        public FloatRange ColumnDiameterRange = new FloatRange(2.5f, 5f);
        [Tooltip("How far standing columns extend below their top (min..max).")]
        public FloatRange ColumnDepthRange = new FloatRange(40f, 70f);
        [Tooltip("Hanging monolith cross-section (min..max).")]
        public FloatRange MonolithWidthRange = new FloatRange(3f, 7f);
        [Tooltip("How far hanging monoliths extend above the path (min..max).")]
        public FloatRange MonolithHeightRange = new FloatRange(35f, 60f);
        public float ColumnCapDownOffset = 0.4f;
        public float ColumnCapDiameterScale = 1.25f;
        public float ColumnCapHeight = 0.8f;
        public float MonolithLedgeInset = 0.35f;
        public float MonolithBodyDrop = 2f;
        public FloatRange MonolithYawJitterRange = new FloatRange(-8f, 8f);
        public FloatRange MonolithDepthScaleRange = new FloatRange(0.9f, 1.6f);
        public float LedgeDownOffset = 0.35f;
        public float LedgeWidthScale = 1.4f;
        public float LedgeThickness = 0.7f;

        [Header("Branching")]
        [Tooltip("Sideways offset applied to extra candidates, meters (min..max).")]
        public FloatRange BranchLateralOffsetRange = new FloatRange(2.5f, 5f);

        public override void BuildGeometry(LevelBuildContext ctx, List<PathPoint> path)
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (i > 0)
                {
                    bool allowBranch = i < path.Count - 1;
                    List<PathPoint> candidates = GenerateJumpCandidates(ctx, path[i - 1], path[i], allowBranch, BranchLateralOffsetRange);
                    for (int c = 0; c < candidates.Count; c++)
                    {
                        bool isPrimary = c == 0;
                        if (ctx.Chance(HangingChance))
                            BuildHangingMonolith(ctx, candidates[c], i, c, isPrimary);
                        else
                            BuildStandingColumn(ctx, candidates[c], i, c, isPrimary);
                    }
                }
            }
        }

        private void BuildStandingColumn(LevelBuildContext ctx, PathPoint point, int index, int candidateIndex, bool isPrimary)
        {
            float diameter = Mathf.Max(RollPlatformSize(ctx, PlatformSizeRange, DifficultyPlatformShrink), ctx.Range(ColumnDiameterRange));
            float depth = ctx.Range(ColumnDepthRange);

            LevelGeometry.CreateCylinder(
                ctx.LevelElements, $"Column_{index}_{candidateIndex}",
                point.Position + Vector3.down * (depth * 0.5f),
                Quaternion.identity,
                diameter, depth, ctx.Palette.StructureMaterial);

            LevelGeometry.CreateCylinder(
                ctx.LevelElements, $"ColumnCap_{index}_{candidateIndex}",
                point.Position + Vector3.down * ColumnCapDownOffset,
                Quaternion.identity,
                diameter * ColumnCapDiameterScale, ColumnCapHeight, ctx.Palette.PlatformMaterial);

            RegisterWalkable(ctx, point.Position, index, isPrimary);
        }

        private void BuildHangingMonolith(LevelBuildContext ctx, PathPoint point, int index, int candidateIndex, bool isPrimary)
        {
            float width = ctx.Range(MonolithWidthRange);
            float height = ctx.Range(MonolithHeightRange);
            Vector3 right = Vector3.Cross(Vector3.up, point.Forward).normalized;
            float ledgeSize = RollPlatformSize(ctx, PlatformSizeRange, DifficultyPlatformShrink);

            float sideSign = ctx.Chance(0.5f) ? -1f : 1f;
            Vector3 bodyCenter = point.Position
                                 + right * sideSign * (width * 0.5f + ledgeSize * MonolithLedgeInset)
                                 + Vector3.up * (height * 0.5f - MonolithBodyDrop);

            LevelGeometry.CreateBox(
                ctx.LevelElements, $"Monolith_{index}_{candidateIndex}",
                bodyCenter,
                Quaternion.LookRotation(point.Forward) * Quaternion.Euler(0f, ctx.Range(MonolithYawJitterRange), 0f),
                new Vector3(width, height, width * ctx.Range(MonolithDepthScaleRange)),
                ctx.Palette.StructureMaterial);

            LevelGeometry.CreateBox(
                ctx.LevelElements, $"MonolithLedge_{index}_{candidateIndex}",
                point.Position + Vector3.down * LedgeDownOffset,
                Quaternion.LookRotation(point.Forward),
                new Vector3(ledgeSize * LedgeWidthScale, LedgeThickness, ledgeSize),
                ctx.Palette.PlatformMaterial);

            RegisterWalkable(ctx, point.Position, index, isPrimary);
        }
    }
}
