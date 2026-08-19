using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// Everything an archetype needs while generating one level.
    /// All geometry is built in the local space of <see cref="Root"/>;
    /// the root is already positioned and rotated in the world.
    /// </summary>
    public class LevelBuildContext
    {
        public System.Random Rng;
        public int LevelIndex;

        /// <summary>0..1, grows with depth. Widens gaps and shrinks platforms.</summary>
        public float Difficulty;

        public Transform Root;
        public Transform LevelElements;
        public Transform Entrance;
        public Transform Exit;
        public ModulePalette Palette;
        public ProceduralLevelsConfig Config;

        /// <summary>
        /// Local-space positions of walkable platform tops along the guide path,
        /// registered by archetypes. Used for death-floor sizing and guidance markers.
        /// </summary>
        public List<Vector3> PlatformTops = new List<Vector3>();

        /// <summary>
        /// The death-zone floor built by EnclosingShellBuilder. The tunnel exit wires it
        /// to deactivate on commit so it can't kill the player inside the next level,
        /// which hangs below this floor's plane.
        /// </summary>
        public GameObject DeathFloor;

        public float Range(float min, float max) => Mathf.Lerp(min, max, (float)Rng.NextDouble());

        public float Range(FloatRange range) => range.Sample(Rng);

        public int RangeInt(int minInclusive, int maxExclusive) => Rng.Next(minInclusive, maxExclusive);

        public int RangeInt(IntRange range) => range.Sample(Rng);

        public Vector3 Range(Vector3Range range) => range.Sample(Rng);

        public Vector3 RangeEven(Vector3Range range) => range.SampleEven(Rng);

        public bool Chance(float probability) => Rng.NextDouble() < probability;
    }
}
