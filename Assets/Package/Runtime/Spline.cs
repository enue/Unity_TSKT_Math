using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;


namespace TSKT
{
    public class Spline
    {
        struct Double9
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

            public static Double9 LeftValue(double t, double v)
            {
                return new Double9()
                {
                    a = t * t * t,
                    b = t * t,
                    c = t,
                    d = 1,
                    i = v,
                };
            }
            public static Double9 BuildByVelocitiesEquality(double t)
            {
                return new Double9()
                {
                    a = 3 * t * t,
                    b = 2 * t,
                    c = 1,
                    d = 0,
                    e = -3 * t * t,
                    f = -2 * t,
                    g = -1,
                    h = 0,
                    i = 0,
                };
            }
            public static Double9 BuildByAccelerationsEquality(double t)
            {
                return new Double9()
                {
                    a = 6 * t,
                    b = 2,
                    c = 0,
                    d = 0,
                    e = -6 * t,
                    f = -2,
                    g = 0,
                    h = 0,
                    i = 0,
                };
            }
            public Double9 AddScaling(Double9 src, double scale)
            {
                return new Double9()
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
                get => new(a, b, c, d);
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
                get => new(e, f, g, h);
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
        readonly double[] endTimes;
        public System.ReadOnlySpan<double> EndTimes => endTimes;
        public double Duration => endTimes[^1];

        public Spline(params (double time, double value)[] values)
        {
            if (values.Length <= 1)
            {
                throw new System.ArgumentException();
            }

            intervals = new CubicFunction[values.Length - 1];
            endTimes = values.Skip(1).Select(_ => _.time).ToArray();

            // 最初の加速は0とする.6ax + 2b = 0
            var current = new Double9()
            {
                Right = new double4(0f, 2f, 0f, 0f),
                Value = 0f,
            };

            for (int i = 0; i < values.Length - 1; ++i)
            {
                var start = values[i];
                var end = values[i + 1];
                var a = Double9.LeftValue(start.time, start.value);
                var b = Double9.LeftValue(end.time, end.value);
                var c = Double9.BuildByAccelerationsEquality(end.time);
                var d = Double9.BuildByVelocitiesEquality(end.time);
                current = Solve(current, a, b, c, d);
            }

            // 最後の加速は0
            var trail = (left: new double4(x: 6.0 * values[^1].time, y: 2.0, z: 0.0, w: 0.0), right: 0.0);;
            var trail2 = (left: current.Left, right: current.Value);

            for (int i = 0; i < values.Length - 1; ++i)
            {
                var index = values.Length - i - 1;
                var t1 = values[index].time;
                var t2 = values[index - 1].time;

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

                var inversedMatrix = math.inverse(matrix);
                var k = math.mul(inversedMatrix, right);
                var interval = new CubicFunction(k.x, k.y, k.z, k.w);

                UnityEngine.Assertions.Assert.IsFalse(double.IsNaN(interval.a));
                UnityEngine.Assertions.Assert.IsFalse(double.IsNaN(interval.b));
                UnityEngine.Assertions.Assert.IsFalse(double.IsNaN(interval.c));
                UnityEngine.Assertions.Assert.IsFalse(double.IsNaN(interval.d));

                intervals[index - 1] = interval;
                var startTime = values[index - 1].time;
                trail = (new double4(6d * startTime, 2d, 0d, 0d), interval.Acceleration(startTime));
                trail2 = (new double4(3d * startTime * startTime, 2d * startTime, 1d, 0f), interval.Velocity(startTime));
            }
        }

        public double Evaluate(double t)
        {
            return Get(t).Evaluate(t);
        }

        public double Velocity(double t)
        {
            return Get(t).Velocity(t);
        }

        public double Acceleration(double t)
        {
            return Get(t).Acceleration(t);
        }

        public CubicFunction Get(double t)
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

        Double9 Solve(
            Double9 a,
            Double9 b,
            Double9 c,
            Double9 d,
            Double9 e)
        {
            var list = new List<Double9>() { a, b, c, d, e };
            {
                var x = list.Where(_ => _.d != 0).ToArray();
                list.RemoveAll(_ => _.d != 0);
                for (int i = 1; i < x.Length; ++i)
                {
                    var p = x[0];
                    var q = x[i];
                    var f = p.AddScaling(q, -p.d / q.d);
                    f.d = 0;
                    list.Add(f);
                }
            }
            {
                var x = list.Where(_ => _.c != 0).ToArray();
                list.RemoveAll(_ => _.c != 0);
                for (int i = 1; i < x.Length; ++i)
                {
                    var p = x[0];
                    var q = x[i];
                    var f = p.AddScaling(q, -p.c / q.c);
                    f.c = 0;
                    list.Add(f);
                }
            }
            {
                var x = list.Where(_ => _.b != 0).ToArray();
                list.RemoveAll(_ => _.b != 0);
                for (int i = 1; i < x.Length; ++i)
                {
                    var p = x[0];
                    var q = x[i];
                    var f = p.AddScaling(q, -p.b / q.b);
                    f.b = 0;
                    list.Add(f);
                }
            }
            {
                var x = list.Where(_ => _.a != 0).ToArray();
                list.RemoveAll(_ => _.a != 0);
                for (int i = 1; i < x.Length; ++i)
                {
                    var p = x[0];
                    var q = x[i];
                    var f = p.AddScaling(q, -p.a / q.a);
                    f.a = 0;
                    list.Add(f);
                }
            }
            UnityEngine.Assertions.Assert.AreEqual(1, list.Count);

            UnityEngine.Assertions.Assert.IsFalse(double.IsNaN(list[0].Value));
            UnityEngine.Assertions.Assert.IsFalse(double.IsNaN(list[0].e));
            UnityEngine.Assertions.Assert.IsFalse(double.IsNaN(list[0].f));
            UnityEngine.Assertions.Assert.IsFalse(double.IsNaN(list[0].g));
            UnityEngine.Assertions.Assert.IsFalse(double.IsNaN(list[0].h));
            return new Double9()
            {
                Left = list[0].Right,
                Value = list[0].Value,
            };
        }
    }
}

