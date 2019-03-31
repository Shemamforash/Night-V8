using System.Collections.Generic;
using System.Linq;
using FastLights;
using NUnit.Framework;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Barrier : Polygon
    {
        public PolygonCollider2D Collider;
        private GameObject _barrierObject;
        private static GameObject _barrierPrefab;
        public readonly string Name;
        private static Transform _barrierParent;

        public Barrier(List<Vector2> vertices, string name, Vector2 position, List<Barrier> barriers) : base(vertices, position)
        {
            if (position == Vector2.negativeInfinity) Debug.Log("wat!?");
            Name = name;
            if (!WorldGrid.AddBarrier(this)) return;
            barriers.Add(this);
        }

        public void CreateObject()
        {
            Assert.IsNull(_barrierObject);
            if (_barrierPrefab == null) _barrierPrefab = Resources.Load<GameObject>("Prefabs/Combat/Basic Barrier");
            if (_barrierParent == null) _barrierParent = GameObject.Find("Barriers").transform;
            WorldGrid.AddBarrier(this);
            _barrierObject = GameObject.Instantiate(_barrierPrefab, _barrierParent, true);
            _barrierObject.AddComponent<BarrierBehaviour>().SetBarrier(this);
            _barrierObject.layer = 8;
            _barrierObject.name = Name;
            _barrierObject.tag = "Barrier";
            _barrierObject.transform.localScale = Vector2.one;
            _barrierObject.transform.position = Position;
            Material material = new Material(Shader.Find("Sprites/Mask"));
            material.color = Color.green;
            _barrierObject.GetComponent<Renderer>().material = material;
            Collider = _barrierObject.GetComponent<PolygonCollider2D>();
            Vector3[] meshVerts = CreateMesh();
            if (meshVerts == null)
            {
                WorldGrid.RemoveBarrier(this);
                GameObject.Destroy(_barrierObject);
                return;
            }

            AddCollider(meshVerts);
        }

        private class BarrierBehaviour : MonoBehaviour
        {
            private Barrier _barrier;

            public void SetBarrier(Barrier b)
            {
                _barrier = b;
            }

            private void OnDestroy()
            {
                _barrierParent = null;
                _barrier._barrierObject = null;
            }
        }

        private Vector3[] CreateMesh()
        {
            Mesh mesh = _barrierObject.GetComponent<MeshFilter>().mesh;
            Triangulator.Triangulate(Vertices, ref mesh);
            int vertCount = mesh.vertices.Length;

            Vector3[] normals = new Vector3[vertCount];
            for (int i = 0; i < normals.Length; i++) normals[i] = -Vector3.forward;
            mesh.normals = normals;
            _barrierObject.GetComponent<LightObstructor>().UpdateMesh();
//            Draw();
            return mesh.vertices;
        }

        private void AddCollider(Vector3[] meshVerts)
        {
            Vector2[] colliderPath = new Vector2[meshVerts.Length];
            for (int i = 0; i < meshVerts.Length; i++)
            {
                Vector2 point = meshVerts[i];
                colliderPath[i] = point;
            }

            Collider.SetPath(0, colliderPath);
        }
    }
}