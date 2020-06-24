using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TSKT
{
    public interface IGraph<T>
    {
        IEnumerable<(T endNode, double weight)> GetEdgesFrom(T begin);
    }
}
