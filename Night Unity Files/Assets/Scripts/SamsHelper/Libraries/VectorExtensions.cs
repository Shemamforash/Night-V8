using System.Globalization;
using Extensions;
using UnityEngine;

namespace SamsHelper.Libraries
{
	public static class VectorExtensions
	{
		public static Vector3 ToVector3(this string vectorString)
		{
			vectorString = CleanupVectorString(vectorString);
			string[] arr  = vectorString.Split(',');
			Vector3  vect = new Vector3();
			vect.x = float.Parse(arr[0], CultureInfo.InvariantCulture.NumberFormat);
			vect.y = float.Parse(arr[1], CultureInfo.InvariantCulture.NumberFormat);
			vect.z = float.Parse(arr[2], CultureInfo.InvariantCulture.NumberFormat);
			return vect;
		}

		private static string CleanupVectorString(string vectorString)
		{
			vectorString = vectorString.Replace(" ", "");
			vectorString = vectorString.Replace("(", "");
			vectorString = vectorString.Replace(")", "");
			return vectorString;
		}

		public static Vector2 ToVector2(this string vectorString)
		{
			vectorString = CleanupVectorString(vectorString);
			string[] arr  = vectorString.Split(',');
			Vector2  vect = new Vector2();
			vect.x = float.Parse(arr[0], CultureInfo.InvariantCulture.NumberFormat);
			vect.y = float.Parse(arr[1], CultureInfo.InvariantCulture.NumberFormat);
			return vect;
		}

		public static Vector3 Direction(this Transform from, Transform to) => Direction(from.position, to.position);
		public static Vector3 Direction(this Vector3   from, Vector3   to) => (from - to).normalized;
		public static Vector3 Direction(this Transform from, Vector3   to) => Direction(from.position, to);
		public static Vector3 Direction(this Vector3   from, Transform to) => Direction(from,          to.position);

		public static Vector2 Direction(this Vector2   from, Vector2   to) => (from - to).normalized;
		public static Vector2 Direction(this Transform from, Vector2   to) => Direction((Vector2) from.position, to);
		public static Vector2 Direction(this Vector2   from, Transform to) => Direction(from,                    (Vector2) to.position);


		public static string ToNiceString(this Vector2 vector2) =>
			"(" + vector2.x.Round(5).ToString(CultureInfo.InvariantCulture) + ", " + vector2.y.Round(5).ToString(CultureInfo.InvariantCulture) + ")";

		public static float Distance(this    Transform from, Transform to) => from.position.Distance(to.position);
		public static float Distance(this    Transform from, Vector2   to) => from.position.Distance(to);
		public static float Distance(this    Vector2   from, Vector2   to) => Vector2.Distance(from, to);
		public static float Distance(this    Vector3   from, Vector2   to) => Vector3.Distance(from, to);
		public static float SqrDistance(this Transform from, Transform to) => from.position.SqrDistance(to.position);
		public static float SqrDistance(this Vector3   from, Vector3   to) => Helper.FastSquareMagnitude(from, to);

		public static float SqrMag(this Vector2 a)
		{
			return a.x * a.x + a.y * a.y;
		}

		public static float SqrMag2D(this Vector3 a)
		{
			return a.x * a.x + a.y * a.y;
		}
	}
}