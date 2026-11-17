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
        readonly HashSet<T> tasksToResume;

        public AStarSearch(IGraph<T> graph, in T start, System.Func<T, T, float> heuristicFunction)
        {
            this.heuristicFunction = heuristicFunction;
            this.graph = graph;

            memo = new DistanceMapCore<T>(
                start,
                new Dictionary<T, float>(),
                new Dictionary<T, List<T>>());
            memo.Distances.Add(start, 0f);
            tasksToResume = new HashSet<T>() { start };
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
            var tasks = new Graphs.PriorityQueue<(T node, float expectedDistance, float startToNodeDistance)>();
            foreach (var it in tasksToResume)
            {
                memo.Distances.TryGetValue(it, out var startToItDistance);

                var h = heuristicFunction(it, goal);
                var expectedDistance = startToItDistance + h;
                tasks.Enqueue(expectedDistance, h, (it, expectedDistance, startToItDistance));
            }
            tasksToResume.Clear();
            bool memoModified = true;

            while (tasks.Count > 0)
            {
                var (currentNode, expectedDistance, enqueuedDistance) = tasks.Peek;
                var startToCurrentNodeDistance = memo.Distances[currentNode];
                // より短い距離が見つかって積み直されたノードの古い要素。短い方の要素が別にあるので捨てる
                if (enqueuedDistance > startToCurrentNodeDistance)
                {
                    tasks.Dequeue();
                    continue;
                }
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
                        memo.ReversedEdges.Add(next, new List<T>() { currentNode });
                    }

                    memo.Distances[next] = startToNextNodeDistance;
                    memoModified = true;

                    float h = heuristicFunction(next, goal);
                    float nextExpectedDistance = startToNextNodeDistance + h;

                    tasks.Enqueue(nextExpectedDistance, h, (next, nextExpectedDistance, startToNextNodeDistance));
                }
            }
            while (tasks.Count > 0)
            {
                var n = tasks.Dequeue().node;
                tasksToResume.Add(n);
            }

            // memoには以前の探索で見つけたmaxDistanceを超える距離も入っているので、距離で判定する
            return memo.Distances.TryGetValue(goal, out var goalDistance)
                && goalDistance <= maxDistance;
        }
   }
}
