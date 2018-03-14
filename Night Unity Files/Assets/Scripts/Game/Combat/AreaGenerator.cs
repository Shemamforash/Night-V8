using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public class AreaGenerator : MonoBehaviour
    {
        private const int Radius = 40;
        private const float Cutoff = 0.5f;
        private static AreaPoint[,] validPoints;
        private List<Shape> _shapes = new List<Shape>();

        private class AreaPoint
        {
            public Vector2 position;
            public Shape OwnedShape = null;
            public bool IsValid = false;
            public bool OutOfBounds = false;

            public void ConstructShape(Shape currentShape, int xIndex, int yIndex)
            {
                OwnedShape = currentShape;
                if (OutOfBounds) OwnedShape.Tainted = true;
                OwnedShape.AddPoint(position);
                for (int x = xIndex - 1; x <= xIndex + 1; ++x)
                {
                    if (x < 0 || x >= Radius * 2) continue;
                    for (int y = yIndex - 1; y <= yIndex + 1; ++y)
                    {
                        if (y < 0 || y >= Radius * 2) continue;
                        AreaPoint neighbor = validPoints[x, y];
                        if (neighbor.IsValid && neighbor.OwnedShape == null) neighbor.ConstructShape(currentShape, x, y);
                    }
                }
            }
        }

        public void Awake()
        {
            PerlinNoise.Generate(0.5f, 1.75f, 1, 5);
            validPoints = new AreaPoint[Radius * 2, Radius * 2];
            for (int x = 0; x < Radius * 2; ++x)
            {
                for (int y = 0; y < Radius * 2; ++y)
                {
                    float xPos = (x - Radius) / 10f;
                    float yPos = (y - Radius) / 10f;
                    float noiseValue = (float) PerlinNoise.GetValue(xPos, yPos);
                    bool valid = noiseValue > Cutoff;
                    bool outOfBounds = x == 0 || x == Radius * 2 - 1 || y == 0 || y == Radius * 2 - 1;
                    AreaPoint p = new AreaPoint();
                    p.OutOfBounds = outOfBounds;
                    p.IsValid = valid;
                    p.position = new Vector2(xPos, yPos);
                    validPoints[x, y] = p;
                }
            }

            Shape currentShape;
            for (int x = 0; x < Radius * 2; ++x)
            {
                for (int y = 0; y < Radius * 2; ++y)
                {
                    if (validPoints[x, y].IsValid && validPoints[x, y].OwnedShape == null)
                    {
                        currentShape = new Shape();
                        _shapes.Add(currentShape);
                        validPoints[x, y].ConstructShape(currentShape, x, y);
                    }
                }
            }

            Debug.Log(_shapes.Count);
        }

        private class Shape
        {
            public readonly List<Vector2> _points = new List<Vector2>();
            public readonly Color Color;
            public bool Tainted;

            public Shape()
            {
                Color = Random.ColorHSV(0.5f, 1, 1, 1);
            }
            
            public void AddPoint(Vector2 point)
            {
                _points.Add(point);
            }
        }

        public void OnDrawGizmos()
        {
            _shapes.ForEach(s =>
            {
                if (s.Tainted) return;
                Gizmos.color = s.Color;
                s._points.ForEach(p => { Gizmos.DrawCube(p, new Vector3(0.1f, 0.1f, 0.1f)); });
            });
        }
    }
}