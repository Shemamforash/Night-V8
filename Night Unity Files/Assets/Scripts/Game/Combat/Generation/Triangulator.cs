﻿using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using UnityEngine;
using Polygon = TriangleNet.Geometry.Polygon;

//triangulation code from https://github.com/Ranguna/Triangle-NET-Unity-Port

namespace Game.Combat.Generation
{
    public static class Triangulator
    {
        private static List<Vector3> m_points;

        public static int[] Triangulate(Vector3[] points)
        {
            Polygon p = new Polygon();
            List<Vertex> verts = points.Select(vector3 => new Vertex(vector3.x, vector3.y)).ToList();
            p.Add(new Contour(verts));
            
            IMesh m = p.Triangulate();
            int[] triangles = new int[m.Triangles.Count * 3];
            int current = 0;
            foreach (Triangle mTriangle in m.Triangles)
            {
                triangles[current] = mTriangle.GetVertexID(0);
                triangles[current + 1] = mTriangle.GetVertexID(2);
                triangles[current + 2] = mTriangle.GetVertexID(1);
                current += 3;
            }
            return triangles;
        }
    }
}