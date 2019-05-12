using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace Extensions
{
	public static class ObjectExtensions
	{
		public static object GetPropertyValue<T>(this T obj, string propertyName) where T : class
		{
			PropertyInfo property = obj.GetType().GetProperty(propertyName);
			return property.Null() ? null : property.GetValue(obj, null);
		}

		public static object GetFieldValue(this object obj, string fieldName)
		{
			FieldInfo field = obj.GetType().GetField(fieldName);
			return field.Null() ? null : field.GetValue(obj);
		}

		[ContractAnnotation("null => false")]
		public static bool NotNull(this object obj) => obj != null;

		[ContractAnnotation("null => true")]
		public static bool Null(this object obj) => obj == null;

		public static bool Null(this Object go) => !go;

		public static bool NotNull(this Object go) => go;
	}
}