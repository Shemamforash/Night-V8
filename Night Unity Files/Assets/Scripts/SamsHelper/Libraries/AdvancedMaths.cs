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
            if (intersectionPoint == Vector2.zero || a == Vector2.zero) Debug.Log(a + " " + t + " " + r);
            return Tuple.Create(true, intersectionPoint);
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
    }
}