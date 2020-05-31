using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Mathematics;

// http://neue.cc/2013/03/06_399.html

namespace TSKT
{
    public static class RandomProvider
    {
        public class Core
        {
            public Unity.Mathematics.Random random;
            readonly public uint seed;

            public Core(int seed)
            {
                this.seed = (uint)seed;
                random = new Unity.Mathematics.Random(this.seed);
            }

            public float Range(float min, float max)
            {
                return random.NextFloat(min, max);
            }

            public int Range(int min, int max)
            {
                return random.NextInt(min, max);
            }

            public double Range(double min, double max)
            {
                return random.NextDouble(min, max);
            }

            public int[] GenerateShuffledRange(int start, int count)
            {
                if (count == 0)
                {
                    return System.Array.Empty<int>();
                }

                var result = new int[count];
                for (int i = 0; i < count; ++i)
                {
                    result[i] = i + start;
                }
                Shuffle(ref result);
                return result;
            }

            public T[] GenerateShuffledArray<T>(List<T> list)
            {
                if (list.Count == 0)
                {
                    return System.Array.Empty<T>();
                }

                var result = new T[list.Count];
                for (int i = 0; i < result.Length; ++i)
                {
                    result[i] = list[i];
                }
                Shuffle(ref result);
                return result;
            }

            public void Shuffle<T>(ref List<T> list)
            {
                for (int i = 0; i < list.Count - 1; ++i)
                {
                    var swapIndex = Range(i, list.Count);
                    var t = list[i];
                    list[i] = list[swapIndex];
                    list[swapIndex] = t;
                }
            }

            public void Shuffle<T>(ref T[] list)
            {
                for (int i = 0; i < list.Length - 1; ++i)
                {
                    var swapIndex = Range(i, list.Length);
                    var t = list[i];
                    list[i] = list[swapIndex];
                    list[swapIndex] = t;
                }
            }
        }

        static int seed = System.Environment.TickCount;

        static ThreadLocal<Core> randomWrapper = new ThreadLocal<Core>(() =>
            new Core(Interlocked.Increment(ref seed))
        );

        public static Core GetThreadRandom()
        {
            return randomWrapper.Value;
        }

        public static Core GetNewRandom()
        {
            return new Core(Interlocked.Increment(ref seed));
        }
    }

    public static class Random
    {
        public static float Range(float min, float max)
        {
            return RandomProvider.GetThreadRandom().Range(min,max);
        }
        public static float value => Range(0f, 1f);

        public static int Range(int min, int max)
        {
            return RandomProvider.GetThreadRandom().Range(min, max);
        }

        public static double Range(double min, double max)
        {
            return RandomProvider.GetThreadRandom().Range(min, max);
        }
    }
}
