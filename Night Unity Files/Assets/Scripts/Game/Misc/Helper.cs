using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Game.Misc;
using NUnit.Framework;
using UnityEngine.UI;

public static class Helper
{
    public static string[] ReadLinesFromFile(string fileName)
    {
        TextAsset file = Resources.Load(fileName) as TextAsset;
        string contents = file.text;
        string[] lines = Regex.Split(contents, "\r\n|\r|\n");
        return lines;
    }

    public static void Log<T>(List<T> aList)
    {
        foreach (T t in aList)
        {
            Debug.Log(t);
        }
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
        Transform t = g.transform;
        return FindChildWithName(t, name).GetComponent<T>();
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
            throw new Exceptions.UnspecificGameObjectNameException(noNameOccurences);
        }
        return foundChild;
    }

    public enum NavigationDirections
    {
        Up,
        Down,
        Left,
        Right
    };

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
}