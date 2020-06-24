using System.Collections.Generic;
using System.Linq;

namespace TSKT
{
    public readonly struct DistanceMap<T>
    {
        public T Pivot { get; }
        public Dictionary<T, double> Distances { get; }
        public Dictionary<T, (T node, double distance)[]> Edges { get; }

        public DistanceMap(T pivot, Dictionary<T, double> distances, Dictionary<T, (T node, double distance)[]> edges)
        {
            Pivot = pivot;
            Distances = distances;
            Edges = edges;
        }

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
                    var pivotToCurrentNodeDistance = Distances[currentNode];
                    foreach (var (nextNode, edgeWeight) in nexts)
                    {
                        UnityEngine.Debug.Assert(edgeWeight > 0.0, "weight must be greater than 0.0");

                        var pivotToNextNodeDistance = pivotToCurrentNodeDistance + edgeWeight;
                        if (pivotToNextNodeDistance <= maxDistance)
                        {
                            if (Distances.TryGetValue(nextNode, out var oldDistance))
                            {
                                if (oldDistance > pivotToNextNodeDistance)
                                {
                                    tasks.Enqueue(nextNode);
                                    Distances[nextNode] = pivotToNextNodeDistance;
                                }
                            }
                            else
                            {
                                tasks.Enqueue(nextNode);
                                Distances.Add(nextNode, pivotToNextNodeDistance);
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
                foreach (var edge in Edges)
                {
                    if (edge.Value == null)
                    {
                        continue;
                    }
                    if (edge.Value.Length == 0)
                    {
                        continue;
                    }
                    var from = edge.Key;
                    if (!Distances.TryGetValue(from, out var fromDistance))
                    {
                        continue;
                    }
                    foreach (var (to, weight) in edge.Value)
                    {
                        if (!Distances.TryGetValue(to, out var toDistance))
                        {
                            continue;
                        }
                        if (fromDistance + weight != toDistance)
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
