using UnityEngine;
using System.Collections;

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
        public float Evaluate(float x)
        {
            return a * x * x * x + b * x * x + c * x + d;
        }

        public float Velocity(float x)
        {
            return 3f * a * x * x + 2f * b * x + c;
        }

        static public CubicFunction Process3PointsAndVelocity(
            float pt, float p,
            float qt, float q,
            float rt, float r,
            float vt, float v)
        {
            var matrix = new Matrix4x4();
            matrix[0, 0] = pt * pt * pt;
            matrix[0, 1] = pt * pt;
            matrix[0, 2] = pt;
            matrix[0, 3] = 1f;
            matrix[1, 0] = qt * qt * qt;
            matrix[1, 1] = qt * qt;
            matrix[1, 2] = qt;
            matrix[1, 3] = 1f;
            matrix[2, 0] = rt * rt * rt;
            matrix[2, 1] = rt * rt;
            matrix[2, 2] = rt;
            matrix[2, 3] = 1f;
            matrix[3, 0] = 3f * vt * vt;
            matrix[3, 1] = 2f * vt;
            matrix[3, 2] = 1f;
            matrix[3, 3] = 0f;

            Vector4 rightMatrix = new Vector4(p, q, r, v);
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
            var matrix = new Matrix4x4();
            matrix[0, 0] = pt * pt * pt;
            matrix[0, 1] = pt * pt;
            matrix[0, 2] = pt;
            matrix[0, 3] = 1f;

            matrix[1, 0] = qt * qt * qt;
            matrix[1, 1] = qt * qt;
            matrix[1, 2] = qt;
            matrix[1, 3] = 1f;

            matrix[2, 0] = 3f * ut * ut;
            matrix[2, 1] = 2f * ut;
            matrix[2, 2] = 1f;
            matrix[2, 3] = 0f;

            matrix[3, 0] = 3f * vt * vt;
            matrix[3, 1] = 2f * vt;
            matrix[3, 2] = 1f;
            matrix[3, 3] = 0f;

            Vector4 rightMatrix = new Vector4(p, q, u, v);
            var inversedMatrix = matrix.inverse;

            var result = new CubicFunction(
                a: Vector4.Dot(inversedMatrix.GetRow(0), rightMatrix),
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
            var matrix = new Matrix4x4();
            matrix[0, 0] = pt * pt * pt;
            matrix[0, 1] = pt * pt;
            matrix[0, 2] = pt;
            matrix[0, 3] = 1f;

            matrix[1, 0] = qt * qt * qt;
            matrix[1, 1] = qt * qt;
            matrix[1, 2] = qt;
            matrix[1, 3] = 1f;

            matrix[2, 0] = 3f * vt * vt;
            matrix[2, 1] = 2f * vt;
            matrix[2, 2] = 1f;
            matrix[2, 3] = 0f;

            matrix[3, 0] = 6f * accelT;
            matrix[3, 1] = 2f;
            matrix[3, 2] = 0f;
            matrix[3, 3] = 0f;

            Vector4 rightMatrix = new Vector4(p, q, v, accel);
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
            var matrix = new Matrix4x4();
            matrix[0, 0] = 0f;
            matrix[0, 1] = pt * pt;
            matrix[0, 2] = pt;
            matrix[0, 3] = 1f;

            matrix[1, 0] = 0f;
            matrix[1, 1] = qt * qt;
            matrix[1, 2] = qt;
            matrix[1, 3] = 1f;

            matrix[2, 0] = 0f;
            matrix[2, 1] = rt * rt;
            matrix[2, 2] = rt;
            matrix[2, 3] = 1f;

            matrix[3, 0] = 1f;
            matrix[3, 1] = 0f;
            matrix[3, 2] = 0f;
            matrix[3, 3] = 0f;

            Vector4 rightMatrix = new Vector4(p, q, r, 0f);
            var inversedMatrix = matrix.inverse;

            var result = new CubicFunction(
                a: Vector4.Dot(inversedMatrix.GetRow(0), rightMatrix),
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
            var matrix = new Matrix4x4();
            matrix[0, 0] = pt * pt * pt;
            matrix[0, 1] = pt * pt;
            matrix[0, 2] = pt;
            matrix[0, 3] = 1f;
            matrix[1, 0] = qt * qt * qt;
            matrix[1, 1] = qt * qt;
            matrix[1, 2] = qt;
            matrix[1, 3] = 1f;
            matrix[2, 0] = rt * rt * rt;
            matrix[2, 1] = rt * rt;
            matrix[2, 2] = rt;
            matrix[2, 3] = 1f;
            matrix[3, 0] = st * st * st;
            matrix[3, 1] = st * st;
            matrix[3, 2] = st;
            matrix[3, 3] = 1f;

            Vector4 rightMatrix = new Vector4(p, q, r, s);
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
            var matrix = new Matrix4x4();
            matrix[0, 0] = 0f;
            matrix[0, 1] = pt * pt;
            matrix[0, 2] = pt;
            matrix[0, 3] = 1f;

            matrix[1, 0] = 0f;
            matrix[1, 1] = qt * qt;
            matrix[1, 2] = qt;
            matrix[1, 3] = 1f;

            matrix[2, 0] = 0f;
            matrix[2, 1] = 2f * vt;
            matrix[2, 2] = 1f;
            matrix[2, 3] = 0f;

            matrix[3, 0] = 1f;
            matrix[3, 1] = 0f;
            matrix[3, 2] = 0f;
            matrix[3, 3] = 0f;

            Vector4 rightMatrix = new Vector4(p, q, v, 0f);
            var inversedMatrix = matrix.inverse;

            var result = new CubicFunction(
                a: Vector4.Dot(inversedMatrix.GetRow(0), rightMatrix),
                b: Vector4.Dot(inversedMatrix.GetRow(1), rightMatrix),
                c: Vector4.Dot(inversedMatrix.GetRow(2), rightMatrix),
                d: Vector4.Dot(inversedMatrix.GetRow(3), rightMatrix));

            return result;
        }

        static public CubicFunction Process2Points(
            float pt, float p,
            float qt, float q)
        {
            var matrix = new Matrix4x4();
            matrix[0, 0] = 0f;
            matrix[0, 1] = 0f;
            matrix[0, 2] = pt;
            matrix[0, 3] = 1f;

            matrix[1, 0] = 0f;
            matrix[1, 1] = 0f;
            matrix[1, 2] = qt;
            matrix[1, 3] = 1f;

            matrix[2, 0] = 1f;
            matrix[2, 1] = 0f;
            matrix[2, 2] = 0f;
            matrix[2, 3] = 0f;

            matrix[3, 0] = 0f;
            matrix[3, 1] = 1f;
            matrix[3, 2] = 0f;
            matrix[3, 3] = 0f;

            Vector4 rightMatrix = new Vector4(p, q, 0f, 0f);
            var inversedMatrix = matrix.inverse;

            var result = new CubicFunction(
                a: Vector4.Dot(inversedMatrix.GetRow(0), rightMatrix),
                b: Vector4.Dot(inversedMatrix.GetRow(1), rightMatrix),
                c: Vector4.Dot(inversedMatrix.GetRow(2), rightMatrix),
                d: Vector4.Dot(inversedMatrix.GetRow(3), rightMatrix));

            return result;
        }
    }
}

