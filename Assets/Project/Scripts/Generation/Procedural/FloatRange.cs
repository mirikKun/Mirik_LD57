using System;
using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    [Serializable]
    public struct FloatRange
    {
        public float x;
        public float y;

        public FloatRange(float min, float max)
        {
            x = min;
            y = max;
        }

        public float Sample(System.Random rng)
        {
            return Mathf.Lerp(x, y, (float)rng.NextDouble());
        }
    }

    [Serializable]
    public struct IntRange
    {
        public int x;
        public int y;

        public IntRange(int min, int max)
        {
            x = min;
            y = max;
        }

        public int Sample(System.Random rng)
        {
            return rng.Next(x, y + 1);
        }
    }

    [Serializable]
    public struct Vector3Range
    {
        public Vector3 min;
        public Vector3 max;

        public Vector3Range(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }

        public Vector3 Sample(System.Random rng)
        {
            return new Vector3(
                Mathf.Lerp(min.x, max.x, (float)rng.NextDouble()),
                Mathf.Lerp(min.y, max.y, (float)rng.NextDouble()),
                Mathf.Lerp(min.z, max.z, (float)rng.NextDouble()));
        }

        public Vector3 SampleEven(System.Random rng)
        {
            return Vector3.Lerp(min, max, (float)rng.NextDouble());
        }
    }
}
