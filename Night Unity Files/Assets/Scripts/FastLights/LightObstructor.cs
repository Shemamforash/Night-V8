using System;
using System.Collections.Generic;
using System.Linq;
using Fastlights;
using SamsHelper;
using SamsHelper.Libraries;
using UnityEngine;

namespace FastLights
{
    public class LightObstructor : MonoBehaviour
    {
        public bool UseCollider;
        private Mesh _mesh;
        private Collider2D _collider;
        private readonly List<FLVertex> _worldVerts = new List<FLVertex>();
        private readonly List<FLEdge> _edges = new List<FLEdge>();
        private Vector3 position;
        private Vector3 scale;
        private float rotation;

        public void Awake()
        {
            if (UseCollider)
            {
                _collider = GetComponent<Collider2D>();
                if (_collider == null) throw new Exceptions.ComponentNotFoundException(gameObject.name, typeof(Collider2D));
            }
            else
            {
                _mesh = GetComponent<MeshFilter>().mesh;
                if (_mesh == null) throw new UnnassignedMeshException();
            }

            FastLight.RegisterObstacle(this);
            position = transform.position;
            scale = transform.localScale;
            rotation = transform.rotation.eulerAngles.z;
        }

        public float MaxRadius()
        {
            return UseCollider ? _collider.bounds.max.magnitude : _mesh.bounds.max.magnitude;
        }

        private List<Vector3> GetVertices()
        {
            if (!UseCollider) return _mesh.vertices.ToList();
            List<Vector3> points = new List<Vector3>();
            if (_collider is PolygonCollider2D)
            {
                PolygonCollider2D polygon = (PolygonCollider2D) _collider;
                points.AddRange(polygon.points.Select(polygonPoint => (Vector3) polygonPoint));
            }
            else if (_collider is BoxCollider2D)
            {
                BoxCollider2D box = (BoxCollider2D) _collider;
                points.Add(new Vector2(-box.size.x / 2, box.size.y / 2));
                points.Add(new Vector2(box.size.x / 2, box.size.y / 2));
                points.Add(new Vector2(box.size.x / 2, -box.size.y / 2));
                points.Add(new Vector2(-box.size.x / 2, -box.size.y / 2));
            }
            else if (_collider is CircleCollider2D)
            {
                CircleCollider2D circle = (CircleCollider2D) _collider;
                float radius = circle.radius;
                float angleInterval = 5 / radius;
                for (float angle = 0; angle < 360; angle += angleInterval)
                {
                    float x = Mathf.Cos(angle * Mathf.Deg2Rad);
                    float y = Mathf.Sin(angle * Mathf.Deg2Rad);
                    points.Add(new Vector2(x, y));
                }
            }

            return points;
        }

        public void Update()
        {
            if (transform.position == position && transform.localScale == scale && transform.rotation.eulerAngles.z == rotation) return;
            UpdateMesh();
            FastLight.UpdateLights();
            position = transform.position;
            scale = transform.localScale;
            rotation = transform.rotation.eulerAngles.z;
        }

        public void UpdateMesh()
        {
            _worldVerts.Clear();
            _edges.Clear();
            foreach (Vector3 meshVertex in GetVertices())
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
                _worldVerts[prevIndex].EdgeB = edge;
                _worldVerts[i].EdgeA = edge;
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
            _worldVerts.ForEach(v => { v.SetDistanceAndAngle(origin, sqrRadius); });
            _edges.ForEach(e =>
            {
                if (e.From.OutOfRange && e.To.OutOfRange) return;
                if (!e.CalculateVisibility(origin)) return;
                visibleEdges.Add(e);
//                e.Draw(Color.yellow, Color.blue);
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

//            edgeSegments.ForEach(s =>
//            {
//                Color a = Helper.RandomColour();
//                Color b = Helper.RandomColour();
//                s.ForEach(e => e.Draw(a, b));
//            });

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