using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#nullable enable

namespace TSKT
{
    public readonly struct AStarSearch<T>
    {
        public readonly IGraph<T> graph;
        readonly System.Func<T, T, double> heuristicFunction;
        public readonly DistanceMap<T> memo;
        public readonly T Start => memo.Start;
        readonly HashSet<T> tasksToResume;

        public AStarSearch(IGraph<T> graph, in T start, System.Func<T, T, double> heuristicFunction)
        {
            this.heuristicFunction = heuristicFunction;
            this.graph = graph;

            memo = new DistanceMap<T>(
                start,
                new Dictionary<T, double>(),
                new Dictionary<T, HashSet<T>>());
            memo.Distances.Add(start, 0.0);
            tasksToResume = new HashSet<T>();
            tasksToResume.Add(start);
        }

        public readonly T[] SearchPath(T goal, double maxDistance = double.PositiveInfinity)
        {
            return SearchPathToNearestGoal(maxDistance, goal);
        }

        public readonly T[] SearchPathToNearestGoal(params T[] goals)
        {
            return SearchPathToNearestGoal(double.PositiveInfinity, goals);
        }

        public readonly T[] SearchPathToNearestGoal(double maxDistance, params T[] goals)
        {
            var containsAllGoals = true;
            var minDistance = double.PositiveInfinity;
            T? nearestGoal = default;
            foreach (var goal in goals)
            {
                if (tasksToResume.Contains(goal))
                {
                    containsAllGoals = false;
                    break;
                }
                else if (memo.Distances.TryGetValue(goal, out var distance))
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
                return memo.SearchPath(nearestGoal!);
            }
            if (SolvePath(goals, searchAllPaths: false, maxDistance: maxDistance, out nearestGoal))
            {
                return memo.SearchPath(nearestGoal);
            }
            return System.Array.Empty<T>();
        }
        public readonly IEnumerable<T[]> SearchAllPaths(T goal, double maxDistance = double.PositiveInfinity)
        {
            return SearchAllPathsToNearestGoal(maxDistance, goal);
        }

        public readonly IEnumerable<T[]> SearchAllPathsToNearestGoal(params T[] goals)
        {
            return SearchAllPathsToNearestGoal(double.PositiveInfinity, goals);
        }

        public readonly IEnumerable<T[]> SearchAllPathsToNearestGoal(double maxDistance, params T[] goals)
        {
            if (SolvePath(goals, searchAllPaths: true, maxDistance: maxDistance, out var nearestGoal))
            {
                return memo.SearchPaths(nearestGoal);
            }
            else
            {
                return System.Array.Empty<T[]>();
            }
        }

        readonly bool SolvePath(T[] goals, bool searchAllPaths, double maxDistance, out T nearestGoal)
        {
            var tasks = new Graphs.DoublePriorityQueue<(T node, double expectedDistance)>();
            foreach (var it in tasksToResume)
            {
                memo.Distances.TryGetValue(it, out var startToItDistance);
                var h = heuristicFunction;
                var expectedDistance = startToItDistance + goals.Min(_ => h(it, _));
                tasks.Enqueue(expectedDistance, -startToItDistance, (it, expectedDistance));
            }
            tasksToResume.Clear();
            bool memoModified = true;

            while (tasks.Count > 0)
            {
                var (currentNode, expectedDistance) = tasks.Peek;
                if (memoModified)
                {
                    bool shouldBreak = false;
                    foreach (var it in goals)
                    {
                        if (memo.Distances.TryGetValue(it, out var startToGoalDistance))
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
                    }
                    if (shouldBreak)
                    {
                        break;
                    }
                    memoModified = false;
                }
                tasks.Dequeue();
                var startToCurrentNodeDistance = memo.Distances[currentNode];

                foreach (var (next, edgeWeight) in graph.GetEdgesFrom(currentNode))
                {
                    var startToNextNodeDistance = edgeWeight + startToCurrentNodeDistance;
                    if (startToNextNodeDistance > maxDistance)
                    {
                        tasksToResume.Add(currentNode);
                        continue;
                    }

                    if (memo.Distances.TryGetValue(next, out var oldDistance))
                    {
                        if (oldDistance >= startToNextNodeDistance)
                        {
                            var nearNodes = memo.ReversedEdges[next];
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
                        memo.ReversedEdges.Add(next, nearNodes);
                        nearNodes.Add(currentNode);
                    }

                    memo.Distances[next] = startToNextNodeDistance;
                    memoModified = true;

                    var h = heuristicFunction;
                    var start = Start;
                    var nextExpectedDistance = goals.Min(_ => h(next, _)) + startToNextNodeDistance;

                    // nextExpectedDistanceは昇順、startToNextNodeDistanceは降順で処理する
                    tasks.Enqueue(nextExpectedDistance, -startToNextNodeDistance, (next, nextExpectedDistance));
                }
            }
            while (tasks.Count > 0)
            {
                tasksToResume.Add(tasks.Dequeue().node);
            }

            var nearestGoalDistance = double.PositiveInfinity;
            nearestGoal = default!;
            bool pathExist = false;
            foreach (var it in goals)
            {
                if (memo.Distances.TryGetValue(it, out var d))
                {
                    if (nearestGoalDistance > d)
                    {
                        nearestGoalDistance = d;
                        nearestGoal = it;
                        pathExist = true;
                    }
                }
            }

            return pathExist;
        }

        public readonly Dictionary<T, double> GetDistances(params T[] nodes)
        {
            var distances = memo.Distances;
            return nodes.ToDictionary(_ => _, _ => distances[_]);
        }

        public static T[] SearchPath(IGraph<T> graph, in T start, in T goal, System.Func<T, T, double> heuristicFunction)
        {
            var search = new AStarSearch<T>(graph, start, heuristicFunction);
            return search.SearchPath(goal);
        }
        public static IEnumerable<T[]> SearchAllPaths(IGraph<T> graph, in T start, in T goal, System.Func<T, T, double> heuristicFunction)
        {
            var search = new AStarSearch<T>(graph, start, heuristicFunction);
            return search.SearchAllPaths(goal);
        }
    }
}
