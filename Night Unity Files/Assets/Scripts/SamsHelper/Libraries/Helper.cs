//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Xml;
//using SamsHelper.BaseGameFunctionality.Basic;
//using SamsHelper.ReactiveUI.Elements;
//using UnityEngine;
//using UnityEngine.UI;
//using Object = UnityEngine.Object;
//using Random = UnityEngine.Random;
//using Stopwatch = System.Diagnostics.Stopwatch;
//
//namespace SamsHelper.Libraries
//{
//    public static class Helper
//    {
//        public static string AttributeToDisplayString(this AttributeType attributeType)
//        {
//            return string.Join(" ", attributeType.ToString().SplitOnCamelCase());
//        }
//
//        public static bool Empty<T>(this Queue<T> queue) => queue.Count == 0;
//
//        public static bool Empty<T>(this List<T> list) => list.Count == 0;
//
//        public static bool Empty<T>(this T[] arr) => arr.Length == 0;
//
//        private static Transform _dynamicParent;
//
//        public static string Pluralise(this string str, int count)
//        {
//            if (count <= 1) return str;
//            return str + "s";
//        }
//
//        public static string[] SplitOnCamelCase(this string str)
//        {
//            return Regex.Split(str, @"(?<!^)(?=[A-Z])");
//        }
//
//        public static void SetAsDynamicChild(this Transform t)
//        {
//            if (_dynamicParent == null) _dynamicParent = GameObject.Find("Dynamic").transform;
//            t.SetParent(_dynamicParent);
//            t.transform.position = Vector3.zero;
//        }
//
//        public static bool Empty<T>(this Stack<T> stack) => stack.Count == 0;
//
//        public static bool NotEmpty<T>(this Queue<T> queue) => queue.Count != 0;
//
//        public static bool NotEmpty<T>(this List<T> list) => list.Count != 0;
//
//        public static bool NotEmpty<T>(this T[] arr) => arr.Length != 0;
//
//        public static bool NotEmpty<T>(this Stack<T> stack) => stack.Count != 0;
//
//        public static void PauseParticles(this ParticleSystem ps)
//        {
//            ParticleSystem.MainModule main = ps.main;
//            main.simulationSpeed = 0;
//        }
//
//        public static void ResumeParticles(this ParticleSystem ps)
//        {
//            ParticleSystem.MainModule main = ps.main;
//            main.simulationSpeed = 1;
//        }
//
//        public static void DrawLine(this Transform transform, Vector2 other, Color color, float duration)
//        {
//            transform.position.DrawLine(other, color, duration);
//        }
//
//        public static void DrawLine(this Vector3 point, Vector2 other, Color color, float duration)
//        {
//            Debug.DrawLine(point, other, color, duration);
//        }
//
//        public static void DrawLine(this Vector2 point, Vector2 other, Color color, float duration)
//        {
//            Debug.DrawLine(point, other, color, duration);
//        }
//
//        public static bool RollDie(float target, float max)
//        {
//            return Random.Range(0, max) == target;
//        }
//
//        public static bool RollDie(int target, int max)
//        {
//            return Random.Range(0, max) == target;
//        }
//
//        public static Vector3 MouseToWorldCoordinates(float z = 0f)
//        {
//            if (mainCamera == null || mainCamera.gameObject == null) mainCamera = Camera.main;
//            Vector3 mousePos = UnityEngine.Input.mousePosition;
//            mousePos.z = -mainCamera.transform.position.z - z;
//            mousePos = mainCamera.ScreenToWorldPoint(mousePos);
//            return mousePos;
//        }
//
//        public static int ParseInt(this XmlNode root, string nodeName)
//        {
//            return int.Parse(ParseString(root, nodeName), CultureInfo.InvariantCulture.NumberFormat);
//        }
//
//        public static float ParseFloat(this XmlNode root, string nodeName)
//        {
//            return float.Parse(ParseString(root, nodeName), CultureInfo.InvariantCulture.NumberFormat);
//        }
//
//        public static XmlNode GetNode(this XmlNode root, string nodeName)
//        {
//            if (root.Name == nodeName) return root;
//            XmlNode node = root.SelectSingleNode(nodeName);
//            if (node == null) throw new Exceptions.XmlNodeDoesNotExistException(nodeName);
//            return node;
//        }
//
//        public static string ParseString(this XmlNode root, string name)
//        {
//            return GetNode(root, name).InnerText;
//        }
//
//        public static string NodeAttributeValue(this XmlNode root, string attributeName)
//        {
//            if (root.Attributes == null) throw new Exceptions.NodeHasNoAttributesException(root.Name);
//            return root.Attributes[attributeName].Value;
//        }
//
//        public static bool ParseBool(this XmlNode root, string nodeName)
//        {
//            return ParseString(root, nodeName).ToLower() == "true";
//        }
//
//        private static Camera mainCamera;
//
//        private static bool InCameraView(this Vector3 position, Camera camera = null)
//        {
//            if (camera == null)
//            {
//                if (mainCamera == null || mainCamera.gameObject == null) mainCamera = Camera.main;
//                camera = mainCamera;
//            }
//
//            Vector3 screenPosition = camera.WorldToViewportPoint(position);
//            return screenPosition.z > 0 &&
//                   screenPosition.x > 0 &&
//                   screenPosition.y > 0 &&
//                   screenPosition.x < 1 &&
//                   screenPosition.y < 1;
//        }
//
//        public static bool InCameraView(this Vector2 position, Camera camera = null)
//        {
//            return ((Vector3) position).InCameraView(camera);
//        }
//
//        public static T RemoveRandom<T>(this List<T> list)
//        {
//            int randomIndex = Random.Range(0, list.Count);
//            T element = list[randomIndex];
//            list.RemoveAt(randomIndex);
//            return element;
//        }
//
//        public static List<object> ToObjectList<T>(this List<T> list)
//        {
//            List<object> objectList = new List<object>();
//            int objectListCount = list.Count;
//            for (int i = 0; i < objectListCount; ++i)
//            {
//                objectList.Add(list[i]);
//            }
//
//            return objectList;
//        }
//
//        public static void Shuffle<T>(this List<T> list)
//        {
//            List<T> randomList = new List<T>();
//            while (list.Count > 0)
//            {
//                int removePosition = Random.Range(0, list.Count);
//                T element = list[removePosition];
//                randomList.Add(element);
//                list.RemoveAt(removePosition);
//            }
//
//            list.Clear();
//            list.AddRange(randomList);
//        }
//
//        public static void Shuffle<T>(this T[] arr)
//        {
//            List<T> list = new List<T>(arr);
//            Shuffle(list);
//            for (int i = 0; i < arr.Length; ++i)
//            {
//                arr[i] = list[i];
//            }
//        }
//
//        public static List<T> FindAllComponentsInChildren<T>(this Transform t)
//        {
//            return FindAllChildren(t).Select(child => child.GetComponent<T>()).Where(component => component != null).ToList();
//        }
//
//        public static List<Transform> FindAllChildren(this Transform t)
//        {
//            List<Transform> children = new List<Transform>();
//            int noChildren = t.childCount;
//            for (int i = 0; i < noChildren; ++i)
//            {
//                Transform child = t.GetChild(i);
//                children.Add(child);
//                children.AddRange(FindAllChildren(child));
//            }
//
//            return children;
//        }
//
//        public static float Round(this float val, int precision = 0)
//        {
//            float precisionDivider = (float) Math.Pow(10f, precision);
//            return (float) (Math.Round(val * precisionDivider) / precisionDivider);
//        }
//
//        public static T FindChildWithName<T>(this GameObject g, string name) where T : class
//        {
//            if (typeof(T) == typeof(GameObject)) throw new Exceptions.CannotGetGameObjectComponent();
//            Transform t = g.transform;
//            Transform foundChild = FindChildWithName(t, name);
//            if (foundChild == null) throw new Exceptions.ChildNotFoundException(g, name);
//            T foundComponent = foundChild.GetComponent<T>();
//            if (foundComponent == null) throw new Exceptions.ComponentNotFoundException(foundChild.name, typeof(T));
//            return foundComponent;
//        }
//
//        public static T FindChildWithName<T>(this Transform t, string name) where T : class
//        {
//            return t.gameObject.FindChildWithName<T>(name);
//        }
//
//        public static T FindChildWithName<T>(this MonoBehaviour u, string name) where T : class
//        {
//            return u.gameObject.FindChildWithName<T>(name);
//        }
//
//        public static GameObject FindChildWithName(this GameObject g, string name)
//        {
//            Transform t = FindChildWithName(g.transform, name);
//            return t != null ? t.gameObject : null;
//        }
//
//        public static Transform FindChildWithName(this Transform t, string name)
//        {
//            List<Transform> children = FindAllChildren(t);
//            Transform foundChild = null;
//            int noNameOccurences = 0;
//            foreach (Transform child in children)
//            {
//                if (child.name != name) continue;
//                ++noNameOccurences;
//                foundChild = child;
//            }
//
//            if (noNameOccurences > 1) throw new Exceptions.UnspecificGameObjectNameException(noNameOccurences, name);
//            return foundChild;
//        }
//
//        public static void Print<T>(this List<T> list)
//        {
//            string listString = "";
//            for (int i = 0; i < list.Count; ++i)
//            {
//                if (i != 0) listString += ", ";
//                listString += list[i];
//            }
//
//            Debug.Log(listString);
//        }
//
//        public static void Print<T>(this T[] arr)
//        {
//            new List<T>(arr).Print();
//        }
//
//
//        private static float _startingTime = -1;
//
//        public static float TimeInSeconds()
//        {
//            if (_startingTime == -1) _startingTime = Time.timeSinceLevelLoad;
//            return Time.timeSinceLevelLoad;
//        }
//
//        public static bool HasSameSignAs(this float a, float b)
//        {
//            if (a > 0 && b <= 0 || a < 0 && b >= 0) return false;
//            return true;
//        }
//
//        public static T RandomElement<T>(this T[] arr) => arr[Random.Range(0, arr.Length)];
//
//        public static T RandomElement<T>(this List<T> arr) => arr[Random.Range(0, arr.Count)];
//
//        public static float Normalise(this float value, float maxValue)
//        {
//            return value /= maxValue;
//        }
//
//        public static float Polarity(this float direction)
//        {
//            if (direction < 0) return -1;
//            if (direction > 0) return 1;
//            return 0;
//        }
//
//        public static T NextElement<T>(this List<T> list, int iterator) => list[list.NextIndex(iterator)];
//
//        public static T PrevElement<T>(this List<T> list, int iterator) => list[list.PrevIndex(iterator)];
//
//        public static int NextIndex<T>(this List<T> list, int iterator)
//        {
//            if (iterator + 1 == list.Count) return 0;
//            return iterator + 1;
//        }
//
//        public static int PrevIndex<T>(this List<T> list, int iterator)
//        {
//            if (iterator - 1 == -1) return list.Count - 1;
//            return iterator - 1;
//        }
//
//        public static T RemoveLast<T>(this List<T> list)
//        {
//            int endIndex = list.Count - 1;
//            T end = list[endIndex];
//            list.RemoveAt(endIndex);
//            return end;
//        }
//
//        public static Color RandomColour(bool randomAlpha = false)
//        {
//            Color c = new Color();
//            c.r = Random.Range(0f, 1f);
//            c.g = Random.Range(0f, 1f);
//            c.b = Random.Range(0f, 1f);
//            if (randomAlpha)
//            {
//                c.a = Random.Range(0f, 1f);
//            }
//            else
//            {
//                c.a = 1;
//            }
//
//            return c;
//        }
//
//        public static string PrintHierarchy(this GameObject parent)
//        {
//            string hierarchy = "";
//            Transform t = parent.transform;
//            while (t != null)
//            {
//                hierarchy = t.name + "/" + hierarchy;
//                t = t.parent;
//            }
//
//            return hierarchy;
//        }
//
//        public static void Swap<T>(this List<T> list, int from, int to)
//        {
//            T temp = list[to];
//            list[to] = list[from];
//            list[from] = temp;
//        }
//
//
//
//        public static void PrintTime(this Stopwatch stopWatch, string message)
//        {
//#if UNITY_EDITOR
//            Debug.Log(message + stopWatch.Elapsed.ToString("mm\\:ss\\.ffff"));
//#endif
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Extensions
{
	public static class Helper
	{
		private static Transform _dynamicParent;

		private static Camera mainCamera;

		private static float _startingTime = -1;

		public static string AttributeToDisplayString(this AttributeType attributeType)
		{
			return string.Join(" ", attributeType.ToString().SplitOnCamelCase());
		}

		public static string Pluralise(this string str, int count)
		{
			if (count <= 1) return str;
			return str + "s";
		}

		public static string AddSignPrefix(this float value)
		{
			if (value <= 0) return value.ToString();
			return "+" + value;
		}

		public static string[] SplitOnCamelCase(this string str) => Regex.Split(str, @"(?<!^)(?=[A-Z])");

		public static void SetAsDynamicChild(this Transform t)
		{
			if (_dynamicParent.Null()) _dynamicParent = GameObject.Find("Dynamic").transform;
			t.SetParent(_dynamicParent);
			t.transform.position = Vector3.zero;
		}

		public static void PauseParticles(this ParticleSystem ps)
		{
			ParticleSystem.MainModule main = ps.main;
			main.simulationSpeed = 0;
		}

		public static void ResumeParticles(this ParticleSystem ps)
		{
			ParticleSystem.MainModule main = ps.main;
			main.simulationSpeed = 1;
		}

		public static void DrawLine(this Transform transform, Vector2 other, Color color, float duration)
		{
			transform.position.DrawLine(other, color, duration);
		}

		public static void DrawLine(this Vector3 point, Vector2 other, Color color, float duration)
		{
			Debug.DrawLine(point, other, color, duration);
		}

		public static void DrawLine(this Vector2 point, Vector2 other, Color color, float duration)
		{
			Debug.DrawLine(point, other, color, duration);
		}

		public static XmlNodeList GetNodesWithName(this XmlNode root, string subNodeName)
		{
			XmlNodeList nodes = root.SelectNodes(subNodeName);
			if (nodes == null) throw new Exceptions.NoNodesWithNameException(subNodeName);
			return nodes;
		}

		public static void DoFor(int count, Action action)
		{
			if (action.Null()) return;
			for (int i = 0; i < count; ++i) action();
		}

		public static void DoFor(int count, Action<int> action)
		{
			if (action.Null()) return;
			for (int i = 0; i < count; ++i) action(i);
		}

		public static void DoForEach<T>(IEnumerable<T> list, Action<T> action)
		{
			if (action == null) return;
			foreach (T t in list)
				action(t);
		}

		public static bool InEditMode() => Application.isEditor && !Application.isPlaying;

		public static Vector3 MouseToWorldCoordinates(float z = 0f)
		{
			if (mainCamera.Null() || mainCamera.gameObject.Null()) mainCamera = Camera.main;
			Vector3 mousePos                                                  = Input.mousePosition;
			mousePos.z = -mainCamera.transform.position.z - z;
			mousePos   = mainCamera.ScreenToWorldPoint(mousePos);
			return mousePos;
		}

		public static XmlNode OpenRootNode(string xmlName, string rootNodeName = null)
		{
			TextAsset   xmlFile = Resources.Load<TextAsset>("XML/" + xmlName);
			XmlDocument xml     = new XmlDocument();
			xml.LoadXml(xmlFile.text);
			if (rootNodeName == null) rootNodeName = xmlName;
			return xml.SelectSingleNode("//" + rootNodeName);
		}

		private static bool InCameraView(this Vector3 position, Camera camera = null)
		{
			if (camera.Null())
			{
				if (mainCamera.Null() || mainCamera.gameObject.Null()) mainCamera = Camera.main;
				camera = mainCamera;
			}

			Vector3 screenPosition = camera.WorldToViewportPoint(position);
			return screenPosition.z > 0 &&
			       screenPosition.x > 0 &&
			       screenPosition.y > 0 &&
			       screenPosition.x < 1 &&
			       screenPosition.y < 1;
		}

		public static bool InCameraView(this Vector2 position, Camera camera = null) => ((Vector3) position).InCameraView(camera);

		public static GameObject InstantiateUiObject(string prefabLocation, Transform parent) => InstantiateUiObject(Resources.Load(prefabLocation) as GameObject, parent);

		public static GameObject InstantiateUiObject(GameObject prefab, Transform parent)
		{
			GameObject newUiObject = Object.Instantiate(prefab, parent, false);
			newUiObject.transform.localScale = new Vector3(1, 1, 1);
			return newUiObject;
		}

		public static List<object> ToObjectList<T>(this List<T> list)
		{
			List<object> objectList      = new List<object>();
			int          objectListCount = list.Count;
			DoFor(objectListCount, i => objectList.Add(list[i]));
			return objectList;
		}


		public static float TimeInSeconds()
		{
			if (_startingTime == -1) _startingTime = Time.timeSinceLevelLoad;
			return Time.timeSinceLevelLoad;
		}

		public static Color RandomColour(bool randomAlpha = false)
		{
			Color c = new Color();
			c.r = Random.Range(0f, 1f);
			c.g = Random.Range(0f, 1f);
			c.b = Random.Range(0f, 1f);
			if (randomAlpha)
			{
				c.a = Random.Range(0f, 1f);
			}
			else
			{
				c.a = 1;
			}

			return c;
		}

		public static string PrintHierarchy(this GameObject parent)
		{
			string    hierarchy = "";
			Transform t         = parent.transform;
			while (t.NotNull())
			{
				hierarchy = t.name + "/" + hierarchy;
				t         = t.parent;
			}

			return hierarchy;
		}


		public static void PrintTime(this Stopwatch stopWatch, string message)
		{
#if UNITY_EDITOR
			Debug.Log(message + stopWatch.Elapsed.ToString("mm\\:ss\\.ffff"));
#endif
		}
	}
}