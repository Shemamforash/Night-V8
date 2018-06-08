﻿using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using FastLights;
using NUnit.Framework;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Generation
{
    public class Barrier : Polygon, IPersistenceTemplate
    {
        public PolygonCollider2D Collider;
        private GameObject _barrierObject;
        private static GameObject _barrierPrefab;
        private readonly string _barrierName;
        private static Transform _barrierParent;

        public void Load(XmlNode doc, PersistenceType saveType)
        {
        }

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            XmlNode barrierNode = SaveController.CreateNodeAndAppend("Barrier", doc);
            string vertexString = "";
            for (int i = 0; i < Vertices.Count; ++i)
            {
                vertexString += Helper.VectorToString(Vertices[i]);
                if (i == Vertices.Count - 1) break;
                vertexString += ",";
            }

            SaveController.CreateNodeAndAppend("Name", barrierNode, _barrierName);
            SaveController.CreateNodeAndAppend("Position", barrierNode, Helper.VectorToString(Position));
            SaveController.CreateNodeAndAppend("Vertices", barrierNode, vertexString);
            return barrierNode;
        }

        public Barrier(List<Vector2> vertices, string barrierName, Vector2 position, bool forcePlace = false) : base(vertices, position)
        {
            if (position == Vector2.negativeInfinity) Debug.Log("wat!?");
            _barrierName = barrierName;
            Valid = PathingGrid.AddBarrier(this, forcePlace);
        }

        public readonly bool Valid;

        public void CreateObject()
        {
            Assert.IsNull(_barrierObject);
            if (_barrierPrefab == null) _barrierPrefab = Resources.Load<GameObject>("Prefabs/Combat/Basic Barrier");
            if (_barrierParent == null) _barrierParent = GameObject.Find("Barriers").transform;
            PathingGrid.AddBarrier(this, true);
            _barrierObject = GameObject.Instantiate(_barrierPrefab);
            _barrierObject.AddComponent<BarrierBehaviour>().SetBarrier(this);
            _barrierObject.transform.SetParent(_barrierParent);
            _barrierObject.layer = 8;
            _barrierObject.name = _barrierName;
            _barrierObject.tag = "Barrier";
            _barrierObject.transform.localScale = Vector2.one;
            _barrierObject.transform.position = Position;
            Collider = _barrierObject.GetComponent<PolygonCollider2D>();
            Vector3[] meshVerts = CreateMesh();
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
            Vector3[] meshVerts = new Vector3[Vertices.Count];
            for (int i = 0; i < Vertices.Count; ++i) meshVerts[i] = Vertices[i];
            mesh.vertices = meshVerts;
            mesh.triangles = Triangulator.Triangulate(meshVerts);
            Vector3[] normals = new Vector3[meshVerts.Length];
            for (int i = 0; i < normals.Length; i++) normals[i] = -Vector3.forward;
            mesh.normals = normals;
            _barrierObject.GetComponent<LightObstructor>().UpdateMesh();
//            Draw();
            return meshVerts;
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