using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Articy.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper
{
    public static class Helper
    {
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

        public static void ConstructObjectsFromCsv(string fileName, Action<string[]> constructionMethod)
        {
            List<string> lines = ReadLinesFromFile(fileName);
            foreach (string line in lines)
            {
                string[] attributes = line.Split(',');
                constructionMethod(attributes);
            }
        }

        public static void Log<T>(List<T> aList)
        {
            foreach (T t in aList)
            {
                Debug.Log(t);
            }
        }

        public static List<T> FindAllComponentsInChildren<T>(Transform t)
        {
            List<T> childrenWithComponents = new List<T>();
            foreach (Transform child in FindAllChildren(t))
            {
                T component = child.GetComponent<T>();
                if (component != null)
                {
                    childrenWithComponents.Add(component);
                }
            }
            return childrenWithComponents;
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

        public static float Round(float val, int precision)
        {
            float precisionDivider = (float)Math.Pow(10f, precision);
            return (float)(Math.Round(val * precisionDivider) / precisionDivider);
        }

        public static T FindChildWithName<T>(GameObject g, string name)
        {
            if (typeof(T) == typeof(GameObject))
            {
                throw new Exceptions.CannotGetGameObjectComponent();
            }
            Transform t = g.transform;
            return FindChildWithName(t, name).GetComponent<T>();
        }

        public static GameObject FindChildWithName(GameObject g, string name)
        {
            return FindChildWithName(g.transform, name).gameObject;
        }

        public static Transform FindChildWithName(Transform t, string name)
        {
            List<Transform> children = FindAllChildren(t);
            Transform foundChild = null;
            int noNameOccurences = 0;
            foreach (Transform child in children)
            {
                if (child.name == name)
                {
                    ++noNameOccurences;
                    foundChild = child;
                }
            }
            if (noNameOccurences > 1)
            {
                throw new Exceptions.UnspecificGameObjectNameException(noNameOccurences, name);
            }
            return foundChild;
        }

        public static void PrintList<T>(List<T> list)
        {
            string listString = "";
            for (int i = 0; i < list.Count; ++i)
            {
                if (i != 0)
                {
                    listString += ", ";
                }
                listString += list[i];
            }
            Debug.Log(listString);
        }

        public static void PrintList<T>(T[] arr)
        {
            PrintList(new List<T>(arr));
        }
        
        public enum NavigationDirections
        {
            Up,
            Down,
            Left,
            Right
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
            GameObject newUiObject = GameObject.Instantiate(prefab);
            newUiObject.transform.SetParent(parent);
            newUiObject.transform.localScale = new Vector3(1, 1, 1);
            return newUiObject;
        }

        public static void SetNavigation(GameObject origin, GameObject target, NavigationDirections d)
        {
            Button originButton = origin.GetComponent<Button>();
            Button targetButton = target.GetComponent<Button>();
            Navigation originButtonNavigation = originButton.navigation;
            switch (d)
            {
                case NavigationDirections.Up:
                    originButtonNavigation.selectOnUp = targetButton;
                    break;
                case NavigationDirections.Down:
                    originButtonNavigation.selectOnDown = targetButton;
                    break;
                case NavigationDirections.Left:
                    originButtonNavigation.selectOnLeft = targetButton;
                    break;
                case NavigationDirections.Right:
                    originButtonNavigation.selectOnRight = targetButton;
                    break;
            }
            originButton.navigation = originButtonNavigation;
        }

        public static void SetReciprocalNavigation(GameObject origin, GameObject target)
        {
            SetNavigation(origin, target, NavigationDirections.Down);
            SetNavigation(target, origin, NavigationDirections.Up);
        }
    }
}