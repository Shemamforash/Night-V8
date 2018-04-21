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

        private static float PerpendicularDistance(Vector3 pointOnLine, Vector3 lineDirection, Vector3 targetPoint)
        {
            Vector3 directionToPoint = targetPoint - pointOnLine;
            float dot = Vector3.Dot(directionToPoint, lineDirection);
            Vector3 pointOnline = pointOnLine + lineDirection * dot;
            float distance = Vector2.Distance(targetPoint, pointOnline);
            if (AdvancedMaths.Dot(pointOnLine, pointOnLine + lineDirection, targetPoint) < 0) distance = -distance;
            return distance;
        }

        private List<FLVertex> GetVerticesInRange(Vector3 origin, float radius)
        {
            List<FLVertex> verts = new List<FLVertex>();
            Vector2 dirToObstructor = (origin - transform.position).normalized;
            foreach (Vector3 v in mesh.vertices)
            {
                Vector3 worldVertex = transform.TransformPoint(v);
                float distance = Vector2.Distance(worldVertex, origin);
                if (distance > radius) continue;
                float angle = 360 - AdvancedMaths.AngleFromUp(origin, worldVertex);
                FLVertex flVertex = new FLVertex(worldVertex, distance, angle);
                flVertex.PerpendicularDistance = PerpendicularDistance(transform.position, dirToObstructor, worldVertex);
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

            List<List<int>> edges = new List<List<int>>();
            List<int> currentList = null;
            int prevValue = -10;

            foreach (int vertIndex in visibleVertexIndices)
            {
                if (currentList == null || vertIndex != prevValue + 1)
                {
                    currentList = new List<int>();
                    edges.Add(currentList);
                    currentList.Add(vertIndex);
                }
                else if (vertIndex == prevValue + 1)
                {
                    currentList.Add(vertIndex);
                }

                prevValue = vertIndex;
            }

            if (visibleVertexIndices.Contains(0) && visibleVertexIndices.Contains(worldVertices.Count - 1) && edges.Count > 1)
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
                edgeList.Sort((a, b) => a.PerpendicularDistance.CompareTo(b.PerpendicularDistance));
                edgeList.Reverse();
                for (int i = 0; i < edgeList.Count - 1; ++i)
                {
                    edgeList[i].NextFlVertex = edgeList[i + 1];
                    edgeList[i + 1].PreviousFlVertex = edgeList[i];
                }

//                for (int i = 0; i < edge.Count; i++)
//                {
//                    int vertIndex = edge[i];
//                    if (i < edge.Count - 1)
//                    {
//                        int nextVertIndex = edge[i + 1];
//                        if (worldVertices[vertIndex].Angle > worldVertices[nextVertIndex].Angle)
//                        {
//                            edgeList.Remove(worldVertices[vertIndex]);
//                        }
//                    }
//
//                    if (i == 0) continue;
//                    {
//                        int nextVertIndex = edge[i - 1];
//                        if (worldVertices[vertIndex].Angle < worldVertices[nextVertIndex].Angle)
//                        {
//                            edgeList.Remove(worldVertices[vertIndex]);
//                        }
//                    }
//                }
            }

//            Debug.Log(finalEdges.Count);

            return finalEdges;
        }

        private void DrawVertices(List<FLVertex> vertices)
        {
            float alphaIncrement = 0.5f / vertices.Count;

            float currentAlpha = 0.5f;
            for (int i = 0; i < vertices.Count - 1; ++i)
            {
                Vector2 mid = (vertices[i].Position + vertices[i + 1].Position) / 2;
                Debug.DrawLine(vertices[i].Position, mid, new Color(0, 1, 0, currentAlpha - alphaIncrement), 2f);
                Debug.DrawLine(mid, vertices[i + 1].Position, new Color(0, 1, 0, currentAlpha), 2f);
                currentAlpha += alphaIncrement;
            }
        }
    }

    public class UnnassignedMeshException : Exception
    {
        public string Message() => "No mesh assigned, a mesh is required for FastLight to work";
    }
}