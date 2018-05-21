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
        private readonly List<FLVertex> _worldVerts = new List<FLVertex>();
        private readonly List<FLEdge> _edges = new List<FLEdge>();

        public void Awake()
        {
            mesh = GetComponent<MeshFilter>().mesh;
            if (mesh == null) throw new UnnassignedMeshException();
            FastLight.RegisterObstacle(this);
        }

        public void UpdateMesh()
        {
            _worldVerts.Clear();
            _edges.Clear();
            foreach (Vector3 meshVertex in mesh.vertices)
            {
                FLVertex vertex = new FLVertex(transform, meshVertex);
                _worldVerts.Add(vertex);
            }

            for (int i = 0; i < _worldVerts.Count; ++i)
            {
                int prevIndex = Helper.PrevIndex(i, _worldVerts);
                int nextIndex = Helper.NextIndex(i, _worldVerts);
                _worldVerts[i].PreviousFlVertex = _worldVerts[prevIndex];
                _worldVerts[i].NextFlVertex = _worldVerts[nextIndex];
                FLEdge edge = new FLEdge(_worldVerts[prevIndex], _worldVerts[i]);
                _edges.Add(edge);
            }
        }

        private void OnDestroy()
        {
            FastLight.UnregisterObstacle(this);
        }

        private List<FLEdge> GetVisibleEdges(Vector2 origin, float sqrRadius)
        {
            List<FLEdge> visibleEdges = new List<FLEdge>();
            //direction from light source to the obstructor
            Vector2 dirToObstructor = (origin - (Vector2) transform.position).normalized;
            _worldVerts.ForEach(v => { v.SetDistanceAndAngle(origin, sqrRadius, dirToObstructor); });

            _edges.ForEach(e =>
            {
                if (e.From.OutOfRange && e.To.OutOfRange) return;
                if (!e.CalculateVisibility(origin)) return;
                visibleEdges.Add(e);
//                e.Draw();
            });

            return visibleEdges;
        }

        public List<List<FLEdge>> GetVisibleVertices(Vector3 origin, float radius)
        {
            List<List<FLEdge>> edgeSegments = new List<List<FLEdge>>();

            List<FLEdge> edges = GetVisibleEdges(origin, radius);
            if (edges.Count == 1)
            {
                edges[0].From.IsStart = true;
                edges[0].To.IsEnd = true;
                edgeSegments.Add(edges);
                return edgeSegments;
            }

            List<FLEdge> currentEdgeSegment = new List<FLEdge>();

            bool completedSegment = true;
            for (int i = 0; i < edges.Count; ++i)
            {
                FLEdge next = edges[Helper.NextIndex(i, edges)];
                FLEdge current = edges[i];
                if (next.From != current.To)
                {
                    next.From.IsStart = true;
                    current.To.IsEnd = true;
                    currentEdgeSegment.Add(current);
                    if (currentEdgeSegment.Count == 1) edgeSegments.Add(currentEdgeSegment);
                    currentEdgeSegment = new List<FLEdge>();
                    completedSegment = true;
                }
                else
                {
                    next.From.IsEnd = false;
                    current.To.IsStart = false;
                    currentEdgeSegment.Add(current);
                    if (currentEdgeSegment.Count == 1) edgeSegments.Add(currentEdgeSegment);
                    completedSegment = false;
                }
            }

            if (!completedSegment)
            {
                List<FLEdge> endSegment = Helper.RemoveEnd(edgeSegments);
                endSegment.AddRange(edgeSegments[0]);
                edgeSegments[0] = endSegment;
            }

            edgeSegments.ForEach(s => s.ForEach(e => e.Draw()));

            return edgeSegments;
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