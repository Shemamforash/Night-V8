using System.Collections.Generic;
using UnityEngine;

public static class Helper
{
    public static void Log<T>(List<T> aList)
    {
        foreach (T t in aList)
        {
            Debug.Log(t);
        }
    }
}
