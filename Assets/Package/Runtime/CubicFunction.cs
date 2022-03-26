#nullable enable
using UnityEngine;
using Unity.Mathematics;

namespace TSKT
{
    public readonly struct CubicFunction
    {
        public readonly double a;
        public readonly double b;
        public readonly double c;
        public readonly double d;

        public CubicFunction(double a, double b, double c, double d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }
        readonly public double Evaluate(double x)
        {
            return a * x * x * x + b * x * x + c * x + d;
        }

        readonly public double Velocity(double x)
        {
            return 3f * a * x * x + 2f * b * x + c;
        }

        readonly public double Acceleration(double x)
        {
            return 6f * a * x + 2f * b;
        }

        static public CubicFunction Solve3PointsAndVelocity(
            (double t, double v) p1,
            (double t, double v) p2,
            (double t, double v) p3,
            (double t, double v) v)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // a * rt^3 + b * rt^2 + c * rt + d = r;
            // 3a * vt^2 + 2b * vt + c = v;
            var matrix = new double4x4(
                m00: p1.t * p1.t * p1.t,
                m01: p1.t * p1.t,
                m02: p1.t,
                m03: 1f,

                m10: p2.t * p2.t * p2.t,
                m11: p2.t * p2.t,
                m12: p2.t,
                m13: 1f,

                m20: p3.t * p3.t * p3.t,
                m21: p3.t * p3.t,
                m22: p3.t,
                m23: 1f,

                m30: 3f * v.t * v.t,
                m31: 2f * v.t,
                m32: 1f,
                m33: 0f
            );
            var rightMatrix = new double4(p1.v, p2.v, p3.v, v.v);
            var inversedMatrix = math.inverse(matrix);
            var k = math.mul(inversedMatrix, rightMatrix);
            return new CubicFunction(k.x, k.y, k.z, k.w);
        }

        static public CubicFunction Solve2PointsAnd2Velocities(
            (double t, double v) p1,
            (double t, double v) p2,
            (double t, double v) v1,
            (double t, double v) v2)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // 3a * ut^2 + 2b * ut + c = u;
            // 3a * vt^2 + 2b * vt + c = v;

            var matrix = new double4x4(
                m00: p1.t * p1.t * p1.t,
                m01: p1.t * p1.t,
                m02: p1.t,
                m03: 1f,

                m10: p2.t * p2.t * p2.t,
                m11: p2.t * p2.t,
                m12: p2.t,
                m13: 1f,

                m20: 3f * v1.t * v1.t,
                m21: 2f * v1.t,
                m22: 1f,
                m23: 0f,

                m30: 3f * v2.t * v2.t,
                m31: 2f * v2.t,
                m32: 1f,
                m33: 0f
            );

            var rightMatrix = new double4(p1.v, p2.v, v1.v, v2.v);

            var inversedMatrix = math.inverse(matrix);
            var k = math.mul(inversedMatrix, rightMatrix);
            return new CubicFunction(k.x, k.y, k.z, k.w);
        }

        static public CubicFunction Solve2PointsAndConstantAccel(
            (double t, double v) p1,
            (double t, double v) p2,
            double accell)
        {
            // b * pt^2 + pt * c + d = p
            // b * qt^2 + qt * c + d = q
            // 2b = accell
            // a = 0

            var matrix = new double3x3(
                m00: p1.t * p1.t,
                m01: p1.t,
                m02: 1f,

                m10: p2.t * p2.t,
                m11: p2.t,
                m12: 1f,

                m20: 2f,
                m21: 0f,
                m22: 0f
            );

            var rightMatrix = new double3(p1.v, p2.v, accell);

            var inversedMatrix = math.inverse(matrix);
            var k = math.mul(inversedMatrix, rightMatrix);
            return new CubicFunction(0, k.x, k.y, k.z);
        }


        static public CubicFunction Solve2PointsVelocityAndAccel(
            (double t, double v) p1,
            (double t, double v) p2,
            (double t, double v) v,
            (double t, double v) a)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // 3a * vt^2 + 2b * vt + c = v;
            // 6a * accelT + 2b = accel;

            var matrix = new double4x4(
                m00: p1.t * p1.t * p1.t,
                m01: p1.t * p1.t,
                m02: p1.t,
                m03: 1f,

                m10: p2.t * p2.t * p2.t,
                m11: p2.t * p2.t,
                m12: p2.t,
                m13: 1f,

                m20: 3f * v.t * v.t,
                m21: 2f * v.t,
                m22: 1f,
                m23: 0f,

                m30: 6f * a.t,
                m31: 2f,
                m32: 0f,
                m33: 0f
            );

            var rightMatrix = new double4(p1.v, p2.v, v.v, a.v);
            var inversedMatrix = math.inverse(matrix);
            var k = math.mul(inversedMatrix, rightMatrix);
            return new CubicFunction(k.x, k.y, k.z, k.w);
        }


        static public CubicFunction Solve3Points(
            (double t, double v) p1,
            (double t, double v) p2,
            (double t, double v) p3)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // a * rt^3 + b * rt^2 + c * rt + d = r;
            // a = 0

            var matrix = new double3x3(
                m00: p1.t * p1.t,
                m01: p1.t,
                m02: 1f,

                m10: p2.t * p2.t,
                m11: p2.t,
                m12: 1f,

                m20: p3.t * p3.t,
                m21: p3.t,
                m22: 1f
            );

            var rightMatrix = new double3(p1.v, p2.v, p3.v);
            var inversedMatrix = math.inverse(matrix);
            var k = math.mul(inversedMatrix, rightMatrix);
            return new CubicFunction(0, k.x, k.y, k.z);
        }

        static public CubicFunction Solve4Points(
            (double t, double v) p1,
            (double t, double v) p2,
            (double t, double v) p3,
            (double t, double v) p4)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // a * rt^3 + b * rt^2 + c * rt + d = r;
            // a * st^3 + b * st^2 + c * st + d = s;

            var matrix = new double4x4(
                m00: p1.t * p1.t * p1.t,
                m01: p1.t * p1.t,
                m02: p1.t,
                m03: 1f,

                m10: p2.t * p2.t * p2.t,
                m11: p2.t * p2.t,
                m12: p2.t,
                m13: 1f,

                m20: p3.t * p3.t * p3.t,
                m21: p3.t * p3.t,
                m22: p3.t,
                m23: 1f,

                m30: p4.t * p4.t * p4.t,
                m31: p4.t * p4.t,
                m32: p4.t,
                m33: 1f
            );

            var rightMatrix = new double4(p1.v, p2.v, p3.v, p4.v);
            var inversedMatrix = math.inverse(matrix);
            var k = math.mul(inversedMatrix, rightMatrix);
            return new CubicFunction(k.x, k.y, k.z, k.w);
        }

        static public CubicFunction Solve2PointsAndVelocity(
            (double t, double v) p1,
            (double t, double v) p2,
            (double t, double v) v)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // 3a * vt^2 + 2b * vt + c = v;
            // a = 0;

            var matrix = new double3x3(
                m00: p1.t * p1.t,
                m01: p1.t,
                m02: 1f,

                m10: p2.t * p2.t,
                m11: p2.t,
                m12: 1f,

                m20: 2f * v.t,
                m21: 1,
                m22: 0f
            );

            var rightMatrix = new double3(p1.v, p2.v, v.v);
            var inversedMatrix = math.inverse(matrix);
            var k = math.mul(inversedMatrix, rightMatrix);
            return new CubicFunction(0, k.x, k.y, k.z);
        }

        static public CubicFunction SolvePointAndVelocityAndConstantAccel(
            (double t, double v) p,
            (double t, double v) v,
            double accel)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // 3a * vt^2 + 2b * vt + c = v;
            // 2b = accel
            // a = 0;

            var matrix = new double3x3(
                m00: p.t * p.t,
                m01: p.t,
                m02: 1f,

                m10: 2f * v.t,
                m11: 1f,
                m12: 0f,

                m20: 2f,
                m21: 0f,
                m22: 0f
            );

            var rightMatrix = new double3(p.v, v.v, accel);
            var inversedMatrix = math.inverse(matrix);
            var k = math.mul(inversedMatrix, rightMatrix);
            return new CubicFunction(0, k.x, k.y, k.z);
        }

        static public CubicFunction Solve2Points(
            (double t, double v) p1,
            (double t, double v) p2)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // a = 0;
            // b = 0;

            var matrix = new double2x2(
                m00: p1.t,
                m01: 1f,

                m10: p2.t,
                m11: 1f
            );

            var rightMatrix = new double2(p1.v, p2.v);
            var inversedMatrix = math.inverse(matrix);
            var k = math.mul(inversedMatrix, rightMatrix);
            return new CubicFunction(0, 0, k.x, k.y);
        }
    }
}

