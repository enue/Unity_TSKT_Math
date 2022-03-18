using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#nullable enable

namespace TSKT
{
    public class Vector2IntComparer : IComparer<Vector2Int>
    {
        public static Vector2IntComparer Instance { get; } = new Vector2IntComparer();

        Vector2IntComparer()
        {
        }

        public int Compare(Vector2Int a, Vector2Int b)
        {
            return Comparison(a, b);
        }

        public static int Comparison(Vector2Int a, Vector2Int b)
        {
            if (a.x > b.x)
            {
                return 1;
            }
            if (a.x < b.x)
            {
                return -1;
            }
            if (a.y > b.y)
            {
                return 1;
            }
            if (a.y < b.y)
            {
                return -1;
            }
            return 0;
        }
    }
}
