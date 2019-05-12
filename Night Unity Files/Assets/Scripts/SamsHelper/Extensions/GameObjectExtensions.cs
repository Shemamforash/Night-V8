using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extensions
{
	public static class GameObjectExtensions
	{
		public static T FindChildWithName<T>(this GameObject g, string name) where T : class
		{
			if (typeof(T) == typeof(GameObject)) throw new Exception();
			Transform t          = g.transform;
			Transform foundChild = FindChildWithName(t, name);
			if (foundChild.Null()) throw new Exception(name);
			T foundComponent = foundChild.GetComponent<T>();
			if (foundComponent.Null()) throw new Exception(foundChild.name);
			return foundComponent;
		}

		public static T FindChildWithName<T>(this Transform t, string name) where T : class => t.gameObject.FindChildWithName<T>(name);

		public static T FindChildWithName<T>(this MonoBehaviour u, string name) where T : class => u.gameObject.FindChildWithName<T>(name);

		public static GameObject FindChildWithName(this GameObject g, string name)
		{
			Transform t = FindChildWithName(g.transform, name);
			return t.NotNull() ? t.gameObject : null;
		}

		public static Transform FindChildWithName(this Transform t, string name)
		{
			List<Transform> children         = FindAllChildren(t);
			Transform       foundChild       = null;
			int             noNameOccurences = 0;
			foreach (Transform child in children)
			{
				if (child.name != name) continue;
				++noNameOccurences;
				foundChild = child;
			}

			if (noNameOccurences > 1) throw new Exception(name);
			return foundChild;
		}

		public static GameObject InstantiateOrigin(this string prefabLocation, Transform parent = null) => InstantiateOrigin(Resources.Load(prefabLocation) as GameObject, parent);

		public static GameObject InstantiateOrigin(this GameObject prefab, Transform parent = null)
		{
			GameObject newUiObject = Object.Instantiate(prefab, parent, false);
			newUiObject.transform.localScale = new Vector3(1, 1, 1);
			return newUiObject;
		}

		public static List<T> FindAllComponentsInChildren<T>(this Transform t)
		{
			return FindAllChildren(t).Select(child => child.GetComponent<T>()).Where(component => component.NotNull()).ToList();
		}

		public static List<Transform> FindAllChildren(this Transform t)
		{
			List<Transform> children   = new List<Transform>();
			int             noChildren = t.childCount;
			Helper.DoFor(noChildren, i =>
			{
				Transform child = t.GetChild(i);
				children.Add(child);
				children.AddRange(FindAllChildren(child));
			});
			return children;
		}
	}
}