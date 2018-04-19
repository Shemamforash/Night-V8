using Game.Exploration.Environment;
using Game.Exploration.Regions;
using UnityEngine;

namespace Game.Exploration.Ui
{
    public class UiPathDrawController : MonoBehaviour
    {
        public static void CreatePathBetweenNodes(Region from, Region to)
        {
            GameObject pathObject = new GameObject();
            pathObject.transform.SetParent(GameObject.Find("Path").transform);
            pathObject.transform.position = Vector2.zero;
            pathObject.AddComponent<Path>().DrawPath(from, to);
        }
    }
}