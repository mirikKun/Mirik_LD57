using System.Collections.Generic;
using Scripts.ActionObjects;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// Adds the death-zone floor that catches missed jumps. Deactivated when the
    /// player commits to the exit tunnel (the next level hangs below this plane).
    /// </summary>
    public static class EnclosingShellBuilder
    {
        private const float FloorHorizontalMargin = 25f;

        public static void Build(LevelBuildContext ctx, List<PathPoint> path)
        {
            if (path.Count == 0)
                return;

            BuildDeathFloor(ctx, path);
        }

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
            collider.sharedMaterial = LevelGeometry.ColliderPhysicsMaterial;
            collider.isTrigger = true;
            collider.size = new Vector3(sizeX, 2f, sizeZ);
            floor.AddComponent<DeathZone>();

            ProceduralLevelBuilder.TileDarknessPlanes(
                floor.transform, ctx.Config.DeathZoneDarknessPlanePrefab, sizeX, sizeZ);

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
