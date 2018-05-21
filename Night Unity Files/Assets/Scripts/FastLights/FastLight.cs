﻿using System;
using System.Collections.Generic;
using System.Net;
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

        private static void UpdateLights()
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

        private Tuple<bool, Vector2, float> GetIntersectionWithEdge(Vector2 end, List<FLEdge> edges, FLVertex flVertex)
        {
            Vector2 nearestIntersection = Vector2.negativeInfinity;
            float nearestSqrDistance = float.MaxValue;
            bool intersectionExists = false;
            float rayAngle = 360 - AdvancedMaths.AngleFromUp(_position, end);
            edges.ForEach(e =>
            {
                if (e.BelongsToEdge(flVertex)) return;
                if (e.From.Angle > rayAngle && e.To.Angle > rayAngle) return;
                if (e.From.Angle < rayAngle && e.To.Angle < rayAngle) return;
                Tuple<bool, Vector2> intersection = AdvancedMaths.LineIntersection(e.From.Position, e.To.Position, _position, end);
                if (!intersection.Item1) return;
                float sqrDistance = Vector2.SqrMagnitude(_position - intersection.Item2);
                if (sqrDistance >= nearestSqrDistance) return;
                nearestSqrDistance = sqrDistance;
                intersectionExists = true;
                nearestIntersection = intersection.Item2;
            });
            return new Tuple<bool, Vector2, float>(intersectionExists, nearestIntersection, nearestSqrDistance);
        }

        private bool CheckIsNan(Vector2 vect)
        {
            return float.IsNaN(vect.x) || float.IsNaN(vect.y);
        }

        private List<List<FLEdge>> _visibleEdges;
        private Tuple<bool, Vector2, float> _intersection;

        private void DrawLight()
        {
            List<List<FLEdge>> edgeSegments = new List<List<FLEdge>>();

            float sqrRadius = Radius * Radius;
            _allObstructors.ForEach(o =>
            {
                float maxRadius = o.mesh.bounds.max.magnitude;
                float sqrDistanceToMesh = Vector2.SqrMagnitude((Vector2) o.transform.position - _position);
                if (sqrDistanceToMesh - maxRadius > sqrRadius) return;
                _visibleEdges = o.GetVisibleVertices(_position, sqrRadius);
                edgeSegments.AddRange(_visibleEdges);
            });

            List<int> positionsToRemoveAt = new List<int>();
            for (int i = 0; i < edgeSegments.Count; ++i)
            {
                float angleFromA = edgeSegments[i][0].From.Angle;
                int lastSegmentIndex = edgeSegments[i].Count - 1;
                float angleToA = edgeSegments[i][lastSegmentIndex].To.Angle;
                if (angleFromA > angleToA) angleFromA = angleFromA - 360;

                for (int j = i + 1; j < edgeSegments.Count; ++j)
                {
                    if (i == j || positionsToRemoveAt.Contains(j)) continue;
                    float angleFromB = edgeSegments[j][0].From.Angle;
                    lastSegmentIndex = edgeSegments[j].Count - 1;
                    float angleToB = edgeSegments[j][lastSegmentIndex].To.Angle;

                    if (angleFromB < angleFromA || angleToB > angleToA) continue;
                    positionsToRemoveAt.Add(j);
                }
            }

            positionsToRemoveAt.Sort();
            for (int i = positionsToRemoveAt.Count - 1; i >= 0; --i)
            {
                edgeSegments.RemoveAt(positionsToRemoveAt[i]);
            }

            edgeSegments.ForEach(s =>
            {
                Debug.DrawLine(s[0].From.Position, _position, Color.yellow, 10f);
                Debug.DrawLine(s[s.Count - 1].To.Position, _position, Color.magenta, 10f);
//                s.ForEach(e => e.Draw());
            });

            List<Vector2> meshVertices = new List<Vector2>();
            if (edgeSegments.Count == 0)
            {
                InsertLineSegments(meshVertices, 0, 360);
            }

            //works to here

            List<FLVertex> vertices = new List<FLVertex>();
            List<FLEdge> edges = new List<FLEdge>();

            edgeSegments.ForEach(s => s.ForEach(e =>
            {
                vertices.Add(e.From);
                vertices.Add(e.To);
                edges.Add(e);
            }));
            vertices.Sort((a, b) => a.Angle.CompareTo(b.Angle));

            for (int i = 0; i < vertices.Count; i++)
            {
                FLVertex flVertex = vertices[i];
                Vector2 dirToVertex = (flVertex.Position - _position).normalized;
                Vector2 lineSegmentEnd = _position + dirToVertex * Radius;
                _intersection = GetIntersectionWithEdge(lineSegmentEnd, edges, flVertex);

                //if there is no intersection check to see if the current vertex is at the end of the mesh, if it is, draw a circular area from this vertex to the next vertex to round off the light
                if (_intersection.Item1 == false)
                {
                    if (flVertex.IsEnd)
                    {
                        int nextVertexIndex = Helper.NextIndex(i, vertices);
                        FLVertex nextFlVertex = vertices[nextVertexIndex];
                        Vector2 dirToNextVertex = (nextFlVertex.Position - _position).normalized;
                        Vector2 nextLineSegmentEnd = _position + dirToNextVertex * Radius;
                        meshVertices.Add(flVertex.Position);
                        meshVertices.Add(lineSegmentEnd);
                        InsertLineSegments(meshVertices, flVertex.Angle, nextFlVertex.Angle);
                        meshVertices.Add(nextLineSegmentEnd);
                        meshVertices.Add(nextFlVertex.Position);
                    }
                    else
                    {
                        meshVertices.Add(flVertex.Position);
                    }

                    continue;
                }

                //if these is an intersection, but it is closer than the current vertex, we can ignore it as it will be dealt with later
                if (_intersection.Item3 < flVertex.SqrDistanceToOrigin) continue;
                Vector2 intersectionPoint = _intersection.Item2;
                if (flVertex.PreviousFlVertex == null)
                {
//                    Vector2 mid = (flVertex.Position + intersectionPoint) / 2f;
//                    Debug.DrawLine(flVertex.Position, mid, new Color(0.5f, 0f, 1, 1f), 2f);
//                    Debug.DrawLine(mid, intersectionPoint, new Color(0, 1f, 1, 1f), 2f);          
                    meshVertices.Add(intersectionPoint);
                    meshVertices.Add(flVertex.Position);
                }
                else if (flVertex.NextFlVertex == null)
                {
//                    Vector2 mid = (flVertex.Position + intersectionPoint) / 2f;
//                    Debug.DrawLine(flVertex.Position, mid, new Color(1, 0.5f, 0, 1f), 2f);
//                    Debug.DrawLine(mid, intersectionPoint, new Color(1, 0.5f, 1, 1f), 2f);
                    meshVertices.Add(flVertex.Position);
                    meshVertices.Add(intersectionPoint);
                }
                else
                {
                    meshVertices.Add(flVertex.Position);
                }
            }

            Mesh mesh = _meshFilter.mesh;
            mesh.Clear();
            meshVertices.Insert(0, _position);
            Vector3[] v = new Vector3[meshVertices.Count];
            for (int i = 0; i < meshVertices.Count; ++i)
            {
                Vector3 vert = transform.InverseTransformPoint(meshVertices[i]);
                vert.z = 2;
                v[i] = vert;
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
            List<int> triangles = new List<int>();
            for (int i = 1; i < vertices.Length; ++i)
            {
                int nextTriangle = i + 1 == vertices.Length ? 1 : i + 1;
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(nextTriangle);
            }

            return triangles.ToArray();
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

        private void InsertLineSegments(List<Vector2> meshVertices, float from, float to)
        {
            float angleIncrement = 1f;
            if (to < from) to += 360;
            for (float angle = from + angleIncrement; angle < to - angleIncrement; angle += angleIncrement)
            {
                float x = _position.x + Radius * Mathf.Sin(angle * Mathf.Deg2Rad);
                float y = _position.y + Radius * Mathf.Cos(angle * Mathf.Deg2Rad);
                meshVertices.Add(new Vector2(x, y));
            }
        }

        private void ResizeLight()
        {
            Mesh mesh = _meshFilter.mesh;
            for (int i = 1; i < mesh.vertices.Length; ++i)
            {
                Vector2 vertex = transform.TransformPoint(mesh.vertices[i]);
                Vector2 dir = (_position - vertex).normalized;
                vertex = dir * Radius;
                mesh.vertices[i] = transform.InverseTransformPoint(vertex);
            }
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