using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TSKT
{
    public readonly struct Segment2 : IEquatable<Segment2>
    {
        public readonly Vector2 from;
        public readonly Vector2 to;

        public Segment2(Vector2 from, Vector2 to)
        {
            this.from = from;
            this.to = to;
        }

        readonly public bool Intersect(in Segment2 target)
        {
            {
                Vector3 a = to - from;
                Vector3 b = target.to - from;
                Vector3 c = target.from - from;

                var p = Vector3.Cross(a, b).z;
                var q = Vector3.Cross(a, c).z;

                // 一直線上に並んでいる場合
                if (p == 0f && q == 0f)
                {
                    var aa = Vector3.Dot(a, a);
                    var ab = Vector3.Dot(a, b);
                    var ac = Vector3.Dot(a, c);

                    // bとcがaを挟む
                    if ((ab - aa) * (ac - aa) <= 0f)
                    {
                        return true;
                    }
                    // bとcが原点を挟む
                    if (ab * ac <= 0f)
                    {
                        return true;
                    }
                    // bが原点とaの間にある
                    if (ab * (ab - aa) <= 0f)
                    {
                        return true;
                    }
                    return false;
                }

                if (p * q > 0f)
                {
                    return false;
                }
            }
            {
                Vector3 a = target.to - target.from;
                Vector3 b = to - target.from;
                Vector3 c = from - target.from;

                var p = Vector3.Cross(a, b).z;
                var q = Vector3.Cross(a, c).z;

                if (p * q > 0f)
                {
                    return false;
                }
            }
            return true;
        }

        readonly public bool Collide(in Rect rect)
        {
            if (rect.Contains(from))
            {
                return true;
            }

            if (from == to)
            {
                return false;
            }

            if (rect.Contains(to))
            {
                return true;
            }

            var a = new Segment2(rect.min, rect.max);
            if (Intersect(a))
            {
                return true;
            }

            var b = new Segment2(
                new Vector2(rect.xMin, rect.yMax),
                new Vector2(rect.xMax, rect.yMin)
            );
            if (Intersect(b))
            {
                return true;
            }
            return false;
        }

        readonly public Rect Bounds
        {
            get
            {
                return new Rect(
                    Mathf.Min(from.x, to.x),
                    Mathf.Min(from.y, to.y),
                    Mathf.Abs(to.x - from.x),
                    Mathf.Abs(to.y - from.y));
            }
        }

        readonly public override int GetHashCode()
        {
            return to.GetHashCode() ^ from.GetHashCode();
        }

        readonly bool IEquatable<Segment2>.Equals(Segment2 other)
        {
            return from == other.from
                && to == other.to;
        }
    }
}
