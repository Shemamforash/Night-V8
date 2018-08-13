using UnityEngine;

namespace SamsHelper.Libraries
{
    public static class VectorExtensions
    {
        public static Vector3 ToVector3(this string vectorString)
        {
            string[] arr = vectorString.Split(':');
            Vector3 vect = new Vector3();
            vect.x = float.Parse(arr[0]);
            vect.y = float.Parse(arr[1]);
            vect.z = float.Parse(arr[2]);
            return vect;
        }

        public static Vector2 ToVector2(this string vectorString)
        {
            string[] arr = vectorString.Split(':');
            Vector2 vect = new Vector2();
            vect.x = float.Parse(arr[0]);
            vect.y = float.Parse(arr[1]);
            return vect;
        }

        public static Vector3 Direction(this Transform from, Transform to)
        {
            return (from.position - to.position).normalized;
        }

        public static float Distance(this Transform from, Transform to)
        {
            return from.position.Distance(to.position);
        }

        public static float Distance(this Transform from, Vector2 to)
        {
            return from.position.Distance(to);
        }

        public static float Distance(this Vector2 from, Vector2 to)
        {
            return Vector2.Distance(from, to);
        }

        public static float Distance(this Vector3 from, Vector2 to)
        {
            return Vector3.Distance(from, to);
        }

        public static float SqrDistance(this Transform from, Transform to)
        {
            return from.position.SqrDistance(to.position);
        }

        public static float SqrDistance(this Vector3 from, Vector3 to)
        {
            return Vector3.SqrMagnitude(from - to);
        }

    }
}