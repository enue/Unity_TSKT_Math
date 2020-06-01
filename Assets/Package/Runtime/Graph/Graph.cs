﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TSKT
{
    public class Graph<T> : IGraph<T>
    {
        readonly Dictionary<T, Dictionary<T, double>> edges = new Dictionary<T, Dictionary<T, double>>();

        public void Clear()
        {
            edges.Clear();
        }

        public bool CreateNode(T node)
        {
            if (!edges.ContainsKey(node))
            {
                edges.Add(node, new Dictionary<T, double>());
                return true;
            }
            return false;
        }

        public void Link(T first, T second, double weight)
        {
            CreateNode(first);
            edges[first][second] = weight;
        }

        public void DoubleOrderedLink(T first, T second, double weight)
        {
            Link(first, second, weight);
            Link(second, first, weight);
        }

        public bool Unlink(T first, T second)
        {
            if (edges.TryGetValue(first, out var nexts))
            {
                return nexts.Remove(second);
            }
            return false;
        }

        public void DoubleOrderedUnlink(T first, T second)
        {
            Unlink(first, second);
            Unlink(second, first);
        }

        public void Remove(T node)
        {
            if (edges.ContainsKey(node))
            {
                edges.Remove(node);
            }

            foreach (var it in edges.Values)
            {
                if (it.ContainsKey(node))
                {
                    it.Remove(node);
                }
            }
        }

        public Dictionary<T, double> NextNodesFrom(T node)
        {
            if (edges.TryGetValue(node, out var nodes))
            {
                return nodes;
            }
            else
            {
                return null;
            }
        }

        public Dictionary<T, Dictionary<T, double>>.KeyCollection StartingNodes => edges.Keys;

        public HashSet<T> ComputeAllNodes()
        {
            var result = new HashSet<T>();
            foreach (var it in edges)
            {
                result.Add(it.Key);
                result.UnionWith(it.Value.Keys);
            }
            return result;
        }

        public IEnumerable<(T, double)> GetNextNodeDistancesFrom(T node)
        {
            var nodes = NextNodesFrom(node);
            if (nodes != null)
            {
                foreach (var it in nodes)
                {
                    yield return (it.Key, it.Value);
                }
            }
        }

        public DistanceMap<T> ComputeDistancesFrom(T node, double maxDistance = double.PositiveInfinity)
        {
            return new DistanceMap<T>(this, node, maxDistance);
        }
    }
}