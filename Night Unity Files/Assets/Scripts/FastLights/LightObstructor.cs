using System;
using System.Collections.Generic;
using System.Linq;
using Fastlights;
using SamsHelper.Libraries;
using UnityEngine;

namespace FastLights
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

        private List<FLVertex> GetVerticesInRange(Vector3 origin, float radius)
        {
            List<FLVertex> verts = new List<FLVertex>();
            foreach (Vector3 v in mesh.vertices)
            {
                Vector3 worldVertex = transform.TransformPoint(v);
                float distance = Vector2.Distance(worldVertex, origin);
                if (distance > radius) continue;
                float angle = 360 - AdvancedMaths.AngleFromUp(origin, worldVertex);
                FLVertex flVertex = new FLVertex(worldVertex, distance, angle);
                verts.Add(flVertex);
            }

            return verts;
        }

        private List<int> GetVisibleVertexIndices(List<FLVertex> vertices, Vector3 origin)
        {
            HashSet<int> visibleVerticesIndexes = new HashSet<int>();
            for (int i = 0; i < vertices.Count; ++i)
            {
                Vector3 from = vertices[i].Position;
                int toIndex = i + 1 == vertices.Count ? 0 : i + 1;
                Vector3 to = vertices[toIndex].Position;
                Vector3 normal = from - to;
                normal = Quaternion.AngleAxis(90, Vector3.forward) * normal;
                Vector3 midpoint = (from + to) / 2f;
                float dot = Vector3.Dot(normal, origin - midpoint);
                if (dot <= 0) continue;
                visibleVerticesIndexes.Add(i);
                visibleVerticesIndexes.Add(toIndex);
            }

            return visibleVerticesIndexes.ToList();
        }
        
        public List<List<FLVertex>> GetVisibleVertices(Vector3 origin, float radius)
        {
            List<FLVertex> worldVertices = GetVerticesInRange(origin, radius);

            List<int> visibleVertexIndices = GetVisibleVertexIndices(worldVertices, origin);
            visibleVertexIndices.Sort((a, b) => a.CompareTo(b));

            List<List<int>> edges = new List<List<int>>();
            List<int> currentList = null;
            int prevValue = -10;
            bool beganAtZero = false;
            bool endedAtEndOfList = false;

            foreach (int vertIndex in visibleVertexIndices)
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

            List<List<FLVertex>> finalEdges = new List<List<FLVertex>>();
            
            foreach (List<int> edge in edges)
            {
                List<FLVertex> edgeList = new List<FLVertex>();
                finalEdges.Add(edgeList);
                edge.ForEach(i => edgeList.Add(worldVertices[i]));
                edgeList.Sort((a, b) => a.Angle.CompareTo(b.Angle));
                for (int i = 0; i < edge.Count; i++)
                {
                    int vertIndex = edge[i];
                    if (i < edge.Count - 1)
                    {
                        int nextVertIndex = edge[i + 1];
                        if (worldVertices[vertIndex].Angle > worldVertices[nextVertIndex].Angle)
                        {
                            edgeList.Remove(worldVertices[vertIndex]);
                        }
                    }

                    if (i > 0)
                    {
                        int nextVertIndex = edge[i - 1];
                        if (worldVertices[vertIndex].Angle < worldVertices[nextVertIndex].Angle)
                        {
                            edgeList.Remove(worldVertices[vertIndex]);
                        }
                    }
                }

                for (int i = 0; i < edgeList.Count - 1; ++i)
                {
                    Debug.DrawLine(edgeList[i].Position, edgeList[i + 1].Position, Color.magenta, 2f);
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