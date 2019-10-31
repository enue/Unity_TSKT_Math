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

            public Core(int seed)
            {
                random = new Unity.Mathematics.Random((uint)seed);
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
    }

    public static class Random
    {
        public static float Range(float min, float max)
        {
            var core = RandomProvider.GetThreadRandom();
            return core.random.NextFloat(min, max);
        }
        public static float value => Range(0f, 1f);

        public static int Range(int min, int max)
        {
            var core = RandomProvider.GetThreadRandom();
            return core.random.NextInt(min, max);
        }

        public static double Range(double min, double max)
        {
            var core = RandomProvider.GetThreadRandom();
            return core.random.NextDouble(min, max);
        }
    }
}
