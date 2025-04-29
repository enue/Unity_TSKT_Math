using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
#nullable enable

namespace TSKT
{
    public class Graph<T> : IGraph<T>
    {
        readonly Dictionary<T, Dictionary<T, float>> edges = new();
        public IReadOnlyDictionary<T, Dictionary<T, float>> Edges => edges;
        public Dictionary<T, Dictionary<T, float>>.KeyCollection StartingNodes => edges.Keys;
        public int MaxEdgeCountFromOneNode { get; private set; } = 0;

        public Graph()
        {
        }

        public Graph(Graph<T> source)
        {
            foreach(var it in source.edges)
            {
                var endNodes = new Dictionary<T, float>(it.Value);
                edges.Add(it.Key, endNodes);
            }
            MaxEdgeCountFromOneNode = source.MaxEdgeCountFromOneNode;
        }

        public void Clear()
        {
            edges.Clear();
            MaxEdgeCountFromOneNode = 0;
        }

        public bool CreateNode(T node)
        {
            if (!edges.ContainsKey(node))
            {
                edges.Add(node, new Dictionary<T, float>());
                return true;
            }
            return false;
        }

        void CreateNode(T node, out Dictionary<T, float> endNodes)
        {
            if (!edges.TryGetValue(node, out endNodes))
            {
                endNodes = new Dictionary<T, float>();
                edges.Add(node, endNodes);
            }
        }

        public void Link(T first, T second, float weight)
        {
            CreateNode(first, out var edge);
            edge[second] = weight;
            MaxEdgeCountFromOneNode = Mathf.Max(MaxEdgeCountFromOneNode, edge.Count);
        }

        public void DoubleOrderedLink(T first, T second, float weight)
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

        public IEnumerable<(T endNode, float weight)> GetEdgesFrom(T node)
        {
            if (edges.TryGetValue(node, out var nodes))
            {
                foreach (var it in nodes)
                {
                    yield return (it.Key, it.Value);
                }
            }
        }

        public float GetWeight(T start, T end)
        {
            return edges[start][end];
        }

        public bool TryGetWeight(T start, T end, out float result)
        {
            if (edges.TryGetValue(start, out var edge))
            {
                return edge.TryGetValue(end, out result);
            }
            result = default;
            return false;
        }
        public DistanceMap<T> CreateDistanceMapFrom(T node) => new(this, node);
        public AStarSearch<T> CreateAStarSearch(T start, Func<T, T, float> heuristicFunction)
        {
            return new AStarSearch<T>(this, start, heuristicFunction);
        }
    }
}
