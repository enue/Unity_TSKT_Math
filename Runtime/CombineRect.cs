using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT
{
    public class CombineRect
    {
        public List<Rect> Rects { get; } = new List<Rect>();

        public void Append(Rect rect)
        {
            if (Rects.Any(_ => Contains(_, rect)))
            {
                return;
            }

            while (true)
            {
                Rects.RemoveAll(_ => Contains(rect, _));

                var combined = false;
                for (int i = 0; i < Rects.Count;)
                {
                    if (TryCombine(Rects[i], rect, out var combinedRect))
                    {
                        rect = combinedRect;
                        Rects.RemoveAt(i);
                        combined = true;
                        continue;
                    }

                    if (TryCombine(rect, Rects[i], out combinedRect))
                    {
                        rect = combinedRect;
                        Rects.RemoveAt(i);
                        combined = true;
                        continue;
                    }

                    ++i;
                }
                if (!combined)
                {
                    break;
                }
            }

            Rects.Add(rect);
        }

        static bool Contains(Rect a, Rect b)
        {
            return a.Contains(new Vector2(b.xMin, b.yMin), true)
                && a.Contains(new Vector2(b.xMax, b.yMax), true);
        }

        static bool TryCombine(in Rect a, in Rect b, out Rect result)
        {
            if (a.yMin == b.yMin
                && a.yMax == b.yMax
                && a.xMin <= b.xMin
                && b.xMin <= a.xMax
                && a.xMax <= b.xMax)
            {
                result = Rect.MinMaxRect(a.xMin, a.yMin, b.xMax, b.yMax);
                return true;
            }

            if (a.xMin == b.xMin
                && a.xMax == b.xMax
                && a.yMin <= b.yMin
                && b.yMin <= a.yMax
                && a.yMax <= b.yMax)
            {
                result = Rect.MinMaxRect(a.xMin, a.yMin, b.xMax, b.yMax);
                return true;
            }

            result = Rect.zero;
            return false;
        }
    }
}
