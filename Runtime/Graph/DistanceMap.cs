using System.Collections.Generic;
using System.Linq;

namespace TSKT
{
    public readonly struct DistanceMap<T>
    {
        public T Pivot { get; }
        public Dictionary<T, double> Distances { get; }
        public Dictionary<T, (T node, double distance)[]> Edges { get; }

        public DistanceMap(IGraph<T> graph, T pivot, double maxDistance = double.PositiveInfinity)
        {
            Pivot = pivot;
            Distances = new Dictionary<T, double>();
            Edges = new Dictionary<T, (T node, double distance)[]>();

            var tasks = new Queue<T>();
            tasks.Enqueue(pivot);

            Distances.Add(pivot, 0.0);

            while (tasks.Count > 0)
            {
                var currentNode = tasks.Dequeue();

                if (!Edges.TryGetValue(currentNode, out var nexts))
                {
                    nexts = graph.GetNextNodeDistancesFrom(currentNode)?.ToArray();
                    Edges.Add(currentNode, nexts);
                }

                if (nexts != null && nexts.Length > 0)
                {
                    var currentNodePosition = Distances[currentNode];
                    foreach (var (nextNode, distance) in nexts)
                    {
                        UnityEngine.Assertions.Assert.IsTrue(distance > 0.0, "weight must be greater than 0.0");

                        var newWeight = currentNodePosition + distance;
                        if (newWeight <= maxDistance)
                        {
                            if (Distances.TryGetValue(nextNode, out var oldWeight))
                            {
                                if (oldWeight > newWeight)
                                {
                                    tasks.Enqueue(nextNode);
                                    Distances[nextNode] = newWeight;
                                }
                            }
                            else
                            {
                                tasks.Enqueue(nextNode);
                                Distances.Add(nextNode, newWeight);
                            }
                        }
                    }
                }
            }
        }

        public Dictionary<T, List<T>> EdgesToPivot
        {
            get
            {
                var result = new Dictionary<T, List<T>>();
                foreach (var it in Edges)
                {
                    if (it.Value != null && it.Value.Length > 0)
                    {
                        var from = it.Key;
                        var fromDistance = Distances[from];
                        foreach (var (to, _) in it.Value)
                        {
                            if (fromDistance > Distances[to])
                            {
                                continue;
                            }

                            if (!result.TryGetValue(to, out var froms))
                            {
                                froms = new List<T>();
                                result.Add(to, froms);
                            }
                            froms.Add(from);
                        }
                    }
                }
                return result;
            }
        }


        public IEnumerable<List<T>> ComputeRoutesToPivotFrom(T from)
        {
            if (!Distances.ContainsKey(from))
            {
                yield break;
            }

            var edgesToPivot = EdgesToPivot;
            var tasks = new Stack<List<T>>();
            tasks.Push(new List<T>() { from });

            while(tasks.Count > 0)
            {
                var route = tasks.Pop();

                if (!edgesToPivot.TryGetValue(route[route.Count - 1], out var nextPoints))
                {
                    yield return route;
                    continue;
                }
                if (nextPoints.Count > 1)
                {
                    for(int i=1; i<nextPoints.Count; ++i)
                    {
                        var newTask = new List<T>(route)
                        {
                            nextPoints[i]
                        };
                        tasks.Push(newTask);
                    }
                }
                route.Add(nextPoints[0]);
                tasks.Push(route);
            }
        }
    }
}
