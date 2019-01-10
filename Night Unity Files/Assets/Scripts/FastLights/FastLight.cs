using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FastLights;
using SamsHelper.Libraries;
using UnityEngine;

namespace Fastlights
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class FastLight : MonoBehaviour
    {
        private static readonly List<FastLight> _lights = new List<FastLight>();
        private static readonly List<LightObstructor> _allObstructors = new List<LightObstructor>();
        private static readonly List<MeshFilter> _meshFilters = new List<MeshFilter>();

        private Vector2 _position = Vector2.negativeInfinity;
        private bool _needsUpdate;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private MaterialPropertyBlock _propBlock;
        private Color _lastColour;
        private float _lastRadius;

        public Color Colour = Color.white;
        [Range(0.01f, 30f)] public float Radius;
        public Material LightMaterial;
        public GameObject Target;

        private List<List<FLEdge>> _visibleEdges;
        private Tuple<bool, Vector2, float> _intersection;
        private readonly List<Vector2> meshVertices = new List<Vector2>();
        private Vector2 dirToVertex, lineSegmentEnd, intersectPoint;
        private float nearestDistance;
        private bool didIntersect;
        private int edgeCount;
        private bool _hasUpdated;
        private Transform _lightTransform;
        private bool _intersectionExists;
        private Vector2 _intersectionPoint;
        private List<List<FLEdge>> _edgeSegments = new List<List<FLEdge>>();
        private readonly List<FLEdge> _edges = new List<FLEdge>();
        private readonly List<FLVertex> _verts = new List<FLVertex>();
        private List<FLEdge> _segments = new List<FLEdge>();
        private ConcurrentQueue<List<FLEdge>> _edgeSegmentQueue;

        public static void UpdateLights()
        {
            _lights.ForEach(l => l._needsUpdate = true);
        }

        public void Awake()
        {
            _propBlock = new MaterialPropertyBlock();
            _lights.Add(this);
            if (LightMaterial == null) LightMaterial = new Material(Shader.Find("Unlit/Texture"));
            gameObject.layer = 21;
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer.material = LightMaterial;
            _lightTransform = transform;
            Vector3 position = _lightTransform.position;
            _lightTransform.position = position;
            _meshFilters.Add(_meshFilter);
        }

        private void OnDestroy()
        {
            _meshFilters.Remove(_meshFilter);
            _lights.Remove(this);
        }

        public static void RegisterObstacle(LightObstructor lightObstructor)
        {
            _allObstructors.Add(lightObstructor);
            UpdateLights();
        }

        public static void UnregisterObstacle(LightObstructor lightObstructor)
        {
            _allObstructors.Remove(lightObstructor);
        }

        private void CalculateNearestIntersection(FLVertex target, List<FLEdge> edges)
        {
            dirToVertex = (target.InRangePosition - _position).normalized;
            lineSegmentEnd = _position + dirToVertex * Radius;
            intersectPoint = lineSegmentEnd * 2;

            nearestDistance = float.MaxValue;
            didIntersect = false;

            edgeCount = edges.Count;

            for (int i = 0; i < edgeCount; i++)
            {
                FLEdge e = edges[i];
                if (e.BelongsToEdge(target)) continue;
                float fromAngle = e.From.InRangeAngle;
                float toAngle = e.To.InRangeAngle;
                float tempRayAngle = target.InRangeAngle;
                if (fromAngle > toAngle)
                {
                    if (tempRayAngle > fromAngle && tempRayAngle > toAngle)
                    {
                        tempRayAngle -= 360;
                    }

                    fromAngle -= 360f;
                }

                if (fromAngle > tempRayAngle && toAngle > tempRayAngle) continue;
                if (fromAngle < tempRayAngle && toAngle < tempRayAngle) continue;

                Vector2? tempIntersect = AdvancedMaths.LineIntersection(_position, lineSegmentEnd, e.From.InRangePosition, e.To.InRangePosition);
                if (tempIntersect == null) continue;

                didIntersect = true;

                float distance = Vector2.SqrMagnitude(_position - tempIntersect.Value);
                if (distance >= nearestDistance) continue;
                nearestDistance = distance;
                intersectPoint = tempIntersect.Value;
            }

            _intersectionExists = didIntersect;
            _intersectionPoint = intersectPoint;
        }


        private void DrawLight()
        {
            meshVertices.Clear();
            _edgeSegments.Clear();
            _edgeSegmentQueue = new ConcurrentQueue<List<FLEdge>>();

            float sqrRadius = Radius * Radius;
            int obstructorCount = _allObstructors.Count;

            Parallel.For(0, obstructorCount, i =>
            {
                LightObstructor o = _allObstructors[i];
                List<List<FLEdge>> visibleEdges = o.GetVisibleVertices(_position, sqrRadius, Radius);
                visibleEdges?.ForEach(v => _edgeSegmentQueue.Enqueue(v));
            });

            _edgeSegments = _edgeSegmentQueue.ToList();
            if (_edgeSegments.Count == 0)
            {
                InsertLineSegments(0, 360);
                BuildMesh();
                return;
            }

            _edges.Clear();
            _verts.Clear();
            int edgeSegmentCount = _edgeSegments.Count;
            for (int i = 0; i < edgeSegmentCount; i++)
            {
                _segments = _edgeSegments[i];
                int segmentLength = _segments.Count;
                for (int j = 0; j < segmentLength; ++j)
                {
                    _edges.Add(_segments[j]);
                    _verts.Add(_segments[j].From);
                    if (j != segmentLength - 1) continue;
                    _verts.Add(_segments[j].To);
                }
            }

            _verts.Sort((a, b) => a.InRangeAngle.CompareTo(b.InRangeAngle));

            FLVertex endVert = null;
            int vertCount = _verts.Count;
            for (int i = 0; i < vertCount; i++)
            {
                FLVertex vert = _verts[i];
                CalculateNearestIntersection(vert, _edges);
                if (_intersectionExists)
                {
                    float distance = Vector2.SqrMagnitude(_intersectionPoint - _position);
                    if (distance > vert.SqrDistanceToOrigin)
                    {
                        if (vert.IsStart)
                        {
                            meshVertices.Add(_intersectionPoint);
                            meshVertices.Add(vert.InRangePosition);
                        }

                        if (vert.IsEnd)
                        {
                            meshVertices.Add(vert.InRangePosition);
                            meshVertices.Add(_intersectionPoint);
                        }
                        else meshVertices.Add(vert.InRangePosition);
                    }
                    else meshVertices.Add(_intersectionPoint);
                }
                else
                {
                    if (endVert != null) InsertLineSegments(endVert.InRangeAngle, vert.InRangeAngle);
                    meshVertices.Add(vert.InRangePosition);
                    endVert = vert.IsEnd ? vert : null;
                }
            }

            if (endVert != null) InsertLineSegments(endVert.InRangeAngle, _verts[0].InRangeAngle);
            BuildMesh();
        }

        private void BuildMesh()
        {
            Mesh mesh = _meshFilter.mesh;
            mesh.Clear();
            meshVertices.Insert(0, _position);
            Vector3[] v = new Vector3[meshVertices.Count];
            for (int i = 0; i < meshVertices.Count; ++i)
            {
                v[i] = meshVertices[i];
                if (Target != null) v[i] -= (Vector3) _position;
            }

            mesh.vertices = v;
            mesh.triangles = Triangulate(v);
            Vector3[] normals = new Vector3[mesh.vertices.Length];
            for (int i = 0; i < normals.Length; i++) normals[i] = Vector2.up;
            mesh.normals = normals;
            mesh.uv = CalculateUvs(mesh.vertices);
        }

        private int[] Triangulate(Vector3[] vertices)
        {
            int[] triangles = new int[(vertices.Length - 1) * 3];
            for (int i = 1; i < vertices.Length; ++i)
            {
                int nextTriangle = i + 1 == vertices.Length ? 1 : i + 1;
                int triIndex = (i - 1) * 3;
                triangles[triIndex] = 0;
                triangles[triIndex + 1] = i;
                triangles[triIndex + 2] = nextTriangle;
            }

            return triangles;
        }

        private Vector2[] CalculateUvs(Vector3[] vertices)
        {
            Vector2[] uvs = new Vector2[vertices.Length];

            for (int i = 0; i < uvs.Length; i++)
            {
                float u = vertices[i].x / Radius / 2 + 0.5f;
                float v = vertices[i].y / Radius / 2 + 0.5f;

                u = Mathf.Clamp(u, 0f, 1f);
                v = Mathf.Clamp(v, 0f, 1f);

                uvs[i] = new Vector2(u, v);
            }

            return uvs;
        }

        private void AddPointOnCircleEdge(float angle)
        {
            float x = _position.x + Radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            float y = _position.y + Radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            meshVertices.Add(new Vector2(x, y));
        }

        private void InsertLineSegments(float from, float to)
        {
            float angleIncrement = 0.2f * Radius + 0.5f;
            if (to < from) to += 360;
            AddPointOnCircleEdge(from);
            for (float angle = from + angleIncrement; angle < to - angleIncrement; angle += angleIncrement) AddPointOnCircleEdge(angle);
            AddPointOnCircleEdge(to);
        }

        private void ResizeLight()
        {
            float sqrRadius = Radius * Radius;
            Mesh mesh = _meshFilter.mesh;
            Vector3[] verts = mesh.vertices;
            for (int i = 1; i < verts.Length; ++i)
            {
                float sqrDistance = verts[i].sqrMagnitude;
                if (sqrDistance <= sqrRadius) continue;
                verts[i] = verts[i] / Mathf.Sqrt(sqrDistance) * Radius;
            }

            mesh.vertices = verts;
        }


        private void UpdateLight(Vector2 newPosition)
        {
            bool isPositionSame = newPosition == _position;
            _position = newPosition;
            if (!_needsUpdate && _lastRadius == Radius && isPositionSame) return;
            bool radiusSmaller = _lastRadius > Radius;
            bool noObstructor = _allObstructors.Count == 0;
            if ((radiusSmaller || noObstructor) && isPositionSame)
            {
                ResizeLight();
            }
            else
            {
                DrawLight();
            }

            _needsUpdate = false;
            _hasUpdated = true;
            _lastRadius = Radius;
        }

        private void UpdateColour()
        {
            if (!_hasUpdated && _lastColour == Colour) return;
            _meshRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor("_Color", Colour);
            _meshRenderer.SetPropertyBlock(_propBlock);
            _lastColour = Colour;
        }

        public void Update()
        {
            FollowTarget();
            Vector3 position = _lightTransform.position;
            Vector2 newPosition = new Vector2(position.x, position.y);
            Stopwatch watch = Stopwatch.StartNew();
            UpdateLight(newPosition);
            watch.Restart();
            UpdateColour();
            _hasUpdated = false;
        }

        private void FollowTarget()
        {
            if (Target == null) return;
            transform.position = Target.transform.position;
        }

        public List<Vector2> Vertices()
        {
            return meshVertices;
        }

        public static List<MeshFilter> GetLightMeshes()
        {
            return _meshFilters;
        }
    }
}