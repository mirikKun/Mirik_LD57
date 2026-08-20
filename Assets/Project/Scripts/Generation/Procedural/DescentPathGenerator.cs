using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    [System.Serializable]
    public struct PathPoint
    {
        public Vector3 Position;
        public Vector3 Forward;
    }

    public class PathSettings
    {
        public FloatRange Length;
        public FloatRange StepDistance;
        public float FirstStepDistance;
        public float StepDistanceDifficultyBonus;
        public FloatRange StepDown;
        public float StepDownJitter = 1f;
        public FloatRange MeanderAngle;
        public FloatRange CurvatureBias;
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
 

        public static List<PathPoint> Generate(PathSettings settings, System.Random rng)
        {
            var points = new List<PathPoint>();

            float stepDistance = settings.StepDistance.Sample(rng) + settings.StepDistanceDifficultyBonus;
            int stepCount = Mathf.Max(3, Mathf.CeilToInt(settings.Length.Sample(rng) / stepDistance));
            float curvatureBias = settings.CurvatureBias.Sample(rng) * (rng.NextDouble() < 0.5 ? -1f : 1f);

            Vector3 position = Vector3.zero;
            float heading = 0f;

            for (int i = 0; i <= stepCount; i++)
            {
                Vector3 forward = Quaternion.Euler(0f, heading, 0f) * Vector3.forward;
                points.Add(new PathPoint { Position = position, Forward = forward });

                float meanderAngle = settings.MeanderAngle.Sample(rng);
                heading += curvatureBias;
                heading += Mathf.Lerp(-meanderAngle, meanderAngle, (float)rng.NextDouble());

                Vector3 nextForward = Quaternion.Euler(0f, heading, 0f) * Vector3.forward;

                float drop = settings.StepDown.Sample(rng) + Mathf.Lerp(-settings.StepDownJitter, settings.StepDownJitter, (float)rng.NextDouble());
                if (rng.NextDouble() < settings.PlateauChance)
                    drop = 0f;

                float distance = i == 0 && settings.FirstStepDistance > 0f
                    ? settings.FirstStepDistance
                    : stepDistance;
                position += nextForward * distance + Vector3.down * drop;
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
