using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TSKT
{
    public class Board : IGraph<Vector2Int>
    {
        readonly double?[,] costs;
        public UnlimitedArray2<double> DirectionCostMap { get; set; }

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
                    if (costs[next.x, next.y].HasValue)
                    {
                        var cost = costs[next.x, next.y].Value;
                        if (DirectionCostMap != null)
                        {
                            cost += DirectionCostMap[it.x, it.y];
                        }
                        yield return (next, cost);
                    }
                }
            }
        }

        public int Width => costs.GetLength(0);
        public int Height => costs.GetLength(1);

        public DistanceMap<Vector2Int> ComputeDistancesFrom(Vector2Int node, double maxDistance = double.PositiveInfinity)
        {
            return new DistanceMap<Vector2Int>(this, node, maxDistance);
        }
    }
}
