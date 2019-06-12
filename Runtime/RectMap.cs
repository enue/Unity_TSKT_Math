﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT
{
    public class RectMap<T>
    {
        readonly float cellSize = 1f;
        readonly float offset = 0.5f;

        UnlimitedArray2<List<(Rect key, T value)>> cells;

        public RectMap(float cellSize, float offset)
        {
            this.cellSize = cellSize;
            this.offset = offset;
        }

        public void Add(Rect rect, T value)
        {
            var range = GetCellRange(rect);
            if (cells == null)
            {
                cells = new UnlimitedArray2<List<(Rect key, T value)>>(range.xMin, range.yMin, range.width, range.height);
            }
            else
            {
                cells.EnsureCapacity(range);
            }
            for (int i = 0; i < range.width + 1; ++i)
            {
                for (int j = 0; j < range.height + 1; ++j)
                {
                    var list = cells[i + range.xMin, j + range.yMin];
                    if (list == null)
                    {
                        list = new List<(Rect key, T value)>();
                        cells[i + range.xMin, j + range.yMin] = list;
                    }
                    list.Add((rect, value));
                }
            }
        }

        public bool TryGetFirst(Vector2 position, out (Rect key, T value) result)
        {
            if (cells == null)
            {
                result = default;
                return false;
            }

            var range = GetCellRange(position);
            for (int i = 0; i < range.width + 1; ++i)
            {
                for (int j = 0; j < range.height + 1; ++j)
                {
                    var pairs = cells?[i + range.xMin, j + range.yMin];
                    if (pairs != null && pairs.Count > 0)
                    {
                        foreach (var pair in pairs)
                        {
                            if (pair.key.Contains(position))
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
            if (cells == null)
            {
                yield break;
            }
            var range = GetCellRange(position);
            for (int i = 0; i < range.width + 1; ++i)
            {
                for (int j = 0; j < range.height + 1; ++j)
                {
                    var pairs = cells?[i + range.xMin, j + range.yMin];
                    if (pairs != null && pairs.Count > 0)
                    {
                        foreach(var pair in pairs)
                        {
                            if (pair.key.Contains(position))
                            {
                                yield return pair;
                            }
                        }
                    }
                }
            }
        }

        RectInt GetCellRange(Vector2 position)
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

        RectInt GetCellRange(Rect rect)
        {
            var min = GetCellRange(rect.min).min;
            // Rect.Containsは[min, max)で処理されるので、rect.maxがCellの境界をまたぐ場合は大きい側のCellを切り捨てられる
            var max = GetCellRange(rect.max).min;

            return new RectInt(min.x, min.y, max.x - min.x, max.y - min.y);
        }
    }
}
