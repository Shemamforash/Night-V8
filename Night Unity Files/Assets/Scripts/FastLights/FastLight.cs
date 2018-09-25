using System;
using System.Collections.Generic;
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
        private Color _lastColour;
        private float _lastRadius;

        public Color Colour = Color.white;
        [Range(0.01f, 30f)] public float Radius;
        public Material LightMaterial;
        public GameObject Target;

        public static void UpdateLights()
        {
            _lights.ForEach(l => l._needsUpdate = true);
        }

        public void Awake()
        {
            _lights.Add(this);
            if (LightMaterial == null) LightMaterial = new Material(Shader.Find("Unlit/Texture"));
            gameObject.layer = 21;
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer.material = LightMaterial;
            Vector3 position = transform.position;
            transform.position = position;
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

        private List<List<FLEdge>> _visibleEdges;
        private Tuple<bool, Vector2, float> _intersection;

        private readonly List<Vector2> meshVertices = new List<Vector2>();

        private void CalculateNearestIntersection(FLVertex target, List<FLEdge> edges)
        {
            Vector2 dirToVertex = (target.InRangePosition - _position).normalized;
            Vector2 lineSegmentEnd = _position + dirToVertex * Radius;
            Vector2 intersectPoint = lineSegmentEnd * 2;

            float nearestDistance = float.MaxValue;
            bool didIntersect = false;

//            int targetBucket = Mathf.FloorToInt(target.InRangeAngle / bucketAngle);
//            int bucketCount = _edgeBuckets.Count;
//            for (int j = 0; j < bucketCount; ++j)
//            {
//                EdgeBucket b = _edgeBuckets[j];
//                if (!b.AngleFallsInBucket(target.InRangeAngle)) continue;
            int edgeCount = edges.Count;

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

                Tuple<bool, Vector2> tempIntersect = AdvancedMaths.LineIntersection(_position, lineSegmentEnd, e.From.InRangePosition, e.To.InRangePosition);
                if (!tempIntersect.Item1) continue;

                didIntersect = true;

                float distance = Vector2.SqrMagnitude(_position - tempIntersect.Item2);
                if (distance >= nearestDistance) continue;
                nearestDistance = distance;
                intersectPoint = tempIntersect.Item2;
            }
//            }

            _intersectionExists = didIntersect;
            _intersectionPoint = intersectPoint;
        }

        private bool _intersectionExists;
        private Vector2 _intersectionPoint;

        private void DrawLight()
        {
            meshVertices.Clear();
            List<List<FLEdge>> edgeSegments = new List<List<FLEdge>>();

            float sqrRadius = Radius * Radius;
            int obstructorCount = _allObstructors.Count;
            for (int i = 0; i < obstructorCount; i++)
            {
                LightObstructor o = _allObstructors[i];
//                if (!o.Visible()) continue;
                _visibleEdges = o.GetVisibleVertices(_position, sqrRadius, Radius);
                if (_visibleEdges == null) continue;
                edgeSegments.AddRange(_visibleEdges);
            }

            if (edgeSegments.Count == 0)
            {
                InsertLineSegments(0, 360);
                BuildMesh();
                return;
            }

            List<FLEdge> edges = new List<FLEdge>();
            List<FLVertex> verts = new List<FLVertex>();
            int edgeSegmentCount = edgeSegments.Count;
            for (int i = 0; i < edgeSegmentCount; i++)
            {
                List<FLEdge> s = edgeSegments[i];
                int segmentLength = s.Count;
                for (int j = 0; j < segmentLength; ++j)
                {
                    edges.Add(s[j]);
                    verts.Add(s[j].From);
                    if (j != segmentLength - 1) continue;
                    verts.Add(s[j].To);
                }
            }

            verts.Sort((a, b) => a.InRangeAngle.CompareTo(b.InRangeAngle));

            FLVertex endVert = null;
            int vertCount = verts.Count;
            for (int i = 0; i < vertCount; i++)
            {
                FLVertex vert = verts[i];
                CalculateNearestIntersection(vert, edges);
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

            if (endVert != null) InsertLineSegments(endVert.InRangeAngle, verts[0].InRangeAngle);
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
                if (Target != null) v[i] -= Target.transform.position;
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

        private bool _hasUpdated;

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
            Mesh mesh = _meshFilter.mesh;
            Color32[] colors = new Color32[mesh.vertices.Length];
            for (int i = 0; i < colors.Length; ++i)
            {
                colors[i] = Colour;
            }

            mesh.colors32 = colors;
            _lastColour = Colour;
        }

        public void Update()
        {
            FollowTarget();
            Vector2 newPosition = new Vector2(transform.position.x, transform.position.y);
            UpdateLight(newPosition);
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