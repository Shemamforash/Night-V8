﻿using System;
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

        private Vector2 _position = Vector2.negativeInfinity;
        private bool _needsUpdate;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private Color _lastColour;
        private float _lastRadius;

        public Color Colour = Color.white;
        public float Radius;
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
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer.material = LightMaterial;
            Vector3 position = transform.position;
            transform.position = position;
        }

        private void OnDestroy()
        {
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

//        private class EdgeBucket
//        {
//            public readonly float AngleFrom, AngleTo;
//            public readonly List<FLEdge> edges = new List<FLEdge>();

//            public EdgeBucket(float angleFrom, float angleTo)
//            {
//                AngleFrom = angleFrom;
//                AngleTo = angleTo;
//            }
            
//            public void AddEdgeToBucket(FLEdge edge)
//            {
//                if (edge.From.InRangeAngle >= AngleFrom && edge.From.InRangeAngle <= AngleTo)
//                {
//                    edges.Add(edge);
//                    return;
//                }

//                if (edge.To.InRangeAngle >= AngleFrom && edge.To.InRangeAngle <= AngleTo)
//                {
//                    edges.Add(edge);
//                }
//            }

//            public bool AngleFallsInBucket(float angle)
//            {
//                return angle >= AngleFrom && angle <= AngleTo;
//            }
            
            //d = 90
            //angle = 45
            //360 / 90 = 4
            //angle / 90 = 0
//        }

//        private readonly List<EdgeBucket> _edgeBuckets = new List<EdgeBucket>();
//        private float bucketAngle = 10f;

        
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
                float sqrDistanceToMesh = Vector2.SqrMagnitude((Vector2) o.transform.position - _position);
                if (sqrDistanceToMesh - o.MaxRadius() * o.MaxRadius() > sqrRadius) continue;
                _visibleEdges = o.GetVisibleVertices(_position, sqrRadius, Radius);
                edgeSegments.AddRange(_visibleEdges);
            }

            if (edgeSegments.Count == 0)
            {
                InsertLineSegments(0, 360);
                BuildMesh();
                return;
            }


//            _edgeBuckets.Clear();
//            for (float angle = 0; angle < 360; angle += bucketAngle)
//            {
//                EdgeBucket bucket = new EdgeBucket(angle, angle+bucketAngle);
//                _edgeBuckets.Add(bucket);
//            }

            List<FLEdge> edges = new List<FLEdge>();
            List<FLVertex> verts = new List<FLVertex>();
            int edgeSegmentCount = edgeSegments.Count;
//            int bucketCount = _edgeBuckets.Count;
            for (int i = 0; i < edgeSegmentCount; i++)
            {
                List<FLEdge> s = edgeSegments[i];
                int segmentLength = s.Count;
                for (int j = 0; j < segmentLength; ++j)
                {
//                    for (int k = 0; k < bucketCount; k++)
//                    {
//                        EdgeBucket b = _edgeBuckets[k];
//                        b.AddEdgeToBucket(s[j]);
//                    }

                    edges.Add(s[j]);
                    verts.Add(s[j].From);
                    if (j != segmentLength - 1) continue;
                    verts.Add(s[j].To);
                }
            }

            verts.Sort((a, b) => a.InRangeAngle.CompareTo(b.InRangeAngle));

            float endAngle = -1f;
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
                    if (endAngle != -1)
                    {
                        InsertLineSegments(endAngle, vert.InRangeAngle);
                    }

                    meshVertices.Add(vert.InRangePosition);

                    if (vert.IsEnd)
                        endAngle = vert.InRangeAngle;
                    else
                        endAngle = -1f;
                }
            }

            if(endAngle != -1) InsertLineSegments(endAngle, verts[0].InRangeAngle);

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
//                Vector3 vert = transform.InverseTransformPoint(meshVertices[i]);
//                vert.z = 0;
//                v[i] = vert;
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

        private void InsertLineSegments(float from, float to)
        {
            float angleIncrement = 0.5f;
            if (to < from) to += 360;
            float x, y;
            for (float angle = from; angle < to - angleIncrement; angle += angleIncrement)
            {
                x = _position.x + Radius * Mathf.Sin(angle * Mathf.Deg2Rad);
                y = _position.y + Radius * Mathf.Cos(angle * Mathf.Deg2Rad);
                meshVertices.Add(new Vector2(x, y));
            }

            x = _position.x + Radius * Mathf.Sin(to * Mathf.Deg2Rad);
            y = _position.y + Radius * Mathf.Cos(to * Mathf.Deg2Rad);
            meshVertices.Add(new Vector2(x, y));
        }

        private void ResizeLight()
        {
            Mesh mesh = _meshFilter.mesh;
            Vector3[] verts = mesh.vertices;
            for (int i = 1; i < verts.Length; ++i)
            {
                Vector2 vertex = transform.TransformPoint(verts[i]);
                Vector2 dir = (_position - vertex).normalized;
                vertex = dir * Radius;
                verts[i] = transform.InverseTransformPoint(vertex);
            }

            mesh.vertices = verts;
        }

        private bool _hasUpdated;

        private void UpdateLight(Vector2 newPosition)
        {
            bool isPositionSame = newPosition == _position;
            _position = newPosition;
            if (!_needsUpdate && _lastRadius == Radius && isPositionSame) return;
            if (_lastRadius > Radius && isPositionSame)
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
    }
}