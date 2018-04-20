using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    public class LightObstructor : MonoBehaviour
    {
        [HideInInspector] public Mesh mesh;

        public void Awake()
        {
            mesh = GetComponent<MeshFilter>().mesh;
            if (mesh == null) throw new UnnassignedMeshException();
            FastLight.RegisterObstacle(this);
        }

        private void OnDestroy()
        {
            FastLight.UnregisterObstacle(this);
        }

        public List<List<Vector2>> GetVisibleVertices(Vector3 origin, float radius)
        {
            HashSet<int> visibleVerticesIndexes = new HashSet<int>();
            List<Vector3> worldVertices = mesh.vertices.Where(v => Vector2.Distance(v, origin) <= radius).ToList();
            worldVertices = worldVertices.Select(meshVertex => transform.TransformPoint(meshVertex)).ToList();
            for (int i = 0; i < worldVertices.Count; ++i)
            {
                Vector3 from = worldVertices[i];
                int toIndex = i + 1 == worldVertices.Count ? 0 : i + 1;
                Vector3 to = worldVertices[toIndex];
                Vector3 normal = from - to;
                normal = Quaternion.AngleAxis(90, Vector3.forward) * normal;
                Vector3 midpoint = (from + to) / 2f;
                float dot = Vector3.Dot(normal, origin - midpoint);
                if (dot <= 0) continue;
                visibleVerticesIndexes.Add(i);
                visibleVerticesIndexes.Add(toIndex);
            }

            List<int> sortedVertices = visibleVerticesIndexes.ToList();
            sortedVertices.Sort((a, b) => a.CompareTo(b));

            List<List<int>> edges = new List<List<int>>();
            List<int> currentList = null;
            int prevValue = -10;
            bool beganAtZero = false;
            bool endedAtEndOfList = false;

            foreach (int vertIndex in sortedVertices)
            {
                if (vertIndex == worldVertices.Count - 1) endedAtEndOfList = true;
                if (currentList == null || vertIndex != prevValue + 1)
                {
                    currentList = new List<int>();
                    edges.Add(currentList);
                    currentList.Add(vertIndex);
                    if (vertIndex == 0) beganAtZero = true;
                }
                else if (vertIndex == prevValue + 1)
                {
                    currentList.Add(vertIndex);
                }

                prevValue = vertIndex;
            }

            if (beganAtZero && endedAtEndOfList && edges.Count > 1)
            {
                List<int> startList = edges[0];
                List<int> endList = edges[edges.Count - 1];
                edges.RemoveAt(0);
                edges.RemoveAt(edges.Count - 1);
                endList.AddRange(startList);
                edges.Add(endList);
            }

            List<List<Vector2>> finalEdges = new List<List<Vector2>>();
            foreach (List<int> edge in edges)
            {
                List<Vector2> edgeList = new List<Vector2>();
                finalEdges.Add(edgeList);
                foreach (int vertIndex in edge)
                {
                    edgeList.Add(worldVertices[vertIndex]);
                }
            }

            return finalEdges;
        }
    }

    public class UnnassignedMeshException : Exception
    {
        public string Message() => "No mesh assigned, a mesh is required for FastLight to work";
    }
}