using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// One class per generation type, many assets (variants) per class:
    /// the same archetype with different parameters should read as a completely
    /// different level. FloatRange fields are (min, max) sampled per generate call.
    /// </summary>
    public abstract class LevelArchetype : ScriptableObject
    {
        [Header("Level Size")]
        [Tooltip("Total horizontal length of the level, meters (min..max).")]
        public FloatRange LengthRange = new FloatRange(60f, 110f);
        [Tooltip("Horizontal center-to-center distance between platforms (min..max).")]
        public FloatRange StepDistanceRange = new FloatRange(5f, 6.5f);
        [Tooltip("Horizontal length of the first step, meters. 0 = use Step Distance Range like the rest.")]
        public float FirstStepDistance = 0f;
        [Tooltip("Vertical drop per platform (min..max).")]
        public FloatRange StepDownRange = new FloatRange(1.5f, 3.5f);
        [Range(0f, 0.6f)] public float PlateauChance = 0.15f;

        [Header("Path Shape")]
        [Tooltip("Max random heading change per step, degrees (min..max).")]
        public FloatRange MeanderAngleRange = new FloatRange(5f, 25f);
        [Tooltip("Constant turn per step, degrees (min..max); produces arcs. Direction is randomized.")]
        public FloatRange CurvatureBiasRange = new FloatRange(0f, 0f);
        [Tooltip("Random +- variation applied to each step's drop.")]
        public float StepDownJitter = 1f;

        [Header("Difficulty Scaling")]
        [Tooltip("Extra step distance added at difficulty 1.")]
        public float DifficultyGapBonus = 1.2f;

        [Header("Platforms")]
        [Tooltip("Walkable tops are never smaller than this, after difficulty shrink.")]
        public float MinPlatformSize = 1.5f;

        [Header("Lighting")]
        [Tooltip("A guidance point light is placed every Nth platform.")]
        public int LightEveryNthPlatform = 4;
        public float GuideLightHeight = 2f;
        public float GuideLightIntensity = 2.5f;
        public float GuideLightRange = 10f;
        public FloatRange GuideMarkerOffsetRange = new FloatRange(-0.6f, 0.6f);
        public float GuideMarkerYOffset = 0.06f;
        public Vector3 GuideMarkerSize = new Vector3(0.5f, 0.12f, 0.5f);
        public FloatRange GuideMarkerYawRange = new FloatRange(0f, 90f);

        [Header("Palette")]
        public ModulePalette Palette;

        public virtual List<PathPoint> BuildPath(LevelBuildContext ctx)
        {
            return DescentPathGenerator.Generate(new PathSettings
            {
                Length = LengthRange,
                StepDistance = StepDistanceRange,
                FirstStepDistance = FirstStepDistance,
                StepDistanceDifficultyBonus = DifficultyGapBonus * ctx.Difficulty,
                StepDown = StepDownRange,
                StepDownJitter = StepDownJitter,
                MeanderAngle = MeanderAngleRange,
                CurvatureBias = CurvatureBiasRange,
                PlateauChance = PlateauChance,
            }, ctx.Rng);
        }

        public abstract void BuildGeometry(LevelBuildContext ctx, List<PathPoint> path);

        /// <summary>
        /// How much vertical space this archetype's structures occupy above the entry
        /// platform. The incoming tunnel shaft is deepened by this amount so the player
        /// always exits it in open air below any overhead geometry.
        /// </summary>
        public virtual float RollEntryOverheadHeight(LevelBuildContext ctx) => 0f;

        protected float RollPlatformSize(LevelBuildContext ctx, FloatRange sizeRange, float difficultyShrink)
        {
            float size = ctx.Range(sizeRange) - difficultyShrink * ctx.Difficulty;
            return Mathf.Max(MinPlatformSize, size);
        }

        /// <summary>
        /// Registers a walkable top and, for the primary candidate only, decorates it with
        /// a glow marker and (every Nth platform) a guide light. Extra branch candidates stay
        /// unmarked so the player has to judge them visually instead of following a lit trail.
        /// </summary>
        protected void RegisterWalkable(LevelBuildContext ctx, Vector3 topLocalPosition, int pathIndex, bool isPrimary = true, Vector3 guideOffset = default)
        {
            ctx.PlatformTops.Add(topLocalPosition);

            if (!isPrimary)
                return;

            Vector3 markerOffset = new Vector3(ctx.Range(GuideMarkerOffsetRange), GuideMarkerYOffset, ctx.Range(GuideMarkerOffsetRange));
            LevelGeometry.CreateBox(
                ctx.LevelElements, "GuideMarker",
                topLocalPosition + guideOffset + markerOffset,
                Quaternion.Euler(0f, ctx.Range(GuideMarkerYawRange), 0f),
                GuideMarkerSize,
                ctx.Palette.GlowMaterial,
                withCollider: false);

            if (LightEveryNthPlatform > 0 && pathIndex % LightEveryNthPlatform == 0 && pathIndex > 0)
            {
                LevelGeometry.CreatePointLight(
                    ctx.LevelElements, "GuideLight",
                    topLocalPosition + guideOffset + Vector3.up * GuideLightHeight,
                    ctx.Palette.GuideLightColor, GuideLightIntensity, GuideLightRange);
            }
        }
    }
}
