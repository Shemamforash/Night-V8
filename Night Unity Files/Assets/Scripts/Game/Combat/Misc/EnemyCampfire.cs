using System.Xml;
using Facilitating.Persistence;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class EnemyCampfire : IPersistenceTemplate
    {
        public readonly Vector2 FirePosition;
        private static GameObject _prefab;

        public EnemyCampfire(Vector2 position)
        {
            FirePosition = position;
        }
        public void Load(XmlNode doc, PersistenceType saveType)
        {
        }

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            XmlNode campfireNode = SaveController.CreateNodeAndAppend("Campfire", doc);
            SaveController.CreateNodeAndAppend("Position", campfireNode, Helper.VectorToString(FirePosition));
            return doc;
        }
        
        public void CreateObject()
        {
            if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Campfire");
            GameObject campfireObject = GameObject.Instantiate(_prefab);
            campfireObject.transform.position = FirePosition;
            campfireObject.transform.localScale = Vector3.one;
            campfireObject.AddComponent<CampfireBehaviour>();
        }

        private class CampfireBehaviour : MonoBehaviour
        {
            private FastLight _light;

            public void Awake()
            {
                _light = Helper.FindChildWithName<FastLight>(gameObject, "Light");
            }

            public void Update()
            {
                Color c = _light.Colour;
                c.a = Mathf.PerlinNoise(Time.time, 0) * 0.5f + 0.5f;
                _light.Colour = c;
            }
        }
    }
}