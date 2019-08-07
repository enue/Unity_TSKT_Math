using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;

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

        public float Acceleration(float x)
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
            var matrix = float4x4(
                pt * pt * pt,
                pt * pt,
                pt,
                1f,

                qt * qt * qt,
                qt * qt,
                qt,
                1f,

                rt * rt * rt,
                rt * rt,
                rt,
                1f,

                3f * vt * vt,
                2f * vt,
                1f,
                0f);

            var m = mul(inverse(matrix), float4(p, q, r, v));

            var result = new CubicFunction(
                a: m.x,
                b: m.y,
                c: m.z,
                d: m.w);

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

            var matrix = float4x4(
                pt * pt * pt,
                pt * pt,
                pt,
                1f,

                qt * qt * qt,
                qt * qt,
                qt,
                1f,

                3f * ut * ut,
                2f * ut,
                1f,
                0f,

                3f * vt * vt,
                2f * vt,
                1f,
                0f);

            var m = mul(inverse(matrix), float4(p, q, u, v));

            var result = new CubicFunction(
                a: m.x,
                b: m.y,
                c: m.z,
                d: m.w);

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
            var matrix = float3x3(
                pt * pt,
                pt,
                1f,

                qt * qt,
                qt,
                1f,

                2f,
                0f,
                0f);


            var m = mul(inverse(matrix), float3(p, q, accell));

            var result = new CubicFunction(
                a: 0f,
                b: m.x,
                c: m.y,
                d: m.z);

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

            var matrix = float4x4(
                pt * pt * pt,
                pt * pt,
                pt,
                1f,

                qt * qt * qt,
                qt * qt,
                qt,
                1f,

                3f * vt * vt,
                2f * vt,
                1f,
                0f,

                6f * accelT,
                2f,
                0f,
                0f);

            var m = mul(inverse(matrix), float4(p, q, v, accel));

            var result = new CubicFunction(
                a: m.x,
                b: m.y,
                c: m.z,
                d: m.w);

            return result;
        }


        static public CubicFunction Process3Points(
            float pt, float p,
            float qt, float q,
            float rt, float r)
        {
            // b * pt^2 + c * pt + d = p;
            // b * qt^2 + c * qt + d = q;
            // b * rt^2 + c * rt + d = r;
            // a = 0

            var matrix = float3x3(
                pt * pt,
                pt,
                1f,

                qt * qt,
                qt,
                1f,

                rt * rt,
                rt,
                1f);

            var m = mul(inverse(matrix), float3(p, q, r));

            var result = new CubicFunction(
                a: 0f,
                b: m.x,
                c: m.y,
                d: m.z);

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
            var matrix = float4x4(
                pt * pt * pt,
                pt * pt,
                pt,
                1f,
                qt * qt * qt,
                qt * qt,
                qt,
                1f,
                rt * rt * rt,
                rt * rt,
                rt,
                1f,
                st * st * st,
                st * st,
                st,
                1f);

            var m = mul(inverse(matrix), float4(p, q, r, s));

            var result = new CubicFunction(
                a: m.x,
                b: m.y,
                c: m.z,
                d: m.w);

            return result;
        }

        static public CubicFunction Process2PointsAndVelocity(
            float pt, float p,
            float qt, float q,
            float vt, float v)
        {
            // b * pt^2 + c * pt + d = p;
            // b * qt^2 + c * qt + d = q;
            // 2b * vt + c = v;
            // a = 0;

            var matrix = new float3x3(
                pt * pt,
                pt,
                1f,

                qt * qt,
                qt,
                1f,

                2f * vt,
                1f,
                0f);

            var m = mul(inverse(matrix), float3(p, q, v));

            var result = new CubicFunction(
                a: 0f,
                b: m.x,
                c: m.y,
                d: m.z);

            return result;
        }

        static public CubicFunction Process2Points(
            float pt, float p,
            float qt, float q)
        {
            // c * pt + d = p;
            // c * qt + d = q;
            // a = 0;
            // b = 0;

            var matrix = float2x2(
                pt,
                1f,

                qt,
                1f);

            var m = mul(inverse(matrix), float2(p, q));

            var result = new CubicFunction(
                a: 0f,
                b: 0f,
                c: m.x,
                d: m.y);

            return result;
        }
    }
}

