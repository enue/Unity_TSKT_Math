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
            var batchedGraph = new BatchedGraph<Vector2Int>(board,
                new Vector2Int(UnityEngine.Random.Range(0, board.Width), UnityEngine.Random.Range(0, board.Height)),
                25.0);

            foreach (var it in batchedGraph.nodeBatchMap)
            {
                CreateLine(it.Key, it.Value.root, Color.gray, 0.02f, 0f);
            }
            foreach (var it in batchedGraph.batchGraph.StartingNodes)
            {
                foreach (var end in batchedGraph.batchGraph.NextNodesFrom(it))
                {
                    CreateLine(it.root, end.Key.root, Color.red, 0.05f, -0.1f);
                }
            }

            var start = new Vector2Int(UnityEngine.Random.Range(0, board.Width), UnityEngine.Random.Range(0, board.Height));
            var goal = new Vector2Int(UnityEngine.Random.Range(0, board.Width), UnityEngine.Random.Range(0, board.Height));
            var path = batchedGraph.GetPath(start, goal).ToArray();
            for(int i=1; i<path.Length; ++i)
            {
                var begin = path[i - 1];
                var end = path[i];
                CreateLine(begin, end, Color.green, 0.1f, -0.2f);
            }
        }

        void CreateLine(Vector2Int start, Vector2Int end, Color color, float width, float z)
        {
            var obj = new GameObject();
            var line = obj.AddComponent<LineRenderer>();
            line.SetPositions(new[] { GetPosition(start, z), GetPosition(end, z) });
            line.material = lineMaterial;
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = color;
            line.endColor = color;

        }
        Vector3 GetPosition(Vector2Int cell, float z)
        {
            return new Vector3(cell.x * 0.1f, cell.y * 0.1f, z);
        }
    }
}
