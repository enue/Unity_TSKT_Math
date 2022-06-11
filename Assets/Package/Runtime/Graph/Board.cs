using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#nullable enable

namespace TSKT
{
    public class Board : IGraph<Vector2Int>, IGraph<int>
    {
        public readonly double?[,] costs;
        public int Width => costs.GetLength(0);
        public int Height => costs.GetLength(1);
        public DirectionMap<double>? DirectionCostMap { get; set; }

        public Board(int w, int h)
        {
            costs = new double?[w, h];
            for (int i = 0; i < costs.GetLength(0); ++i)
            {
                for (int j = 0; j < costs.GetLength(1); ++j)
                {
                    costs[i, j] = 1;
                }
            }
        }

        public bool TryGetCost(int i, int j, out double value)
        {
            var cost = costs[i, j];
            if (cost.HasValue)
            {
                value = cost.Value;
                return true;
            }
            value = default;
            return false;
        }

        public void SetCost(int i, int j, double cost)
        {
            costs[i, j] = cost;
        }

        public void Disable(int i, int j)
        {
            costs[i, j] = null;
        }

        public bool Contains(int i, int j)
        {
            return i >= 0 && j >= 0 && i < Width && j < Height;
        }

        public IEnumerable<(Vector2Int, double)> GetEdgesFrom(Vector2Int node)
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
                    var cost = costs[next.x, next.y];
                    if (cost.HasValue)
                    {
                        if (DirectionCostMap != null)
                        {
                            cost += DirectionCostMap[it];
                        }
                        yield return (next, cost.Value);
                    }
                }
            }
        }

        public IEnumerable<(int endNode, double weight)> GetEdgesFrom(int begin)
        {
            var node = GetCellById(begin);
            foreach (var (endNode, weight) in GetEdgesFrom(node))
            {
                var id = GetCellId(endNode);
                yield return (id, weight);
            }
        }

        public Vector2Int GetCellById(int id)
        {
            return new Vector2Int(id % Width, id / Width);
        }
        public int GetCellId(Vector2Int cell)
        {
            return cell.x + cell.y * Width;
        }
        public DistanceMap<Vector2Int> ComputeDistancesFrom(in Vector2Int node, double maxDistance = double.PositiveInfinity)
        {
            return new DistanceMap<Vector2Int>(this, node, maxDistance);
        }

        public AStarSearch<Vector2Int> CreateAStarSearch(in Vector2Int start)
        {
            var minCost = double.PositiveInfinity;
            foreach (var it in costs)
            {
                if (it.HasValue)
                {
                    if (minCost > it.Value)
                    {
                        minCost = it.Value;
                    }
                }
            }
            minCost += DirectionCostMap?.Select(_ => _.Value).Min() ?? 0.0;
            return new AStarSearch<Vector2Int>(this, start, GetHeuristicFunctionForAStarSearch());
        }

        public System.Func<Vector2Int, Vector2Int, double> GetHeuristicFunctionForAStarSearch()
        {
            var minCost = double.PositiveInfinity;
            foreach (var it in costs)
            {
                if (it.HasValue)
                {
                    if (minCost > it.Value)
                    {
                        minCost = it.Value;
                    }
                }
            }
            minCost += DirectionCostMap?.Select(_ => _.Value).Min() ?? 0.0;

            UnityEngine.Assertions.Assert.IsTrue(minCost > 0.0);

            var lessEffectiveDigit = minCost * Width * Height * Unity.Mathematics.math.pow(2.0, -54.0);
            var cost = minCost - lessEffectiveDigit;
            if (cost < 0.0)
            {
                cost = 0.0;
            }

            return (a, b) => Vector2IntUtil.GetManhattanDistance(a, b) * cost;
        }
#if TSKT_MATH_BURST_SUPPORT
        public DistanceMap<Vector2Int> ComputeDistancesWithBurst(Vector2Int node, double maxDistance = double.PositiveInfinity)
        {
            var map = BoardProcessor.Calculate(GetCellId(node), Width * Height, this, maxDistance);

            var reversedEdges = map.ReversedEdges.ToDictionary(
                _ => GetCellById(_.Key),
                _ => new HashSet<Vector2Int>(_.Value.Select(x => GetCellById(x))));

            return new DistanceMap<Vector2Int>(
                GetCellById(map.Start),
                map.Distances.ToDictionary(_ => GetCellById(_.Key), _ => _.Value),
                reversedEdges);
        }
#endif
    }
}
