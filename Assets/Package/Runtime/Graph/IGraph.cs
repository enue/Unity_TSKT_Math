using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
#nullable enable

namespace TSKT
{
    public interface IGraph<T>
    {
        IEnumerable<(T endNode, float weight)> GetEdgesFrom(T begin);
    }
    public interface IUnmanagedGraph<T> where T : unmanaged
    {
        int MaxEdgeCountFromOneNode { get; }
        void GetEdgesFrom(T begin, Span<(T endNode, float weight)> dest, out int writtenCount);
    }
}
