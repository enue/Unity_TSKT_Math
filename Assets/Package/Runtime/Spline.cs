using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;


namespace TSKT
{
    public class Spline
    {
        struct float9
        {
            public double a;
            public double b;
            public double c;
            public double d;
            public double e;
            public double f;
            public double g;
            public double h;
            public double i;

            public static float9 LeftValue(double t, double v)
            {
                return new float9()
                {
                    a = t * t * t,
                    b = t * t,
                    c = t,
                    d = 1f,
                    i = v,
                };
            }
            public static float9 Velocity(double t)
            {
                return new float9()
                {
                    a = 3f * t * t,
                    b = 2f * t,
                    c = 1f,
                    d = 0f,
                    e = -3f * t * t,
                    f = -2f * t,
                    g = -1f,
                    h = 0f,
                    i = 0f,
                };
            }
            public static float9 Acceleration(double t)
            {
                return new float9()
                {
                    a = 6f * t,
                    b = 2f,
                    c = 0f,
                    d = 0f,
                    e = -6f * t,
                    f = -2f,
                    g = 0f,
                    h = 0f,
                    i = 0f,
                };
            }
            public float9 AddScale(float9 src, double scale)
            {
                return new float9()
                {
                    a = a + src.a * scale,
                    b = b + src.b * scale,
                    c = c + src.c * scale,
                    d = d + src.d * scale,
                    e = e + src.e * scale,
                    f = f + src.f * scale,
                    g = g + src.g * scale,
                    h = h + src.h * scale,
                    i = i + src.i * scale,
                };
            }
            public double4 Left
            {
                get => new double4(a, b, c, d);
                set
                {
                    a = value.x;
                    b = value.y;
                    c = value.z;
                    d = value.w;
                }
            }

            public double4 Right
            {
                get => new double4(e, f, g, h);
                set
                {
                    e = value.x;
                    f = value.y;
                    g = value.z;
                    h = value.w;
                }
            }
            public double Value
            {
                get => i;
                set => i = value;
            }
        }

        readonly CubicFunction[] intervals;
        public System.ReadOnlySpan<CubicFunction> Intervals => intervals;
        readonly float[] endTimes;
        public System.ReadOnlySpan<float> EndTimes => endTimes;
        public float Duration => endTimes[^1];

        public Spline(params (float time, float value)[] values)
        {
            if (values.Length <= 1)
            {
                throw new System.ArgumentException();
            }

            intervals = new CubicFunction[values.Length - 1];
            endTimes = values.Skip(1).Select(_ => _.time).ToArray();

            // 最初の加速は0とする.6ax + 2b = 0
            var current = new float9()
            {
                Right = new double4(0f, 2f, 0f, 0f),
                Value = 0f,
            };

            var trails = new List<(double4 left, double right)>();
            for (int i = 0; i < values.Length - 1; ++i)
            {
                var start = values[i];
                var end = values[i + 1];
                var a = float9.LeftValue(start.time, start.value);
                var b = float9.LeftValue(end.time, end.value);
                var c = float9.Acceleration(end.time);
                var d = float9.Velocity(end.time);
                current = Solve(current, a, b, c, d);
                trails.Add((current.Left, current.Value));
            }

            // 最後の加速は0
            var trail = (left: new double4(x: 6.0 * values[^1].time, y: 2.0, z: 0.0, w: 0.0), right: 0.0);;
            var trail2 = (left: current.Left, right: current.Value);

            for (int i = 0; i < values.Length - 1; ++i)
            {
                var index = values.Length - i - 1;
                var t1 = values[index].time;
                var t2 = values[index - 1].time;
                //var trail2 = trails[index - 1];

                var matrix = new double4x4(
                    m00: t1 * t1 * t1,
                    m01: t1 * t1,
                    m02: t1,
                    m03: 1f,

                    m10: t2 * t2 * t2,
                    m11: t2 * t2,
                    m12: t2,
                    m13: 1f,

                    m20: trail.left.x,
                    m21: trail.left.y,
                    m22: trail.left.z,
                    m23: trail.left.w,

                    m30: trail2.left.x,
                    m31: trail2.left.y,
                    m32: trail2.left.z,
                    m33: trail2.left.w
                );

                var right = new double4();
                right.x = values[index].value;
                right.y = values[index - 1].value;
                right.z = trail.right;
                right.w = trail2.right;
                Debug.Log(right);

                var inversedMatrix = math.inverse(matrix);
                var k = math.mul(inversedMatrix, right);
                var interval = new CubicFunction((float)k.x, (float)k.y, (float)k.z, (float)k.w);

                UnityEngine.Assertions.Assert.IsFalse(float.IsNaN(interval.a), trail.right.ToString());
                UnityEngine.Assertions.Assert.IsFalse(float.IsNaN(interval.b));
                UnityEngine.Assertions.Assert.IsFalse(float.IsNaN(interval.c));
                UnityEngine.Assertions.Assert.IsFalse(float.IsNaN(interval.d));

                intervals[index - 1] = interval;
                var startTime = values[index - 1].time;
                trail = (new double4(6d * startTime, 2d, 0d, 0d), interval.Acceleration(startTime));
                trail2 = (new double4(3d * startTime * startTime, 2d * startTime, 1d, 0f), interval.Velocity(startTime));
                Debug.Log(matrix);
                Debug.Log(inversedMatrix);
                Debug.Log(right);
                Debug.Log((interval.a, interval.b, interval.c, interval.d));
            }
        }

        public float Evaluate(float t)
        {
            return Get(t).Evaluate(t);
        }

        public float Velocity(float t)
        {
            return Get(t).Velocity(t);
        }

        public float Acceleration(float t)
        {
            return Get(t).Acceleration(t);
        }

        public CubicFunction Get(float t)
        {
            var index = System.Array.BinarySearch(endTimes, t);
            if (index < 0)
            {
                index = ~index;
            }
            if (index >= endTimes.Length)
            {
                index = endTimes.Length - 1;
            }
            return intervals[index];
        }

        float9 Solve(
            float9 a,
            float9 b,
            float9 c,
            float9 d,
            float9 e)
        {
            var list = new List<float9>() { a, b, c, d, e };
            {
                var x = list.Where(_ => _.d != 0f).ToArray();
                list.RemoveAll(_ => _.d != 0f);
                for (int i = 1; i < x.Length; ++i)
                {
                    var p = x[0];
                    var q = x[i];
                    var f = p.AddScale(q, -p.d / q.d);
                    f.d = 0f;
                    list.Add(f);
                }
            }
            {
                var x = list.Where(_ => _.c != 0f).ToArray();
                list.RemoveAll(_ => _.c != 0f);
                for (int i = 1; i < x.Length; ++i)
                {
                    var p = x[0];
                    var q = x[i];
                    var f = p.AddScale(q, -p.c / q.c);
                    f.c = 0f;
                    list.Add(f);
                }
            }
            {
                var x = list.Where(_ => _.b != 0f).ToArray();
                list.RemoveAll(_ => _.b != 0f);
                for (int i = 1; i < x.Length; ++i)
                {
                    var p = x[0];
                    var q = x[i];
                    var f = p.AddScale(q, -p.b / q.b);
                    f.b = 0f;
                    list.Add(f);
                }
            }
            {
                var x = list.Where(_ => _.a != 0f).ToArray();
                list.RemoveAll(_ => _.a != 0f);
                for (int i = 1; i < x.Length; ++i)
                {
                    var p = x[0];
                    var q = x[i];
                    var f = p.AddScale(q, -p.a / q.a);
                    f.a = 0f;
                    list.Add(f);
                }
            }
            UnityEngine.Assertions.Assert.AreEqual(1, list.Count);

            UnityEngine.Assertions.Assert.IsFalse(double.IsNaN(list[0].Value));
            UnityEngine.Assertions.Assert.IsFalse(double.IsNaN(list[0].e));
            UnityEngine.Assertions.Assert.IsFalse(double.IsNaN(list[0].f));
            UnityEngine.Assertions.Assert.IsFalse(double.IsNaN(list[0].g));
            UnityEngine.Assertions.Assert.IsFalse(double.IsNaN(list[0].h));
            return new float9()
            {
                Left = list[0].Right,
                Value = list[0].Value,
            };
        }
    }
}

