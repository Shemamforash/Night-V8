using System.Collections.Generic;
using Game.Combat.Enemies;
using Game.Combat.Misc;
using Game.Exploration.Region;
using Game.Gear;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation
{
    public static class AreaGenerator
    {
        private const float SmallPolyWidth = 0.4f, MediumPolyWidth = 1f, LargePolyWidth = 2f, HugePolyWidth = 4f;

        private static List<Vector3> GenerateDistortedPoly(int definition, Ellipse ellipse)
        {
            int angleIncrement;
            List<Vector3> polyVertices = new List<Vector3>();
            for (int i = 0; i < 360; i += angleIncrement)
            {
                Vector3 vertex = RandomPointBetweenRadii(i, ellipse);
                polyVertices.Add(vertex);
                angleIncrement = Random.Range(definition / 2, definition);
            }

            return polyVertices;
        }

        private static Vector2 RandomPointBetweenRadii(float angle, Ellipse e)
        {
            Vector2 randomPoint;
            angle *= Mathf.Deg2Rad;
            if (e.IsCircle)
            {
                randomPoint = new Vector2();
                float pointRadius = Random.Range(e.InnerRingWidth, e.OuterRingWidth);
                randomPoint.x = pointRadius * Mathf.Cos(angle);
                randomPoint.y = pointRadius * Mathf.Sin(angle);
                return randomPoint;
            }

            Vector2 innerRadiusPoint = new Vector2();
            innerRadiusPoint.x = e.InnerRingWidth * Mathf.Cos(angle);
            innerRadiusPoint.y = e.InnerRingHeight * Mathf.Sin(angle);
            Vector2 outerRadiusPoint = new Vector2();
            outerRadiusPoint.x = e.OuterRingWidth * Mathf.Cos(angle);
            outerRadiusPoint.y = e.OuterRingHeight * Mathf.Sin(angle);

            randomPoint = outerRadiusPoint - innerRadiusPoint;
            randomPoint *= Random.Range(0f, 1f);
            randomPoint += innerRadiusPoint;
            return randomPoint;
        }

        public static List<Vector3> GeneratePoly(float baseWidth)
        {
            float width = baseWidth * Random.Range(0.6f, 1f);
            float radius = width / 2f;
            float minX = Random.Range(0, radius * 0.9f);
            float minY = Random.Range(0, radius * 0.9f);
            float maxX = Random.Range(minX, radius);
            float maxY = Random.Range(minY, radius);
//            Ellipse e = new Ellipse(minX, minY, maxX, maxY);
            Ellipse e = new Ellipse(radius * 0.8f, radius);
            List<Vector3> barrierVertices = GenerateDistortedPoly(50, e);
            return barrierVertices;
        }

        private static List<EnemyCampfire> GenerateCampfires(List<Barrier> barriers)
        {
            float size = PathingGrid.GameWorldWidth / 2f;
            Vector2 campfirePosition = new Vector2(Random.Range(-size, size), Random.Range(-size, size));
            EnemyCampfire campfire = new EnemyCampfire(campfirePosition);
            for (int i = barriers.Count - 1; i >= 0; --i)
            {
                Barrier b = barriers[i];
                foreach (Vector2 v in b.WorldVerts)
                {
                    if (Vector2.Distance(v, campfirePosition) > 2f) continue;
                    barriers.RemoveAt(i);
                    break;
                }
            }
            barriers.AddRange(campfire._stones);
            return new List<EnemyCampfire> {campfire};
        }

        public static void GenerateArea(Region region)
        {
            List<Barrier> barriers = new List<Barrier>();
            int barrierNumber = 0;

            List<Vector2> positions = AdvancedMaths.GetPoissonDiscDistribution(500, 1f, 3f, PathingGrid.GameWorldWidth, true);
            positions.RemoveAt(0);

            foreach (Vector2 position in positions)
            {
                List<Vector3> barrierVertices = GeneratePoly(SmallPolyWidth);
                Barrier barrier = new Barrier(barrierVertices.ToArray(), "Barrier " + barrierNumber, position);
                barriers.Add(barrier);
                ++barrierNumber;
            }

            region.Fires = GenerateCampfires(barriers);
            region.Barriers = barriers;
            DesolationInventory inventory = new DesolationInventory("Cache");
            inventory.Move(WeaponGenerator.GenerateWeapon(ItemQuality.Shining, WeaponType.Rifle, 5), 1);
            ContainerController container = new ContainerController(AdvancedMaths.RandomVectorWithinRange(region.Fires[0].FirePosition, 1), inventory);
            region.Containers.Add(container);
        }

        private class Ellipse
        {
            public readonly float InnerRingWidth, InnerRingHeight, OuterRingWidth, OuterRingHeight;
            public readonly bool IsCircle;

            public Ellipse(float innerRingWidth, float innerRingHeight, float outerRingWidth, float outerRingHeight)
            {
                InnerRingWidth = innerRingWidth;
                InnerRingHeight = innerRingHeight;
                OuterRingWidth = outerRingWidth;
                OuterRingHeight = outerRingHeight;
            }

            public Ellipse(float innerRadius, float outerRadius) : this(innerRadius, innerRadius, outerRadius, outerRadius)
            {
                IsCircle = true;
            }
        }
    }
}