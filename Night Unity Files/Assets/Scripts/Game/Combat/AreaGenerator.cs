using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat
{
    public class AreaGenerator : MonoBehaviour
    {
        private List<Vector2> polyVertices = new List<Vector2>();

        private Texture2D GenerateDistortedPoly(int definition)
        {
            int innerCircleRadius = Random.Range(100, 200);
            int outerCircleRadius = Random.Range(300, 500);
            int angleIncrement;
            for (int i = 0; i < 360; i += angleIncrement)
            {
                float pointRadius = Random.Range(innerCircleRadius, outerCircleRadius);
                int xPos = (int) (pointRadius * Mathf.Cos(i * Mathf.Deg2Rad)) + 500;
                int yPos = (int) (pointRadius * Mathf.Sin(i * Mathf.Deg2Rad)) + 500;
                polyVertices.Add(new Vector2(xPos, yPos));
                angleIncrement = Random.Range(definition / 2, definition);
            }

            return CreateTextureFromPoly(polyVertices);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            for (int i = 1; i < polyVertices.Count; ++i)
            {
                Gizmos.DrawLine(polyVertices[i - 1] / 100f, polyVertices[i] / 100f);
            }

            Gizmos.DrawLine(polyVertices[polyVertices.Count - 1] / 100f, polyVertices[0] / 100f);
        }

        public static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
        {
            int polygonLength = polygon.Count, i = 0;
            bool inside = false;
            // x, y for tested point.
            float pointX = point.x, pointY = point.y;
            // start / end point for the current polygon segment.
            Vector2 endPoint = polygon[polygonLength - 1];
            float endX = endPoint.x;
            float endY = endPoint.y;
            while (i < polygonLength)
            {
                float startX = endX;
                float startY = endY;
                endPoint = polygon[i++];
                endX = endPoint.x;
                endY = endPoint.y;
                //
                inside ^= endY > pointY ^ startY > pointY /* ? pointY inside [startY;endY] segment ? */
                          && /* if so, test if it is under the segment */
                          pointX - endX < (pointY - endY) * (startX - endX) / (startY - endY);
            }

            return inside;
        }

        public void Start()
        {
            GameObject g = new GameObject();
//            Rigidbody2D rb = g.AddComponent<Rigidbody2D>();
//            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            g.tag = "Barrier";
            g.transform.position = new Vector2(2,2);
            SpriteRenderer sr = g.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.9f, 0.9f, 0.9f, 1.0f);

            transform.position = new Vector3(1.5f, 1.5f, 0.0f);
            Texture2D tex = GenerateDistortedPoly(50);
            Sprite mySprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            sr.sprite = mySprite;

            PolygonCollider2D polyCol = g.AddComponent<PolygonCollider2D>();
            Vector2[] colliderPath = polyVertices.ToArray();
            for (int i = 0; i < colliderPath.Length; i++)
            {
                Vector2 point = colliderPath[i];
                point.x -= 500;
                point.y -= 500;
                point /= 100f;
                colliderPath[i] = point;
            }

            polyCol.SetPath(0, colliderPath);
            g.transform.localScale = new Vector2(0.4f, 0.4f);
        }

        private static Texture2D CreateTextureFromPoly(List<Vector2> vertices)
        {
            Texture2D texture = new Texture2D(1000, 1000);
            for (int x = 0; x < texture.width; ++x)
            {
                for (int y = 0; y < texture.height; ++y)
                {
                    texture.SetPixel(x, y, IsPointInPolygon(new Vector2(x, y), vertices) ? Color.white : new Color(1f, 1f, 1f, 0f));
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

        private static class Line
        {
            private static void Plot(Texture2D bitmap, float x, float y, float alpha)
            {
                alpha = Mathf.Clamp(alpha, 0f, 1f);
                Color color = new Color(1f, 1f, 1f, alpha);
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
//                if (x < 0) return (float) (1f - (x - Math.Floor(x)));
//                return (float) (x - Math.Floor(x));
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