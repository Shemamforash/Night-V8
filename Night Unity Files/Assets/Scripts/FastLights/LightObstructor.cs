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
        private Polygon _polygon;
        private readonly List<Vector2> _verts2d = new List<Vector2>();
        private readonly List<FLEdge> _visibleEdges = new List<FLEdge>();
        private float _sqrRadius, _radius;
        private Vector2 _origin;
        private readonly List<List<FLEdge>> _edgeSegments = new List<List<FLEdge>>();
        private Transform _obstructorTransform;

        public void Awake()
        {
            gameObject.layer = 21;

            if (UseCollider)
            {
                _collider = GetComponent<Collider2D>();
                if (_collider == null) throw new Exceptions.ComponentNotFoundException(gameObject.name, typeof(Collider2D));
            }
            else
            {
                _mesh = GetComponent<MeshFilter>().mesh;
                if (_mesh == null) throw new UnassignedMeshException();
            }

            FastLight.RegisterObstacle(this);
            _obstructorTransform = transform;
            position = _obstructorTransform.position;
            scale = _obstructorTransform.localScale;
            rotation = _obstructorTransform.rotation.eulerAngles.z;
            UpdateMesh();
        }

        public float MaxRadius()
        {
            return UseCollider ? _collider.bounds.max.magnitude : _mesh.bounds.max.magnitude;
        }

        private List<Vector3> _vertices = new List<Vector3>();

        private void GetVertices()
        {
            if (!UseCollider)
            {
                _vertices = _mesh.vertices.ToList();
                return;
            }

            _vertices.Clear();
            switch (_collider)
            {
                case PolygonCollider2D polygon:
                    _vertices.AddRange(polygon.points.Select(polygonPoint => (Vector3) polygonPoint));
                    break;
                case BoxCollider2D box:
                {
                    Vector2 size = box.size;
                    _vertices.Add(new Vector2(-size.x / 2, size.y / 2));
                    _vertices.Add(new Vector2(box.size.x / 2, size.y / 2));
                    _vertices.Add(new Vector2(box.size.x / 2, -size.y / 2));
                    _vertices.Add(new Vector2(-box.size.x / 2, -size.y / 2));
                    break;
                }
                case CircleCollider2D circle:
                {
                    float radius = circle.radius;
                    float angleInterval = 5 / radius;
                    for (float angle = 0; angle < 360; angle += angleInterval)
                    {
                        float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
                        float y = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
                        _vertices.Add(new Vector2(x, y));
                    }

                    break;
                }
            }
        }

        public void Update()
        {
            if (transform.position == position && transform.localScale == scale && transform.rotation.eulerAngles.z == rotation) return;
            UpdateMesh();
            FastLight.UpdateLights();
            position = _obstructorTransform.position;
            scale = _obstructorTransform.localScale;
            rotation = _obstructorTransform.rotation.eulerAngles.z;
        }


        public void UpdateMesh()
        {
            _worldVerts.Clear();
            _edges.Clear();
            _verts2d.Clear();
            GetVertices();
            int vertexCount = _vertices.Count;
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 meshVertex = _vertices[i];
                FLVertex vertex = new FLVertex(transform, meshVertex);
                _worldVerts.Add(vertex);
                _verts2d.Add(_vertices[i]);
            }

            _polygon = new Polygon(_verts2d, transform.position);

            for (int i = 0; i < vertexCount; ++i)
            {
                int prevIndex = _worldVerts.PrevIndex(i);
                int nextIndex = _worldVerts.NextIndex(i);
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


        private void GetVisibleEdges()
        {
            _visibleEdges.Clear();
            int vertCount = _worldVerts.Count;
            bool outOfRange = true;
            for (int i = 0; i < vertCount; i++)
            {
                FLVertex v = _worldVerts[i];
                if (outOfRange)
                {
                    float sqrDistance = (v.Position - _origin).sqrMagnitude;
                    if (sqrDistance < _sqrRadius) outOfRange = false;
                }

                v.SetDistanceAndAngle(_origin, _sqrRadius);
            }

            if (outOfRange) return;

            int edgeCount = _edges.Count;
            for (int i = 0; i < edgeCount; i++)
            {
                FLEdge e = _edges[i];
                if (e.From.OutOfRange && e.To.OutOfRange)
                    continue;
                List<Vector2> circleIntersections = AdvancedMaths.FindLineSegmentCircleIntersections(e.From.Position, e.To.Position, _origin, _radius);
                if (circleIntersections.Count != 0)
                {
                    Vector2 newVertexPos = circleIntersections[0];
                    if (e.From.OutOfRange)
                        e.From.SetInRangePosition(newVertexPos, _origin);
                    if (e.To.OutOfRange)
                        e.To.SetInRangePosition(newVertexPos, _origin);
                }

                if (!e.CalculateVisibility(_origin)) continue;
                _visibleEdges.Add(e);
            }
        }

        public List<List<FLEdge>> GetVisibleVertices(Vector3 origin, float sqrRadius, float radius)
        {
            _sqrRadius = sqrRadius;
            _radius = radius;
            _origin = origin;
            _edgeSegments.Clear();
            GetVisibleEdges();
            if (_visibleEdges.Count == 0) return null;
            int edgeCount = _visibleEdges.Count;

            if (edgeCount == 1)
            {
                _visibleEdges[0].From.IsStart = true;
                _visibleEdges[0].To.IsEnd = true;
                _edgeSegments.Add(_visibleEdges);
                return _edgeSegments;
            }

            List<FLEdge> currentEdgeSegment = new List<FLEdge>();
            int segmentCount = 0;

            bool completedSegment = true;
            for (int i = 0; i < edgeCount; ++i)
            {
                FLEdge next = _visibleEdges[_visibleEdges.NextIndex(i)];
                FLEdge current = _visibleEdges[i];
                if (next.From != current.To)
                {
                    next.From.IsStart = true;
                    current.To.IsEnd = true;
                    currentEdgeSegment.Add(current);
                    ++segmentCount;
                    if (segmentCount == 1) _edgeSegments.Add(currentEdgeSegment);
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
                    if (segmentCount == 1) _edgeSegments.Add(currentEdgeSegment);
                    completedSegment = false;
                }
            }

            if (completedSegment) return _edgeSegments;
            List<FLEdge> endSegment = _edgeSegments.RemoveLast();
            endSegment.AddRange(_edgeSegments[0]);
            _edgeSegments[0] = endSegment;
            return _edgeSegments;
        }

        public bool Visible()
        {
            return _polygon.IsVisible();
        }
    }

    public class UnassignedMeshException : Exception
    {
        public override string Message => "No mesh assigned, a mesh is required for FastLight to work";
    }
}