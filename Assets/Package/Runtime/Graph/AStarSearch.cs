using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT
{
    public readonly struct AStarSearch<T>
    {
        readonly public IGraph<T> graph;
        readonly System.Func<T, T, double> heuristicFunction;
        public readonly DistanceMap<T> memo;
        public T Start => memo.Start;
        
        public AStarSearch(IGraph<T> graph, T start, System.Func<T, T, double> heuristicFunction, DistanceMap<T> memo = default)
        {
            this.heuristicFunction = heuristicFunction;
            this.graph = graph;

            if (memo.Distances == null)
            {
                this.memo = new DistanceMap<T>(
                    start,
                    new Dictionary<T, double>(),
                    new Dictionary<T, HashSet<T>>());
                this.memo.Distances.Add(start, 0.0);
            }
            else
            {
                this.memo = memo;
            }
        }

        public T[] SearchPath(params T[] goals)
        {
            return SearchPath(double.PositiveInfinity, goals);
        }

        public T[] SearchPath(double maxDistance, params T[] goals)
        {
            var containsAllGoals = true;
            var minDistance = double.PositiveInfinity;
            T nearestGoal = default;
            foreach (var goal in goals)
            {
                if (memo.Distances.TryGetValue(goal, out var distance))
                {
                    if (minDistance > distance)
                    {
                        minDistance = distance;
                        nearestGoal = goal;
                    }
                }
                else
                {
                    containsAllGoals = false;
                    break;
                }
            }
            if (containsAllGoals)
            {
                return memo.SearchPaths(nearestGoal).First();
            }
            return SearchPaths(goals, searchAllPaths: false, maxDistance: maxDistance).FirstOrDefault();
        }

        public IEnumerable<T[]> SearchAllPaths(params T[] goals)
        {
            return SearchAllPaths(double.PositiveInfinity, goals);
        }

        public IEnumerable<T[]> SearchAllPaths(double maxDistance, params T[] goals)
        {
            return SearchPaths(goals, searchAllPaths: true, maxDistance: maxDistance);
        }

        IEnumerable<T[]> SearchPaths(T[] goals, bool searchAllPaths, double maxDistance)
        {
            T[] sortedGoals;
            if (goals.Length < 2)
            {
                sortedGoals = goals;
            }
            else
            {
                var h = heuristicFunction;
                var start = Start;
                sortedGoals = goals.OrderBy(_ => h(start, _)).ToArray();
            }

            var cloneReversedEdges = new Dictionary<T, HashSet<T>>(memo.ReversedEdges.Count);
            foreach (var it in memo.ReversedEdges)
            {
                cloneReversedEdges.Add(it.Key, new HashSet<T>(it.Value));
            }

            var distanceMap = new DistanceMap<T>(
                Start,
                new Dictionary<T, double>(memo.Distances),
                cloneReversedEdges);

            var tasks = new Graphs.DoublePriorityQueue<(T node, double expectedDistance)>();

            foreach (var it in distanceMap.Distances)
            {
                var startToItDistance = it.Value;
                var h = heuristicFunction;
                var expectedDistance = startToItDistance + goals.Min(_ => h(it.Key, _));
                tasks.Enqueue(expectedDistance, -startToItDistance, (it.Key, expectedDistance));
            }

            var farestNodeSearched = 0.0;

            while (tasks.Count > 0)
            {
                var (currentNode, expectedDistance) = tasks.Dequeue();
                {
                    bool shouldBreak = false;
                    foreach (var it in sortedGoals)
                    {
                        if (distanceMap.Distances.TryGetValue(it, out var startToGoalDistance))
                        {
                            if (searchAllPaths)
                            {
                                if (expectedDistance > startToGoalDistance)
                                {
                                    shouldBreak = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (expectedDistance >= startToGoalDistance)
                                {
                                    shouldBreak = true;
                                    break;
                                }
                            }
                        }
                        if (farestNodeSearched < heuristicFunction(Start, it))
                        {
                            break;
                        }
                    }
                    if (shouldBreak)
                    {
                        break;
                    }
                }

                var startToCurrentNodeDistance = distanceMap.Distances[currentNode];

                foreach (var (next, edgeWeight) in graph.GetEdgesFrom(currentNode))
                {
                    var startToNextNodeDistance = edgeWeight + startToCurrentNodeDistance;
                    if (startToNextNodeDistance <= maxDistance)
                    {
                        if (distanceMap.Distances.TryGetValue(next, out var oldDistance))
                        {
                            if (oldDistance >= startToNextNodeDistance)
                            {
                                var nearNodes = distanceMap.ReversedEdges[next];
                                if (oldDistance > startToNextNodeDistance)
                                {
                                    nearNodes.Clear();
                                }
                                nearNodes.Add(currentNode);
                            }

                            if (oldDistance <= startToNextNodeDistance)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            var nearNodes = new HashSet<T>();
                            distanceMap.ReversedEdges.Add(next, nearNodes);
                            nearNodes.Add(currentNode);
                        }

                        distanceMap.Distances[next] = startToNextNodeDistance;

                        var h = heuristicFunction;
                        var start = Start;
                        var k = h(next, sortedGoals[0]);
                        var nextExpectedDistance = sortedGoals
                            .TakeWhile(_ => h(start, _) <= k + startToNextNodeDistance)
                            .Min(_ => h(next, _)) + startToNextNodeDistance;

                        // nextExpectedDistanceは昇順、startToNextNodeDistanceは降順で処理する
                        tasks.Enqueue(nextExpectedDistance, -startToNextNodeDistance, (next, nextExpectedDistance));

                        if (farestNodeSearched < startToNextNodeDistance)
                        {
                            farestNodeSearched = startToNextNodeDistance;
                        }
                    }
                }
            }

            var nearestGoalDistance = double.PositiveInfinity;
            foreach(var it in sortedGoals)
            {
                if (heuristicFunction(Start, it) > nearestGoalDistance)
                {
                    break;
                }
                if (distanceMap.Distances.TryGetValue(it, out var d))
                {
                    if (nearestGoalDistance > d)
                    {
                        nearestGoalDistance = d;
                    }
                }
            }

            foreach (var goal in sortedGoals)
            {
                if (heuristicFunction(Start, goal) > nearestGoalDistance)
                {
                    break;
                }
                if (!distanceMap.Distances.TryGetValue(goal, out var distance))
                {
                    continue;
                }
                if (distance != nearestGoalDistance)
                {
                    continue;
                }
                foreach (var path in distanceMap.SearchPaths(goal))
                {
                    // goalまでの経路は最適なので蓄積しておく
                    foreach (var it in path)
                    {
                        var value = distanceMap.Distances[it];
                        UnityEngine.Assertions.Assert.IsTrue(!memo.Distances.TryGetValue(it, out var oldValue) || oldValue == value);
                        memo.Distances[it] = value;
                    }

                    for (int i = 1; i < path.Length; ++i)
                    {
                        var nearNode = path[i - 1];
                        var farNode = path[i];
                        UnityEngine.Assertions.Assert.IsTrue(memo.Distances[nearNode] <= memo.Distances[farNode]);

                        if (memo.ReversedEdges.TryGetValue(farNode, out var nearNodes))
                        {
                            nearNodes.Add(nearNode);
                        }
                        else
                        {
                            nearNodes = new HashSet<T>();
                            memo.ReversedEdges.Add(farNode, nearNodes);
                            nearNodes.Add(nearNode);
                        }
                    }

                    yield return path;
                }
            }
        }

        public Dictionary<T, double> GetDistances(params T[] nodes)
        {
            var distances = memo.Distances;
            return nodes.ToDictionary(_ => _, _ => distances[_]);
        }

        static public T[] SearchPath(IGraph<T> graph, T start, T goal, System.Func<T, T, double> heuristicFunction)
        {
            var search = new AStarSearch<T>(graph, start, heuristicFunction, default);
            return search.SearchPath(goal);
        }
        static public IEnumerable<T[]> SearchAllPaths(IGraph<T> graph, T start, T goal, System.Func<T, T, double> heuristicFunction)
        {
            var search = new AStarSearch<T>(graph, start, heuristicFunction, default);
            return search.SearchAllPaths(goal);
        }
    }
}
