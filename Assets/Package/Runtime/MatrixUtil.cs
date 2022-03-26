using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace TSKT
{
    public static class MatrixUtil
    {
        public static double4 SolveSimultaneousEquations(double4x4 left, double4 right)
        {
            if (TryLUDecomposition(left, out var L, out var U))
            {
                var a = right.x;
                var b = right.y - L.c0.y * a;
                var c = right.z - L.c0.z * a - L.c1.z * b;
                var d = right.w - L.c0.w * a - L.c1.w * b - L.c2.w * c;
                var p = new double4(a, b, c, d);

                var w = p.w / U.c3.w;
                var z = (p.z - U.c3.z * w) / U.c2.z;
                var y = (p.y - U.c2.y * z - U.c3.y * w) / U.c1.y;
                var x = (p.x - U.c1.x * y - U.c2.x * z - U.c3.x * w) / U.c0.x;
                var result = new double4(x, y, z, w);

                return result;
            }
            var inversed = math.inverse(left);
            return math.mul(inversed, right);
        }

        public static bool TryLUDecomposition(double4x4 matrix, out double4x4 L, out double4x4 U)
        {
            L = double4x4.identity;
            U = double4x4.zero;

            var U00 = U.c0.x = matrix.c0.x;
            if (U00 == 0)
            {
                return false;
            }
            var U01 = U.c1.x = matrix.c1.x;
            var U02 = U.c2.x = matrix.c2.x;
            var U03 = U.c3.x = matrix.c3.x;

            var L10 = L.c0.y = matrix.c0.y / U00;

            var U11 = U.c1.y = matrix.c1.y - L10 * U01;
            if (U11 == 0)
            {
                return false;
            }
            var U12 = U.c2.y = matrix.c2.y - L10 * U02;
            var U13 = U.c3.y = matrix.c3.y - L10 * U03;

            var L20 = L.c0.z = matrix.c0.z / U00;
            var L21 = L.c1.z = (matrix.c1.z - L20 * U01) / U11;
            var U22 = U.c2.z = matrix.c2.z - L20 * U02 - L21 * U12;
            if (U22 == 0)
            {
                return false;
            }
            var U23 = U.c3.z = matrix.c3.z - L20 * U03 - L21 * U13;

            var L30 = L.c0.w = matrix.c0.w / U00;
            var L31 = L.c1.w = (matrix.c1.w - L30 * U01) / U11;
            var L32 = L.c2.w = (matrix.c2.w - L30 * U02 - L31 * U12) / U22;
            var U33 = U.c3.w = matrix.c3.w - L30 * U03 - L31 * U13 - L32 * U23;

            if (U33 == 0)
            {
                return false;
            }

            return true;
        }
        public static void LUDecomposition(double3x3 matrix, out double3x3 L, out double3x3 U)
        {
            L = double3x3.identity;
            U = double3x3.zero;

            var U00 = U.c0.x = matrix.c0.x;
            var U01 = U.c1.x = matrix.c1.x;
            var U02 = U.c2.x = matrix.c2.x;

            var L10 = L.c0.y = matrix.c0.y / U00;

            var U11 = U.c1.y = matrix.c1.y - L10 * U01;
            var U12 = U.c2.y = matrix.c2.y - L10 * U02;

            var L20 = L.c0.z = matrix.c0.z / U00;
            var L21 = L.c1.z = (matrix.c1.z - L20 * U01) / U11;
            var U22 = U.c2.z = matrix.c2.z - L20 * U02 - L21 * U12;
        }

        public static void LUDecomposition(double2x2 matrix, out double2x2 L, out double2x2 U)
        {
            L = double2x2.identity;
            U = double2x2.zero;

            var U00 = U.c0.x = matrix.c0.x;
            var U01 = U.c1.x = matrix.c1.x;

            var L10 = L.c0.y = matrix.c0.y / U00;

            var U11 = U.c1.y = matrix.c1.y - L10 * U01;
        }
    }
}
