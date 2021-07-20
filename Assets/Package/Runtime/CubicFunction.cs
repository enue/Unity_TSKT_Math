using Unity.Mathematics;
using UnityEngine;
#nullable enable

namespace TSKT
{
    public readonly struct CubicFunction
    {
        readonly public float a;
        readonly public float b;
        readonly public float c;
        readonly public float d;

        public CubicFunction(float a, float b, float c, float d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }
        readonly public float Evaluate(float x)
        {
            return a * x * x * x + b * x * x + c * x + d;
        }

        readonly public float Velocity(float x)
        {
            return 3f * a * x * x + 2f * b * x + c;
        }

        readonly public float Acceleration(float x)
        {
            return 6f * a * x + 2f * b;
        }

        static public CubicFunction Solve3PointsAndVelocity(
            (float t, float v) p1,
            (float t, float v) p2,
            (float t, float v) p3,
            (float t, float v) v)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // a * rt^3 + b * rt^2 + c * rt + d = r;
            // 3a * vt^2 + 2b * vt + c = v;
            var matrix = new float4x4(
                m00: p1.t * p1.t * p1.t,
                m10: p1.t * p1.t,
                m20: p1.t,
                m30: 1f,

                m01: p2.t * p2.t * p2.t,
                m11: p2.t * p2.t,
                m21: p2.t,
                m31: 1f,

                m02: p3.t * p3.t * p3.t,
                m12: p3.t * p3.t,
                m22: p3.t,
                m32: 1f,

                m03: 3f * v.t * v.t,
                m13: 2f * v.t,
                m23: 1f,
                m33: 0f);
            var inversedMatrix =  math.inverse(matrix);

            var rightMatrix = new float4(p1.v, p2.v, p3.v, v.v);

            var result = new CubicFunction(
                a: math.dot(inversedMatrix.c0, rightMatrix),
                b: math.dot(inversedMatrix.c1, rightMatrix),
                c: math.dot(inversedMatrix.c2, rightMatrix),
                d: math.dot(inversedMatrix.c3, rightMatrix)
            );

            return result;
        }

        static public CubicFunction Solve2PointsAnd2Velocities(
            (float t, float v) p1,
            (float t, float v) p2,
            (float t, float v) v1,
            (float t, float v) v2)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // 3a * ut^2 + 2b * ut + c = u;
            // 3a * vt^2 + 2b * vt + c = v;

            var matrix = new float4x4(
                m00: p1.t * p1.t * p1.t,
                m10: p1.t * p1.t,
                m20: p1.t,
                m30: 1f,

                m01: p2.t * p2.t * p2.t,
                m11: p2.t * p2.t,
                m21: p2.t,
                m31: 1f,

                m02: 3f * v1.t * v1.t,
                m12: 2f * v1.t,
                m22: 1f,
                m32: 0f,

                m03: 3f * v2.t * v2.t,
                m13: 2f * v2.t,
                m23: 1f,
                m33: 0f
            );

            var rightMatrix = new float4(p1.v, p2.v, v1.v, v2.v);
            var inversedMatrix = math.inverse(matrix);

            var result = new CubicFunction(
                a: math.dot(inversedMatrix.c0, rightMatrix),
                b: math.dot(inversedMatrix.c1, rightMatrix),
                c: math.dot(inversedMatrix.c2, rightMatrix),
                d: math.dot(inversedMatrix.c3, rightMatrix));

            return result;
        }

        static public CubicFunction Solve2PointsAndConstantAccel(
            (float t, float v) p1,
            (float t, float v) p2,
            float accell)
        {
            // b * pt^2 + pt * c + d = p
            // b * qt^2 + qt * c + d = q
            // 2b = accell
            // a = 0

            var matrix = new float4x4(
                m00: 0f,
                m10: p1.t * p1.t,
                m20: p1.t,
                m30: 1f,

                m01: 0f,
                m11: p2.t * p2.t,
                m21: p2.t,
                m31: 1f,

                m02: 0f,
                m12: 2f,
                m22: 0f,
                m32: 0f,

                m03: 1f,
                m13: 0f,
                m23: 0f,
                m33: 0f
            );

            var rightMatrix = new float4(p1.v, p2.v, accell, 0f);
            var inversedMatrix = math.inverse(matrix);

            var result = new CubicFunction(
                a: 0f,
                b: math.dot(inversedMatrix.c1, rightMatrix),
                c: math.dot(inversedMatrix.c2, rightMatrix),
                d: math.dot(inversedMatrix.c3, rightMatrix));

            return result;
        }


        static public CubicFunction Solve2PointsVelocityAndAccel(
            (float t, float v) p1,
            (float t, float v) p2,
            (float t, float v) v,
            (float t, float v) a)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // 3a * vt^2 + 2b * vt + c = v;
            // 6a * accelT + 2b = accel;

            var matrix = new float4x4(
                m00: p1.t * p1.t * p1.t,
                m10: p1.t * p1.t,
                m20: p1.t,
                m30: 1f,

                m01: p2.t * p2.t * p2.t,
                m11: p2.t * p2.t,
                m21: p2.t,
                m31: 1f,

                m02: 3f * v.t * v.t,
                m12: 2f * v.t,
                m22: 1f,
                m32: 0f,

                m03: 6f * a.t,
                m13: 2f,
                m23: 0f,
                m33: 0f
            );

            var rightMatrix = new float4(p1.v, p2.v, v.v, a.v);
            var inversedMatrix = math.inverse(matrix);

            var result = new CubicFunction(
                a: math.dot(inversedMatrix.c0, rightMatrix),
                b: math.dot(inversedMatrix.c1, rightMatrix),
                c: math.dot(inversedMatrix.c2, rightMatrix),
                d: math.dot(inversedMatrix.c3, rightMatrix));

            return result;
        }


        static public CubicFunction Solve3Points(
            (float t, float v) p1,
            (float t, float v) p2,
            (float t, float v) p3)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // a * rt^3 + b * rt^2 + c * rt + d = r;
            // a = 0

            var matrix = new float4x4(
                m00: 0f,
                m10: p1.t * p1.t,
                m20: p1.t,
                m30: 1f,

                m01: 0f,
                m11: p2.t * p2.t,
                m21: p2.t,
                m31: 1f,

                m02: 0f,
                m12: p3.t * p3.t,
                m22: p3.t,
                m32: 1f,

                m03: 1f,
                m13: 0f,
                m23: 0f,
                m33: 0f
            );

            var rightMatrix = new float4(p1.v, p2.v, p3.v, 0f);
            var inversedMatrix = math.inverse(matrix);

            var result = new CubicFunction(
                a: 0f,
                b: math.dot(inversedMatrix.c1, rightMatrix),
                c: math.dot(inversedMatrix.c2, rightMatrix),
                d: math.dot(inversedMatrix.c3, rightMatrix));

            return result;
        }

        static public CubicFunction Solve4Points(
            (float t, float v) p1,
            (float t, float v) p2,
            (float t, float v) p3,
            (float t, float v) p4)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // a * rt^3 + b * rt^2 + c * rt + d = r;
            // a * st^3 + b * st^2 + c * st + d = s;

            var matrix = new float4x4(
                m00: p1.t * p1.t * p1.t,
                m10: p1.t * p1.t,
                m20: p1.t,
                m30: 1f,

                m01: p2.t * p2.t * p2.t,
                m11: p2.t * p2.t,
                m21: p2.t,
                m31: 1f,

                m02: p3.t * p3.t * p3.t,
                m12: p3.t * p3.t,
                m22: p3.t,
                m32: 1f,

                m03: p4.t * p4.t * p4.t,
                m13: p4.t * p4.t,
                m23: p4.t,
                m33: 1f
            );

            var rightMatrix = new float4(p1.v, p2.v, p3.v, p4.v);
            var inversedMatrix = math.inverse(matrix);

            var result = new CubicFunction(
                a: math.dot(inversedMatrix.c0, rightMatrix),
                b: math.dot(inversedMatrix.c1, rightMatrix),
                c: math.dot(inversedMatrix.c2, rightMatrix),
                d: math.dot(inversedMatrix.c3, rightMatrix));

            return result;
        }

        static public CubicFunction Solve2PointsAndVelocity(
            (float t, float v) p1,
            (float t, float v) p2,
            (float t, float v) v)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // 3a * vt^2 + 2b * vt + c = v;
            // a = 0;

            var matrix = new float4x4(
                m00: 0f,
                m10: p1.t * p1.t,
                m20: p1.t,
                m30: 1f,

                m01: 0f,
                m11: p2.t * p2.t,
                m21: p2.t,
                m31: 1f,

                m02: 0f,
                m12: 2f * v.t,
                m22: 1,
                m32: 0f,

                m03: 1f,
                m13: 0f,
                m23: 0f,
                m33: 0f
            );

            var rightMatrix = new float4(p1.v, p2.v, v.v, 0f);
            var inversedMatrix = math.inverse(matrix);

            var result = new CubicFunction(
                a: 0f,
                b: math.dot(inversedMatrix.c1, rightMatrix),
                c: math.dot(inversedMatrix.c2, rightMatrix),
                d: math.dot(inversedMatrix.c3, rightMatrix));

            return result;
        }

        static public CubicFunction SolvePointAndVelocityAndConstantAccel(
            (float t, float v) p,
            (float t, float v) v,
            float accel)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // 3a * vt^2 + 2b * vt + c = v;
            // 2b = accel
            // a = 0;

            var matrix = new float4x4(
                m00: 0f,
                m10: p.t * p.t,
                m20: p.t,
                m30: 1f,

                m01: 0f,
                m11: 2f * v.t,
                m21: 1f,
                m31: 0f,

                m02: 0f,
                m12: 2f,
                m22: 0f,
                m32: 0f,

                m03: 1f,
                m13: 0f,
                m23: 0f,
                m33: 0f
            );

            var rightMatrix = new float4(p.v, v.v, accel, 0f);
            var inversedMatrix = math.inverse(matrix);

            var result = new CubicFunction(
                a: 0f,
                b: math.dot(inversedMatrix.c1, rightMatrix),
                c: math.dot(inversedMatrix.c2, rightMatrix),
                d: math.dot(inversedMatrix.c3, rightMatrix));

            return result;
        }

        static public CubicFunction Solve2Points(
            (float t, float v) p1,
            (float t, float v) p2)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // a = 0;
            // b = 0;

            var matrix = new float4x4(
                m00: 0f,
                m10: 0f,
                m20: p1.t,
                m30: 1f,

                m01: 0f,
                m11: 0f,
                m21: p2.t,
                m31: 1f,

                m02: 1f,
                m12: 0f,
                m22: 0f,
                m32: 0f,

                m03: 0f,
                m13: 1f,
                m23: 0f,
                m33: 0f
            );

            var rightMatrix = new float4(p1.v, p2.v, 0f, 0f);
            var inversedMatrix = math.inverse(matrix);

            var result = new CubicFunction(
                a: 0f,
                b: 0f,
                c: math.dot(inversedMatrix.c2, rightMatrix),
                d: math.dot(inversedMatrix.c3, rightMatrix));

            return result;
        }
    }
}

