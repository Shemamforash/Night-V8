using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SamsHelper.Libraries
{
    public class Polygon
    {
        public Vector2 Position;
        public readonly List<Vector2> Vertices;
        public Vector2 TopLeft, BottomRight;

        public Polygon(List<Vector2> vertices, Vector2 position)
        {
            Vertices = vertices;
            Position = position;
            SetBoundingCorners();
        }

        protected void SetBoundingCorners()
        {
            Tuple<Vector3, Vector3> boundingCorners = AdvancedMaths.GetBoundingCornersOfPolygon(Vertices);
            TopLeft = boundingCorners.Item1;
            BottomRight = boundingCorners.Item2;
            TopLeft += Position;
            BottomRight += Position;
        }

        public void Draw()
        {
            for (int i = 0; i < Vertices.Count; ++i)
            {
                int next = i + 1 == Vertices.Count ? 0 : i + 1;
                float pos = (float)i / Vertices.Count;
                Debug.DrawLine(Vertices[i] + Position, Vertices[next] + Position, Color.Lerp(Color.yellow, Color.cyan, pos), 20f);
            }
        }
    }
}