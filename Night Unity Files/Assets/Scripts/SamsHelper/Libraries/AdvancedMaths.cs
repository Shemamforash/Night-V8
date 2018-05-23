using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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

        private static float Cross(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        public static Tuple<bool, Vector2> LineIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            Vector2 intersectionPoint = new Vector2();
            Vector2 r = b - a;
            Vector2 s = d - c;
            float rxs = Cross(r, s);
            if (Mathf.Abs(rxs) < 0.001f) return Tuple.Create(false, intersectionPoint);
            float t = Cross(c - a, s) / rxs;
            float u = Cross(c - a, r) / rxs;
            if (0 > t || t > 1 || 0 > u || u > 1) return Tuple.Create(false, intersectionPoint);
            intersectionPoint = a + t * r;
//            if (intersectionPoint == Vector2.zero || a == Vector2.zero) Debug.Log(a + " " + t + " " + r);
            return Tuple.Create(true, intersectionPoint);
        }

        public static Tuple<bool, Vector2> LineSegmentIntersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            Tuple<bool, Vector2> intersection = LineIntersection(a, b, c, d);
            if (!intersection.Item1) return intersection;
            if (!DoesPointLieOnLine(a, b, intersection.Item2)) intersection = Tuple.Create(false, intersection.Item2);
            return intersection;
        }
        
        public static Vector2 RandomVectorWithinRange(Vector2 origin, float range)
        {
            float newX = Random.Range(origin.x - range, origin.x + range);
            float newY = Random.Range(origin.y - range, origin.y + range);
            return new Vector2(newX, newY);
        }

        public static List<Vector2> GetPoissonDiscDistribution(int numberOfPoints, float minRadius, float maxRadius, float maxSampleDistance, bool includeInitialSample = false)
        {
            List<Vector2> samples = new List<Vector2>();
            List<Vector2> activeSamples = new List<Vector2>();
            if (includeInitialSample)
            {
                samples.Add(new Vector2(0, 0));
            }
            else
            {
                Vector2 random = new Vector2(Random.Range(-maxSampleDistance, maxSampleDistance), Random.Range(-maxSampleDistance, maxSampleDistance));
                samples.Add(random);
            }

            activeSamples.Add(samples[0]);

            for (int i = 0; i < numberOfPoints - 1 && activeSamples.Count != 0; ++i)
            {
                int targetSamples = 100;
                Vector2 randomSample = Vector2.positiveInfinity;
                while (targetSamples != 0)
                {
                    randomSample = activeSamples[Random.Range(0, activeSamples.Count)];
                    Vector2 randomPoint = new Vector2();
                    float leftBound = randomSample.x - maxRadius;
                    leftBound = Mathf.Clamp(leftBound, -maxSampleDistance, maxSampleDistance);
                    float rightBound = randomSample.x + maxRadius;
                    rightBound = Mathf.Clamp(rightBound, -maxSampleDistance, maxSampleDistance);
                    float lowerBound = randomSample.y - maxRadius;
                    lowerBound = Mathf.Clamp(lowerBound, -maxSampleDistance, maxSampleDistance);
                    float upperBound = randomSample.y + maxRadius;
                    upperBound = Mathf.Clamp(upperBound, -maxSampleDistance, maxSampleDistance);
                    randomPoint.x = Random.Range(leftBound, rightBound);
                    randomPoint.y = Random.Range(lowerBound, upperBound);
                    float distance = Vector2.Distance(randomSample, randomPoint);
                    if (distance >= minRadius && distance <= maxRadius)
                        if (!samples.Any(s => Vector2.Distance(s, randomPoint) < minRadius))
                        {
                            samples.Add(randomPoint);
                            activeSamples.Add(randomPoint);
                            break;
                        }

                    --targetSamples;
                }

                if (targetSamples == 0) activeSamples.Remove(randomSample);
            }

            if (samples.Count != numberOfPoints)
                Debug.Log("Found " + samples.Count + " samples out of " + numberOfPoints + " desired samples, consider modifying the parameters to reach desired number of samples");
            return samples;
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

        public static float Dot(Vector2 from, Vector2 to, Vector2 point)
        {
            return (point.x - from.x) * (to.y - from.y) - (point.y - from.y) * (to.x - from.x);
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
                inside ^= (endY > pointY) ^ (startY > pointY) /* ? pointY inside [startY;endY] segment ? */
                          && /* if so, test if it is under the segment */
                          pointX - endX < (pointY - endY) * (startX - endX) / (startY - endY);
            }

            return inside;
        }

        private static float Dot(Vector2 a, Vector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static Tuple<Vector3, Vector3> GetBoundingCornersOfPolygon(List<Vector2> vertices)
        {
            Vector3 topLeft = vertices[0];
            Vector3 bottomRight = vertices[0];

            foreach (Vector3 n in vertices)
            {
                if (n.x < topLeft.x) topLeft.x = n.x;
                else if (n.x > bottomRight.x) bottomRight.x = n.x;
                if (n.y < topLeft.y) topLeft.y = n.y;
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

            public BoundingBox(float width) : this(width, width)
            {
            }

            public BoundingBox(float width, float height)
            {
                TopLeft = new Vector2(-width / 2, height / 2);
                TopRight = new Vector2(width / 2, height / 2);
                BottomLeft = new Vector2(-width / 2, -height / 2);
                BottomRight = new Vector2(width / 2, -height / 2);
            }

            public void Draw()
            {
                Debug.DrawLine(TopLeft, TopRight, Color.green, 5f);
                Debug.DrawLine(BottomRight, TopRight, Color.green, 5f);
                Debug.DrawLine(TopLeft, BottomLeft, Color.green, 5f);
                Debug.DrawLine(BottomLeft, BottomRight, Color.green, 5f);
            }
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
    }
}