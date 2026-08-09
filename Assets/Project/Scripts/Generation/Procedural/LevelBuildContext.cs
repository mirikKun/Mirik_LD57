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
        public Transform Decorations;
        public ModulePalette Palette;
        public ProceduralLevelsConfig Config;
        public bool GenerateDecorations = true;

        /// <summary>
        /// Local-space positions of walkable platform tops along the guide path,
        /// registered by archetypes. Used for loot placement and guidance markers.
        /// </summary>
        public List<Vector3> PlatformTops = new List<Vector3>();

        /// <summary>
        /// Set by an archetype's BuildGeometry when it already builds that bounding
        /// surface itself (e.g. TwoWalls sets both, Ceiling sets HasCeiling). Any
        /// surface left false gets a plain far boundary from EnclosingShellBuilder.
        /// </summary>
        public bool HasLeftWall;
        public bool HasRightWall;
        public bool HasCeiling;

        /// <summary>
        /// The death-zone floor built by EnclosingShellBuilder. The tunnel exit wires it
        /// to deactivate on commit so it can't kill the player inside the next level,
        /// which hangs below this floor's plane.
        /// </summary>
        public GameObject DeathFloor;

        public float Range(float min, float max) => Mathf.Lerp(min, max, (float)Rng.NextDouble());

        public float Range(Vector2 range) => Range(range.x, range.y);

        public int RangeInt(int minInclusive, int maxExclusive) => Rng.Next(minInclusive, maxExclusive);

        public bool Chance(float probability) => Rng.NextDouble() < probability;

        /// <summary>Random point on a horizontal ring around a local position.</summary>
        public Vector3 OnRing(Vector3 center, float minRadius, float maxRadius)
        {
            float angle = Range(0f, Mathf.PI * 2f);
            float radius = Range(minRadius, maxRadius);
            return center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }
    }
}
