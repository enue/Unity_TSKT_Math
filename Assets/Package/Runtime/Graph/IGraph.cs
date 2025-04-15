using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
#nullable enable

namespace TSKT
{
    public interface IGraph<T>
    {
        IEnumerable<(T endNode, double weight)> GetEdgesFrom(T begin);
    }
    public interface IGraphU<T> where T : unmanaged
    {
        int MaxEdgeCount { get; }
        void GetEdgesFrom(T begin, Span<(T endNode, double weight)> dest, out int writtenCount);
    }
}
