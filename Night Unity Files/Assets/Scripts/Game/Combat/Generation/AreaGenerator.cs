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

        private static void GenerateRockWall(Vector2 position, float width, float bumpiness, float depth)
        {
            Assert.IsTrue(bumpiness > 0 && bumpiness < 1);
            Assert.IsTrue(depth > 0 && depth < 1);
            depth *= width / 2f;
            bumpiness = 1 - bumpiness;
            List<Vector3> wallVertices = new List<Vector3>();
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

            Barrier wall = new Barrier(wallVertices.ToArray(), "Wall " + wallNumber, position, width);
            barriers.Add(wall);
            ++wallNumber;
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
        private static int barrierNumber, wallNumber;

        //generate fire -> huge rock -> large rock -> medium rock -> small rock

        private static bool ShapeIsTooCloseToPoint(Vector2 point, float distance)
        {
            return barriers.Any(b =>
            {
                float minDistance = distance + b.Radius * 1.1f;
                return Vector2.Distance(b.Position, point) <= minDistance;
            });
        }

        public static void GenerateSplitRock(Region region)
        {
            GenerateSplinteredArea(50, 2);

            barriers = new List<Barrier>();
            barrierNumber = 0;
            if (allValidPositions == null) allValidPositions = AdvancedMaths.GetPoissonDiscDistribution(1000, 1f, 3f, PathingGrid.CombatAreaWidth / 2f);
            validPositions = new List<Vector2>();
            allValidPositions.ForEach(v => validPositions.Add(v));

//            GenerateRockWall(Vector3.one * 3, 5f, 0.5f, 0.5f);
//            GenerateMediumRock();

            region.Barriers = barriers;
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

        private static Vector2 RandomPointOnLine(Vector3 start, Vector3 end)
        {
            float random = Random.Range(0f, 1f);
            float newX = (1 - random) * start.x + random * end.x;
            float newY = (1 - random) * start.y + random * end.y;
            return new Vector2(newX, newY);
        }

        private class BoundingBox
        {
            public readonly Vector2 TopLeft, TopRight, BottomLeft, BottomRight;

            public BoundingBox(float width) : this(width, width)
            {
            }

            public BoundingBox(float width, float height)
            {
                TopLeft = new Vector2(-PathingGrid.CombatAreaWidth, PathingGrid.CombatAreaWidth);
                TopRight = new Vector2(PathingGrid.CombatAreaWidth, PathingGrid.CombatAreaWidth);
                BottomLeft = new Vector2(-PathingGrid.CombatAreaWidth, -PathingGrid.CombatAreaWidth);
                BottomRight = new Vector2(PathingGrid.CombatAreaWidth, -PathingGrid.CombatAreaWidth);
            }

            public void Draw()
            {
                Debug.DrawLine(TopLeft, TopRight, Color.green, 5f);
                Debug.DrawLine(BottomRight, TopRight, Color.green, 5f);
                Debug.DrawLine(TopLeft, BottomLeft, Color.green, 5f);
                Debug.DrawLine(BottomLeft, BottomRight, Color.green, 5f);
            }
        }

        private static List<Node<Vector2>> CreateEdge(Vector2 start, Vector2 end, Graph<Vector2> graph)
        {
            List<Node<Vector2>> edgeNodes = new List<Node<Vector2>>();
            edgeNodes.Add(new Node<Vector2>(start));
            edgeNodes.Add(new Node<Vector2>(end));
            for (int i = 1; i < Random.Range(2, 6); ++i) edgeNodes.Add(new Node<Vector2>(RandomPointOnLine(start, end)));
            edgeNodes.Sort((a, b) => a.Position.x.CompareTo(b.Position.x));
            graph.Nodes().AddRange(edgeNodes);
            return edgeNodes;
        }

        private static void FillEdge(List<Node<Vector2>> edge, Graph<Vector2> tree)
        {
            for (int i = 1; i < edge.Count; ++i)
            {
                Edge<Vector2> e = new Edge<Vector2>(edge[i - 1], edge[i]);
                if (tree.ContainsEdge(e)) continue;
                edge[i - 1].AddNeighbor(edge[i]);
                tree.AddEdge(e);
            }
        }

        private class CrackNode
        {
            public readonly float AngleFrom, AngleRange, Angle;
            public readonly Vector2 Position;
            private readonly List<CrackNode> _neighbors = new List<CrackNode>();

            public CrackNode(float angleFrom, float angleRange, float radius)
            {
                AngleFrom = angleFrom;
                AngleRange = angleRange;
                Angle = Random.Range(AngleFrom, AngleFrom + AngleRange);
                float x = Mathf.Cos(Angle * Mathf.Deg2Rad) * radius;
                float y = Mathf.Sin(Angle * Mathf.Deg2Rad) * radius;
                Position = new Vector2(x, y);
            }

            public void AddNeighbor(CrackNode node)
            {
                if (_neighbors.Contains(node)) return;
                _neighbors.Add(node);
                node._neighbors.Add(this);
            }

            public List<CrackNode> Neighbors()
            {
                return _neighbors;
            }
        }

        private static void JoinNeighbors(List<CrackNode> points, float lastRadius, float originalRadius)
        {
            for (int i = 0; i < points.Count; ++i)
            {
                int next = i + 1 == points.Count ? 0 : i + 1;
                int previous = i - 1 == -1 ? points.Count - 1 : i - 1;
                bool neighborAIntersects = false;
                bool neighborBIntersects = false;
                if (lastRadius >= originalRadius)
                {
                    neighborAIntersects = AdvancedMaths.DoesLineIntersectWithCircle(points[i].Position, points[previous].Position, Vector2.zero, lastRadius);
                    neighborBIntersects = AdvancedMaths.DoesLineIntersectWithCircle(points[i].Position, points[next].Position, Vector2.zero, lastRadius);
                }
                if (!neighborAIntersects) points[i].AddNeighbor(points[previous]);
                if (!neighborBIntersects) points[i].AddNeighbor(points[next]);
            }
        }

        private static void GenerateSplinteredArea(int complexity)
        {
            float originalRadius = 2;
            float radius = originalRadius;
            float lastRadius = originalRadius / 2f;
            float radiusMultiplier = 1.5f;
            List<CrackNode> paths = new List<CrackNode>();
            
            CrackNode origin = new CrackNode(0, 360f / complexity, 0);
            int iterations = 0;
            for (int i = 0; i < complexity; ++i)
            {
                float angleFrom = i * origin.AngleRange;
                float angleRange = origin.AngleRange;
                CrackNode newNode = new CrackNode(angleFrom, angleRange, radius);
                origin.AddNeighbor(newNode);
                paths.Add(newNode);
            }
            
            JoinNeighbors(paths, lastRadius, originalRadius);
            lastRadius = radius;
            radius += originalRadius * Mathf.Pow(radiusMultiplier, iterations);

            while (radius < PathingGrid.CombatAreaWidth)
            {
                List<CrackNode> newPaths = new List<CrackNode>();
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
                        CrackNode splitNode = new CrackNode(angleFrom, splitAngleRange, radius);
                        parent.AddNeighbor(splitNode);
                        newPaths.Add(splitNode);
                    }
                }

                JoinNeighbors(newPaths, lastRadius, originalRadius);

                paths = newPaths;
                lastRadius = radius;
                radius += originalRadius * Mathf.Pow(radiusMultiplier, iterations);
                ++iterations;
            }

            HashSet<CrackNode> visited = new HashSet<CrackNode>();
            Queue<CrackNode> unvisited = new Queue<CrackNode>();
            unvisited.Enqueue(origin);
            visited.Add(origin);
            while (unvisited.Count != 0)
            {
                CrackNode current = unvisited.Dequeue();
                foreach(CrackNode neighbor in current.Neighbors())
                {
                    Debug.DrawLine(current.Position, neighbor.Position, Color.red, 5f);
                    if (visited.Contains(neighbor)) continue;
                    visited.Add(neighbor);
                    unvisited.Enqueue(neighbor);
                }
            }
        }

        private static void GenerateSplinteredArea(int complexity, int connectedness)
        {
            GenerateSplinteredArea(5);
            return;
            List<Vector2> points = AdvancedMaths.GetPoissonDiscDistribution(4 * complexity, PathingGrid.CombatAreaWidth / 5f, PathingGrid.CombatAreaWidth, PathingGrid.CombatAreaWidth, false);
            Helper.Shuffle(ref points);
            Graph<Vector2> graph = new Graph<Vector2>();
            graph.AddNode(new Node<Vector2>(Vector2.zero));
            while (complexity > 0)
            {
                graph.AddNode(new Node<Vector2>(points[complexity]));
                --complexity;
            }

            BoundingBox box = new BoundingBox(PathingGrid.CombatAreaWidth);

            List<Node<Vector2>> topEdge = CreateEdge(box.TopLeft, box.TopRight, graph);
            List<Node<Vector2>> rightEdge = CreateEdge(box.TopRight, box.BottomRight, graph);
            List<Node<Vector2>> leftEdge = CreateEdge(box.BottomRight, box.BottomLeft, graph);
            List<Node<Vector2>> bottomEdge = CreateEdge(box.BottomLeft, box.TopLeft, graph);

            graph.ComputeMinimumSpanningTree((a, b) => ((a.Position + b.Position) / 2f).magnitude / 10f + 1f);
            FillEdge(topEdge, graph);
            FillEdge(rightEdge, graph);
            FillEdge(leftEdge, graph);
            FillEdge(bottomEdge, graph);

//            box.Draw();
            graph.Edges().ForEach(e => { Debug.DrawLine(e.A.Position, e.B.Position, Color.red, 5f); });

            List<Edge<Vector2>> newEdges = new List<Edge<Vector2>>();
            graph.Nodes().ForEach(n =>
            {
                if (!n.IsLeaf()) return;
                int randomStops = Random.Range(0, 4);
                Node<Vector2> currentNode = Helper.RandomInList(n.Neighbors());
                Node<Vector2> from = n;
                while (randomStops > 0)
                {
                    Node<Vector2> nextNode = currentNode.NavigateLeft(from);
                    from = currentNode;
                    currentNode = nextNode;
                    --randomStops;
                }

                if (currentNode == n) currentNode = currentNode.NavigateLeft(from);
                newEdges.Add(new Edge<Vector2>(n, currentNode));
            });
//            newEdges.ForEach(e => graph.AddEdge(e));

            graph.Edges().ForEach(e => { Debug.DrawLine(e.A.Position, e.B.Position, Color.green, 5f); });

            while (connectedness > 0)
            {
                --connectedness;
            }
        }
    }
}