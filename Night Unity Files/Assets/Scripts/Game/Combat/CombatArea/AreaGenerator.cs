using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat
{
    public class AreaGenerator : MonoBehaviour
    {
        private const float SmallPolyWidth = 0.4f;
        private static AreaGenerator _instance;
        private static int _barrierNumber;
        private static GameObject _barrierPrefab;
        private static readonly List<Shape> _barriers = new List<Shape>();

        public void Awake()
        {
            _instance = this;
        }

        private class Ellipse
        {
            public readonly float InnerRingWidth, InnerRingHeight, OuterRingWidth, OuterRingHeight;
            public bool IsCircle;

            public Ellipse(float innerRingWidth, float innerRingHeight, float outerRingWidth, float outerRingHeight)
            {
                InnerRingWidth = innerRingWidth;
                InnerRingHeight = innerRingHeight;
                OuterRingWidth = outerRingWidth;
                OuterRingHeight = outerRingHeight;
            }

            public Ellipse(float innerRadius, float outerRadius) : this(innerRadius, innerRadius, outerRadius, outerRadius)
            {
                IsCircle = true;
            }
        }

        private static Shape GenerateDistortedPoly(int definition, Ellipse ellipse, GameObject o)
        {
            int angleIncrement;
            List<Vector3> polyVertices = new List<Vector3>();
            for (int i = 0; i < 360; i += angleIncrement)
            {
                Vector3 vertex = RandomPointBetweenRadii(i, ellipse);
                polyVertices.Add(vertex);
                angleIncrement = Random.Range(definition / 2, definition);
            }

            Shape newPoly = new Shape(CreateMesh(polyVertices, o), polyVertices);
            return newPoly;
        }

        private static GameObject CreateMesh(List<Vector3> vertices, GameObject newShape)
        {
            newShape.GetComponent<MeshRenderer>();
            Mesh mesh = new Mesh();
            newShape.GetComponent<MeshFilter>().mesh = mesh;
            mesh.vertices = vertices.ToArray();
            mesh.triangles = Triangulator.Triangulate(mesh.vertices);
            Vector3[] normals = new Vector3[vertices.Count];
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = -Vector3.forward;
            }

            mesh.normals = normals;
            return newShape;
        }

        private static Vector2 RandomPointBetweenRadii(float angle, Ellipse e)
        {
            Vector2 randomPoint;
            angle *= Mathf.Deg2Rad;
            if (e.IsCircle)
            {
                randomPoint = new Vector2();
                float pointRadius = Random.Range(e.InnerRingWidth, e.OuterRingWidth);
                randomPoint.x = pointRadius * Mathf.Cos(angle);
                randomPoint.y = pointRadius * Mathf.Sin(angle);
                return randomPoint;
            }

            Vector2 innerRadiusPoint = new Vector2();
            innerRadiusPoint.x = e.InnerRingWidth * Mathf.Cos(angle);
            innerRadiusPoint.y = e.InnerRingHeight * Mathf.Sin(angle);
            Vector2 outerRadiusPoint = new Vector2();
            outerRadiusPoint.x = e.OuterRingWidth * Mathf.Cos(angle);
            outerRadiusPoint.y = e.OuterRingHeight * Mathf.Sin(angle);

            randomPoint = outerRadiusPoint - innerRadiusPoint;
            randomPoint *= Random.Range(0f, 1f);
            randomPoint += innerRadiusPoint;
            return randomPoint;
        }

        private static GameObject GenerateBasicBarrier()
        {
            if (_barrierPrefab == null) _barrierPrefab = Resources.Load<GameObject>("Prefabs/Combat/Basic Barrier");
            GameObject basicBarrier = Instantiate(_barrierPrefab);
            basicBarrier.transform.SetParent(_instance.transform);
            basicBarrier.layer = 8;
            basicBarrier.name = "Barrier " + _barrierNumber;
            ++_barrierNumber;
            basicBarrier.tag = "Barrier";
            return basicBarrier;
        }

        private static Shape GenerateSmallPoly()
        {
            GameObject g = GenerateBasicBarrier();
            float width = SmallPolyWidth * Random.Range(0.6f, 1f);
            float radius = width / 2f;
            float minX = Random.Range(0, radius * 0.9f);
            float minY = Random.Range(0, radius * 0.9f);
            float maxX = Random.Range(minX, radius);
            float maxY = Random.Range(minY, radius);
//            Ellipse e = new Ellipse(minX, minY, maxX, maxY);
            Ellipse e = new Ellipse(radius * 0.8f, radius);
            Shape shape = GenerateDistortedPoly(50, e, g);
            Vector2[] colliderPath = new Vector2[shape.Vertices.Count];
            Vector3[] verts = shape.Vertices.ToArray();
            for (int i = 0; i < verts.Length; i++)
            {
                Vector2 point = verts[i];
                colliderPath[i] = point;
            }

            shape.Collider.SetPath(0, colliderPath);
            g.transform.localScale = Vector2.one;
            g.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
            return shape;
        }

        public static List<Shape> GenerateArea()
        {
            _barrierNumber = 0;
            _barriers.Clear();
            for (int i = 0; i < 100; ++i)
            {
                float rx = Random.Range(-8f, 8f);
                float ry = Random.Range(-8f, 8f);

                Shape shape = GenerateSmallPoly();
                _barriers.Add(shape);
                shape.SetPosition(rx, ry);
            }
            return _barriers;
        }

        public class Shape
        {
            public readonly List<Vector3> Vertices;
            public GameObject ShapeObject;
            public PolygonCollider2D Collider;
            public readonly List<Vector2> WorldVerts = new List<Vector2>();
            public readonly List<Cell> OccupiedCells = new List<Cell>();

            public Shape(GameObject shapeObject, List<Vector3> vertices)
            {
                ShapeObject = shapeObject;
                Vertices = vertices;
                Collider = shapeObject.GetComponent<PolygonCollider2D>();
            }

            public void SetPosition(float rx, float ry)
            {
                ShapeObject.transform.position = new Vector2(rx, ry);
                foreach (Vector2 colliderPoint in Collider.points)
                {
                    WorldVerts.Add(ShapeObject.transform.TransformPoint(colliderPoint));
                }
            }
        }
    }
}