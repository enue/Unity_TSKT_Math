#nullable enable
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace TSKT
{
    public class UnmanagedGraph<T> : Graph<T>, IUnmanagedGraph<T> where T : unmanaged, IEquatable<T>
    {
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
