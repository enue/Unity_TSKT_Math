using UnityEngine;
using System.Collections;
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

        static public CubicFunction Process3PointsAndVelocity(
            float pt, float p,
            float qt, float q,
            float rt, float r,
            float vt, float v)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // a * rt^3 + b * rt^2 + c * rt + d = r;
            // 3a * vt^2 + 2b * vt + c = v;
            var matrix = new Matrix4x4()
            {
                m00 = pt * pt * pt,
                m01 = pt * pt,
                m02 = pt,
                m03 = 1f,

                m10 = qt * qt * qt,
                m11 = qt * qt,
                m12 = qt,
                m13 = 1f,

                m20 = rt * rt * rt,
                m21 = rt * rt,
                m22 = rt,
                m23 = 1f,

                m30 = 3f * vt * vt,
                m31 = 2f * vt,
                m32 = 1f,
                m33 = 0f
            };
            var rightMatrix = new Vector4(p, q, r, v);
            var inversedMatrix = matrix.inverse;

            var result = new CubicFunction(
                a: Vector4.Dot(inversedMatrix.GetRow(0), rightMatrix),
                b: Vector4.Dot(inversedMatrix.GetRow(1), rightMatrix),
                c: Vector4.Dot(inversedMatrix.GetRow(2), rightMatrix),
                d: Vector4.Dot(inversedMatrix.GetRow(3), rightMatrix)
            );

            return result;
        }

        static public CubicFunction Process2PointsAnd2Velocities(
            float pt, float p,
            float qt, float q,
            float ut, float u,
            float vt, float v)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // 3a * ut^2 + 2b * ut + c = u;
            // 3a * vt^2 + 2b * vt + c = v;

            var matrix = new Matrix4x4()
            {
                m00 = pt * pt * pt,
                m01 = pt * pt,
                m02 = pt,
                m03 = 1f,

                m10 = qt * qt * qt,
                m11 = qt * qt,
                m12 = qt,
                m13 = 1f,

                m20 = 3f * ut * ut,
                m21 = 2f * ut,
                m22 = 1f,
                m23 = 0f,

                m30 = 3f * vt * vt,
                m31 = 2f * vt,
                m32 = 1f,
                m33 = 0f
            };

            var rightMatrix = new Vector4(p, q, u, v);
            var inversedMatrix = matrix.inverse;

            var result = new CubicFunction(
                a: Vector4.Dot(inversedMatrix.GetRow(0), rightMatrix),
                b: Vector4.Dot(inversedMatrix.GetRow(1), rightMatrix),
                c: Vector4.Dot(inversedMatrix.GetRow(2), rightMatrix),
                d: Vector4.Dot(inversedMatrix.GetRow(3), rightMatrix));

            return result;
        }

        static public CubicFunction Process2PointsAndConstantAccel(
            float pt, float p,
            float qt, float q,
            float accell)
        {
            // b * pt^2 + pt * c + d = p
            // b * qt^2 + qt * c + d = q
            // 2b = accell
            // a = 0

            var matrix = new Matrix4x4()
            {
                m00 = 0f,
                m01 = pt * pt,
                m02 = pt,
                m03 = 1f,

                m10 = 0f,
                m11 = qt * qt,
                m12 = qt,
                m13 = 1f,

                m20 = 0f,
                m21 = 2f,
                m22 = 0f,
                m23 = 0f,

                m30 = 1f,
                m31 = 0f,
                m32 = 0f,
                m33 = 0f
            };

            var rightMatrix = new Vector4(p, q, accell, 0f);
            var inversedMatrix = matrix.inverse;

            var result = new CubicFunction(
                a: 0f,
                b: Vector4.Dot(inversedMatrix.GetRow(1), rightMatrix),
                c: Vector4.Dot(inversedMatrix.GetRow(2), rightMatrix),
                d: Vector4.Dot(inversedMatrix.GetRow(3), rightMatrix));

            return result;
        }


        static public CubicFunction Process2PointsVelocityAndAccel(
            float pt, float p,
            float qt, float q,
            float vt, float v,
            float accelT, float accel)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // 3a * vt^2 + 2b * vt + c = v;
            // 6a * accelT + 2b = accel;

            var matrix = new Matrix4x4()
            {
                m00 = pt * pt * pt,
                m01 = pt * pt,
                m02 = pt,
                m03 = 1f,

                m10 = qt * qt * qt,
                m11 = qt * qt,
                m12 = qt,
                m13 = 1f,

                m20 = 3f * vt * vt,
                m21 = 2f * vt,
                m22 = 1f,
                m23 = 0f,

                m30 = 6f * accelT,
                m31 = 2f,
                m32 = 0f,
                m33 = 0f
            };

            var rightMatrix = new Vector4(p, q, v, accel);
            var inversedMatrix = matrix.inverse;

            var result = new CubicFunction(
                a: Vector4.Dot(inversedMatrix.GetRow(0), rightMatrix),
                b: Vector4.Dot(inversedMatrix.GetRow(1), rightMatrix),
                c: Vector4.Dot(inversedMatrix.GetRow(2), rightMatrix),
                d: Vector4.Dot(inversedMatrix.GetRow(3), rightMatrix));

            return result;
        }


        static public CubicFunction Process3Points(
            float pt, float p,
            float qt, float q,
            float rt, float r)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // a * rt^3 + b * rt^2 + c * rt + d = r;
            // a = 0

            var matrix = new Matrix4x4()
            {
                m00 = 0f,
                m01 = pt * pt,
                m02 = pt,
                m03 = 1f,

                m10 = 0f,
                m11 = qt * qt,
                m12 = qt,
                m13 = 1f,

                m20 = 0f,
                m21 = rt * rt,
                m22 = rt,
                m23 = 1f,

                m30 = 1f,
                m31 = 0f,
                m32 = 0f,
                m33 = 0f
            };

            var rightMatrix = new Vector4(p, q, r, 0f);
            var inversedMatrix = matrix.inverse;

            var result = new CubicFunction(
                a: 0f,
                b: Vector4.Dot(inversedMatrix.GetRow(1), rightMatrix),
                c: Vector4.Dot(inversedMatrix.GetRow(2), rightMatrix),
                d: Vector4.Dot(inversedMatrix.GetRow(3), rightMatrix));

            return result;
        }

        static public CubicFunction Process4Points(
            float pt, float p,
            float qt, float q,
            float rt, float r,
            float st, float s)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // a * rt^3 + b * rt^2 + c * rt + d = r;
            // a * st^3 + b * st^2 + c * st + d = s;

            var matrix = new Matrix4x4()
            {
                m00 = pt * pt * pt,
                m01 = pt * pt,
                m02 = pt,
                m03 = 1f,

                m10 = qt * qt * qt,
                m11 = qt * qt,
                m12 = qt,
                m13 = 1f,

                m20 = rt * rt * rt,
                m21 = rt * rt,
                m22 = rt,
                m23 = 1f,

                m30 = st * st * st,
                m31 = st * st,
                m32 = st,
                m33 = 1f
            };

            var rightMatrix = new Vector4(p, q, r, s);
            var inversedMatrix = matrix.inverse;

            var result = new CubicFunction(
                a: Vector4.Dot(inversedMatrix.GetRow(0), rightMatrix),
                b: Vector4.Dot(inversedMatrix.GetRow(1), rightMatrix),
                c: Vector4.Dot(inversedMatrix.GetRow(2), rightMatrix),
                d: Vector4.Dot(inversedMatrix.GetRow(3), rightMatrix));

            return result;
        }

        static public CubicFunction Process2PointsAndVelocity(
            float pt, float p,
            float qt, float q,
            float vt, float v)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // 3a * vt^2 + 2b * vt + c = v;
            // a = 0;

            var matrix = new Matrix4x4()
            {
                m00 = 0f,
                m01 = pt * pt,
                m02 = pt,
                m03 = 1f,

                m10 = 0f,
                m11 = qt * qt,
                m12 = qt,
                m13 = 1f,

                m20 = 0f,
                m21 = 2f * vt,
                m22 = 1,
                m23 = 0f,

                m30 = 1f,
                m31 = 0f,
                m32 = 0f,
                m33 = 0f
            };

            var rightMatrix = new Vector4(p, q, v, 0f);
            var inversedMatrix = matrix.inverse;

            var result = new CubicFunction(
                a: 0f,
                b: Vector4.Dot(inversedMatrix.GetRow(1), rightMatrix),
                c: Vector4.Dot(inversedMatrix.GetRow(2), rightMatrix),
                d: Vector4.Dot(inversedMatrix.GetRow(3), rightMatrix));

            return result;
        }

        static public CubicFunction Process2Points(
            float pt, float p,
            float qt, float q)
        {
            // a * pt^3 + b * pt^2 + c * pt + d = p;
            // a * qt^3 + b * qt^2 + c * qt + d = q;
            // a = 0;
            // b = 0;

            var matrix = new Matrix4x4()
            {
                m00 = 0f,
                m01 = 0f,
                m02 = pt,
                m03 = 1f,

                m10 = 0f,
                m11 = 0f,
                m12 = qt,
                m13 = 1f,

                m20 = 1f,
                m21 = 0f,
                m22 = 0f,
                m23 = 0f,

                m30 = 0f,
                m31 = 1f,
                m32 = 0f,
                m33 = 0f
            };

            var rightMatrix = new Vector4(p, q, 0f, 0f);
            var inversedMatrix = matrix.inverse;

            var result = new CubicFunction(
                a: 0f,
                b: 0f,
                c: Vector4.Dot(inversedMatrix.GetRow(2), rightMatrix),
                d: Vector4.Dot(inversedMatrix.GetRow(3), rightMatrix));

            return result;
        }
    }
}

