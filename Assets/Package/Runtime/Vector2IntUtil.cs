using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#nullable enable

namespace TSKT
{
    public static class Vector2IntUtil
    {
        public static readonly Vector2Int[] Directions = new Vector2Int[]
        {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down };

        public static int Cross(Vector2Int lhs, Vector2Int rhs)
        {
            return lhs.x * rhs.y - lhs.y * rhs.x;
        }
        public static int Dot(Vector2Int lhs, Vector2Int rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }

        public static Vector2Int Rotate(Vector2Int src, float radian)
        {
            var sin = Mathf.Sin(radian);
            var cos = Mathf.Cos(radian);
            return Rotate(src, cos, sin);
        }

        public static Vector2Int Rotate(Vector2Int src, float cos, float sin)
        {
            return new Vector2Int(
                Mathf.RoundToInt(src.x * cos - src.y * sin),
                Mathf.RoundToInt(src.x * sin + src.y * cos));
        }

        public static Vector2Int Rotate(Vector2Int src, Vector2Int rotate)
        {
            return Rotate(src, rotate.x, rotate.y);
        }

        public static int GetManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}
