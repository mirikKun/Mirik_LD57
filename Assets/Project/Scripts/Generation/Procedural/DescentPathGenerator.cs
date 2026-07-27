using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    public struct PathPoint
    {
        /// <summary>Local-space position of the walkable surface (platform top).</summary>
        public Vector3 Position;

        /// <summary>Horizontal direction toward the next point.</summary>
        public Vector3 Forward;
    }

    public class PathSettings
    {
        /// <summary>Total horizontal distance the path should cover.</summary>
        public float Length = 80f;

        /// <summary>Horizontal center-to-center distance between consecutive platforms.</summary>
        public float StepDistance = 6f;

        /// <summary>Average vertical drop per step.</summary>
        public float StepDown = 2.5f;

        /// <summary>Random +- variation applied to StepDown.</summary>
        public float StepDownJitter = 1f;

        /// <summary>Max random heading change per step, in degrees.</summary>
        public float MeanderAngle = 20f;

        /// <summary>Constant turn per step in degrees; produces arcs/spirals. Sign matters.</summary>
        public float CurvatureBias = 0f;

        /// <summary>Chance a step keeps the current height (a breather).</summary>
        public float PlateauChance = 0.15f;
    }

    /// <summary>
    /// Produces the guide path every archetype builds its walkable geometry on.
    /// The path starts at local origin heading +Z and only ever goes down,
    /// within limits the player can always cover with a plain jump.
    /// </summary>
    public static class DescentPathGenerator
    {
        // Derived from player tuning: MovementSpeed 7, JumpSpeed 8.5 held 0.2s, gravity 30.
        // A flat jump covers ~6.5m; leave margin for platform edges.
        public const float MaxStepDistance = 7.5f;
        public const float MaxStepDown = 6f;

        public static List<PathPoint> Generate(PathSettings settings, System.Random rng)
        {
            var points = new List<PathPoint>();

            float stepDistance = Mathf.Min(settings.StepDistance, MaxStepDistance);
            int stepCount = Mathf.Max(3, Mathf.CeilToInt(settings.Length / stepDistance));

            Vector3 position = Vector3.zero;
            float heading = 0f; // degrees around Y, 0 = +Z
            float curvatureSign = rng.NextDouble() < 0.5 ? -1f : 1f;

            for (int i = 0; i <= stepCount; i++)
            {
                Vector3 forward = Quaternion.Euler(0f, heading, 0f) * Vector3.forward;
                points.Add(new PathPoint { Position = position, Forward = forward });

                heading += settings.CurvatureBias * curvatureSign;
                heading += Mathf.Lerp(-settings.MeanderAngle, settings.MeanderAngle, (float)rng.NextDouble());

                Vector3 nextForward = Quaternion.Euler(0f, heading, 0f) * Vector3.forward;

                float drop = settings.StepDown + Mathf.Lerp(-settings.StepDownJitter, settings.StepDownJitter, (float)rng.NextDouble());
                drop = Mathf.Clamp(drop, 0f, MaxStepDown);
                if (rng.NextDouble() < settings.PlateauChance)
                    drop = 0f;

                position += nextForward * stepDistance + Vector3.down * drop;
            }

            // Make every point's forward look at the actual next point (horizontal only).
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 delta = points[i + 1].Position - points[i].Position;
                delta.y = 0f;
                if (delta.sqrMagnitude > 0.001f)
                {
                    var p = points[i];
                    p.Forward = delta.normalized;
                    points[i] = p;
                }
            }

            return points;
        }
    }
}
