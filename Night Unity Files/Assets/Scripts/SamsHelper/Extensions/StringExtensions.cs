using UnityEngine;

namespace Extensions
{
	public static class StringExtensions
	{
		public static void PrintIfObjectIsNull(this string str, object obj)
		{
			if (obj.NotNull()) return;
			Debug.Log(str);
		}

		public static void PrintIfObjectIsNull(this string str, GameObject gameObject)
		{
			if (gameObject.NotNull()) return;
			Debug.Log(str);
		}

		public static void PrintIfNotNullOrEmpty(this string str)
		{
			if (string.IsNullOrEmpty(str)) return;
			Debug.Log(str);
		}

		public static void PrintIfNotNull(this string str)
		{
			if (str.Null()) return;
			Debug.Log(str);
		}
	}
}