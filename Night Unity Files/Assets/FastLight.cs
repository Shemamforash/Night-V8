using System;
using System.Collections.Generic;
using DefaultNamespace;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using UnityEngine;

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
        position.z = 2;
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

    private class Edge
    {
        public Vertex From;
        public Vertex To;

        public bool BelongsToEdge(Vertex v)
        {
            return v.Edge == this;
        }

        public void SetVertices(Vertex a, Vertex b, Vector2 origin)
        {
            Vector2 midPoint = (a.Position + b.Position) / 2f;
            float dot = AdvancedMaths.Dot(origin, midPoint, a.Position);
            if (dot < 0)
            {
                From = a;
                To = b;
            }
            else
            {
                To = a;
                From = b;
            }
        }
    }

   

    private class Vertex
    {
        public readonly Vector2 Position;
        public readonly float Distance;
        public readonly Edge Edge;
        public readonly float Angle;

        public Vertex(Vector2 position, float distance, Edge e, float angle)
        {
            Position = position;
            Distance = distance;
            Edge = e;
            Angle = angle;
        }
    }

    private Vector2 intersectionPoint;

    private Tuple<bool, Vector2, float> GetIntersectionWithEdge(Vector2 end, List<Edge> edges, Vertex vertex)
    {
        Vector2 nearestIntersection = Vector2.negativeInfinity;
        float nearestDistance = float.MaxValue;
        bool intersectionExists = false;
        edges.ForEach(e =>
        {
            if (e.BelongsToEdge(vertex)) return;
            if (!AdvancedMaths.LineIntersection(e.From.Position, e.To.Position, _position, end, out intersectionPoint)) return;
            float distance = Vector2.Distance(_position, intersectionPoint);
            if (distance >= nearestDistance) return;
            nearestDistance = distance;
            nearestIntersection = intersectionPoint;
            intersectionExists = true;
        });
        return new Tuple<bool, Vector2, float>(intersectionExists, nearestIntersection, nearestDistance);
    }

    private void DrawLight()
    {
        List<Vertex> vertices = new List<Vertex>();
        List<Edge> edges = new List<Edge>();
        _allObstructors.ForEach(o =>
        {
            float maxRadius = o.mesh.bounds.max.magnitude;
            float distanceToMesh = Vector2.Distance(o.transform.position, _position);
            if (distanceToMesh - maxRadius > Radius) return;
            List<Vector2> widestPoint = o.GetVisibleVertices(_position, Radius);
            Edge e = new Edge();
            float aDistance = Vector2.Distance(_position, widestPoint[0]);
            float bDistance = Vector2.Distance(_position, widestPoint[1]);
            if (aDistance > Radius && bDistance > Radius) return;
            Vertex a = new Vertex(widestPoint[0], aDistance, e, 360 - AdvancedMaths.AngleFromUp(_position, widestPoint[0]));
            Vertex b = new Vertex(widestPoint[1], bDistance, e, 360 - AdvancedMaths.AngleFromUp(_position, widestPoint[1]));
            e.SetVertices(a, b, _position);
            edges.Add(e);
            vertices.Add(a);
            vertices.Add(b);
        });
        vertices.Sort((a, b) => a.Angle.CompareTo(b.Angle));
        List<Vector2> meshVertices = new List<Vector2>();

        if (vertices.Count == 0)
        {
            InsertLineSegments(meshVertices, 0, 360);
        }

        for (int i = 0; i < vertices.Count; i++)
        {
            Vertex vertex = vertices[i];
            Vector2 dirToVertex = (vertex.Position - _position).normalized;
            Vector2 lineSegmentEnd = _position + dirToVertex * Radius;
            Tuple<bool, Vector2, float> intersection = GetIntersectionWithEdge(lineSegmentEnd, edges, vertex);
            if (intersection.Item1 == false)
            {
                if (vertex.Edge.To == vertex)
                {
                    Vertex nextVertex = i + 1 == vertices.Count ? vertices[0] : vertices[i + 1];
                    Vector2 dirToNextVertex = (nextVertex.Position - _position).normalized;
                    Vector2 nextLineSegmentEnd = _position + dirToNextVertex * Radius;
                    meshVertices.Add(vertex.Position);
                    meshVertices.Add(lineSegmentEnd);
                    InsertLineSegments(meshVertices, vertex.Angle, nextVertex.Angle);
                    meshVertices.Add(nextLineSegmentEnd);
                    meshVertices.Add(nextVertex.Position);
                }

                continue;
            }

            if (intersection.Item3 < vertex.Distance) continue;
            if (vertex.Edge.From == vertex)
            {
                meshVertices.Add(intersection.Item2);
                meshVertices.Add(vertex.Position);
            }
            else
            {
                meshVertices.Add(vertex.Position);
                meshVertices.Add(intersection.Item2);
            }
        }

        Mesh mesh = _meshFilter.mesh;
        mesh.Clear();
        Vector3[] v = new Vector3[meshVertices.Count];
        for (int i = 0; i < meshVertices.Count; ++i)
        {
            Vector3 vert = transform.InverseTransformPoint(meshVertices[i]);
            vert.z = -1;
            v[i] = vert;
        }

        mesh.vertices = v;
        mesh.triangles = Triangulator.Triangulate(v);
        Vector3[] normals = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < normals.Length; i++) normals[i] = Vector2.up;
        mesh.normals = normals;
        mesh.uv = CalculateUvs(mesh.vertices);
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
        for (int i = 0; i < mesh.vertices.Length; ++i)
        {
            Vector2 vertex = transform.TransformPoint(mesh.vertices[i]);
            Vector2 dir = (_position - vertex).normalized;
            vertex = dir * Radius;
            mesh.vertices[i] = transform.InverseTransformPoint(vertex);
        }
    }

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
        UpdateColour();
        _needsUpdate = false;
        _lastRadius = Radius;
    }

    private void UpdateColour()
    {
        if (_lastColour == Colour) return;
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
        Vector2 newPosition = new Vector2(transform.position.x, transform.position.y);
        UpdateLight(newPosition);
        UpdateColour();
    }
}