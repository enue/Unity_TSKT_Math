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
        }

        public void Clear()
        {
            edges.Clear();
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

        public Dictionary<T, float> NextNodesFrom(T node)
        {
            if (TryGetNextNodesFrom(node, out var result))
            {
                return result;
            }
            return new Dictionary<T, float>();
        }

        public bool TryGetNextNodesFrom(T node, out Dictionary<T, float> result)
        {
            if (edges.TryGetValue(node, out result))
            {
                return true;
            }
            else
            {
                return false;
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
            if (TryGetNextNodesFrom(node, out var nodes))
            {
                foreach (var it in nodes)
                {
                    yield return (it.Key, it.Value);
                }
            }
        }

        public double GetWeight(T start, T end)
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

    public class UnmanagedGraph<T> : Graph<T>, IUnmanagedGraph<T> where T : unmanaged, IEquatable<T>
    {
        public int MaxEdgeCountFromOneNode => Edges.Max(_ => _.Value.Count);

        public void GetEdgesFrom(T begin, Span<(T endNode, float weight)> dest, out int writtenCount)
        {
            writtenCount = 0;
            if (Edges.TryGetValue(begin, out var edges))
            {
                foreach (var it in edges)
                {
                    dest[writtenCount] = (it.Key, it.Value);
                    ++writtenCount;
                }
            }
        }
        public UnmanagedDistanceMap<T> CreateUnmanagedDistanceMapFrom(T node) => new(this, node);
        public UnmanagedAStarSearch<T> CreateUnmanagedAStarSearch(T start, Func<T, T, float> heuristicFunction)
        {
            return new UnmanagedAStarSearch<T>(this, start, heuristicFunction);
        }
    }
}
