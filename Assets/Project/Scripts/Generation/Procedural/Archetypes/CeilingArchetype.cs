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
        [Header("Platforms")]
        [Tooltip("Width of walkable stalactite/mushroom caps (min..max).")]
        public FloatRange PlatformSizeRange = new FloatRange(2.5f, 4.5f);
        [Tooltip("Platform size removed at difficulty 1.")]
        public float DifficultyPlatformShrink = 1f;

        [Header("Ceiling")]
        [Tooltip("Clearance between the path and the ceiling (min..max).")]
        public FloatRange ClearanceRange = new FloatRange(10f, 18f);
        [Tooltip("Ceiling slab width to each side of the path (min..max).")]
        public FloatRange CeilingHalfWidthRange = new FloatRange(18f, 30f);
        [Tooltip("Ceiling slab thickness (min..max).")]
        public FloatRange CeilingThicknessRange = new FloatRange(2f, 5f);
        public float CeilingSegmentLengthMultiplier = 1.5f;
        public float EntryOverheadMargin = 2f;

        [Header("Hanging Growth")]
        [Range(0f, 1f)]
        [Tooltip("Chance a waypoint grows a mushroom instead of a stalactite.")]
        public float MushroomChance = 0.35f;
        [Tooltip("Stalactite/stem diameter (min..max).")]
        public FloatRange StemDiameterRange = new FloatRange(0.8f, 1.8f);
        [Tooltip("Mushroom cap thickness (min..max).")]
        public FloatRange MushroomCapThicknessRange = new FloatRange(0.5f, 1f);
        public float MushroomStemDiameterScale = 0.6f;
        public float MushroomCapSizeScale = 1.2f;
        public IntRange StalactiteSectionCountRange = new IntRange(2, 3);
        public float StalactiteTipReserve = 0.6f;
        public float StalactiteTopDiameterScale = 2.2f;
        public float StalactiteSectionOverlap = 0.3f;
        public FloatRange StalactiteYawRange = new FloatRange(0f, 90f);
        public float StalactitePlatformDownOffset = 0.3f;
        public float StalactitePlatformThickness = 0.6f;

        /// <summary>
        /// The incoming shaft must pierce below the ceiling slab; use the worst-case
        /// clearance/thickness rolls plus a margin so the exit is always in open air.
        /// </summary>
        public override float RollEntryOverheadHeight(LevelBuildContext ctx)
            => ClearanceRange.y + CeilingThicknessRange.y + EntryOverheadMargin;

        public override void BuildGeometry(LevelBuildContext ctx, List<PathPoint> path)
        {
            float clearance = ctx.Range(ClearanceRange);
            float halfWidth = ctx.Range(CeilingHalfWidthRange);

            for (int i = 0; i < path.Count; i++)
            {
                Vector3 ceilingPoint = path[i].Position + Vector3.up * clearance;

                if (i > 0)
                    BuildHangingPlatform(ctx, path[i], ceilingPoint, i);

                if (i > 0 && i < path.Count - 1)
                {
                    Vector3 nextCeiling = path[i + 1].Position + Vector3.up * clearance;
                    BuildCeilingSegment(ctx, ceilingPoint, nextCeiling, halfWidth, i);
                }
            }
        }

        private void BuildHangingPlatform(LevelBuildContext ctx, PathPoint point, Vector3 ceilingPoint, int index)
        {
            float platformSize = RollPlatformSize(ctx, PlatformSizeRange, DifficultyPlatformShrink);
            float stemDiameter = ctx.Range(StemDiameterRange);
            float hangLength = ceilingPoint.y - point.Position.y;

            if (ctx.Chance(MushroomChance))
            {
                float capThickness = ctx.Range(MushroomCapThicknessRange);
                LevelGeometry.CreateCylinder(
                    ctx.LevelElements, $"MushroomStem_{index}",
                    point.Position + Vector3.up * (hangLength * 0.5f + capThickness),
                    Quaternion.identity,
                    stemDiameter * MushroomStemDiameterScale, hangLength, ctx.Palette.StructureMaterial);
                LevelGeometry.CreateCylinder(
                    ctx.LevelElements, $"MushroomCap_{index}",
                    point.Position + Vector3.down * (capThickness * 0.5f),
                    Quaternion.identity,
                    platformSize * MushroomCapSizeScale, capThickness, ctx.Palette.AccentMaterial);
            }
            else
            {
                int sections = ctx.RangeInt(StalactiteSectionCountRange);
                float sectionHeight = (hangLength - StalactiteTipReserve) / sections;
                for (int s = 0; s < sections; s++)
                {
                    float t = (float)s / sections;
                    float diameter = Mathf.Lerp(stemDiameter * StalactiteTopDiameterScale, stemDiameter, t);
                    float centerY = ceilingPoint.y - sectionHeight * (s + 0.5f);
                    LevelGeometry.CreateBox(
                        ctx.LevelElements, $"Stalactite_{index}_{s}",
                        new Vector3(point.Position.x, centerY, point.Position.z),
                        Quaternion.Euler(0f, ctx.Range(StalactiteYawRange), 0f),
                        new Vector3(diameter, sectionHeight + StalactiteSectionOverlap, diameter),
                        ctx.Palette.StructureMaterial);
                }

                LevelGeometry.CreateBox(
                    ctx.LevelElements, $"StalactitePlatform_{index}",
                    point.Position + Vector3.down * StalactitePlatformDownOffset,
                    Quaternion.Euler(0f, ctx.Range(0f, 360f), 0f),
                    new Vector3(platformSize, StalactitePlatformThickness, platformSize),
                    ctx.Palette.PlatformMaterial);
            }

            RegisterWalkable(ctx, point.Position, index);
        }

        private void BuildCeilingSegment(LevelBuildContext ctx, Vector3 from, Vector3 to, float halfWidth, int index)
        {
            Vector3 mid = (from + to) * 0.5f;
            Vector3 delta = to - from;
            float length = delta.magnitude * CeilingSegmentLengthMultiplier;
            float thickness = ctx.Range(CeilingThicknessRange);

            Quaternion rotation = Quaternion.LookRotation(delta.normalized);
            LevelGeometry.CreateBox(
                ctx.LevelElements, $"Ceiling_{index}",
                mid + Vector3.up * (thickness * 0.5f),
                rotation,
                new Vector3(halfWidth * 2f, thickness, length),
                ctx.Palette.StructureMaterial);
        }
    }
}
