using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SamsHelper.Libraries
{
    public class Polygon
    {
        public readonly Vector2 Position;
        public readonly List<Vector2> Vertices;
        public readonly List<Vector2> Edges = new List<Vector2>();
        public Vector2 TopLeft, BottomRight;

        public Polygon(List<Vector2> vertices, Vector2 position)
        {
            Vertices = vertices;
            Position = position;
            BuildEdges();
            SetBoundingCorners();
        }
        
        private void BuildEdges()
        {
            for (int i = 0; i < Vertices.Count; i++) {
                Vector2 p1 = Vertices[i];
                Vector2 p2 = i + 1 == Vertices.Count ? Vertices[0] : Vertices[i + 1];
                Edges.Add(p2 - p1);
            }
        }
        
        private void SetBoundingCorners()
        {
            Tuple<Vector3, Vector3> boundingCorners = AdvancedMaths.GetBoundingCornersOfPolygon(Vertices);
            TopLeft = boundingCorners.Item1;
            BottomRight = boundingCorners.Item2;
            TopLeft += Position;
            BottomRight += Position;
        }
    }
}