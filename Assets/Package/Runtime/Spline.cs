#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TSKT
{
    public readonly struct Spline
    {
        readonly double[] sortedKeys;
        public ReadOnlySpan<double> SortedKeys => sortedKeys;
        readonly CubicFunction[] functions;
        public ReadOnlySpan<CubicFunction> Functions => functions;
        public double Duration => sortedKeys[^1];

        Spline(double[] sortedKeys, CubicFunction[] functions)
        {
            this.sortedKeys = sortedKeys;
            this.functions = functions;
        }
        public static Spline SolvePoints(params (double time, double value)[] points)
        {
            if (points.Length == 1)
            {
                return new Spline(
                    new[] { points[0].time },
                    new[] { CubicFunction.Constant(points[0].value) });
            }
            if (points.Length == 2)
            {
                var func = CubicFunction.Solve2Points(points[0], points[1]);
                return new Spline(
                    new[] { points[1].time },
                    new[] { func });
            }
            if (points.Length == 3)
            {
                var func = CubicFunction.Solve3Points(points[0], points[1], points[2]);
                return new Spline(
                    new[] { points[2].time },
                    new[] { func });
            }

            var times = points.Select(_ => _.time).ToArray();
            var current = CubicFunction.Solve4Points(points[0], points[1], points[2], points[3]);
            var intervals = new List<(double endTime, CubicFunction func)>();
            intervals.Add((points[3].time, current));

            for (int i = 3; i < points.Length - 1; i++)
            {
                var start = points[i];
                var end = points[i + 1];
                var startV = current.Velocity(start.time);
                var startA = current.Acceleration(start.time);
                current = CubicFunction.Solve2PointsVelocityAndAccel(start, end, (start.time, startV), (start.time, startA));
                intervals.Add((end.time, current));
            }
            var sortedKeys = intervals.Select(_ => _.endTime).ToArray();
            var functions = intervals.Select(_ => _.func).ToArray();

            return new Spline(sortedKeys, functions);
        }

        public static Spline SolveAccelAndVelocityAndPoints(double t, double a, double v, params (double time, double value)[] points)
        {
            var times = points.Select(_ => _.time).ToArray();
            var index = Array.BinarySearch(times, t);
            if (index < 0)
            {
                index = ~index;
            }
            if (index >= times.Length - 1)
            {
                index = times.Length - 2;
            }
            var pivot = CubicFunction.Solve2PointsVelocityAndAccel(points[index], points[index + 1], (t, v), (t, a));

            var intervals = new List<(double endTime, CubicFunction func)>();
            intervals.Add((points[index + 1].time, pivot));

            {
                CubicFunction current = pivot;
                for (int i = index + 1; i < times.Length - 1; ++i)
                {
                    var start = points[i];
                    var end = points[i + 1];
                    var startV = current.Velocity(start.time);
                    var startA = current.Acceleration(start.time);

                    current = CubicFunction.Solve2PointsVelocityAndAccel(start, end, (start.time, startV), (start.time, startA));
                    intervals.Add((end.time, current));
                }
            }

            {
                CubicFunction current = pivot;
                for (int i = 0; i < index; ++i)
                {
                    var j = index - i - 1;
                    var start = points[j];
                    var end = points[j + 1];
                    var endV = current.Velocity(end.time);
                    var endA = current.Acceleration(end.time);

                    current = CubicFunction.Solve2PointsVelocityAndAccel(start, end, (end.time, endV), (end.time, endA));
                    intervals.Insert(0, (end.time, current));
                }
            }

            var sortedKeys = intervals.Select(_ => _.endTime).ToArray();
            var functions = intervals.Select(_ => _.func).ToArray();

            return new Spline(sortedKeys, functions);
        }

        public static Spline SolvedVelocityAndPoints(double t, double v, params (double time, double value)[] points)
        {
            var times = points.Select(_ => _.time).ToArray();
            var index = Array.BinarySearch(times, t);
            if (index < 0)
            {
                index = ~index;
            }
            if (index >= times.Length - 1)
            {
                index = times.Length - 2;
            }
            var pivot = CubicFunction.Solve2PointsAndVelocity(points[index], points[index + 1], (t, v));

            var intervals = new List<(double endTime, CubicFunction func)>();
            intervals.Add((points[index + 1].time, pivot));

            {
                CubicFunction current = pivot;
                for (int i = index + 1; i < times.Length - 1; ++i)
                {
                    var start = points[i];
                    var end = points[i + 1];
                    var startV = current.Velocity(start.time);
                    var startA = current.Acceleration(start.time);

                    current = CubicFunction.Solve2PointsVelocityAndAccel(start, end, (start.time, startV), (start.time, startA));
                    intervals.Add((end.time, current));
                }
            }

            {
                CubicFunction current = pivot;
                for (int i = 0; i < index; ++i)
                {
                    var j = index - i - 1;
                    var start = points[j];
                    var end = points[j + 1];
                    var endV = current.Velocity(end.time);
                    var endA = current.Acceleration(end.time);

                    current = CubicFunction.Solve2PointsVelocityAndAccel(start, end, (end.time, endV), (end.time, endA));
                    intervals.Insert(0, (end.time, current));
                }
            }

            var sortedKeys = intervals.Select(_ => _.endTime).ToArray();
            var functions = intervals.Select(_ => _.func).ToArray();

            return new Spline(sortedKeys, functions);
        }

        public double Evaluate(double t)
        {
            return functions[GetIntervalIndex(t)].Evaluate(t);
        }

        public double Velocity(double t)
        {
            return functions[GetIntervalIndex(t)].Velocity(t);
        }

        public double Acceleration(double t)
        {
            return functions[GetIntervalIndex(t)].Acceleration(t);
        }

        int GetIntervalIndex(double t)
        {
            var index = System.Array.BinarySearch(sortedKeys, t);
            if (index < 0)
            {
                index = ~index;
            }
            if (index >= sortedKeys.Length)
            {
                index = sortedKeys.Length - 1;
            }
            return index;
        }
    }
}
