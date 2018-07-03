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
            UpdateMesh();
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
                    float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                    float y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
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
            List<Vector3> vertices = GetVertices();
            int vertexCount = vertices.Count;
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 meshVertex = vertices[i];
                FLVertex vertex = new FLVertex(transform, meshVertex);
                _worldVerts.Add(vertex);
            }

            for (int i = 0; i < vertexCount; ++i)
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

        private List<FLEdge> GetVisibleEdges()
        {
            List<FLEdge> visibleEdges = new List<FLEdge>();
            int vertCount = _worldVerts.Count;
            for (int i = 0; i < vertCount; i++)
            {
                FLVertex v = _worldVerts[i];
                v.SetDistanceAndAngle(_origin, _sqrRadius);
            }

            int edgeCount = _edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                FLEdge e = _edges[i];
                if (e.From.OutOfRange && e.To.OutOfRange) continue;
                if (e.From.OutOfRange)
                {
                    Vector2 newVertexPos = AdvancedMaths.FindLineSegmentCircleIntersections(e.From.Position, e.To.Position, _origin, _radius)[0];
                    e.From.SetInRangePosition(newVertexPos, _origin);
                }

                if (e.To.OutOfRange)
                {
                    Vector2 newVertexPos = AdvancedMaths.FindLineSegmentCircleIntersections(e.From.Position, e.To.Position, _origin, _radius)[0];
                    e.To.SetInRangePosition(newVertexPos, _origin);
                }

                if (!e.CalculateVisibility(_origin)) continue;
                visibleEdges.Add(e);
//                e.Draw(Color.yellow, Color.blue);
            }

            return visibleEdges;
        }

        private float _sqrRadius, _radius;
        private Vector3 _origin;

        public List<List<FLEdge>> GetVisibleVertices(Vector3 origin, float sqrRadius, float radius)
        {
            _sqrRadius = sqrRadius;
            _radius = radius;
            _origin = origin;
            List<List<FLEdge>> edgeSegments = new List<List<FLEdge>>();

            List<FLEdge> edges = GetVisibleEdges();
            int edgeCount = edges.Count;

            if (edgeCount == 1)
            {
                edges[0].From.IsStart = true;
                edges[0].To.IsEnd = true;
                edgeSegments.Add(edges);
                return edgeSegments;
            }

            List<FLEdge> currentEdgeSegment = new List<FLEdge>();
            int segmentCount = 0;

            bool completedSegment = true;
            for (int i = 0; i < edgeCount; ++i)
            {
                FLEdge next = edges[Helper.NextIndex(i, edges)];
                FLEdge current = edges[i];
                if (next.From != current.To)
                {
                    next.From.IsStart = true;
                    current.To.IsEnd = true;
                    currentEdgeSegment.Add(current);
                    ++segmentCount;
                    if (segmentCount == 1) edgeSegments.Add(currentEdgeSegment);
                    currentEdgeSegment = new List<FLEdge>();
                    segmentCount = 0;
                    completedSegment = true;
                }
                else
                {
                    next.From.IsEnd = false;
                    current.To.IsStart = false;
                    currentEdgeSegment.Add(current);
                    ++segmentCount;
                    if (segmentCount == 1) edgeSegments.Add(currentEdgeSegment);
                    completedSegment = false;
                }
            }

            if (completedSegment) return edgeSegments;
            List<FLEdge> endSegment = Helper.RemoveEnd(edgeSegments);
            endSegment.AddRange(edgeSegments[0]);
            edgeSegments[0] = endSegment;

            return edgeSegments;
        }
    }

    public class UnnassignedMeshException : Exception
    {
        public new string Message() => "No mesh assigned, a mesh is required for FastLight to work";
    }
}