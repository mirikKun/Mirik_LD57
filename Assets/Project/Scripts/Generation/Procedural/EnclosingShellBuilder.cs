using System.Collections.Generic;
using Scripts.ActionObjects;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// Guarantees every level has all four bounding surfaces regardless of archetype.
    /// Whichever of HasLeftWall/HasRightWall/HasCeiling the archetype didn't build
    /// itself gets a plain far boundary here (visible only as a distant backdrop),
    /// and every level always gets a death-zone floor to catch missed jumps.
    /// </summary>
    public static class EnclosingShellBuilder
    {
        private const float FarWallDistance = 90f;
        private const float FarCeilingClearance = 100f;
        private const float SlabThickness = 4f;
        private const float SlabSpan = 220f;
        private const float FloorHorizontalMargin = 25f;

        public static void Build(LevelBuildContext ctx, List<PathPoint> path)
        {
            if (path.Count == 0)
                return;

            if (ctx.GenerateDecorations)
            {
                if (!ctx.HasLeftWall)
                    BuildFarSideWall(ctx, path, sideSign: -1f);
                if (!ctx.HasRightWall)
                    BuildFarSideWall(ctx, path, sideSign: 1f);
                if (!ctx.HasCeiling)
                    BuildFarCeiling(ctx, path);
            }

            BuildDeathFloor(ctx, path);
        }

        private static void BuildFarSideWall(LevelBuildContext ctx, List<PathPoint> path, float sideSign)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 mid = (path[i].Position + path[i + 1].Position) * 0.5f;
                Vector3 forward = path[i].Forward;
                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
                float length = Vector3.Distance(path[i].Position, path[i + 1].Position) * 1.6f;

                Vector3 center = mid + right * (sideSign * FarWallDistance);
                LevelGeometry.CreateBox(
                    ctx.Decorations, $"FarWall_{(sideSign < 0f ? "L" : "R")}_{i}",
                    center,
                    Quaternion.LookRotation(forward),
                    new Vector3(SlabThickness, SlabSpan, length),
                    ctx.Palette.SceneryMaterial,
                    withCollider: true);
            }
        }

        private static void BuildFarCeiling(LevelBuildContext ctx, List<PathPoint> path)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 from = path[i].Position + Vector3.up * FarCeilingClearance;
                Vector3 to = path[i + 1].Position + Vector3.up * FarCeilingClearance;
                Vector3 mid = (from + to) * 0.5f;
                Vector3 delta = to - from;
                float length = delta.magnitude * 1.5f;
                Quaternion rotation = delta.sqrMagnitude > 0.0001f
                    ? Quaternion.LookRotation(delta.normalized)
                    : Quaternion.LookRotation(path[i].Forward);

                LevelGeometry.CreateBox(
                    ctx.Decorations, $"FarCeiling_{i}",
                    mid,
                    rotation,
                    new Vector3(SlabSpan, SlabThickness, length),
                    ctx.Palette.SceneryMaterial,
                    withCollider: true);
            }
        }

        /// <summary>
        /// A single trigger plane sized to the path's horizontal extent, positioned well
        /// below the lowest platform so it never interferes with normal jumps but always
        /// catches a fall. Visualized with the same opaque darkness planes the hand-made
        /// tutorial death zones use. Deactivated when the player commits to the exit
        /// tunnel (the next level hangs below this plane).
        /// </summary>
        private static void BuildDeathFloor(LevelBuildContext ctx, List<PathPoint> path)
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            List<Vector3> sample = ctx.PlatformTops.Count > 0 ? ctx.PlatformTops : GetPositions(path);
            foreach (Vector3 top in sample)
            {
                min = Vector3.Min(min, top);
                max = Vector3.Max(max, top);
            }

            const float floorMargin = 25f;

            float sizeX = Mathf.Max(max.x - min.x, 10f) + FloorHorizontalMargin * 2f;
            float sizeZ = Mathf.Max(max.z - min.z, 10f) + FloorHorizontalMargin * 2f;
            Vector3 center = new Vector3((min.x + max.x) * 0.5f, min.y - floorMargin, (min.z + max.z) * 0.5f);

            var floor = new GameObject("DeathFloor");
            floor.transform.SetParent(ctx.LevelElements, false);
            floor.transform.localPosition = center;
            var collider = floor.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(sizeX, 2f, sizeZ);
            floor.AddComponent<DeathZone>();

            // Same visible darkness surface as the tutorial death zones.
            ProceduralLevelBuilder.TileDarknessPlanes(ctx, floor.transform, ctx.Config != null ? ctx.Config.DeathZoneDarknessPlanePrefab : null, sizeX, sizeZ);

            ctx.DeathFloor = floor;
        }

        private static List<Vector3> GetPositions(List<PathPoint> path)
        {
            var positions = new List<Vector3>(path.Count);
            foreach (PathPoint p in path)
                positions.Add(p.Position);
            return positions;
        }
    }
}
