using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#nullable enable

namespace TSKT
{
    public interface IGraph<T>
    {
        IEnumerable<(T endNode, double weight)> GetEdgesFrom(T begin);
    }
}
