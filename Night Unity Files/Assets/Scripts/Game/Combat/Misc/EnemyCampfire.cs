using System.Collections.Generic;
using Game.Combat.Generation;
using LOS;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class EnemyCampfire : MonoBehaviour
    {
        private const float StoneWidth = 0.1f;
        private LOSRadialLight _light;

        public static List<AreaGenerator.Shape> Create(Vector2 position)
        {
            GameObject campfireObject = Instantiate(Resources.Load<GameObject>("Prefabs/Combat/Campfire"));
            campfireObject.transform.position = position;
            campfireObject.transform.localScale = Vector3.one;
            return campfireObject.GetComponent<EnemyCampfire>().GenerateStones();
        }

        private List<AreaGenerator.Shape> GenerateStones()
        {
            List<AreaGenerator.Shape> stones = new List<AreaGenerator.Shape>();
            int numberOfStones = Random.Range(6, 10);
            while (numberOfStones > 0)
            {
                stones.Add(AreaGenerator.Instance().GeneratePoly(StoneWidth));
                --numberOfStones;
            }

            float radius = 0.2f;
            int angle = 0;
            int angleStep = 360 / stones.Count;
            foreach (AreaGenerator.Shape stone in stones)
            {
                float randomisedAngle = (angle + Random.Range(-angleStep, angleStep)) * Mathf.Deg2Rad;
                float x = radius * Mathf.Cos(randomisedAngle) + transform.position.x;
                float y = radius * Mathf.Sin(randomisedAngle) + transform.position.y;
                stone.ShapeObject.name = "Stone " + stones.IndexOf(stone);
                stone.ShapeObject.transform.SetParent(transform);
                stone.SetPosition(x, y);
                angle += angleStep;
            }

            return stones;
        }

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