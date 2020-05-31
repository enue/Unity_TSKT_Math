using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TSKT
{
    public interface IGraph<T>
    {
        IEnumerable<(T nextNode, double distance)> GetNextNodeDistancesFrom(T node);
    }
}
