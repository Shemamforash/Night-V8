using UnityEngine;

namespace SamsHelper
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
            if (cross.z < 0)
            {
                return angle;
            }
            return 360 - angle;
        }

        public static Quaternion RotationToTarget(Vector3 origin, Vector3 target)
        {
            float angle = AngleFromUp(origin, target);
            return Quaternion.Euler(new Vector3(0,0, angle));
        }

        public static float AngleBetween(Transform from, Transform to, bool absoluteVal = true)
        {
            Vector3 direction = to.position - from.position;
            float xDir = -Mathf.Sin(from.transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
            float yDir = Mathf.Cos(from.transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
            Vector2 fromDir = new Vector2(xDir, yDir);
            float angle = Vector2.Angle(fromDir, direction);
            if (absoluteVal) return angle;
            Vector3 cross = Vector3.Cross(direction, fromDir);
            if (cross.z < 0)
            {
                angle = -angle;
            }

            return angle;
        }
    }
}