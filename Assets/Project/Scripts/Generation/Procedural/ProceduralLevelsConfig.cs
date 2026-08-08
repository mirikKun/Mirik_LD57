using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Scripts.Generation.Procedural
{
    [CreateAssetMenu(menuName = "Procedural Levels/Config", fileName = "ProceduralLevelsConfig")]
    public class ProceduralLevelsConfig : ScriptableObject
    {
        [Serializable]
        public class ArchetypeEntry
        {
            public LevelArchetype Archetype;
            public float Weight = 1f;
        }

        [Header("Archetype Pool (variant assets, weighted)")]
        public List<ArchetypeEntry> Archetypes = new List<ArchetypeEntry>();

        [Header("Sequence")]
        [Tooltip("How many procedural levels are played between the basic and the advanced tutorials.")]
        public int LevelsBeforeAdvancedTutorials = 3;

        [Header("Difficulty")]
        [Tooltip("Procedural level index at which difficulty reaches 1.")]
        public int LevelsToMaxDifficulty = 12;

        [Header("Generation Seed")]
        [Tooltip("0 = random seed every run.")]
        public int Seed = 0;

        [Header("Entry & Tunnel")]
        [FormerlySerializedAs("EntryDropHeight")]
        [Tooltip("Subtracted from (level top - start platform) when sizing the entry shaft.")]
        public float TunnelLength = 2f;
        [Tooltip("Minimum entry shaft depth.")]
        public float MinEntryTunnelLength = 4f;
        [Tooltip("Open free-fall distance between the shaft exit and the entry platform below it.")]
        public float TunnelExitDropHeight = 5f;
        public Vector2 EntryPlatformSize = new Vector2(10f, 10f);
        [Tooltip("Height above the tunnel mouth where the darkness seal barrier closes after commit.")]
        public float BarrierHeight = 8f;

        [Header("Shared Prefabs")]
        public GameObject DarknessPlanePrefab;
        [Tooltip("Opaque darkness plane used to visualize death zones, same as the tutorial levels.")]
        public GameObject DeathZoneDarknessPlanePrefab;
        public GameObject LootRandomizerPrefab;
        public GameObject HealPrefab;
    }
}
