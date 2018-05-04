using System.Collections.Generic;
using System.Linq;
using Game.Combat.Misc;
using Game.Exploration.Regions;
using Game.Gear;
using Game.Gear.Weapons;
using Game.Global;
using NUnit.Framework;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation
{
    public static class AreaGenerator
    {
        private const float MinPolyWidth = 0.1f, SmallPolyWidth = 0.2f, MediumPolyWidth = 2f, LargePolyWidth = 4f, HugePolyWidth = 8f;

        private static void GenerateGenericRock(float scale, float radiusVariation, float smoothness, Vector2? position = null)
        {
            smoothness = Mathf.Clamp(1 - smoothness, 0.01f, 1f);
            radiusVariation = 1 - Mathf.Clamp(radiusVariation, 0f, 1f);

            Assert.IsTrue(radiusVariation <= 1);
            Assert.IsTrue(smoothness <= 1);

            float definition = smoothness * 150f;
            radiusVariation *= scale;
            float minX = Random.Range(0, radiusVariation);
            float minY = Random.Range(0, radiusVariation);
            float maxX = Random.Range(minX, scale);
            float maxY = Random.Range(minY, scale);
//            Ellipse e = new Ellipse(minX, minY, maxX, maxY);
            Ellipse e = new Ellipse(radiusVariation, scale);
            float angleIncrement;
            List<Vector3> barrierVertices = new List<Vector3>();
            for (float i = 0; i < 360; i += angleIncrement)
            {
                Vector3 vertex = RandomPointBetweenRadii(i, e);
                barrierVertices.Add(vertex);
                angleIncrement = Random.Range(definition / 2f, definition);
            }

            AssignRockPosition(scale, barrierVertices, position, scale);
        }

        private static void AssignRockPosition(float radius, List<Vector3> barrierVertices, Vector2? position, float scale)
        {
            if (position == null) position = GetValidPosition(radius);
            if (position == Vector2.negativeInfinity) return;
            Barrier barrier = new Barrier(barrierVertices.ToArray(), "Barrier " + barrierNumber, (Vector2) position, scale);
            barriers.Add(barrier);
            ++barrierNumber;
        }


        private static Vector2 GetValidPosition(float radius)
        {
            for (int i = 0; i < MaxPlacementAttempts; ++i)
            {
                Vector2 randomPosition = Helper.RandomInList(validPositions);
                if (ShapeIsTooCloseToPoint(randomPosition, radius * 1.1f)) continue;
                validPositions.Remove(randomPosition);
                return randomPosition;
            }

            return Vector2.negativeInfinity;
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

        private const int MaxPlacementAttempts = 20;

        private static void GenerateHugeRock() => GenerateGenericRock(Random.Range(LargePolyWidth, HugePolyWidth), 0.25f, 0.75f);

        private static void GenerateLargeRock() => GenerateGenericRock(Random.Range(MediumPolyWidth, LargePolyWidth), 0.7f, 0.5f);

        private static void GenerateMediumRock() => GenerateGenericRock(Random.Range(SmallPolyWidth, MediumPolyWidth), 0.5f, 0.75f);

        private static void GenerateSmallRock() => GenerateGenericRock(Random.Range(MinPolyWidth, SmallPolyWidth), 0.1f, 0.75f);


        private static List<Vector2> validPositions;
        private static List<Vector2> allValidPositions;
        private static int barrierNumber;

        //generate fire -> huge rock -> large rock -> medium rock -> small rock

        private static bool ShapeIsTooCloseToPoint(Vector2 point, float distance)
        {
            return barriers.Any(b =>
            {
                float minDistance = distance + b.Radius * 1.1f;
                return Vector2.Distance(b.Position, point) <= minDistance;
            });
        }

        public static void GenerateForest(Region region)
        {
            GenerateArea(region, Random.Range(0, 2), 0, 0, Random.Range(150, 300));
        }

        public static void GenerateCanyon(Region region)
        {
            GenerateArea(region, Random.Range(5, 10), Random.Range(5, 10), Random.Range(20, 30), 0);
        }

        private static void GenerateArea(Region region, int hugeRockCount, int largeRockCount, int mediumRockCount, int smallRockCount)
        {
            barriers = new List<Barrier>();
            barrierNumber = 0;
            if (allValidPositions == null) allValidPositions = AdvancedMaths.GetPoissonDiscDistribution(1000, 1f, 3f, PathingGrid.CombatAreaWidth / 2f);
            validPositions = new List<Vector2>();
            allValidPositions.ForEach(v => validPositions.Add(v));

            region.Fire = GenerateFire();
            for (int i = 0; i < hugeRockCount; ++i) GenerateHugeRock();
            for (int i = 0; i < largeRockCount; ++i) GenerateLargeRock();
            for (int i = 0; i < mediumRockCount; ++i) GenerateMediumRock();
            for (int i = 0; i < smallRockCount; ++i) GenerateSmallRock();
            region.Barriers = barriers;

            DesolationInventory inventory = new DesolationInventory("Cache");
            inventory.Move(WeaponGenerator.GenerateWeapon(ItemQuality.Shining, WeaponType.Rifle, 5), 1);
            ContainerController container = new ContainerController(AdvancedMaths.RandomVectorWithinRange(region.Fire.FirePosition, 1), inventory);
            region.Containers.Add(container);
        }

        private static List<Barrier> barriers;

        private static EnemyCampfire GenerateFire()
        {
            float size = PathingGrid.CombatAreaWidth / 2f;
            Vector2 campfirePosition = AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 2);
            int numberOfStones = Random.Range(6, 10);
            float radius = 0.2f;
            int angle = 0;
            int angleStep = 360 / numberOfStones;
            while (numberOfStones > 0)
            {
                float randomisedAngle = (angle + Random.Range(-angleStep, angleStep)) * Mathf.Deg2Rad;
                float x = radius * Mathf.Cos(randomisedAngle) + campfirePosition.x;
                float y = radius * Mathf.Sin(randomisedAngle) + campfirePosition.y;
                GenerateGenericRock(0.05f, 0.2f, 0.5f, new Vector2(x, y));
                angle += angleStep;
                --numberOfStones;
            }

            return new EnemyCampfire(campfirePosition);
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