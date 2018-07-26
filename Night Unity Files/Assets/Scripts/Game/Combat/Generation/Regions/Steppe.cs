using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Generation
{
    public class Steppe : RegionGenerator
    {
        //place shrine
        //place walls, remove cells if they intersect with shrine
        //place health shrine
        //place fires
        //place items
        //place echo
        //place obstacles

        private const float Scale = 0.25f;
        private const int Width = 200;
        private const int RegionAreaThreshold = 100;
        private static CavePoint[,] _map;
        private List<Region> _regions = new List<Region>();
        private const int RandomFillPercent = 48;
        private static bool _includeOnlyInRangeCells;

        protected override void Generate()
        {
            PlaceShrine();
            GenerateMap();
            PlaceItems();
            PlaceEchoes();
        }

//        private List<CavePoint> drawPoints = new List<CavePoint>();

        private void GenerateMap()
        {
            _map = new CavePoint[Width, Width];
            RandomFillMap();

            for (int i = 0; i < 5; i++) SmoothMap();

            _includeOnlyInRangeCells = true;
            List<Region> inRange = GenerateRegions(true);
            ConnectRegions(inRange);
//            inRange.ForEach(r =>
//            {
//                foreach (CavePoint cavePoint in r.Edges())
//                {
//                    if (cavePoint.OutOfRange()) continue;
//                    drawPoints.Add(cavePoint);
//                }
//
//                foreach (CavePoint cavePoint in r.Points())
//                {
//                    if (cavePoint.OutOfRange()) continue;
//                    drawPoints.Add(cavePoint);
//                }
//            });
            _includeOnlyInRangeCells = false;
            _regions = GenerateRegions(false);
            DrawEdges();
            _includeOnlyInRangeCells = true;
            Assert.IsTrue(GenerateRegions(true).Count == 1);
        }

//        private void OnDrawGizmos()
//        {
//            Vector3 cubeSize = Scale * Vector3.one;
//            Gizmos.color = Color.red;
//            drawPoints.ForEach(c => { Gizmos.DrawCube(c.worldPosition, cubeSize); });
//        }

        private static List<CavePoint> SmoothEdges(Region region)
        {
            List<CavePoint> edges = region.Edges().ToList();
            for (int i = edges.Count - 1; i >= 0; --i)
            {
                CavePoint edge = edges[i];
                edge.IsEdge = false;
                List<Vector2Int> positions = new List<Vector2Int>
                {
                    new Vector2Int(edge.X - 1, edge.Y),
                    new Vector2Int(edge.X + 1, edge.Y),
                    new Vector2Int(edge.X, edge.Y - 1),
                    new Vector2Int(edge.X, edge.Y + 1)
                };
                bool invalidEdge = true;
                foreach (Vector2Int p in positions)
                {
                    if (p.x < 0 || p.y < 0 || p.x >= Width || p.y >= Width) continue;
                    CavePoint neighbor = _map[p.x, p.y];
                    if (!neighbor.accessible) continue;
                    invalidEdge = false;
                    break;
                }

                if (!invalidEdge) continue;
                edges.RemoveAt(i);
            }

            edges.ForEach(e => e.IsEdge = true);
            return edges;
        }

        private static CavePoint ClockWiseFrom(CavePoint current, CavePoint last)
        {
            int startIndex = -1;
            List<CavePoint> neighbors = current.Neighbors();
            if (last != null)
            {
                startIndex = neighbors.IndexOf(last) + 1;
                if (startIndex == neighbors.Count) startIndex = 0;
            }
            else
            {
                for (int i = 0; i < neighbors.Count; i++)
                {
                    CavePoint neighbor = neighbors[i];
                    if (!neighbor.accessible) continue;
                    startIndex = i;
                    break;
                }
            }

            int currentIndex = startIndex;
            for (int i = 0; i < neighbors.Count; ++i)
            {
                if (!neighbors[currentIndex].accessible && neighbors[currentIndex].IsEdge) return neighbors[currentIndex];
                ++currentIndex;
                if (currentIndex == neighbors.Count) currentIndex = 0;
            }

            throw new ArgumentOutOfRangeException();
        }

        private void DrawEdges()
        {
            _regions.ForEach(region =>
            {
                Vector2 centre = new Vector2();
                List<Vector2> verts = new List<Vector2>();
                List<CavePoint> edges = SmoothEdges(region);
                foreach (CavePoint p in region.Points().ToList())
                {
                    if (!p.IsOnScreenEdge || edges.Contains(p)) continue;
                    edges.Add(p);
                    p.IsEdge = true;
                }

                CavePoint current = edges[0];
                CavePoint start = current;
                CavePoint last = null;
                bool returnedToStart = false;
                while (!returnedToStart)
                {
                    Vector2 currentPos = new Vector2(current.X, current.Y) * Scale;
                    currentPos += Vector2.one * Mathf.PerlinNoise(currentPos.x, currentPos.y) * 0.5f;
                    centre += currentPos;
                    verts.Add(currentPos);
                    CavePoint next = ClockWiseFrom(current, last);
                    last = current;
                    current = next;
                    if (current != start) continue;
                    returnedToStart = true;
                }

                centre /= verts.Count;
                for (int i = 0; i < verts.Count; ++i)
                {
                    verts[i] -= centre;
                }

                verts.Reverse();
                centre -= Vector2.one * Width / 2f * Scale;
                new Barrier(verts, "Wall " + GetObjectNumber(), centre, barriers);
            });
        }

        private static Region GenerateRegion(CavePoint startCell, bool accessible)
        {
            HashSet<CavePoint> points = new HashSet<CavePoint>();
            HashSet<CavePoint> edges = new HashSet<CavePoint>();
            Queue<CavePoint> unvisited = new Queue<CavePoint>();
            unvisited.Enqueue(startCell);
            points.Add(startCell);
            while (unvisited.Count != 0)
            {
                CavePoint p = unvisited.Dequeue();
                p.Neighbors().ForEach(neighbor =>
                {
                    if (neighbor == null) return;
                    if (_includeOnlyInRangeCells && neighbor.OutOfRange()) return;
                    if (neighbor.accessible != accessible)
                    {
                        edges.Add(p);
                        return;
                    }

                    if (points.Contains(neighbor)) return;
                    points.Add(neighbor);
                    unvisited.Enqueue(neighbor);
                });
            }

            return new Region(points, edges);
        }
        
        private void ConnectRegions(List<Region> regions)
        {
            for (int i = regions.Count - 1; i >= 0; --i)
            {
                Region region = regions[i];
                bool regionOutOfRange = true;
                foreach (CavePoint point in region.Edges())
                {
                    if (point.OutOfRange()) continue;
                    regionOutOfRange = false;
                    break;
                }

                if (regionOutOfRange) regions.RemoveAt(i);
            }

            while (regions.Count > 1)
            {
                float nearestRegion = Mathf.Infinity;
                int nearestRegionIndex = -1;
                Tuple<CavePoint, CavePoint, float> nearestRegionEdgePoints = null;
                Region fromRegion = regions[0];
                regions.RemoveAt(0);
                for (int i = 0; i < regions.Count; ++i)
                {
                    Tuple<CavePoint, CavePoint, float> edgePoints = NearestEdgePoints(fromRegion, regions[i]);
                    if (edgePoints == null || edgePoints.Item3 > nearestRegion) continue;
                    nearestRegion = edgePoints.Item3;
                    nearestRegionIndex = i;
                    nearestRegionEdgePoints = edgePoints;
                }

                Region b = regions[nearestRegionIndex];
                regions.RemoveAt(nearestRegionIndex);
                regions.Add(fromRegion.Combine(b, nearestRegionEdgePoints));
            }
        }

        private static Tuple<CavePoint, CavePoint, float> NearestEdgePoints(Region a, Region b)
        {
            float nearestDistance = float.PositiveInfinity;
            CavePoint from = null;
            CavePoint to = null;
            foreach (CavePoint edgePoint in a.Edges())
            {
                if (edgePoint.OutOfRange()) continue;
                foreach (CavePoint otherEdge in b.Edges())
                {
                    if (otherEdge.OutOfRange()) continue;
                    float distance = edgePoint.Distance(otherEdge);
                    if (distance >= nearestDistance) continue;
                    nearestDistance = distance;
                    from = edgePoint;
                    to = otherEdge;
                }
            }

            return from == null ? null : Tuple.Create(from, to, nearestDistance);
        }

        private List<Region> GenerateRegions(bool accessible)
        {
            List<Region> regions = new List<Region>();
            FindCells(!accessible).ForEach(c => c.IsEdge = false);
            List<CavePoint> cells = FindCells(accessible);
            while (cells.Count > 0)
            {
                CavePoint startCell = cells[0];
                Region newRegion = GenerateRegion(startCell, accessible);
                cells.RemoveAll(c => newRegion.Points().Contains(c));
                if (!newRegion.IsValid()) continue;
                regions.Add(newRegion);
            }

            return regions;
        }

        private static List<CavePoint> FindCells(bool accessible)
        {
            List<CavePoint> emptyCells = new List<CavePoint>();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Width; y++)
                {
                    if (_map[x, y].accessible != accessible) continue;
                    if (_includeOnlyInRangeCells && _map[x, y].OutOfRange()) continue;
                    emptyCells.Add(_map[x, y]);
                }
            }

            return emptyCells;
        }

        private void RandomFillMap()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Width; y++)
                {
                    _map[x, y] = new CavePoint(x, y);
                    if (_map[x, y].IsOnScreenEdge)
                    {
                        _map[x, y].accessible = false;
                    }
                    else if (ShouldPlaceShrine() && Vector2.Distance(new Vector2((x - Width / 2) * Scale, (y-Width/2) * Scale), _region.ShrinePosition) < 5f)
                    {
                        _map[x, y].accessible = true;
                    }
                    else
                    {
                        _map[x, y].accessible = Random.Range(0, 100) >= RandomFillPercent;
                    }
                }
            }
        }

        private void SmoothMap()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Width; y++)
                {
                    int neighbourWallTiles = GetSurroundingWallCount(_map[x, y]);

                    if (neighbourWallTiles > 4)
                    {
                        _map[x, y].accessible = false;
                    }
                    else if (neighbourWallTiles <
                             4)
                    {
                        _map[x, y].accessible = true;
                    }
                }
            }
        }

        private static int GetSurroundingWallCount(CavePoint point) => point.Neighbors().Count(p => p != null && !p.accessible);

        private class Region
        {
            private readonly HashSet<CavePoint> _edges;
            private readonly HashSet<CavePoint> _points;

            public Region(HashSet<CavePoint> points, HashSet<CavePoint> edges)
            {
                _points = points;
                _edges = edges;
                foreach (CavePoint cavePoint in _points)
                    cavePoint.IsEdge = false;
                foreach (CavePoint cavePoint in _edges)
                    cavePoint.IsEdge = true;
            }

            private void ClearEdge()
            {
                foreach (CavePoint cavePoint in _edges)
                    cavePoint.IsEdge = false;
            }

            public HashSet<CavePoint> Points() => _points;
            public HashSet<CavePoint> Edges() => _edges;

            private void MakeImpassable()
            {
                foreach (CavePoint cavePoint in _points) cavePoint.accessible = false;
            }

            public Region Combine(Region otherRegion, Tuple<CavePoint, CavePoint, float> nearestEdges)
            {
                List<CavePoint> line = GetLine(nearestEdges.Item1, nearestEdges.Item2);
                line.ForEach(c => DrawCircle(c, 4));
                ClearEdge();
                otherRegion.ClearEdge();
                return GenerateRegion(_points.ToList()[0], true);
            }

            private static void DrawCircle(CavePoint point, int r)
            {
                for (int x = -r; x <= r; x++)
                {
                    for (int y = -r; y <= r; y++)
                    {
                        if (x * x + y * y > r * r) continue;
                        int drawX = point.X + x;
                        int drawY = point.Y + y;
                        if (drawX < 0 || drawX >= Width) continue;
                        if (drawY < 0 || drawY >= Width) continue;
                        _map[drawX, drawY].accessible = true;
                    }
                }
            }

            private static List<CavePoint> GetLine(CavePoint from, CavePoint to)
            {
                HashSet<CavePoint> line = new HashSet<CavePoint>();
                line.Add(from);

                int x = from.X;
                int y = from.Y;

                int dx = to.X - from.X;
                int dy = to.Y - from.Y;

                bool inverted = false;
                int step = Math.Sign(dx);
                int gradientStep = Math.Sign(dy);

                int longest = Mathf.Abs(dx);
                int shortest = Mathf.Abs(dy);

                if (longest < shortest)
                {
                    inverted = true;
                    longest = Mathf.Abs(dy);
                    shortest = Mathf.Abs(dx);

                    step = Math.Sign(dy);
                    gradientStep = Math.Sign(dx);
                }

                int gradientAccumulation = longest / 2;
                for (int i = 0; i < longest; i++)
                {
                    line.Add(_map[x, y]);
                    if (inverted)
                    {
                        y += step;
                    }
                    else
                    {
                        x += step;
                    }

                    gradientAccumulation += shortest;
                    if (gradientAccumulation < longest) continue;
                    if (inverted)
                    {
                        x += gradientStep;
                    }
                    else
                    {
                        y += gradientStep;
                    }

                    gradientAccumulation -= longest;
                }

                line.Add(to);

                return line.ToList();
            }

            public bool IsValid()
            {
                bool valid = _points.Count > RegionAreaThreshold;
                if (valid) return true;
                MakeImpassable();
                foreach (CavePoint cavePoint in _edges)
                    cavePoint.IsEdge = false;
                return false;
            }
        }

        private class CavePoint
        {
            private readonly bool _inRange;
            public readonly int X, Y;
            private List<CavePoint> _neighbors;
            public bool accessible;
            public bool IsEdge;
            public readonly bool IsOnScreenEdge;
            public readonly Vector2 worldPosition;

            public CavePoint(int x, int y)
            {
                X = x;
                Y = y;
                float radius = Width / 2f;
                float xCentreDist = x - radius;
                float yCentreDist = y - radius;
                worldPosition = new Vector2(xCentreDist, yCentreDist) * Scale;
                _inRange = worldPosition.magnitude <= PathingGrid.CombatMovementDistance / 2f + 0.25f;
                IsOnScreenEdge = x == 0 || y == 0 || x == Width - 1 || y == Width - 1;
            }

            private void TryAddNeighbor(int x, int y)
            {
                if (x < 0 || x >= Width || y < 0 || y >= Width) return;
                if (x == X && y == Y) return;
                _neighbors.Add(_map[x, y]);
            }

            public List<CavePoint> Neighbors()
            {
                if (_neighbors != null) return _neighbors;
                _neighbors = new List<CavePoint>();

                TryAddNeighbor(X - 1, Y - 1);
                TryAddNeighbor(X - 1, Y);
                TryAddNeighbor(X - 1, Y + 1);
                TryAddNeighbor(X, Y + 1);
                TryAddNeighbor(X + 1, Y + 1);
                TryAddNeighbor(X + 1, Y);
                TryAddNeighbor(X + 1, Y - 1);
                TryAddNeighbor(X, Y - 1);

                return _neighbors;
            }

            public bool OutOfRange() => !_inRange;

            public float Distance(CavePoint other)
            {
                float dx = X - other.X;
                float dy = Y - other.Y;
                return dx * dx + dy * dy;
            }
        }
    }
}