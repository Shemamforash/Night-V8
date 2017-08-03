using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

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

    public static List<Transform> FindAllChildren(Transform t){
        List<Transform> children = new List<Transform>();
        int noChildren = t.childCount;
        for(int i = 0; i < noChildren; ++i){
            Transform child = t.GetChild(i);
            children.Add(child);
            children.AddRange(FindAllChildren(child));
        }
        return children;
    }
}
