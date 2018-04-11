﻿using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Xml;
using Facilitating.Persistence;
using NUnit.Framework;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Barrier : IPersistenceTemplate
    {
        private readonly Vector3[] Vertices;
        public readonly List<Vector2> WorldVerts = new List<Vector2>();
        public PolygonCollider2D Collider;
        private GameObject _barrierObject;
        private static GameObject _barrierPrefab;
        private readonly string _barrierName;
        private readonly float _rotation;
        private readonly Vector2 Position;
        private static Transform _barrierParent;

        public void Load(XmlNode doc, PersistenceType saveType)
        {
        }

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            string vertexString = "";
            for (int i = 0; i < Vertices.Length; ++i)
            {
                vertexString += Helper.VectorToString(Vertices[i]);
                if (i == Vertices.Length - 1) break;
                vertexString += ",";
            }
            SaveController.CreateNodeAndAppend("Name", doc, _barrierName);
            SaveController.CreateNodeAndAppend("Rotation", doc, _rotation);
            SaveController.CreateNodeAndAppend("Position", doc, Helper.VectorToString(Position));
            SaveController.CreateNodeAndAppend("Vertices", doc, vertexString);
            return doc;
        }
        
        public Barrier(Vector3[] vertices, string barrierName, Vector2 position)
        {
            _barrierName = barrierName;
            Vertices = vertices;
            _rotation = Random.RandomRange(0, 360);
            Position = position;
        }

        public void CreateObject()
        {
            Assert.IsNull(_barrierObject);
            if (_barrierPrefab == null) _barrierPrefab = Resources.Load<GameObject>("Prefabs/Combat/Basic Barrier");
            if (_barrierParent == null) _barrierParent = GameObject.Find("Barriers").transform;
            _barrierObject = GameObject.Instantiate(_barrierPrefab);
            _barrierObject.AddComponent<BarrierBehaviour>().SetBarrier(this);
            _barrierObject.transform.SetParent(_barrierParent);
            _barrierObject.layer = 8;
            _barrierObject.name = _barrierName;
            _barrierObject.tag = "Barrier";
            _barrierObject.transform.localScale = Vector2.one;
            _barrierObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, _rotation));
            _barrierObject.transform.position = Position;
            WorldVerts.Clear();
            Collider = _barrierObject.GetComponent<PolygonCollider2D>();
            foreach (Vector2 colliderPoint in Collider.points) WorldVerts.Add(_barrierObject.transform.TransformPoint(colliderPoint));
            CreateMesh();
            AddCollider();
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

        private void CreateMesh()
        {
            _barrierObject.GetComponent<MeshRenderer>();
            Mesh mesh = new Mesh();
            _barrierObject.GetComponent<MeshFilter>().mesh = mesh;
            mesh.vertices = Vertices;
            mesh.triangles = Triangulator.Triangulate(Vertices);
            Vector3[] normals = new Vector3[Vertices.Length];
            for (int i = 0; i < normals.Length; i++) normals[i] = -Vector3.forward;
            mesh.normals = normals;
        }

        private void AddCollider()
        {
            Vector2[] colliderPath = new Vector2[Vertices.Length];
            Vector3[] verts = Vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector2 point = verts[i];
                colliderPath[i] = point;
            }

            Collider.SetPath(0, colliderPath);
        }

    }
}