using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QuickEngine.Extensions;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace SamsHelper.Libraries
{
    public static class AdvancedMaths
    {
        public static bool DoesLinePassThroughPoint(Vector3 from, Vector3 to, Vector3 point)
        {
            float distanceAC = Vector3.Distance(from, point);
            float distanceAB = Vector3.Distance(from, to);
            float distanceBC = Vector3.Distance(to, point);
            float distanceDifference = distanceAC + distanceBC - distanceAB;
            return distanceDifference < 0.01f && distanceDifference > -0.01f;
        }

        public static float AngleFromUp(Vector3 origin, Vector3 target)
        {
            Vector3 direction = target - origin;
            float angle = Vector2.Angle(Vector3.up, direction);
            Vector3 cross = Vector3.Cross(direction, Vector3.up);
            if (cross.z < 0) return angle;
            return 360 - angle;
        }

        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;

        public static Vector2? LineIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            Vector2 r = b - a;
            Vector2 s = d - c;
            float rxs = Cross(r, s);
            if (Mathf.Abs(rxs) < 0.001f) return null;
            float t = Cross(c - a, s) / rxs;
            float u = Cross(c - a, r) / rxs;
            if (0 > t || t > 1 || 0 > u || u > 1) return null;
            Vector2 intersectionPoint = a + t * r;
            if (intersectionPoint == Vector2.zero) Debug.Log(a + " " + t + " " + r);
            return intersectionPoint;
        }

        public static Vector2? LineSegmentIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            Vector2? intersection = LineIntersection(a, b, c, d);
            if (intersection == null) return null;
            return !DoesPointLieOnLine(a, b, intersection.Value) ? null : intersection;
        }

        public static Vector2 RandomVectorWithinRange(Vector2 origin, float range)
        {
            float angle = Random.Range(0f, 360f);
            return CalculatePointOnCircle(angle, Random.Range(0f, range), origin);
        }

        public static float GetSmallestAngleBetweenTwoAngles(float b, float a)
        {
            if (a == b) return 0;
            if (a > b)
            {
                if (a - 180f < b) return a - b;
                return -(360f - (a - b));
            }

            if (b - 180f < a) return a - b;
            return 360f - (b - a);
        }

        public static List<Vector2> GetPoissonDiscDistribution(int numberOfPoints, float maxSampleDistance, bool includeInitialSample = false, float scale = 1)
        {
            List<Vector2> samples = new List<Vector2>();
            Vector2 initialSample = Vector2.zero;
            if (!includeInitialSample)
                initialSample = new Vector2(Random.Range(-maxSampleDistance, maxSampleDistance), Random.Range(-maxSampleDistance, maxSampleDistance));
            samples.Add(initialSample);

            int gridWidth = Mathf.CeilToInt(Mathf.Sqrt(numberOfPoints) / 0.8f);
            if (gridWidth % 2 == 0) ++gridWidth;
            float gridRadius = gridWidth / 2f;
            float squareWidth = maxSampleDistance / gridWidth;

            float maxDistance = gridRadius * squareWidth;
            maxDistance *= maxDistance;
            for (int x = 0; x < gridWidth; ++x)
            {
                for (int y = 0; y < gridWidth; ++y)
                {
                    float sqrRad = squareWidth / 2f;
                    float xPos = (x - gridRadius) * squareWidth;
                    float yPos = (y - gridRadius) * squareWidth;
                    xPos += sqrRad;
                    yPos += sqrRad;
                    float sqrDistance = xPos * xPos + yPos * yPos;

                    if (sqrDistance >= maxDistance) continue;
                    Vector2 topLeft = new Vector2(-sqrRad, -sqrRad);
                    topLeft *= scale;
                    Vector2 bottomRight = new Vector2(sqrRad, sqrRad);
                    bottomRight *= scale;

                    topLeft.x += xPos;
                    topLeft.y += yPos;
                    bottomRight.x += xPos;
                    bottomRight.y += yPos;

//                    Debug.DrawLine(new Vector2(topLeft.x, topLeft.y), new Vector2(bottomRight.x, topLeft.y), Color.yellow, 5f);
//                    Debug.DrawLine(new Vector2(topLeft.x, bottomRight.y), new Vector2(bottomRight.x, bottomRight.y), Color.yellow, 5f);
//
//                    Debug.DrawLine(new Vector2(bottomRight.x, topLeft.y), new Vector2(bottomRight.x, bottomRight.y), Color.yellow, 5f);
//                    Debug.DrawLine(new Vector2(topLeft.x, topLeft.y), new Vector2(topLeft.x, bottomRight.y), Color.yellow, 5f);

                    samples.Add(RandomPointInSquare(topLeft, bottomRight));
                }
            }

            samples.Shuffle();
            return samples;
        }

        public static Vector2 RandomPointInSquare(Vector2 topLeft, Vector2 bottomRight)
        {
            float x = Random.Range(topLeft.x, bottomRight.x);
            float y = Random.Range(topLeft.y, bottomRight.y);
            return new Vector2(x, y);
        }

        public static Quaternion RotationToTarget(Vector3 origin, Vector3 target)
        {
            float angle = AngleFromUp(origin, target);
            return Quaternion.Euler(new Vector3(0, 0, angle));
        }

        public static float AngleBetween(Vector3 from, Vector3 to, float fromRotation, bool absoluteVal = true)
        {
            Vector3 direction = to - from;
            float xDir = -Mathf.Sin(fromRotation * Mathf.Deg2Rad);
            float yDir = Mathf.Cos(fromRotation * Mathf.Deg2Rad);
            Vector2 fromDir = new Vector2(xDir, yDir);
            float angle = Vector2.Angle(fromDir, direction);
            if (absoluteVal) return angle;
            Vector3 cross = Vector3.Cross(direction, fromDir);
            if (cross.z < 0) angle = -angle;

            return angle;
        }

        public static float Dot(Vector2 from, Vector2 to, Vector2 point) => (point.x - from.x) * (to.y - from.y) - (point.y - from.y) * (to.x - from.x);

        public static bool IsPointInPolygon(Vector2 point, List<Vector2> polygon)
        {
            int polygonLength = polygon.Count;
            if (polygonLength == 0) return false;
            int i = 0;
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
                inside ^= (endY > pointY) ^ (startY > pointY) /* ? pointY inside [startY;endY] segment ? */
                          && /* if so, test if it is under the segment */
                          pointX - endX < (pointY - endY) * (startX - endX) / (startY - endY);
            }

            return inside;
        }

        private static float Dot(Vector2 a, Vector2 b) => a.x * b.x + a.y * b.y;

        public static Tuple<Vector3, Vector3> GetBoundingCornersOfPolygon(List<Vector2> vertices)
        {
            Vector3 topLeft = vertices[0];
            Vector3 bottomRight = vertices[0];

            foreach (Vector3 n in vertices)
            {
                if (n.x < topLeft.x)
                {
                    topLeft.x = n.x;
                }
                else if (n.x > bottomRight.x) bottomRight.x = n.x;

                if (n.y < topLeft.y)
                {
                    topLeft.y = n.y;
                }
                else if (n.y > bottomRight.y) bottomRight.y = n.y;
            }

            return Tuple.Create(topLeft, bottomRight);
        }

        public static bool DoesLineIntersectWithCircle(Vector2 a, Vector2 b, Vector2 c, float radius)
        {
            Vector2 ab = b - a;
            Vector2 ac = c - a;
            Vector2 closest = a + (Vector2) Vector3.Project(ac, ab);
            float distance = Vector2.Distance(c, closest);
            return distance <= radius;
        }

        public static float CosineRule(float a, float b, float c)
        {
            float numerator = b * b + c * c - a * a;
            float denominator = 2 * b * c;
            float result = numerator / denominator;
            result = Mathf.Acos(result);
            return result;
        }

        public static Vector3 RotateVector(Vector3 direction, float angle)
        {
            float newX = direction.x * Mathf.Cos(angle) - direction.y * Mathf.Sin(angle);
            float newY = direction.x * Mathf.Sin(angle) + direction.y * Mathf.Cos(angle);
            direction.x = newX;
            direction.y = newY;
            return direction;
        }

        public static Vector2 RandomPointOnLine(Vector3 start, Vector3 end)
        {
            float random = Random.Range(0f, 1f);
            return PointAlongLine(start, end, random);
        }

        public static Vector3 PointAlongLine(Vector3 start, Vector3 end, float amount)
        {
            float newX = (1 - amount) * start.x + amount * end.x;
            float newY = (1 - amount) * start.y + amount * end.y;
            return new Vector2(newX, newY);
        }

        public class BoundingBox
        {
            public readonly Vector2 TopLeft, TopRight, BottomLeft, BottomRight;

            public BoundingBox(Vector2 position, float width, float height)
            {
                TopLeft = new Vector2(-width / 2, height / 2) + position;
                TopRight = new Vector2(width / 2, height / 2) + position;
                BottomLeft = new Vector2(-width / 2, -height / 2) + position;
                BottomRight = new Vector2(width / 2, -height / 2) + position;
            }

            public void Draw()
            {
                Debug.DrawLine(TopLeft, TopRight, Color.green, 5f);
                Debug.DrawLine(BottomRight, TopRight, Color.green, 5f);
                Debug.DrawLine(TopLeft, BottomLeft, Color.green, 5f);
                Debug.DrawLine(BottomLeft, BottomRight, Color.green, 5f);
            }
        }


        public static bool IsRectInCameraView(Vector2 topLeft, Vector2 topRight, Vector2 bottomRight, Vector2 bottomLeft)
        {
            Camera camera = Camera.main;
            Vector2 cameraUpperLeft = camera.ScreenToWorldPoint(new Vector3(0, Screen.height));
            Vector2 cameraUpperRight = camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
            Vector2 cameraLowerLeft = camera.ScreenToWorldPoint(new Vector3(0, 0));
            Vector2 cameraLowerRight = camera.ScreenToWorldPoint(new Vector3(Screen.width, 0));
            List<Vector2> cameraVerts = new List<Vector2> {cameraUpperLeft, cameraUpperRight, cameraLowerLeft, cameraLowerRight};
            List<Vector2> rectVerts = new List<Vector2> {topLeft, topRight, bottomRight, bottomLeft};
            if (cameraVerts.Any(v => IsPointInPolygon(v, rectVerts))) return true;
            if (rectVerts.Any(v => IsPointInPolygon(v, cameraVerts))) return true;
            for (int i = 0; i < 4; ++i)
            {
                Vector2 fromCamera = cameraVerts[i];
                Vector2 toCamera = Helper.NextElement(i, cameraVerts);
                for (int j = 0; j < 4; ++j)
                {
                    Vector2 fromRect = rectVerts[j];
                    Vector2 toRect = Helper.NextElement(j, rectVerts);
                    if (LineSegmentIntersection(fromCamera, toCamera, fromRect, toRect) != null) return true;
                }
            }

            return false;
        }

        public static List<Vector2> FindLineCircleIntersections(Vector2 a, Vector2 b, Vector2 c, float radius)
        {
            List<Vector2> intersections = new List<Vector2>();

            float dx = b.x - a.x;
            float dy = b.y - a.y;

            float cx = c.x;
            float cy = c.y;

            float A = dx * dx + dy * dy;
            float B = 2 * (dx * (a.x - cx) + dy * (a.y - cy));
            float C = (a.x - cx) * (a.x - cx) + (a.y - cy) * (a.y - cy) - radius * radius;

            float det = B * B - 4 * A * C;

            float t;
            if (A < 0.0000001 || det < 0) return intersections;

            if (det == 0)
            {
                t = -B / (2 * A);
                intersections.Add(new Vector2(a.x + t * dx, a.y + t * dy));
                return intersections;
            }

            t = (float) ((-B + Math.Sqrt(det)) / (2 * A));
            intersections.Add(new Vector2(a.x + t * dx, a.y + t * dy));
            t = (float) ((-B - Math.Sqrt(det)) / (2 * A));
            intersections.Add(new Vector2(a.x + t * dx, a.y + t * dy));
            return intersections;
        }

        public static List<Vector2> FindLineSegmentCircleIntersections(Vector2 a, Vector2 b, Vector2 c, float radius)
        {
            List<Vector2> intersections = FindLineCircleIntersections(a, b, c, radius);
            switch (intersections.Count)
            {
                case 1:
                    if (!DoesPointLieOnLine(a, b, intersections[0])) intersections.RemoveAt(0);
                    break;
                case 2:
                    if (!DoesPointLieOnLine(a, b, intersections[1])) intersections.RemoveAt(1);
                    if (!DoesPointLieOnLine(a, b, intersections[0])) intersections.RemoveAt(0);
                    break;
            }

            return intersections;
        }

        public static bool DoesPointLieOnLine(Vector2 a, Vector2 b, Vector2 c)
        {
            float cross = (c.y - a.y) * (b.x - a.x) - (c.x - a.x) * (b.y - a.y);
            if (Mathf.Abs(cross) > 0.0001f) return false;
            float dot = (c.x - a.x) * (b.x - a.x) + (c.y - a.y) * (b.y - a.y);
            if (dot < 0) return false;
            float sqrLen = (b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y);
            if (dot > sqrLen) return false;
            return true;
        }

        public static Vector2 RotatePoint(Vector2 point, int rotateAmount, Vector2 origin)
        {
            float x = point.x - origin.x;
            float y = point.y - origin.y;
            point.x = x * Mathf.Cos(rotateAmount) - y * Mathf.Sin(rotateAmount);
            point.y = x * Mathf.Sin(rotateAmount) + y * Mathf.Cos(rotateAmount);
            point.x += origin.x;
            point.y += origin.y;
            return point;
        }

        public static Vector2 CalculatePointOnCircle(float angle, float radius, Vector3 origin, bool alreadyInRadians = false)
        {
            if (!alreadyInRadians) angle *= Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius + origin.x;
            float y = Mathf.Sin(angle) * radius + origin.y;
            return new Vector2(x, y);
        }

        public static Vector2 RandomDirection()
        {
            return RandomVectorWithinRange(Vector2.zero, 1).normalized;
        }

        public static Vector2 RandomPointInCircle(float radius)
        {
            float randomAngle = Random.Range(0f, 1f) * 2f * Mathf.PI;
            radius = radius * Mathf.Sqrt(Random.Range(0f, 1f));
            float x = radius * Mathf.Cos(randomAngle);
            float y = radius * Mathf.Sin(randomAngle);
            return new Vector2(x, y);
        }

        public static Vector2 WorldToCanvasSpace(Canvas canvas, Camera camera, Transform transform)
        {
            Vector2 position = transform.position;
            position = camera.WorldToScreenPoint(position);
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            Vector2 scaleReference = new Vector2(scaler.referenceResolution.x / Screen.width, scaler.referenceResolution.y / Screen.height);
            position.Scale(scaleReference);
            return position;

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
            float canvasActualWidth = canvasRectTransform.GetWidth();
            float canvasActualHeight = canvasRectTransform.GetHeight();

            position.x *= canvasActualWidth / screenWidth;
            position.y *= canvasActualHeight / screenHeight;

            return position;
        }

        public static Vector2 ScreenToCanvasSpace(Canvas canvas, Camera camera, RectTransform rectTransform)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            Vector2 scaleReference = new Vector2(scaler.referenceResolution.x / Screen.width, scaler.referenceResolution.y / Screen.height);

            Vector2 size = Vector2.Scale(rectTransform.rect.size, rectTransform.lossyScale);
            Rect screenRect = new Rect((Vector2) rectTransform.position - size * 0.5f, size);
            Vector2 topLeft = new Vector2(screenRect.xMin, screenRect.yMin);
            Vector2 bottomRight = new Vector2(screenRect.xMax, screenRect.yMax);
            Vector2 position = (topLeft + bottomRight) / 2f;
            position = camera.WorldToScreenPoint(position);

//            Debug.Log(position);

//            float screenWidth = Screen.width;
//            float screenHeight = Screen.height;
//
//            RectTransform canvasRectTransform = canvas.GetComponent<RectTransform>();
//            float canvasActualWidth = canvasRectTransform.GetWidth();
//            float canvasActualHeight = canvasRectTransform.GetHeight();
//
//            position.x *= canvasActualWidth / screenWidth;
//            position.y *= canvasActualHeight / screenHeight;

            position.Scale(scaleReference);
            return position;
//            Debug.Log(targetWidth + " " + screenWidth + " " + canvasActualWidth + " " + position.x + " " + position.y);
        }
    }
}