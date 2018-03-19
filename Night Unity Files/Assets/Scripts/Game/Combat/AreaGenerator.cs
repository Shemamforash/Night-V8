using System;
using System.Collections.Generic;
using SamsHelper;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat
{
    public class AreaGenerator : MonoBehaviour
    {
        private const int SmallPolyWidth = 2;
        private const int Resolution = 400;

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

        private static Shape GenerateDistortedPoly(int definition, int size, Ellipse ellipse)
        {
            int halfSize = size / 2;
            int angleIncrement;
            List<Vector2> polyVertices = new List<Vector2>();
            for (int i = 0; i < 360; i += angleIncrement)
            {
                Vector2 vertex = RandomPointBetweenRadii(i, ellipse, size);
                vertex.x = (int) vertex.x + halfSize;
                vertex.y = (int) vertex.y + halfSize;
                polyVertices.Add(vertex);
                angleIncrement = Random.Range(definition / 2, definition);
            }

            Shape newPoly = new Shape(CreateTextureFromPoly(polyVertices, size), polyVertices);
            return newPoly;
        }

        private static Vector2 RandomPointBetweenRadii(float angle, Ellipse e, int size)
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


        private int _barrierNumber;

        private GameObject GenerateBasicBarrier()
        {
            GameObject g = new GameObject();
            g.layer = 8;
            g.name = "Barrier " + _barrierNumber;
            ++_barrierNumber;
            g.tag = "Barrier";
            g.AddComponent<PolygonCollider2D>();
            g.AddComponent<SpriteRenderer>();
            return g;
        }

        private Shape GenerateSmallPoly()
        {
            GameObject g = GenerateBasicBarrier();
            int width = (int) ((float) Resolution / SmallPolyWidth * Random.Range(0.6f, 1f));
            if (width % 2 != 0) ++width;
            int radius = width / 2;
            int minX = (int) Random.Range(0, radius * 0.9f);
            int minY = (int) Random.Range(0, radius * 0.9f);
            int maxX = Random.Range(minX, radius);
            int maxY = Random.Range(minY, radius);
//            Ellipse e = new Ellipse(minX, minY, maxX, maxY);
            Ellipse e = new Ellipse(radius * 0.8f, radius);
            Shape shape = GenerateDistortedPoly(50, width, e);
            Vector2[] colliderPath = shape.Vertices.ToArray();
            for (int i = 0; i < colliderPath.Length; i++)
            {
                Vector2 point = colliderPath[i];
                point.x -= width / 2f;
                point.y -= width / 2f;
                point /= (float) Resolution;
                colliderPath[i] = point;
            }

            g.GetComponent<PolygonCollider2D>().SetPath(0, colliderPath);
            Sprite sprite = Sprite.Create(shape.Tex, new Rect(0.0f, 0.0f, width, width), new Vector2(0.5f, 0.5f), Resolution);
            g.GetComponent<SpriteRenderer>().sprite = sprite;
            g.transform.localScale = Vector2.one;
            g.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
            shape.SetObject(g);
            return shape;
        }

        private List<Shape> _barriers = new List<Shape>();

        public void Start()
        {
            for (int i = 0; i < 20; ++i)
            {
                float rx = Random.Range(-8f, 8f);
                float ry = Random.Range(-8f, 8f);

                Shape shape = GenerateSmallPoly();
                _barriers.Add(shape);
                shape.SetPosition(rx, ry);
            }

            PathingGrid.Instance().SetShapes(_barriers);
        }

        private static Texture2D CreateTextureFromPoly(List<Vector2> vertices, int size)
        {
            Texture2D texture = new Texture2D(size, size);
            for (int x = 0; x < texture.width; ++x)
            {
                for (int y = 0; y < texture.height; ++y)
                {
                    texture.SetPixel(x, y, AdvancedMaths.IsPointInPolygon(new Vector2(x, y), vertices) ? Color.black : new Color(0f, 0f, 0f, 0f));
                }
            }

            for (int i = 1; i < vertices.Count; ++i)
            {
                Line.Draw(texture, vertices[i - 1], vertices[i]);
            }

            Line.Draw(texture, vertices[vertices.Count - 1], vertices[0]);
            texture.alphaIsTransparency = true;
            texture.Apply();
            return texture;
        }

        public class Shape
        {
            public readonly Texture2D Tex;
            public readonly List<Vector2> Vertices;
            public GameObject ShapeObject;
            public PolygonCollider2D Collider;
            public List<Vector2> WorldVerts = new List<Vector2>();
            public List<PathingGrid.Cell> OccupiedCells = new List<PathingGrid.Cell>();

            public Shape(Texture2D tex, List<Vector2> vertices)
            {
                Tex = tex;
                Vertices = vertices;
            }

            public void SetObject(GameObject shapeObject)
            {
                ShapeObject = shapeObject;
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

        private static class Line
        {
            private static void Plot(Texture2D bitmap, float x, float y, float alpha)
            {
                alpha = Mathf.Clamp(alpha, 0f, 1f);
                Color color = new Color(0f, 0f, 0f, alpha);
                if ((int) x < 0 || (int) x >= bitmap.width) return;
                if ((int) y < 0 || (int) y >= bitmap.height) return;
                if (bitmap.GetPixel((int) x, (int) y).a == 1) return;
                bitmap.SetPixel((int) x, (int) y, color);
            }

            private static int IPart(float x)
            {
                return Mathf.FloorToInt(x);
            }

            private static int Round(float x)
            {
                return IPart(x + 0.5f);
            }

            private static float FPart(float x)
            {
                return x - Mathf.Floor(x);
            }

            private static float RfPart(float x)
            {
                return 1f - FPart(x);
            }

            public static void Draw(Texture2D image, Vector2 from, Vector2 to)
            {
                bool steep = Math.Abs(to.y - from.y) > Math.Abs(to.x - from.x);
                float temp;
                if (steep)
                {
                    temp = from.x;
                    from.x = from.y;
                    from.y = temp;
                    temp = to.x;
                    to.x = to.y;
                    to.y = temp;
                }

                if (from.x > to.x)
                {
                    temp = from.x;
                    from.x = to.x;
                    to.x = temp;
                    temp = from.y;
                    from.y = to.y;
                    to.y = temp;
                }

                float dx = to.x - from.x;
                float dy = to.y - from.y;
                float gradient = dy / dx;

                if (dx == 0) gradient = 1;

                float xEnd = Round(from.x);
                float yEnd = from.y + gradient * (xEnd - from.x);
                float xGap = RfPart(from.x + 0.5f);
                float xPixel1 = xEnd;
                float yPixel1 = IPart(yEnd);

                if (steep)
                {
                    Plot(image, yPixel1, xPixel1, RfPart(yEnd) * xGap);
                    Plot(image, yPixel1 + 1, xPixel1, FPart(yEnd) * xGap);
                }
                else
                {
                    Plot(image, xPixel1, yPixel1, RfPart(yEnd) * xGap);
                    Plot(image, xPixel1, yPixel1 + 1, FPart(yEnd) * xGap);
                }

                float intery = yEnd + gradient;

                xEnd = Round(to.x);
                yEnd = to.y + gradient * (xEnd - to.x);
                xGap = FPart(to.x + 0.5f);
                float xPixel2 = xEnd;
                float yPixel2 = IPart(yEnd);
                if (steep)
                {
                    Plot(image, yPixel2, xPixel2, RfPart(yEnd) * xGap);
                    Plot(image, yPixel2 + 1f, xPixel2, FPart(yEnd) * xGap);
                }
                else
                {
                    Plot(image, xPixel2, yPixel2, RfPart(yEnd) * xGap);
                    Plot(image, xPixel2, yPixel2 + 1f, FPart(yEnd) * xGap);
                }

                if (steep)
                {
                    for (int x = (int) (xPixel1 + 1f); x <= xPixel2 - 1; x++)
                    {
                        Plot(image, IPart(intery), x, RfPart(intery));
                        Plot(image, IPart(intery) + 1f, x, FPart(intery));
                        intery += gradient;
                    }
                }
                else
                {
                    for (int x = (int) (xPixel1 + 1f); x <= xPixel2 - 1; x++)
                    {
                        Plot(image, x, IPart(intery), RfPart(intery));
                        Plot(image, x, IPart(intery) + 1f, FPart(intery));
                        intery += gradient;
                    }
                }
            }
        }
    }
}