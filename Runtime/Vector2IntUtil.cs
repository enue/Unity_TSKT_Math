using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        public static Vector2Int Rotate(Vector2Int src, Vector2Int rotate)
        {
            return new Vector2Int(
                src.x * rotate.x - src.y * rotate.y,
                src.x * rotate.y + src.y * rotate.x);
        }

        public static int GetManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}
