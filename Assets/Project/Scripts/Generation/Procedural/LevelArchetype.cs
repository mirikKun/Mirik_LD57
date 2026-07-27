using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// One class per generation type, many assets (variants) per class:
    /// the same archetype with different parameters should read as a completely
    /// different level. All Vector2 fields are (min, max) ranges rolled per level.
    /// </summary>
    public abstract class LevelArchetype : ScriptableObject
    {
        [Header("Level Size")]
        [Tooltip("Total horizontal length of the level, meters (min..max).")]
        public Vector2 LengthRange = new Vector2(60f, 110f);
        [Tooltip("Horizontal center-to-center distance between platforms (min..max).")]
        public Vector2 StepDistanceRange = new Vector2(5f, 6.5f);
        [Tooltip("Vertical drop per platform (min..max).")]
        public Vector2 StepDownRange = new Vector2(1.5f, 3.5f);
        [Range(0f, 0.6f)] public float PlateauChance = 0.15f;

        [Header("Path Shape")]
        [Tooltip("Max random heading change per step, degrees (min..max).")]
        public Vector2 MeanderAngleRange = new Vector2(5f, 25f);
        [Tooltip("Constant turn per step, degrees (min..max); produces arcs. Direction is randomized.")]
        public Vector2 CurvatureBiasRange = new Vector2(0f, 0f);

        [Header("Platforms")]
        [Tooltip("Width/depth of walkable platforms (min..max).")]
        public Vector2 PlatformSizeRange = new Vector2(2.5f, 4.5f);

        [Header("Difficulty Scaling")]
        [Tooltip("Extra step distance added at difficulty 1.")]
        public float DifficultyGapBonus = 1.2f;
        [Tooltip("Platform size removed at difficulty 1.")]
        public float DifficultyPlatformShrink = 1f;

        [Header("Scale & Density")]
        [Tooltip("Multiplies the size of non-playable megastructure scenery.")]
        public float SceneryScale = 1f;
        [Tooltip("0 = no decorative geometry, 1 = default amount, >1 = denser.")]
        public float SceneryDensity = 1f;
        [Tooltip("A guidance point light is placed every Nth platform.")]
        public int LightEveryNthPlatform = 4;

        [Header("Palette & Pickups")]
        public ModulePalette Palette;
        [Range(0f, 1f)] public float LootChancePerLevel = 0.6f;
        [Range(0f, 1f)] public float HealChancePerLevel = 0.35f;

        [Header("Branching")]
        [Tooltip("Chance a given step (never the last one before the tunnel) offers extra jump candidates besides the safe primary target.")]
        [Range(0f, 1f)] public float BranchChance = 0.35f;
        [Tooltip("How many extra candidates get added when a step branches (min..max, inclusive).")]
        public Vector2Int ExtraCandidatesRange = new Vector2Int(1, 2);
        [Tooltip("Sideways offset applied to extra candidates, meters (min..max). Ignored by archetypes that vary distance instead.")]
        public Vector2 BranchLateralOffsetRange = new Vector2(2.5f, 5f);
        [Tooltip("Extra candidate distance from the previous point, as a multiplier of the primary jump distance (min..max). Above 1 = a longer, riskier jump.")]
        public Vector2 BranchDistanceMultiplierRange = new Vector2(1.05f, 1.35f);

        /// <summary>Builds the guide path in root-local space, starting at the entry platform.</summary>
        public virtual List<PathPoint> BuildPath(LevelBuildContext ctx)
        {
            var settings = new PathSettings
            {
                Length = ctx.Range(LengthRange),
                StepDistance = ctx.Range(StepDistanceRange) + DifficultyGapBonus * ctx.Difficulty,
                StepDown = ctx.Range(StepDownRange),
                StepDownJitter = 1f,
                MeanderAngle = ctx.Range(MeanderAngleRange),
                CurvatureBias = ctx.Range(CurvatureBiasRange),
                PlateauChance = PlateauChance,
            };
            return DescentPathGenerator.Generate(settings, ctx.Rng);
        }

        /// <summary>Builds all geometry (walkable and scenery) for the rolled path.</summary>
        public abstract void BuildGeometry(LevelBuildContext ctx, List<PathPoint> path);

        /// <summary>
        /// How much vertical space this archetype's structures occupy above the entry
        /// platform. The incoming tunnel shaft is deepened by this amount so the player
        /// always exits it in open air below any overhead geometry.
        /// </summary>
        public virtual float RollEntryOverheadHeight(LevelBuildContext ctx) => 0f;

        protected float RollPlatformSize(LevelBuildContext ctx)
        {
            float size = ctx.Range(PlatformSizeRange) - DifficultyPlatformShrink * ctx.Difficulty;
            return Mathf.Max(1.5f, size);
        }

        /// <summary>
        /// Returns the primary jump target plus, on a branch roll, 1-2 extra candidates
        /// the player may choose instead. Extras are always reachable from <paramref name="previous"/>
        /// with a running jump (capped distance) - they are risk/reward variety, never dead ends.
        /// Pass <paramref name="allowBranch"/> false for the final step into the tunnel to keep a
        /// guaranteed, single-file approach.
        /// </summary>
        /// <param name="lateralAxisOverride">
        /// Axis used for the sideways offset. Null = perpendicular to travel (free-standing
        /// scenery like columns/stalactites). Pass Vector3.zero for archetypes where "sideways"
        /// doesn't apply (e.g. a beam spanning a fixed-width corridor) so only distance varies.
        /// </param>
        protected List<PathPoint> GenerateJumpCandidates(LevelBuildContext ctx, PathPoint previous, PathPoint primary, bool allowBranch, Vector3? lateralAxisOverride = null)
        {
            var candidates = new List<PathPoint> { primary };
            if (!allowBranch || !ctx.Chance(BranchChance))
                return candidates;

            Vector3 flatPrev = new Vector3(previous.Position.x, 0f, previous.Position.z);
            Vector3 flatPrimary = new Vector3(primary.Position.x, 0f, primary.Position.z);
            Vector3 flatDelta = flatPrimary - flatPrev;
            float baseDistance = flatDelta.magnitude;
            Vector3 dir = baseDistance > 0.01f ? flatDelta / baseDistance : primary.Forward;
            Vector3 lateralAxis = lateralAxisOverride ?? Vector3.Cross(Vector3.up, dir).normalized;

            int extraCount = ctx.RangeInt(ExtraCandidatesRange.x, ExtraCandidatesRange.y + 1);
            for (int e = 0; e < extraCount; e++)
            {
                float lateralSign = ctx.Chance(0.5f) ? -1f : 1f;
                float lateral = ctx.Range(BranchLateralOffsetRange) * lateralSign;
                float distanceMul = ctx.Range(BranchDistanceMultiplierRange);
                float cappedDistance = Mathf.Min(baseDistance * distanceMul, DescentPathGenerator.MaxStepDistance * 1.3f);

                Vector3 flatPos = flatPrev + dir * cappedDistance + lateralAxis * lateral;
                Vector3 pos = new Vector3(flatPos.x, primary.Position.y, flatPos.z);
                candidates.Add(new PathPoint { Position = pos, Forward = primary.Forward });
            }

            return candidates;
        }

        /// <summary>
        /// Creates a standable platform whose top is at the given local position,
        /// registers it for loot placement and adds glow markers / guide lights.
        /// </summary>
        protected Transform PlacePlatform(LevelBuildContext ctx, Vector3 topLocalPosition, float width, float depth, float thickness, Quaternion rotation, int pathIndex, bool isPrimary = true)
        {
            Transform platform = LevelGeometry.CreateBox(
                ctx.Root, $"Platform_{pathIndex}",
                topLocalPosition + Vector3.down * (thickness * 0.5f),
                rotation,
                new Vector3(width, thickness, depth),
                ctx.Palette.PlatformMaterial);

            RegisterWalkable(ctx, topLocalPosition, pathIndex, isPrimary);
            return platform;
        }

        /// <summary>
        /// Registers a walkable top and, for the primary candidate only, decorates it with
        /// a glow marker and (every Nth platform) a guide light. Extra branch candidates stay
        /// unmarked so the player has to judge them visually instead of following a lit trail.
        /// </summary>
        protected void RegisterWalkable(LevelBuildContext ctx, Vector3 topLocalPosition, int pathIndex, bool isPrimary = true)
        {
            ctx.PlatformTops.Add(topLocalPosition);

            if (!isPrimary)
                return;

            // Small emissive marker so the route reads in the dark.
            Vector3 markerOffset = new Vector3(ctx.Range(-0.6f, 0.6f), 0.06f, ctx.Range(-0.6f, 0.6f));
            LevelGeometry.CreateBox(
                ctx.Root, "GuideMarker",
                topLocalPosition + markerOffset,
                Quaternion.Euler(0f, ctx.Range(0f, 90f), 0f),
                new Vector3(0.5f, 0.12f, 0.5f),
                ctx.Palette.GlowMaterial,
                withCollider: false);

            if (LightEveryNthPlatform > 0 && pathIndex % LightEveryNthPlatform == 0 && pathIndex > 0)
            {
                LevelGeometry.CreatePointLight(
                    ctx.Root, "GuideLight",
                    topLocalPosition + Vector3.up * 2f,
                    ctx.Palette.GuideLightColor, 2.5f, 10f);
            }
        }
    }
}
