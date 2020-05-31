using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT
{
    public readonly struct Segment2Int
    {
        public readonly Vector2Int from;
        public readonly Vector2Int to;

        public Segment2Int(Vector2Int from, Vector2Int to)
        {
            this.from = from;
            this.to = to;
        }
    }
}
