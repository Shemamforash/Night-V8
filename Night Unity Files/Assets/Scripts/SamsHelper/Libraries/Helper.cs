using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace SamsHelper.Libraries
{
    public static class Helper
    {
        public static bool Empty<T>(this Queue<T> queue) => queue.Count == 0;

        public static bool Empty<T>(this List<T> list) => list.Count == 0;

        public static bool Empty<T>(this T[] arr) => arr.Length == 0;

        private static Transform _dynamicParent;

        public static void SetAsDynamicChild(this Transform t)
        {
            if (_dynamicParent == null) _dynamicParent = GameObject.Find("Dynamic").transform;
            t.SetParent(_dynamicParent);
            t.transform.position = Vector3.zero;
        }

        public static bool Empty<T>(this Stack<T> stack) => stack.Count == 0;

        public static bool NotEmpty<T>(this Queue<T> queue) => queue.Count != 0;

        public static bool NotEmpty<T>(this List<T> list) => list.Count != 0;

        public static bool NotEmpty<T>(this T[] arr) => arr.Length != 0;

        public static bool NotEmpty<T>(this Stack<T> stack) => stack.Count != 0;

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

        public static bool RollDie(float target, float max)
        {
            return Random.Range(0, max) == target;
        }

        public static bool RollDie(int target, int max)
        {
            return Random.Range(0, max) == target;
        }

        public static Vector3 MouseToWorldCoordinates(float z = 0f)
        {
            if (mainCamera == null || mainCamera.gameObject == null) mainCamera = Camera.main;
            Vector3 mousePos = UnityEngine.Input.mousePosition;
            mousePos.z = -mainCamera.transform.position.z - z;
            mousePos = mainCamera.ScreenToWorldPoint(mousePos);
            return mousePos;
        }

        private static List<AssetBundle> _loadedAssetBundles = new List<AssetBundle>();

        public static AssetBundleRequest LoadAllFilesFromAssetBundle<T>(string bundleName) where T : Object
        {
            AssetBundle myLoadedAssetBundle = LoadAssetBundle(bundleName);
            return myLoadedAssetBundle.LoadAllAssetsAsync<T>();
        }

        private static AssetBundle LoadAssetBundle(string bundleName)
        {
            AssetBundle myLoadedAssetBundle = _loadedAssetBundles.FirstOrDefault(b => b.name == bundleName);
            if (myLoadedAssetBundle != null) return myLoadedAssetBundle;
            myLoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, bundleName));
            _loadedAssetBundles.Add(myLoadedAssetBundle);
            return myLoadedAssetBundle;
        }

        public static AssetBundleRequest LoadFileFromAssetBundle<T>(string bundleName, string assetName) where T : Object
        {
            AssetBundle myLoadedAssetBundle = LoadAssetBundle(bundleName);
            return myLoadedAssetBundle.LoadAssetAsync<T>(assetName);
        }

        public static float StartingTime => _startingTime;

        public static bool OnScreen(this GameObject gameObject)
        {
            return InCameraView(gameObject.transform.position);
        }

        public static int IntFromNode(this XmlNode root, string nodeName)
        {
            return int.Parse(StringFromNode(root, nodeName));
        }

        public static float FloatFromNode(this XmlNode root, string nodeName)
        {
            return float.Parse(StringFromNode(root, nodeName));
        }

        public static XmlNode GetNode(this XmlNode root, string nodeName)
        {
            if (root.Name == nodeName) return root;
            XmlNode node = root.SelectSingleNode(nodeName);
            if (node == null) throw new Exceptions.XmlNodeDoesNotExistException(nodeName);
            return node;
        }

        public static string StringFromNode(this XmlNode root, string name)
        {
            return GetNode(root, name).InnerText;
        }

        public static string NodeAttributeValue(this XmlNode root, string attributeName)
        {
            if (root.Attributes == null) throw new Exceptions.NodeHasNoAttributesException(root.Name);
            return root.Attributes[attributeName].Value;
        }

        public static bool BoolFromNode(this XmlNode root, string nodeName)
        {
            return StringFromNode(root, nodeName).ToLower() == "true";
        }

        private static Camera mainCamera;

        public static bool InCameraView(this Vector3 position, Camera camera = null)
        {
            if (camera == null)
            {
                if (mainCamera == null || mainCamera.gameObject == null) mainCamera = Camera.main;
                camera = mainCamera;
            }

            Vector3 screenPosition = camera.WorldToViewportPoint(position);
            return screenPosition.z > 0 &&
                   screenPosition.x > 0 &&
                   screenPosition.y > 0 &&
                   screenPosition.x < 1 &&
                   screenPosition.y < 1;
        }

        public static bool InCameraView(this Vector2 position, Camera camera = null)
        {
            return ((Vector3) position).InCameraView(camera);
        }

        public static T RemoveRandom<T>(this List<T> list)
        {
            int randomIndex = Random.Range(0, list.Count);
            T element = list[randomIndex];
            list.RemoveAt(randomIndex);
            return element;
        }

        public static List<object> ToObjectList<T>(this List<T> list)
        {
            List<object> objectList = new List<object>();
            int objectListCount = list.Count;
            for (int i = 0; i < objectListCount; ++i)
            {
                objectList.Add(list[i]);
            }

            return objectList;
        }

        private class MinSearchList<T>
        {
            private readonly List<T> _list;
            private readonly Func<T, float> _compare;
            private int indexPosition;

            public MinSearchList(List<T> list, Func<T, float> compare)
            {
                _list = list;
                _compare = compare;
            }

            public bool HasNext() => indexPosition != _list.Count;

            public T Next()
            {
                float smallestValue = float.MaxValue;
                int smallestElementIndex = -1;

                for (int i = indexPosition; i < _list.Count; ++i)
                {
                    T element = _list[i];
                    float candidateValue = _compare(element);
                    if (candidateValue >= smallestValue) continue;
                    smallestElementIndex = i;
                    smallestValue = candidateValue;
                }

                T smallestElement = _list[smallestElementIndex];
                _list[smallestElementIndex] = _list[indexPosition];
                _list[indexPosition] = smallestElement;
                ++indexPosition;
                return smallestElement;
            }
        }

        public static List<string> ReadLinesFromFile(string fileName)
        {
            TextAsset file = Resources.Load(fileName) as TextAsset;
            string contents = file.text;
            string[] tempLines = Regex.Split(contents, "\r\n|\r|\n");
            List<string> lines = new List<string>();
            foreach (string line in tempLines)
            {
                if (line != "")
                {
                    lines.Add(line);
                }
            }

            return lines;
        }

        public static string[] SplitAndRemoveWhiteSpace(string line)
        {
            return line.Split(',').Where(str => str != "").ToArray();
        }

        public static void ConstructObjectsFromCsv(string fileName, Action<string[]> constructionMethod)
        {
            List<string> lines = ReadLinesFromFile(fileName);
            foreach (string line in lines)
            {
                string[] attributes = line.Split(',');
                constructionMethod(attributes);
            }
        }

        public static void Shuffle<T>(this List<T> list)
        {
            List<T> randomList = new List<T>();
            while (list.Count > 0)
            {
                int removePosition = Random.Range(0, list.Count);
                T element = list[removePosition];
                randomList.Add(element);
                list.RemoveAt(removePosition);
            }

            list.Clear();
            list.AddRange(randomList);
        }

        public static void Shuffle<T>(this T[] arr)
        {
            List<T> list = new List<T>(arr);
            Shuffle(list);
            for (int i = 0; i < arr.Length; ++i)
            {
                arr[i] = list[i];
            }
        }

        public static List<T> FindAllComponentsInChildren<T>(Transform t)
        {
            return FindAllChildren(t).Select(child => child.GetComponent<T>()).Where(component => component != null).ToList();
        }

        public static List<Transform> FindAllChildren(Transform t)
        {
            List<Transform> children = new List<Transform>();
            int noChildren = t.childCount;
            for (int i = 0; i < noChildren; ++i)
            {
                Transform child = t.GetChild(i);
                children.Add(child);
                children.AddRange(FindAllChildren(child));
            }

            return children;
        }

        public static float Round(this float val, int precision = 0)
        {
            float precisionDivider = (float) Math.Pow(10f, precision);
            return (float) (Math.Round(val * precisionDivider) / precisionDivider);
        }

        public static T FindChildWithName<T>(this GameObject g, string name) where T : class
        {
            if (typeof(T) == typeof(GameObject)) throw new Exceptions.CannotGetGameObjectComponent();
            Transform t = g.transform;
            Transform foundChild = FindChildWithName(t, name);
            if (foundChild == null) throw new Exceptions.ChildNotFoundException(g, name);
            T foundComponent = foundChild.GetComponent<T>();
            if (foundComponent == null) throw new Exceptions.ComponentNotFoundException(foundChild.name, typeof(T));
            return foundComponent;
        }

        public static GameObject FindChildWithName(this GameObject g, string name)
        {
            Transform t = FindChildWithName(g.transform, name);
            return t != null ? t.gameObject : null;
        }

        public static Transform FindChildWithName(this Transform t, string name)
        {
            List<Transform> children = FindAllChildren(t);
            Transform foundChild = null;
            int noNameOccurences = 0;
            foreach (Transform child in children)
            {
                if (child.name != name) continue;
                ++noNameOccurences;
                foundChild = child;
            }

            if (noNameOccurences > 1) throw new Exceptions.UnspecificGameObjectNameException(noNameOccurences, name);
            return foundChild;
        }

        public static void Print<T>(this List<T> list)
        {
            string listString = "";
            for (int i = 0; i < list.Count; ++i)
            {
                if (i != 0) listString += ", ";
                listString += list[i];
            }

            Debug.Log(listString);
        }

        public static void PrintList<T>(this T[] arr)
        {
            new List<T>(arr).Print();
        }

        public static GameObject InstantiateUiObject<T>(string prefabLocation, Transform parent) where T : Component
        {
            GameObject newUiObject = InstantiateUiObject(prefabLocation, parent);
            newUiObject.AddComponent<T>();
            return newUiObject;
        }

        public static GameObject InstantiateUiObject(string prefabLocation, Transform parent) => InstantiateUiObject(Resources.Load(prefabLocation) as GameObject, parent);

        public static GameObject InstantiateUiObject(GameObject prefab, Transform parent)
        {
            GameObject newUiObject = Object.Instantiate(prefab);
            newUiObject.transform.SetParent(parent, false);
            newUiObject.transform.localScale = new Vector3(1, 1, 1);
            return newUiObject;
        }

        public static void SetNavigation(EnhancedButton origin, EnhancedButton target, Direction d)
        {
            SetNavigation(origin.Button(), target.Button(), d);
        }

        public static void SetNavigation(Button origin, EnhancedButton target, Direction d)
        {
            SetNavigation(origin, target.Button(), d);
        }

        public static void SetNavigation(EnhancedButton origin, Button target, Direction d)
        {
            SetNavigation(origin.Button(), target, d);
        }

        public static void SetNavigation(Button origin, Button target, Direction d)
        {
            if (origin == null || target == null) return;
            Navigation originButtonNavigation = origin.navigation;
            switch (d)
            {
                case Direction.Up:
                    originButtonNavigation.selectOnUp = target;
                    break;
                case Direction.Down:
                    originButtonNavigation.selectOnDown = target;
                    break;
                case Direction.Left:
                    originButtonNavigation.selectOnLeft = target;
                    break;
                case Direction.Right:
                    originButtonNavigation.selectOnRight = target;
                    break;
            }

            origin.navigation = originButtonNavigation;
        }

        public static void SetReciprocalNavigation(EnhancedButton origin, EnhancedButton target, Direction direction = Direction.Down)
        {
            SetReciprocalNavigation(origin.Button(), target.Button(), direction);
        }

        public static void SetReciprocalNavigation(EnhancedButton origin, Button target, Direction direction = Direction.Down)
        {
            SetReciprocalNavigation(origin.Button(), target, direction);
        }

        public static void SetReciprocalNavigation(Button origin, EnhancedButton target, Direction direction = Direction.Down)
        {
            SetReciprocalNavigation(origin, target.Button(), direction);
        }

        public static void SetReciprocalNavigation(Button origin, Button target, Direction direction = Direction.Down)
        {
            if (direction == Direction.Down)
            {
                SetNavigation(origin, target, Direction.Down);
                SetNavigation(target, origin, Direction.Up);
                return;
            }

            SetNavigation(origin, target, Direction.Left);
            SetNavigation(target, origin, Direction.Right);
        }

        public static Direction OppositeDirection(Direction inventoryDirection)
        {
            switch (inventoryDirection)
            {
                case Direction.Up:
                    return Direction.Down;
                case Direction.Down:
                    return Direction.Up;
                case Direction.Left:
                    return Direction.Right;
                case Direction.Right:
                    return Direction.Left;
                case Direction.None:
                    return Direction.None;
                default:
                    throw new ArgumentOutOfRangeException(nameof(inventoryDirection), inventoryDirection, null);
            }
        }

        private static float _startingTime = -1;

        public static float TimeInSeconds()
        {
            if (_startingTime == -1) _startingTime = Time.timeSinceLevelLoad;
            return Time.timeSinceLevelLoad;
        }

        public static bool ValuesHaveSameSign(float a, float b)
        {
            if (a > 0 && b <= 0 || a < 0 && b >= 0) return false;
            return true;
        }

        public static T RandomElement<T>(this T[] arr) => arr[Random.Range(0, arr.Length)];

        public static T RandomElement<T>(this List<T> arr) => arr[Random.Range(0, arr.Count)];

        public static float Normalise(this float value, float maxValue)
        {
            return value /= maxValue;
        }

        public static string AddSignPrefix(this float value)
        {
            if (value <= 0) return value.ToString();
            return "+" + value;
        }

        public static void AddDelineator(Transform parent)
        {
            GameObject delineator = new GameObject();
            delineator.AddComponent<RectTransform>();
            Image i = delineator.AddComponent<Image>();
            i.color = UiAppearanceController.FadedColour;
            LayoutElement layout = delineator.AddComponent<LayoutElement>();
            layout.minHeight = 2;
            layout.preferredWidth = 2000;
            delineator.transform.SetParent(parent);
            delineator.transform.localScale = new Vector3(1, 1, 1);
        }

        public static float Polarity(this float direction)
        {
            if (direction < 0) return -1;
            if (direction > 0) return 1;
            return 0;
        }

        public static T NextElement<T>(int iterator, List<T> list) => list[NextIndex(iterator, list)];

        public static T PrevElement<T>(int iterator, List<T> list) => list[PrevIndex(iterator, list)];

        public static int NextIndex<T>(int iteratorPosition, List<T> list)
        {
            if (iteratorPosition + 1 == list.Count) return 0;
            return iteratorPosition + 1;
        }

        public static int PrevIndex<T>(int iteratorPosition, List<T> list)
        {
            if (iteratorPosition - 1 == -1) return list.Count - 1;
            return iteratorPosition - 1;
        }

        public static T RemoveLast<T>(this List<T> list)
        {
            int endIndex = list.Count - 1;
            T end = list[endIndex];
            list.RemoveAt(endIndex);
            return end;
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

        public static string PrintHierarchy(GameObject parent)
        {
            string hierarchy = "";
            Transform t = parent.transform;
            while (t != null)
            {
                hierarchy = t.name + "/" + hierarchy;
                t = t.parent;
            }

            return hierarchy;
        }

        public static void Swap<T>(this List<T> list, int from, int to)
        {
            T temp = list[to];
            list[to] = list[from];
            list[from] = temp;
        }

        public static XmlNode OpenRootNode(string xmlName, string rootNodeName = null)
        {
            TextAsset xmlFile = Resources.Load<TextAsset>("XML/" + xmlName);
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlFile.text);
            if (rootNodeName == null) rootNodeName = xmlName;
            return xml.SelectSingleNode("//" + rootNodeName);
        }

        public static void ChangeImageAlpha(Image image, float newAlpha)
        {
            Color c = image.color;
            c.a = newAlpha;
            image.color = c;
        }

        public static void ChangeSpriteAlpha(SpriteRenderer sprite, float newAlpha)
        {
            Color c = sprite.color;
            c.a = newAlpha;
            sprite.color = c;
        }

        public static XmlNodeList GetNodesWithName(XmlNode root, string subNodeName)
        {
            XmlNodeList nodes = root.SelectNodes(subNodeName);
            if (nodes == null) throw new Exceptions.NoNodesWithNameException(subNodeName);
            return nodes;
        }

        public static void PrintTime(string message, Stopwatch stopWatch)
        {
            Debug.Log(message + stopWatch.Elapsed.ToString("mm\\:ss\\.ffff"));
        }
    }
}