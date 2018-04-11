using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Combat.Generation;
using LOS;
using SamsHelper.Libraries;
using SamsHelper.Persistence;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class EnemyCampfire : IPersistenceTemplate
    {
        private const float StoneWidth = 0.1f;
        public readonly Vector2 FirePosition;
        public List<Barrier> _stones;
        private static GameObject _prefab;

        public EnemyCampfire(Vector2 position)
        {
            FirePosition = position;
            GenerateStones();
        }
        public void Load(XmlNode doc, PersistenceType saveType)
        {
        }

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            XmlNode campfireNode = SaveController.CreateNodeAndAppend("Campfire", doc);
            _stones.ForEach(b => b.Save(campfireNode, saveType));
            SaveController.CreateNodeAndAppend("Position", campfireNode, Helper.VectorToString(FirePosition));
            return doc;
        }
        
        private void GenerateStones()
        {
            _stones = new List<Barrier>();
            int numberOfStones = Random.Range(6, 10);
            float radius = 0.2f;
            int angle = 0;
            int angleStep = 360 / numberOfStones;
            while (numberOfStones > 0)
            {
                float randomisedAngle = (angle + Random.Range(-angleStep, angleStep)) * Mathf.Deg2Rad;
                float x = radius * Mathf.Cos(randomisedAngle) + FirePosition.x;
                float y = radius * Mathf.Sin(randomisedAngle) + FirePosition.y;
                List<Vector3> stoneVertices = AreaGenerator.GeneratePoly(StoneWidth);
                Barrier stone = new Barrier(stoneVertices.ToArray(), "Stone " + numberOfStones, new Vector2(x, y));
                _stones.Add(stone);
                angle += angleStep;
                --numberOfStones;
            }
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
            private LOSRadialLight _light;

            public void Awake()
            {
                _light = Helper.FindChildWithName<LOSRadialLight>(gameObject, "Light");
            }

            public void Update()
            {
                Color c = _light.color;
                c.a = Mathf.PerlinNoise(Time.time, 0) * 0.5f + 0.5f;
                _light.color = c;
            }
        }
    }
}