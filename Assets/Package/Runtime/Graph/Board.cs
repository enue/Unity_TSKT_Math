#nullable enable
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace TSKT
{
    public class Board : IUnmanagedGraph<Vector2Int>, IUnmanagedGraph<int>, IGraph<Vector2Int>, IGraph<int>
    {
        readonly float[] costs;
        public int Width => costs.Length / Height;
        public int Height { get; }
        public DirectionMap<float>? DirectionCostMap { get; set; }

        public Board(int w, int h)
        {
            Height = h;
            costs = new float[w * h];
            System.Array.Fill(costs, 1f);
        }

        public Vector2Int IndexToCell(int index) => new(index / Height, index % Height);

        public int CellToIndex(in Vector2Int cell) => cell.x * Height + cell.y;

        public bool TryGetCost(int i, int j, out float value)
        {
            var cost = costs[i * Height + j];
            if (cost < float.PositiveInfinity)
            {
                value = cost;
                return true;
            }
            value = default;
            return false;
        }

        public void SetCost(int i, int j, float cost)
        {
            costs[i * Height + j] = cost;
        }

        public void Disable(int i, int j)
        {
            costs[i * Height + j] = float.PositiveInfinity;
        }

        public bool Contains(int i, int j)
        {
            return i >= 0 && j >= 0 && i < Width && j < Height;
        }

        public IEnumerable<(Vector2Int endNode, float weight)> GetEdgesFrom(Vector2Int node)
        {
            if (!Contains(node.x, node.y))
            {
                yield break;
            }
            foreach (var it in Vector2IntUtil.Directions)
            {
                var next = it + node;
                if (Contains(next.x, next.y))
                {
                    var cost = costs[next.x * Height + next.y];
                    if (cost < float.PositiveInfinity)
                    {
                        if (DirectionCostMap != null)
                        {
                            cost += DirectionCostMap[it];
                        }
                        yield return (next, cost);
                    }
                }
            }
        }

        public IEnumerable<(int endNode, float weight)> GetEdgesFrom(int begin)
        {
            foreach (var (endNode, weight) in GetEdgesFrom(IndexToCell(begin)))
            {
                yield return (CellToIndex(endNode), weight);
            }
        }

        public void GetEdgesFrom(Vector2Int node, Span<(Vector2Int endNode, float weight)> dest, out int writtenCount)
        {
            writtenCount = 0;
            if (!Contains(node.x, node.y))
            {
                return;
            }

            ReadOnlySpan<Vector2Int> directions = stackalloc Vector2Int[4]
            {
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.left,
                Vector2Int.down
            };

            foreach (var it in directions)
            {
                var next = it + node;
                if (Contains(next.x, next.y))
                {
                    var cost = costs[next.x * Height + next.y];
                    if (cost < float.PositiveInfinity)
                    {
                        if (DirectionCostMap != null)
                        {
                            cost += DirectionCostMap[it];
                        }
                        dest[writtenCount] = (next, cost);
                        ++writtenCount;
                    }
                }
            }
        }

        public void GetEdgesFrom(int begin, Span<(int endNode, float weight)> dest, out int writtenCount)
        {
            Span<(Vector2Int endNode, float weight)> t = stackalloc (Vector2Int endNode, float weight)[MaxEdgeCountFromOneNode];
            GetEdgesFrom(IndexToCell(begin), t, out writtenCount);
            for (int i = 0; i < writtenCount; ++i)
            {
                dest[i] = (CellToIndex(t[i].endNode), t[i].weight);
            }
        }

        public UnmanagedDistanceMap<Vector2Int> CreateDistanceMapFrom(in Vector2Int node) => new(this, node);
        public UnmanagedDistanceMap<int> CreateDistanceMapFrom(int node) => new(this, node);

        public UnmanagedAStarSearch<Vector2Int> CreateAStarSearch(in Vector2Int start)
        {
            return new UnmanagedAStarSearch<Vector2Int>(this, start, GetHeuristicFunctionForAStarSearch());
        }
        public UnmanagedAStarSearch<int> CreateAStarSearch(int start)
        {
            return new UnmanagedAStarSearch<int>(this, start, GetHeuristicFunctionForAStarSearchInCellIndex());
        }

        public System.Func<Vector2Int, Vector2Int, float> GetHeuristicFunctionForAStarSearch()
        {
            var c = ComputeHeuristicCoefficient();
            return (a, b) => Vector2IntUtil.GetManhattanDistance(a, b) * c;
        }
        public System.Func<int, int, float> GetHeuristicFunctionForAStarSearchInCellIndex()
        {
            var c = ComputeHeuristicCoefficient();
            return (a, b) => Vector2IntUtil.GetManhattanDistance(IndexToCell(a), IndexToCell(b)) * c;
        }

        float ComputeHeuristicCoefficient()
        {
            var minCost = costs.Min();
            if (DirectionCostMap != null)
            {
                minCost += Mathf.Min(DirectionCostMap.Right, DirectionCostMap.Left, DirectionCostMap.Up, DirectionCostMap.Down);
            }

            UnityEngine.Assertions.Assert.IsTrue(minCost > 0f);

            var cost = minCost - Mathf.Epsilon;
            if (cost < 0f)
            {
                cost = 0f;
            }

            return cost;
        }

        public int MaxEdgeCountFromOneNode => 4;

    }
}
