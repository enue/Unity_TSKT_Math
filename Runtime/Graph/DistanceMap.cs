using System.Collections.Generic;

namespace TSKT
{
    public readonly struct DistanceMap<T>
    {
        public T Pivot { get; }
        public Dictionary<T, double> Distances { get; }
        public Dictionary<T, List<T>> EdgesToPivot { get; }

        public DistanceMap(IGraph<T> graph, T pivot, double maxDistance = double.PositiveInfinity)
        {
            Pivot = pivot;
            Distances = new Dictionary<T, double>();
            EdgesToPivot = new Dictionary<T, List<T>>();

            var tasks = new Queue<T>();
            tasks.Enqueue(pivot);

            Distances.Add(pivot, 0.0);

            while (tasks.Count > 0)
            {
                var currentNode = tasks.Dequeue();

                var nexts = graph.GetNextNodeDistancesFrom(currentNode);
                if (nexts != null)
                {
                    foreach (var (nextNode, distance) in nexts)
                    {
                        var newWeight = Distances[currentNode] + distance;
                        if (newWeight <= maxDistance)
                        {
                            if (Distances.TryGetValue(nextNode, out var oldWeight))
                            {
                                if (oldWeight > newWeight)
                                {
                                    tasks.Enqueue(nextNode);
                                    Distances[nextNode] = newWeight;

                                    var route = EdgesToPivot[nextNode];
                                    route.Clear();
                                    route.Add(currentNode);
                                }
                                else if (oldWeight == newWeight)
                                {
                                    if (!EdgesToPivot.TryGetValue(nextNode, out var nodes))
                                    {
                                        nodes = new List<T>();
                                        EdgesToPivot.Add(nextNode, nodes);
                                    }
                                    nodes.Add(currentNode);
                                }
                            }
                            else
                            {
                                tasks.Enqueue(nextNode);
                                Distances.Add(nextNode, newWeight);

                                EdgesToPivot.Add(nextNode, new List<T>() { currentNode });
                            }
                        }
                    }
                }
            }
        }

        public List<List<T>> ComputeRoutesToPivotFrom(T from)
        {
            var result = new List<List<T>>();

            if (!EdgesToPivot.ContainsKey(from))
            {
                return result;
            }
            var tasks = new Queue<List<T>>();
            tasks.Enqueue(new List<T>() { from });

            while(tasks.Count > 0)
            {
                var route = tasks.Dequeue();

                if (!EdgesToPivot.TryGetValue(route[route.Count - 1], out var nextPoints))
                {
                    result.Add(route);
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
                        tasks.Enqueue(newTask);
                    }
                }
                route.Add(nextPoints[0]);
                tasks.Enqueue(route);
            }
            return result;
        }
    }
}
