using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT
{
    public class RectMap<T>
    {
        readonly float cellSize = 1f;
        readonly float offset = 0.5f;
        readonly UnlimitedArray2<List<(Rect key, T value)>> cells = new UnlimitedArray2<List<(Rect, T)>>(0, 0);

        public RectMap(float cellSize, float offset)
        {
            this.cellSize = cellSize;
            this.offset = offset;
        }

        public void Add(Rect rect, T value)
        {
            var cells = GetCells(rect);
            for (int i = 0; i < cells.width + 1; ++i)
            {
                for (int j = 0; j < cells.height + 1; ++j)
                {
                    var rects = this.cells[i + cells.xMin, j + cells.yMin];
                    if (rects == null)
                    {
                        rects = new List<(Rect, T)>();
                        this.cells[i + cells.xMin, j + cells.yMin] = rects;
                    }
                    rects.Add((rect, value));
                }
            }
        }

        public bool TryGetFirst(Vector2 position, out (Rect key, T value) result)
        {
            var cells = GetCells(position);
            for (int i = 0; i < cells.width + 1; ++i)
            {
                for (int j = 0; j < cells.height + 1; ++j)
                {
                    var pairs = this.cells[i + cells.xMin, j + cells.yMin];
                    if (pairs != null && pairs.Count > 0)
                    {
                        foreach (var pair in pairs)
                        {
                            if (ContainsAsClosedInterval(pair.key, position))
                            {
                                result = pair;
                                return true;
                            }
                        }
                    }
                }
            }

            result = default;
            return false;
        }

        public IEnumerable<(Rect, T)> Find(Vector2 position)
        {
            var cells = GetCells(position);
            for (int i = 0; i < cells.width + 1; ++i)
            {
                for (int j = 0; j < cells.height + 1; ++j)
                {
                    var pairs = this.cells[i + cells.xMin, j + cells.yMin];
                    if (pairs != null && pairs.Count > 0)
                    {
                        foreach(var pair in pairs)
                        {
                            if (ContainsAsClosedInterval(pair.key, position))
                            {
                                yield return pair;
                            }

                        }
                    }
                }
            }
        }

        RectInt GetCells(Vector2 position)
        {
            var maxI = Mathf.FloorToInt((position.x + offset) / cellSize);
            int minI;
            if ((position.x + offset) % cellSize == 0f)
            {
                minI = maxI - 1;
            }
            else
            {
                minI = maxI;
            }

            var maxJ = Mathf.FloorToInt((position.y + offset) / cellSize);
            int minJ;
            if ((position.y + offset) % cellSize == 0f)
            {
                minJ = maxJ - 1;
            }
            else
            {
                minJ = maxJ;
            }

            return new RectInt(minI, minJ, maxI - minI, maxJ - minJ);
        }

        RectInt GetCells(Rect rect)
        {
            var min = GetCells(rect.min);
            var max = GetCells(rect.max);

            return new RectInt(min.xMin, min.yMin, max.xMax - min.xMin, max.yMax - min.yMin);
        }

        static bool ContainsAsClosedInterval(Rect rect, Vector2 point)
        {
            return (rect.xMin - point.x) * (rect.xMax - point.x) <= 0f
            && (rect.yMin - point.y) * (rect.yMax - point.y) <= 0f;
        }
    }
}
