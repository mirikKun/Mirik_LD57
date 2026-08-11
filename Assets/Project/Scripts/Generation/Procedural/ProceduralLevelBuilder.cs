using System;
using System.Collections.Generic;
using Scripts.LevelObjects;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// Builds one procedural level: picks an archetype variant, generates the guide
    /// path and geometry, and wires the same Location / LocationEnteredTrigger /
    /// darkness-barrier contract the hand-made prefab locations use.
    /// </summary>
    public class ProceduralLevelBuilder
    {
        private readonly ProceduralLevelsConfig _config;
        private readonly System.Random _rng;
        private readonly bool _generateDecorations;
        private readonly LevelArchetype _testArchetype;
        private LevelArchetype _lastPicked;
        private int _builtCount;

        private struct BuildParams
        {
            public LevelArchetype Archetype;
            public Vector3 Position;
            public Quaternion Rotation;
            public int Seed;
            public int LevelIndex;
            public float Difficulty;
        }

        private BuildParams _lastBuild;
        private bool _hasLastBuild;

        public ProceduralLevelBuilder(ProceduralLevelsConfig config, bool generateDecorations = true, LevelArchetype testArchetype = null)
        {
            _config = config;
            _generateDecorations = generateDecorations;
            _testArchetype = testArchetype;
            int seed = config.Seed == 0 ? Environment.TickCount : config.Seed;
            _rng = new System.Random(seed);
        }

        public Location BuildNext(Vector3 worldPosition, Quaternion rotation)
        {
            LevelArchetype archetype = _builtCount == 0 && _testArchetype != null
                ? _testArchetype
                : PickArchetype();
            if (_builtCount == 0 && _testArchetype != null)
                _lastPicked = _testArchetype;
            float difficulty = Mathf.Clamp01((float)_builtCount / Mathf.Max(1, _config.LevelsToMaxDifficulty));

            _lastBuild = new BuildParams
            {
                Archetype = archetype,
                Position = worldPosition,
                Rotation = rotation,
                Seed = _rng.Next(),
                LevelIndex = _builtCount,
                Difficulty = difficulty,
            };
            _hasLastBuild = true;

            Location location = Build(_config, archetype, worldPosition, rotation, _lastBuild.Seed, _builtCount, difficulty, _generateDecorations);
            _builtCount++;
            return location;
        }

        /// <summary>
        /// Rebuilds the most recently built level with the same archetype/anchor and
        /// either the exact same seed or a fresh one. Testing tool; the caller is
        /// responsible for destroying the old Location.
        /// </summary>
        public Location RebuildLast(bool newSeed)
        {
            if (!_hasLastBuild)
                return null;

            if (newSeed)
                _lastBuild.Seed = _rng.Next();

            return Build(_config, _lastBuild.Archetype, _lastBuild.Position, _lastBuild.Rotation,
                _lastBuild.Seed, _lastBuild.LevelIndex, _lastBuild.Difficulty, _generateDecorations);
        }

        private LevelArchetype PickArchetype()
        {
            List<ProceduralLevelsConfig.ArchetypeEntry> pool = _config.Archetypes;
            if (pool == null || pool.Count == 0)
                throw new InvalidOperationException("ProceduralLevelsConfig has no archetypes assigned.");

            for (int attempt = 0; attempt < 8; attempt++)
            {
                float totalWeight = 0f;
                foreach (var entry in pool)
                    totalWeight += Mathf.Max(0f, entry.Weight);

                float roll = (float)_rng.NextDouble() * totalWeight;
                foreach (var entry in pool)
                {
                    roll -= Mathf.Max(0f, entry.Weight);
                    if (roll <= 0f)
                    {
                        // Avoid playing the exact same variant twice in a row when possible.
                        if (entry.Archetype == _lastPicked && pool.Count > 1 && attempt < 7)
                            break;
                        _lastPicked = entry.Archetype;
                        return entry.Archetype;
                    }
                }
            }

            _lastPicked = pool[0].Archetype;
            return _lastPicked;
        }

        /// <summary>
        /// Stateless build used both at runtime and by the editor preview.
        /// </summary>
        public static Location Build(ProceduralLevelsConfig config, LevelArchetype archetype, Vector3 worldPosition, Quaternion rotation, int seed, int levelIndex, float difficulty, bool generateDecorations = true)
        {
            LevelGeometry.ColliderPhysicsMaterial = config.ColliderPhysicsMaterial;

            var rootGo = new GameObject($"ProceduralLevel_{archetype.name}_{seed}");
            rootGo.transform.SetPositionAndRotation(worldPosition, rotation);

            Transform levelElements = CreateCategoryParent(rootGo.transform, "LevelElements");
            Transform entrance = CreateCategoryParent(rootGo.transform, "Entrance");
            Transform exit = CreateCategoryParent(rootGo.transform, "Exit");
            Transform decorations = CreateCategoryParent(rootGo.transform, "Decorations");

            var ctx = new LevelBuildContext
            {
                Rng = new System.Random(seed),
                LevelIndex = levelIndex,
                Difficulty = difficulty,
                Root = rootGo.transform,
                LevelElements = levelElements,
                Entrance = entrance,
                Exit = exit,
                Decorations = decorations,
                Palette = archetype.Palette,
                Config = config,
                GenerateDecorations = generateDecorations,
            };

            List<PathPoint> path = archetype.BuildPath(ctx);

            Transform entryPlatformPoint = BuildEntryLanding(ctx, path[0]);
            archetype.BuildGeometry(ctx, path);
            EnclosingShellBuilder.Build(ctx, path);

            // Shaft depth = how far the start platform sits below the level's top, minus margin.
            Bounds levelBounds = Location.ComputeBoundsUnder(levelElements);
            float startPlatformWorldY = rootGo.transform.TransformPoint(path[0].Position).y;
            float entryShaftDepth = Mathf.Max(
                config.MinEntryTunnelLength,
                levelBounds.max.y - startPlatformWorldY - config.TunnelLength);
            float totalSink = entryShaftDepth + config.TunnelExitDropHeight;

            for (int i = 0; i < path.Count; i++)
            {
                var p = path[i];
                p.Position += Vector3.down * totalSink;
                path[i] = p;
            }

            levelElements.localPosition += Vector3.down * totalSink;
            decorations.localPosition += Vector3.down * totalSink;

            BuildEntryShaft(ctx, entryShaftDepth);

            Transform startPoint = CreateAnchor(ctx.Entrance, "StartPoint", Vector3.zero);
            Transform endPoint = BuildTunnelExit(ctx, path[path.Count - 1], out LocationEnteredTrigger enterTrigger);

            SpawnPickups(ctx, archetype);

            Location location = rootGo.AddComponent<Location>();
            location.InitializeRuntime(
                startPoint,
                new List<Transform> { endPoint },
                enterTrigger,
                levelElements,
                entrance,
                exit,
                decorations,
                entryPlatformPoint);
            ApplyBounds(ctx, location);

            if (Application.isPlaying)
                StaticBatchingUtility.Combine(rootGo);

            return location;
        }

        private static Transform CreateCategoryParent(Transform root, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            return go.transform;
        }

        private static Transform CreateAnchor(Transform parent, string name, Vector3 localPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go.transform;
        }

        private static void BuildEntryShaft(LevelBuildContext ctx, float depth)
        {
            LocationEntryTunnel tunnel = UnityEngine.Object.Instantiate(ctx.Config.EntryTunnelPrefab, ctx.Entrance);
            tunnel.transform.localPosition = Vector3.up * 0.5f;
            tunnel.transform.localRotation = Quaternion.identity;
            tunnel.SetHeight(depth + 0.5f);
        }

        private static Transform BuildEntryLanding(LevelBuildContext ctx, PathPoint entry)
        {
            ProceduralLevelsConfig config = ctx.Config;
            Vector3 top = entry.Position;
            float width = config.EntryPlatformSize.x;
            float depth = config.EntryPlatformSize.y;
            Quaternion facing = Quaternion.LookRotation(entry.Forward);

            LevelGeometry.CreateBox(
                ctx.LevelElements, "EntryPlatform",
                top + Vector3.down * 0.5f, facing,
                new Vector3(width, 1f, depth),
                ctx.Palette.PlatformMaterial);

            ctx.PlatformTops.Add(top);

            Transform entryPlatformPoint = CreateAnchor(ctx.LevelElements, "EntryPlatformPoint", top);

            var staminaZone = new GameObject("StaminaReplenishZone");
            staminaZone.transform.SetParent(ctx.LevelElements, false);
            staminaZone.transform.localPosition = top;
            var staminaCollider = staminaZone.AddComponent<BoxCollider>();
            staminaCollider.sharedMaterial = LevelGeometry.ColliderPhysicsMaterial;
            staminaCollider.isTrigger = true;
            staminaCollider.center = Vector3.up * 5f;
            staminaCollider.size = new Vector3(width, 12f, depth);
            staminaZone.AddComponent<StaminaReplenishZone>();

            return entryPlatformPoint;
        }

        /// <summary>Tiles 10x10 darkness planes over an area. Shared with EnclosingShellBuilder.</summary>
        internal static void TileDarknessPlanes(LevelBuildContext ctx, Transform parent, GameObject planePrefab, float sizeX, float sizeZ)
        {
            if (planePrefab == null)
                return;

            const float planeSize = 10f;
            int countX = Mathf.Max(1, Mathf.CeilToInt(sizeX / planeSize));
            int countZ = Mathf.Max(1, Mathf.CeilToInt(sizeZ / planeSize));
            for (int i = 0; i < countX; i++)
            {
                for (int j = 0; j < countZ; j++)
                {
                    GameObject plane = UnityEngine.Object.Instantiate(planePrefab, parent);
                    plane.transform.localPosition =
                        new Vector3(i * planeSize, 0f, j * planeSize)
                        - new Vector3((countX - 1) * planeSize * 0.5f, 0f, (countZ - 1) * planeSize * 0.5f);
                }
            }
        }

        /// <summary>
        /// Builds this level's exit stub: prefab with commit trigger and seal barrier.
        /// Only a stub is placed here because the real shaft depth depends on the NEXT
        /// level's overhead geometry - the next level continues the pipe from EndPoint.
        /// </summary>
        private static Transform BuildTunnelExit(LevelBuildContext ctx, PathPoint last, out LocationEnteredTrigger enterTrigger)
        {
            ProceduralLevelsConfig config = ctx.Config;
            Vector3 mouth = last.Position + last.Forward * config.ExitTunnelForwardOffset;

            LocationExitTunnel tunnel = UnityEngine.Object.Instantiate(config.ExitTunnelPrefab, ctx.Exit);
            tunnel.transform.localPosition = mouth;
            tunnel.transform.localRotation = Quaternion.identity;
            tunnel.SetHeight(config.ExitTunnelLength);
            tunnel.Configure(config.BarrierHeight, ctx.DeathFloor);

            enterTrigger = tunnel.EnterTrigger;
            return tunnel.EndPoint;
        }

        private static void SpawnPickups(LevelBuildContext ctx, LevelArchetype archetype)
        {
            if (ctx.PlatformTops.Count < 4 || !Application.isPlaying)
                return;

            if (ctx.Config.LootRandomizerPrefab != null && ctx.Chance(archetype.LootChancePerLevel))
                SpawnOnRandomPlatform(ctx, ctx.Config.LootRandomizerPrefab);

            if (ctx.Config.HealPrefab != null && ctx.Chance(archetype.HealChancePerLevel))
                SpawnOnRandomPlatform(ctx, ctx.Config.HealPrefab);
        }

        private static void SpawnOnRandomPlatform(LevelBuildContext ctx, GameObject prefab)
        {
            int index = ctx.RangeInt(1, ctx.PlatformTops.Count - 1);
            Vector3 top = ctx.PlatformTops[index];
            GameObject instance = UnityEngine.Object.Instantiate(prefab, ctx.LevelElements);
            instance.transform.localPosition = top + Vector3.up * 1f;
        }

        private static void ApplyBounds(LevelBuildContext ctx, Location location)
        {
            Bounds bounds = Location.ComputeBoundsUnder(ctx.LevelElements);

            const float horizontalMargin = 15f;
            const float verticalMargin = 10f;
            bounds.Expand(new Vector3(horizontalMargin * 2f, verticalMargin * 2f, horizontalMargin * 2f));

            location.SetWorldBounds(bounds);
        }
    }
}
