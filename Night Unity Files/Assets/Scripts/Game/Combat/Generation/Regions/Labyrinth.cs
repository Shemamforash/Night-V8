using System.Collections.Generic;
using NUnit.Framework;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Labyrinth : RegionGenerator
    {
        //place walls
        //remove walls to place shrine
        //remove wall to place health shrine
        //place fires
        //place items
        //place echo
        //place obstacles
        
        protected override void Generate()
        {
            PlaceShrine();
            GenerateSplinteredArea(Random.Range(4, 7));
            PlaceItems();
        }

        private void JoinNeighbors(List<CrackNode> points, List<Edge> existingEdges)
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

        private class CrackNode : Node
        {
            public readonly float AngleFrom, AngleRange;

            public static CrackNode Create(float angleFrom, float angleRange, float radius)
            {
                float randomAngle = Random.Range(angleFrom, angleFrom + angleRange);
                float x = Mathf.Cos(randomAngle * Mathf.Deg2Rad) * radius;
                float y = Mathf.Sin(randomAngle * Mathf.Deg2Rad) * radius;
                Vector2 nodePosition = new Vector2(x, y);
                return new CrackNode(nodePosition, angleFrom, angleRange);
            }

            private CrackNode(Vector2 position, float angleFrom, float angleRange) : base(position)
            {
                AngleFrom = angleFrom;
                AngleRange = angleRange;
            }
        }

        private void GenerateSplinteredArea(int complexity)
        {
            Graph graph = new Graph();
            float originalRadius = Random.Range(1.5f, 2.5f);
            float radius = originalRadius;
            float radiusMultiplier = Random.Range(1.2f, 1.9f);
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
                paths = newPaths;
                radius += originalRadius * Mathf.Pow(radiusMultiplier, iterations);
                ++iterations;
            }

            graph.GenerateEdges();

            HashSet<Vector3> seenPairs = new HashSet<Vector3>();
            graph.Nodes().ForEach(n => { CreatePolygon(n, seenPairs); });
        }

        private void CreatePolygon(Node initialNode, HashSet<Vector3> seenPairs)
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

                Vector2 topLeft = polygon[0];
                Vector2 bottomRight = polygon[0];

                polygon.ForEach(n =>
                {
                    if (n.x < topLeft.x)
                    {
                        topLeft.x = n.x;
                    }
                    else if (n.x > bottomRight.x) bottomRight.x = n.x;

                    if (n.y < topLeft.y)
                    {
                        topLeft.y = n.y;
                    }
                    else if (n.y > bottomRight.y) bottomRight.y = n.y;
                });

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

                Barrier barrier = new Barrier(polygon, "Polygon " + GetObjectNumber(), centre, barriers);
            }
        }
    }
}