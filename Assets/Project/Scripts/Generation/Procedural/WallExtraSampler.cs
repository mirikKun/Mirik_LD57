using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// Bridson Poisson-disk on the wall unwrap (s along path, h height offset).
    /// Accepted extras are at least minRange from every landing and at most
    /// maxPossibleRange from the nearest path point, in 3D.
    /// </summary>
    public static class WallExtraSampler
    {
        private const int CandidateAttempts = 30;

        public static List<PathPoint> Sample(
            IReadOnlyList<PathPoint> path,
            float minRange,
            float maxPossibleRange,
            FloatRange offsetRange,
            System.Random rng,
            bool offsetAlongRight = false)
        {
            var extras = new List<PathPoint>();
            if (path.Count < 3 || minRange <= 0f || maxPossibleRange < minRange)
                return extras;

            float[] lengths = new float[path.Count];
            for (int i = 1; i < path.Count; i++)
                lengths[i] = lengths[i - 1] + Vector3.Distance(path[i - 1].Position, path[i].Position);

            float sMin = lengths[1];
            float sMax = lengths[path.Count - 1];
            if (sMax <= sMin)
                return extras;

            float hMin = offsetRange.x;
            float hMax = offsetRange.y;

            var occupied = new List<Vector3>(path.Count);
            for (int i = 0; i < path.Count; i++)
                occupied.Add(path[i].Position);

            var samples = new List<Vector2>();
            var active = new List<int>();
            for (int i = 1; i < path.Count; i++)
            {
                samples.Add(new Vector2(lengths[i], 0f));
                active.Add(samples.Count - 1);
            }

            while (active.Count > 0)
            {
                int activeSlot = rng.Next(active.Count);
                int sampleIndex = active[activeSlot];
                Vector2 center = samples[sampleIndex];
                bool found = false;

                for (int n = 0; n < CandidateAttempts; n++)
                {
                    float angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                    float radius = minRange * (1f + (float)rng.NextDouble());
                    float s = center.x + Mathf.Cos(angle) * radius;
                    float h = center.y + Mathf.Sin(angle) * radius;
                    if (s < sMin || s > sMax || h < hMin || h > hMax)
                        continue;

                    PathPoint mapped = Map(path, lengths, s, h, offsetAlongRight);
                    if (NearestPathDistance(path, mapped.Position) > maxPossibleRange)
                        continue;

                    bool tooClose = false;
                    for (int i = 0; i < occupied.Count; i++)
                    {
                        if (Vector3.SqrMagnitude(mapped.Position - occupied[i]) < minRange * minRange)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (tooClose)
                        continue;

                    samples.Add(new Vector2(s, h));
                    occupied.Add(mapped.Position);
                    active.Add(samples.Count - 1);
                    extras.Add(mapped);
                    found = true;
                    break;
                }

                if (!found)
                    active.RemoveAt(activeSlot);
            }

            return extras;
        }

        private static PathPoint Map(IReadOnlyList<PathPoint> path, float[] lengths, float s, float h, bool offsetAlongRight)
        {
            int i = 0;
            while (i < path.Count - 2 && lengths[i + 1] < s)
                i++;

            float span = lengths[i + 1] - lengths[i];
            float t = span > 0.0001f ? (s - lengths[i]) / span : 0f;
            Vector3 forward = Vector3.Slerp(path[i].Forward, path[i + 1].Forward, t).normalized;
            if (forward.sqrMagnitude < 0.001f)
                forward = path[i].Forward;

            Vector3 alongPath = Vector3.Lerp(path[i].Position, path[i + 1].Position, t);
            Vector3 offset = offsetAlongRight
                ? Vector3.Cross(Vector3.up, forward).normalized * h
                : Vector3.up * h;

            return new PathPoint { Position = alongPath + offset, Forward = forward };
        }

        private static float NearestPathDistance(IReadOnlyList<PathPoint> path, Vector3 position)
        {
            float min = float.MaxValue;
            for (int i = 1; i < path.Count; i++)
            {
                float d = Vector3.Distance(position, path[i].Position);
                if (d < min)
                    min = d;
            }

            return min;
        }
    }
}
