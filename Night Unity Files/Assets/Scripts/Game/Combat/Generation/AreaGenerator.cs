using System;
using System.Collections.Generic;
using System.Linq;
using Game.Combat.Misc;
using Game.Exploration.Regions;
using Game.Gear;
using Game.Gear.Weapons;
using Game.Global;
using NUnit.Framework;
using SamsHelper;
using SamsHelper.Libraries;
using UnityEngine;
using Node = SamsHelper.Libraries.Node;
using Random = UnityEngine.Random;

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
            List<Vector2> barrierVertices = new List<Vector2>();
            for (float i = 0; i < 360; i += angleIncrement)
            {
                Vector2 vertex = RandomPointBetweenRadii(i, e);
                barrierVertices.Add(vertex);
                angleIncrement = Random.Range(definition / 2f, definition);
            }

            AssignRockPosition(scale, barrierVertices, position, scale);
        }

        private static void GenerateRockWall(Vector2 position, float width, float bumpiness, float depth)
        {
            Assert.IsTrue(bumpiness > 0 && bumpiness < 1);
            Assert.IsTrue(depth > 0 && depth < 1);
            depth *= width / 2f;
            bumpiness = 1 - bumpiness;
            List<Vector2> wallVertices = new List<Vector2>();
            Vector2 topLeft = new Vector2(position.x - width / 2f, position.y + width / 2f);
            Vector2 topRight = new Vector2(position.x + width / 2f, position.y + width / 2f);
            Vector2 bottomLeft = new Vector2(position.x - width / 2f, position.y - width / 2f);
            Vector2 bottomRight = new Vector2(position.x + width / 2f, position.y - width / 2f);
            wallVertices.Add(bottomRight);
            wallVertices.Add(bottomLeft);
            wallVertices.Add(topLeft);
            wallVertices.Add(topRight);
            float y = topRight.y;
            while (y > bottomRight.y)
            {
                float noise = Mathf.PerlinNoise(width, y);
                float x = topRight.x - depth * noise;
                wallVertices.Add(new Vector3(x, y));
                y -= Random.Range(0f, bumpiness);
            }

            Barrier wall = new Barrier(wallVertices, "Wall " + wallNumber, position, width);
            barriers.Add(wall);
            ++wallNumber;
        }

        private static void AssignRockPosition(float radius, List<Vector2> barrierVertices, Vector2? position, float scale)
        {
            if (position == null) position = GetValidPosition();
            if (position == null) return;
            Barrier barrier = new Barrier(barrierVertices, "Barrier " + barrierNumber, (Vector2) position, scale);
            barriers.Add(barrier);
            ++barrierNumber;
        }

        private static Vector2? GetValidPosition()
        {
            Vector2 randomPosition = Helper.RandomInList(validPositions);
            validPositions.Remove(randomPosition);
            return randomPosition;
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

        private static void GenerateHugeRock() => GenerateGenericRock(Random.Range(LargePolyWidth, HugePolyWidth), 0.25f, 0.75f);

        private static void GenerateLargeRock() => GenerateGenericRock(Random.Range(MediumPolyWidth, LargePolyWidth), 0.7f, 0.5f);

        private static void GenerateMediumRock() => GenerateGenericRock(Random.Range(SmallPolyWidth, MediumPolyWidth), 0.5f, 0.75f);

        private static void GenerateSmallRock() => GenerateGenericRock(Random.Range(MinPolyWidth, SmallPolyWidth), 0.1f, 0.75f);


        private static List<Vector2> validPositions;
        private static List<Vector2> allValidPositions;
        private static int barrierNumber, wallNumber;

        //generate fire -> huge rock -> large rock -> medium rock -> small rock

        public static void GenerateSplitRock(Region region)
        {
            barriers = new List<Barrier>();
            GenerateSplinteredArea(5);
            for (int i = barriers.Count - 1; i >= 0; --i)
            {
                if(Random.Range(0, 2) == 0) barriers.RemoveAt(i);
            }
            barrierNumber = 0;
            if (allValidPositions == null) allValidPositions = AdvancedMaths.GetPoissonDiscDistribution(1000, 1f, 3f, PathingGrid.CombatAreaWidth / 2f);
            validPositions = new List<Vector2>();
            allValidPositions.ForEach(v => validPositions.Add(v));

//            GenerateRockWall(Vector3.one * 3, 5f, 0.5f, 0.5f);
//            GenerateMediumRock();

            region.Barriers = barriers;
        }

        private static void PlaceFire(Region region)
        {
            for (int i = 0; i < validPositions.Count; ++i)
            {
                Vector2 topLeft = new Vector2(validPositions[i].x - 0.5f, validPositions[i].y - 0.5f);
                Vector2 bottomRight = new Vector2(validPositions[i].x + 0.5f, validPositions[i].y + 0.5f);
                if (validPositions[i].magnitude > 10) continue;
                if (PathingGrid.WorldToCellPosition(validPositions[i]) == null) continue;
                if (!PathingGrid.IsSpaceAvailable(topLeft, bottomRight)) continue;
                region.Fire = GenerateFire(validPositions[i]);
                validPositions.RemoveAt(i);
                return;
            }

            int randomIndex = Random.Range(0, barriers.Count);
            Barrier b = barriers[randomIndex];
            barriers.RemoveAt(randomIndex);
            region.Fire = GenerateFire(b.Position);
        }

        private static EnemyCampfire GenerateFire(Vector2 position)
        {
            float size = PathingGrid.CombatAreaWidth / 2f;
            int numberOfStones = Random.Range(6, 10);
            float radius = 0.2f;
            int angle = 0;
            int angleStep = 360 / numberOfStones;
            while (numberOfStones > 0)
            {
                float randomisedAngle = (angle + Random.Range(-angleStep, angleStep)) * Mathf.Deg2Rad;
                float x = radius * Mathf.Cos(randomisedAngle) + position.x;
                float y = radius * Mathf.Sin(randomisedAngle) + position.y;
                GenerateGenericRock(0.05f, 0.2f, 0.5f, new Vector2(x, y));
                angle += angleStep;
                --numberOfStones;
            }

            return new EnemyCampfire(position);
        }

        public static void GenerateForest(Region region)
        {
            GenerateArea(region, Random.Range(0, 2), 0, 0, Random.Range(150, 300));
        }

        public static void GenerateCanyon(Region region)
        {
            GenerateArea(region, Random.Range(5, 10), Random.Range(5, 10), Random.Range(20, 30), 0);
        }

        private static void AddItems(Region region)
        {
            int itemsToAdd = 10;
            Helper.Shuffle(ref validPositions);
            for (int i = validPositions.Count - 1; i >= 0 && itemsToAdd > 0; --i)
            {
                if (validPositions[i].magnitude > PathingGrid.CombatAreaWidth) continue;
                Cell c = PathingGrid.WorldToCellPosition(validPositions[i]);
                if (c == null) continue;
                if (!c.Reachable) continue;
                DesolationInventory inventory = new DesolationInventory("Cache");
                inventory.Move(WeaponGenerator.GenerateWeapon(ItemQuality.Shining, WeaponType.Rifle, 5), 1);
                ContainerController container = new ContainerController(validPositions[i], inventory);
                region.Containers.Add(container);
                --itemsToAdd;
                validPositions.RemoveAt(i);
            }
        }

        private static void GenerateArea(Region region, int hugeRockCount, int largeRockCount, int mediumRockCount, int smallRockCount)
        {
            barriers = new List<Barrier>();
            barrierNumber = 0;
            if (allValidPositions == null) allValidPositions = AdvancedMaths.GetPoissonDiscDistribution(1000, 1f, 3f, PathingGrid.CombatAreaWidth / 2f);
            validPositions = new List<Vector2>();
            allValidPositions.ForEach(v => validPositions.Add(v));

            for (int i = 0; i < hugeRockCount; ++i) GenerateHugeRock();
            for (int i = 0; i < largeRockCount; ++i) GenerateLargeRock();
            for (int i = 0; i < mediumRockCount; ++i) GenerateMediumRock();
            for (int i = 0; i < smallRockCount; ++i) GenerateSmallRock();
            List<Barrier> invalidBarriers = new List<Barrier>();
            PathingGrid.InitialiseGrid();
            barriers.ForEach(b =>
            {
                if (PathingGrid.AddBarrier(b)) return;
                invalidBarriers.Add(b);
            });
            invalidBarriers.ForEach(b => barriers.Remove(b));

            region.Barriers = barriers;

            PathingGrid.FinaliseGrid();

            PlaceFire(region);

            AddItems(region);
        }

        private static List<Barrier> barriers;

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

        private class CrackNode : Node
        {
            public readonly float AngleFrom, AngleRange, Angle;
            private readonly List<CrackNode> _neighbors = new List<CrackNode>();

            public static CrackNode Create(float angleFrom, float angleRange, float radius)
            {
                float randomAngle = Random.Range(angleFrom, angleFrom + angleRange);
                float x = Mathf.Cos(randomAngle * Mathf.Deg2Rad) * radius;
                float y = Mathf.Sin(randomAngle * Mathf.Deg2Rad) * radius;
                Vector2 nodePosition = new Vector2(x, y);
                return new CrackNode(nodePosition, angleFrom, angleRange, randomAngle);
            }

            private CrackNode(Vector2 position, float angleFrom, float angleRange, float angle) : base(position)
            {
                AngleFrom = angleFrom;
                AngleRange = angleRange;
                Angle = angle;
            }
        }

        private static void JoinNeighbors(List<CrackNode> points, List<Edge> existingEdges)
        {
            for (int i = 0; i < points.Count; ++i)
            {
                int nextIndex = i + 1 == points.Count ? 0 : i + 1;
                int previousIndex = i - 1 == -1 ? points.Count - 1 : i - 1;
                CrackNode next = points[nextIndex];
                CrackNode previous = points[previousIndex];
                bool previousPointIntersects = false;
                bool nextPointIntersects = false;
                if (existingEdges != null)
                {
                    foreach (Edge e in existingEdges)
                    {
                        if (nextPointIntersects && previousPointIntersects) break;
                        if (!nextPointIntersects && AdvancedMaths.LineIntersection(points[i].Position, next.Position, e.A.Position, e.B.Position).Item1) nextPointIntersects = true;
                        if (!nextPointIntersects && AdvancedMaths.LineIntersection(points[i].Position, previous.Position, e.A.Position, e.B.Position).Item1) previousPointIntersects = true;
                    }
                }

                if (!previousPointIntersects) points[i].AddNeighbor(previous);
                if (!nextPointIntersects) points[i].AddNeighbor(next);
            }
        }

        private static void GenerateSplinteredArea(int complexity)
        {
            Graph graph = new Graph();

            Assert.IsTrue(complexity >= 4);
            float originalRadius = 2;
            float radius = originalRadius;
            float radiusMultiplier = 1.5f;
            List<CrackNode> paths = new List<CrackNode>();

            CrackNode origin = CrackNode.Create(0, 360f / complexity, 0);
            graph.AddNode(origin);
            int iterations = 0;
            for (int i = 0; i < complexity; ++i)
            {
                float angleFrom = i * origin.AngleRange;
                float angleRange = origin.AngleRange;
                CrackNode newNode = CrackNode.Create(angleFrom, angleRange, radius);
                graph.AddNode(newNode);
                origin.AddNeighbor(newNode);
                paths.Add(newNode);
            }

            JoinNeighbors(paths, null);
            radius += originalRadius * Mathf.Pow(radiusMultiplier, iterations);

            while (radius < PathingGrid.CombatAreaWidth)
            {
                List<CrackNode> newPaths = new List<CrackNode>();
                List<Edge> existingEdges = graph.GenerateEdges();
                foreach (CrackNode parent in paths)
                {
                    int splitCount = (int) (radius / originalRadius);
                    if (splitCount < 1) splitCount = 1;
                    if (splitCount > 3) splitCount = 3;
                    splitCount = Random.Range(1, splitCount);
                    float splitAngleRange = parent.AngleRange / splitCount;

                    for (int j = 0; j < splitCount; ++j)
                    {
                        float angleFrom = parent.AngleFrom + j * splitAngleRange;
                        CrackNode splitNode = CrackNode.Create(angleFrom, splitAngleRange, radius);
                        graph.AddNode(splitNode);
                        parent.AddNeighbor(splitNode);
                        newPaths.Add(splitNode);
                    }
                }

                JoinNeighbors(newPaths, existingEdges);
//                paths = CreateNewPaths(paths, graph, radius, originalRadius, lastRadius);
                paths = newPaths;
                radius += originalRadius * Mathf.Pow(radiusMultiplier, iterations);
                ++iterations;
            }

            graph.GenerateEdges();

            HashSet<Vector3> seenPairs = new HashSet<Vector3>();
            graph.Nodes().ForEach(n => { CreatePolygon(n, seenPairs); });
        }

//        private static List<CrackNode> CreateNewPaths(List<CrackNode> paths, Graph graph, float radius, float originalRadius, float lastRadius)
//        {
//            List<CrackNode> newPaths = new List<CrackNode>();
//            foreach (CrackNode parent in paths)
//            {
//                int splitCount = (int) (radius / originalRadius);
//                if (splitCount < 1) splitCount = 1;
//                if (splitCount > 3) splitCount = 3;
//                splitCount = Random.Range(1, splitCount);
//                float splitAngleRange = parent.AngleRange / splitCount;
//
//                for (int j = 0; j < splitCount; ++j)
//                {
//                    float angleFrom = parent.AngleFrom + j * splitAngleRange;
//                    CrackNode splitNode = CrackNode.Create(angleFrom, splitAngleRange, radius);
//                    graph.AddNode(splitNode);
//                    parent.AddNeighbor(splitNode);
//                    newPaths.Add(splitNode);
//                }
//            }
//
//            JoinNeighbors(newPaths, lastRadius, originalRadius);
//            return newPaths;
//        }

        private static int polygonNumber;

        private static void CreatePolygon(Node initialNode, HashSet<Vector3> seenPairs)
        {
            foreach (Node neighbor in initialNode.Neighbors())
            {
                Vector2 midPoint = neighbor.Position - initialNode.Position;
                if (seenPairs.Contains(midPoint)) continue;
                List<Vector2> polygon = new List<Vector2>();
                polygon.Add(initialNode.Position);
                Node currentNode = neighbor;
                Node last = initialNode;
                bool polyExists = false;
                while (currentNode != initialNode)
                {
                    polygon.Add(currentNode.Position);
                    Node nextNode = currentNode.NavigateClockwise(last);
                    Vector2 mid = nextNode.Position - currentNode.Position;
                    if (seenPairs.Contains(mid))
                    {
                        polyExists = true;
                        break;
                    }

                    last = currentNode;
                    currentNode = nextNode;
                }

                if (polyExists) continue;

                seenPairs.Add(midPoint);

                //find centre of bounding box
                Vector2 topLeft = polygon[0];
                Vector2 bottomRight = polygon[0];

                polygon.ForEach(n =>
                {
                    if (n.x < topLeft.x) topLeft.x = n.x;
                    else if (n.x > bottomRight.x) bottomRight.x = n.x;
                    if (n.y < topLeft.y) topLeft.y = n.y;
                    else if (n.y > bottomRight.y) bottomRight.y = n.y;
                });

                for (int i = 0; i < polygon.Count; ++i)
                {
                    int next = i + 1 == polygon.Count ? 0 : i + 1;
//                    Debug.DrawLine(polygon[i], polygon[next], Color.white, 10f);
                }

                List<Vector2> newPoints = new List<Vector2>();
                for (int i = 0; i < polygon.Count; ++i)
                {
                    int nextIndex = i + 1 == polygon.Count ? 0 : i + 1;
                    Vector2 current = polygon[i];
                    newPoints.Add(current);
                    Vector2 next = polygon[nextIndex];
                    int extraPoints = Random.Range(1, 10);
                    List<float> points = new List<float>();
                    while (extraPoints > 0)
                    {
                        points.Add(Random.Range(0f, 1f));
                        --extraPoints;
                    }

                    points.Sort();
                    points.ForEach(f =>
                    {
                        Vector2 randomPoint = AdvancedMaths.PointAlongLine(current, next, f);
                        newPoints.Add(randomPoint);
                    });
                }

                Vector2 centre = (topLeft + bottomRight) / 2f;
                if (!AdvancedMaths.IsPointInPolygon(centre, polygon)) continue;

                polygon = newPoints;

                float distToCentre = centre.magnitude;
                if (distToCentre <= 1) continue;
                float mod = 1f - 1f / distToCentre;
                if (Random.Range(0f, 1f) < 1 - mod) continue;
                mod = Mathf.Clamp(mod, 0.2f, 1f);

                for (int i = 0; i < polygon.Count; ++i)
                {
                    polygon[i] -= centre;
                    float offset = Mathf.PerlinNoise(polygon[i].x, polygon[i].y);
                    offset = (offset / 2f + 0.5f) * mod;
                    float vertexLength = polygon[i].magnitude;
                    float minOffsetDistance = vertexLength - 1;
                    float minOffset = minOffsetDistance / vertexLength;
                    if (offset < minOffset) offset = minOffset;
                    polygon[i] *= offset;
                }

                polygon.Sort((a, b) => AdvancedMaths.AngleFromUp(Vector2.zero, a).CompareTo(AdvancedMaths.AngleFromUp(Vector2.zero, b)));

                Barrier barrier = new Barrier(polygon, "Polygon " + polygonNumber, centre, 0f);
                barrier.RotateLocked = true;
                barriers.Add(barrier);
                ++polygonNumber;
            }
        }
    }
}