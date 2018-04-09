﻿using Game.Exploration.Environment;
using UnityEngine;

namespace Game.Exploration.Ui
{
    public class UiPathDrawController : MonoBehaviour
    {
        public static void CreatePathBetweenNodes(MapNode from, MapNode to)
        {
            GameObject pathObject = new GameObject();
            pathObject.transform.SetParent(GameObject.Find("Path").transform);
            pathObject.transform.position = Vector2.zero;
            pathObject.AddComponent<Path>().DrawPath(from, to);
        }
    }
}