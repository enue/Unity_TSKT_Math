using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
#nullable enable

namespace TSKT
{
    public readonly struct AStarSearch<T>
    {
        public readonly IGraph<T> graph;
        readonly System.Func<T, T, float> heuristicFunction;
        public readonly DistanceMapCore<T> memo;
        public readonly T Start => memo.Start;
        readonly List<T> tasksToResume;

        public AStarSearch(IGraph<T> graph, in T start, System.Func<T, T, float> heuristicFunction)
        {
            this.heuristicFunction = heuristicFunction;
            this.graph = graph;

            memo = new DistanceMapCore<T>(
                start,
                new Dictionary<T, float>(),
                new Dictionary<T, List<T>>());
            memo.Distances.Add(start, 0f);
            tasksToResume = new List<T>() { start };
        }

        public readonly bool AnyPath(T goal, float maxDistance = float.PositiveInfinity)
        {
            return TrySolve(goal, searchAllPaths: false, maxDistance);
        }
        public readonly T[] SearchPath(T goal, float maxDistance = float.PositiveInfinity)
        {
            if (TrySolve(goal, searchAllPaths: false, maxDistance))
            {
                return memo.SearchPath(goal);
            }
            return System.Array.Empty<T>();
        }
        public readonly void SearchAllPaths(T goal, float maxDistance, Span<T[]> destination, out int writtenCount)
        {
            if (TrySolve(goal, searchAllPaths: true, maxDistance: maxDistance))
            {
                memo.SearchPaths(goal, destination, out writtenCount);
            }
            else
            {
                writtenCount = 0;
            }
        }

        public readonly bool SearchNearestNode(in ReadOnlySpan<T> goals, float maxDistance, out T result)
        {
            var sortedGoals = new Graphs.FloatPriorityQueue<T>();
            foreach (var it in goals)
            {
                var expectedDistance = heuristicFunction(Start, it);
                sortedGoals.Enqueue(expectedDistance, it);
            }

            result = default;
            float nearestGoalDistance = float.PositiveInfinity;
            bool found = false;
            while (sortedGoals.Count > 0)
            {
                var (expectedDistance, goal) = sortedGoals.DequeueKeyAndValue();
                if (expectedDistance >= nearestGoalDistance)
                {
                    break;
                }
                if (TrySolve(goal, searchAllPaths: false, maxDistance))
                {
                    var distance = memo.Distances[goal];
                    if (distance < nearestGoalDistance)
                    {
                        nearestGoalDistance = distance;
                        result = goal;
                        found = true;
                    }
                }
            }

            return found;
        }

        readonly bool TrySolve(T goal, bool searchAllPaths, float maxDistance)
        {
            var tasks = new Graphs.PriorityQueue<(T node, float expectedDistance)>();
            foreach (var it in tasksToResume)
            {
                memo.Distances.TryGetValue(it, out var startToItDistance);

                var h = heuristicFunction(it, goal);
                var expectedDistance = startToItDistance +h;
                tasks.Enqueue(expectedDistance, h, (it, expectedDistance));
            }
            tasksToResume.Clear();
            bool memoModified = true;

            while (tasks.Count > 0)
            {
                var (currentNode, expectedDistance) = tasks.Peek;
                if (memoModified)
                {
                    if (memo.Distances.TryGetValue(goal, out var startToGoalDistance))
                    {
                        if (searchAllPaths)
                        {
                            if (expectedDistance > startToGoalDistance)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (expectedDistance >= startToGoalDistance)
                            {
                                break;
                            }
                        }
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
                        if (!tasksToResume.Contains(currentNode))
                        {
                            tasksToResume.Add(currentNode);
                        }
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
                            if (!nearNodes.Contains(currentNode))
                            {
                                nearNodes.Add(currentNode);
                            }
                        }

                        if (oldDistance <= startToNextNodeDistance)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        var nearNodes = new List<T>() { currentNode };
                        memo.ReversedEdges.Add(next, nearNodes);
                    }

                    memo.Distances[next] = startToNextNodeDistance;
                    memoModified = true;

                    float h = heuristicFunction(next, goal);
                    float nextExpectedDistance = startToNextNodeDistance = h;

                    tasks.Enqueue(nextExpectedDistance, h, (next, nextExpectedDistance));
                }
            }
            while (tasks.Count > 0)
            {
                var n = tasks.Dequeue().node;
                if (!tasksToResume.Contains(n))
                {
                    tasksToResume.Add(n);
                }
            }

            return memo.Distances.ContainsKey(goal);
        }
   }
}
