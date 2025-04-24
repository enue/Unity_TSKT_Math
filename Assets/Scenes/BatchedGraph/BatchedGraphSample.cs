using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TSKT
{
    public class BatchedGraphSample : MonoBehaviour
    {
        [SerializeField]
        Material lineMaterial = default;

        void Start()
        {
            var board = new Board(100, 100);

            for (int i = 0; i < board.Width; ++i)
            {
                for (int j = 0; j < board.Height; ++j)
                {
                    var r = Mathf.PerlinNoise(i * 0.5f, j * 0.5f) * 8f;
                   // var r = UnityEngine.Random.Range(0f, 10f);
                    board.SetCost(i, j, r + 1f);
                }
            }

            var batchedGraph = new BatchedGraph<Vector2Int>(board,
                new Vector2Int(UnityEngine.Random.Range(0, board.Width), UnityEngine.Random.Range(0, board.Height)),
                25f, 50f,
                board.GetHeuristicFunctionForAStarSearch());

            foreach (var it in batchedGraph.nodeBatchMap)
            {
                board.TryGetCost(it.Key.x, it.Key.y, out var beginCost);
                var begin = (float)beginCost / 10f;
                board.TryGetCost(it.Value.Root.x, it.Value.Root.y, out var endCost);
                var end = (float)endCost / 10f;

                CreateLine(it.Key, it.Value.Root, new Color(begin, begin, begin), new Color(end, end, end), 0.01f, 0f);
            }
            foreach (var it in batchedGraph.batchGraph.StartingNodes)
            {
                if (batchedGraph.batchGraph.TryGetNextNodesFrom(it, out var ends))
                {
                    foreach (var end in ends)
                    {
                        CreateLine(it.Root, end.Key.Root, Color.red, Color.red, 0.005f, -0.1f);
                    }
                }
            }

            var start = new Vector2Int(UnityEngine.Random.Range(0, board.Width), UnityEngine.Random.Range(0, board.Height));
            var goal = new Vector2Int(UnityEngine.Random.Range(0, board.Width), UnityEngine.Random.Range(0, board.Height));
            var path = batchedGraph.From(start).To(goal);

            CreatePath(path, new Color(0f, 1f, 0f, 0.5f), 0.02f);

            var aStarPath = board.CreateAStarSearch(start).SearchPath(goal);
            CreatePath(aStarPath, new Color(1f, 0f, 1f, 0.5f), 0.03f);

            double totalWeight = 0.0;
            for (int i = 1; i < path.Length; ++i)
            {
                var startNode = path[i - 1];
                var endNode = path[i];
                totalWeight += batchedGraph.graph.GetEdgesFrom(startNode).First(_ => _.endNode == endNode).weight;
            }
            double aStarTotalWeight = 0.0;
            for (int i = 1; i < aStarPath.Length; ++i)
            {
                var startNode = aStarPath[i - 1];
                var endNode = aStarPath[i];
                aStarTotalWeight += batchedGraph.graph.GetEdgesFrom(startNode).First(_ => _.endNode == endNode).weight;
            }
            Debug.Log("distance : " + totalWeight + " / " + aStarTotalWeight + " ( " + (totalWeight / aStarTotalWeight) + ")");
        }

        void CreatePath(Vector2Int[] path, Color color, float width)
        {
            for (int i = 1; i < path.Length; ++i)
            {
                var begin = path[i - 1];
                var end = path[i];
                CreateLine(begin, end, color, color, width, -0.2f);
            }

        }

        void CreateLine(Vector2Int start, Vector2Int end, Color startColor, Color endColor, float width, float z)
        {
            var obj = new GameObject();
            var line = obj.AddComponent<LineRenderer>();
            line.SetPositions(new[] { GetPosition(start, z), GetPosition(end, z) });
            line.material = lineMaterial;
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = startColor;
            line.endColor = endColor;

        }
        Vector3 GetPosition(Vector2Int cell, float z)
        {
            return new Vector3(cell.x * 0.1f, cell.y * 0.1f, z);
        }
    }
}
