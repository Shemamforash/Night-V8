using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FastLights;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace SamsHelper.Libraries
{
    public static class Helper
    {
        public static bool IsObjectInCameraView(GameObject gameObject)
        {
            Vector3 screenPosition = Camera.main.WorldToViewportPoint(gameObject.transform.position);
            return screenPosition.z > 0 &&
                screenPosition.x > 0 &&
                screenPosition.y > 0 &&
                screenPosition.x < 1 &&
                screenPosition.y < 1;
        }

        public static string VectorToString(Vector2 vector)
        {
            return vector.x + ":" + vector.y;
        }
        
        public static string VectorToString(Vector3 vector)
        {
            return vector.x + ":" + vector.y + ":" + vector.z;
        }

        public static Vector3 StringToVector3(string vectorString)
        {
            string[] arr = vectorString.Split(':');
            Vector3 vect = new Vector3();
            vect.x = float.Parse(arr[0]);
            vect.y = float.Parse(arr[1]);
            vect.z = float.Parse(arr[2]);
            return vect;
        }
        
        public static Vector2 StringToVector2(string vectorString)
        {
            string[] arr = vectorString.Split(':');
            Vector2 vect = new Vector2();
            vect.x = float.Parse(arr[0]);
            vect.y = float.Parse(arr[1]);
            return vect;
        }
        
        private class MinSearchList<T>
        {
            private List<T> _list;
            private Func<T, float> _compare;
            private int indexPosition;

            public MinSearchList(List<T> list, Func<T, float> compare)
            {
                _list = list;
                _compare = compare;
            }

            public bool HasNext()
            {
                return indexPosition != _list.Count;
            }
            
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
                if (line != "")
                    lines.Add(line);
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

        public static void Shuffle<T>(ref List<T> list)
        {
            List<T> randomList = new List<T>();
            while (list.Count > 0)
            {
                int removePosition = Random.Range(0, list.Count);
                T element = list[removePosition];
                randomList.Add(element);
                list.RemoveAt(removePosition);
            }

            list = randomList;
        }

        public static void Log<T>(List<T> aList)
        {
            foreach (T t in aList) Debug.Log(t);
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

        public static float Round(float val, int precision = 0)
        {
            float precisionDivider = (float) Math.Pow(10f, precision);
            return (float) (Math.Round(val * precisionDivider) / precisionDivider);
        }

        public static T FindChildWithName<T>(GameObject g, string name) where T : class
        {
            if (typeof(T) == typeof(GameObject)) throw new Exceptions.CannotGetGameObjectComponent();
            Transform t = g.transform;
            Transform foundChild = FindChildWithName(t, name);
            if (foundChild == null) throw new Exceptions.ChildNotFoundException(g, name);
            T foundComponent = foundChild.GetComponent<T>();
            if (foundComponent == null) throw new Exceptions.ComponentNotFoundException(foundChild.name, foundComponent.GetType());
            return foundComponent;
        }

        public static GameObject FindChildWithName(GameObject g, string name)
        {
            Transform t = FindChildWithName(g.transform, name);
            return t != null ? t.gameObject : null;
        }

        public static Transform FindChildWithName(Transform t, string name)
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

        public static void PrintList<T>(List<T> list)
        {
            string listString = "";
            for (int i = 0; i < list.Count; ++i)
            {
                if (i != 0) listString += ", ";
                listString += list[i];
            }
            Debug.Log(listString);
        }

        public static void PrintList<T>(T[] arr)
        {
            PrintList(new List<T>(arr));
        }


        public static GameObject InstantiateUiObject<T>(string prefabLocation, Transform parent) where T : Component
        {
            GameObject newUiObject = InstantiateUiObject(prefabLocation, parent);
            newUiObject.AddComponent<T>();
            return newUiObject;
        }

        public static GameObject InstantiateUiObject(string prefabLocation, Transform parent)
        {
            return InstantiateUiObject(Resources.Load(prefabLocation) as GameObject, parent);
        }

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

        public static long TimeInMillis()
        {
            return DateTime.Now.Ticks / 10000;
        }

        public static bool ValuesHaveSameSign(float a, float b)
        {
            if (a > 0 && b <= 0 || a < 0 && b >= 0) return false;
            return true;
        }

        public static T RandomInList<T>(T[] arr)
        {
            return arr[Random.Range(0, arr.Length)];
        }

        public static T RandomInList<T>(List<T> arr)
        {
            return arr[Random.Range(0, arr.Count)];
        }

        public static float Normalise(float value, float maxValue)
        {
            return value /= maxValue;
        }

        public static string AddSignPrefix(float value)
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

        public static float Polarity(float direction)
        {
            if (direction < 0) return -1;
            if (direction > 0) return 1;
            return 0;
        }

        public static T NextElement<T>(int iterator, List<T> list)
        {
            return list[NextIndex(iterator, list)];
        }

        public static T PrevElement<T>(int iterator, List<T> list)
        {
            return list[PrevIndex(iterator, list)];
        }
        
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

        public static T RemoveEnd<T>(List<T> list)
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
            if (randomAlpha) c.a = Random.Range(0f, 1f);
            else c.a = 1;
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
    }
}