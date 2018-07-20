using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly List<Region> _regions = new List<Region>();
        private const int RandomFillPercent = 48;

        protected override void Generate()
        {
            GenerateMap();
        }

        private void GenerateMap()
        {
            _map = new CavePoint[Width, Width];
            RandomFillMap();

            for (int i = 0; i < 5; i++) SmoothMap();

            GenerateRegions(true);
            ConnectRegions();
            GenerateRegions(false);
            DrawEdges();
        }

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

        private void ConnectRegions()
        {
            for (int i = _regions.Count - 1; i >= 0; --i)
            {
                Region region = _regions[i];
                bool regionOutOfRange = true;
                foreach (CavePoint point in region.Edges())
                {
                    if (point.OutOfRange()) continue;
                    regionOutOfRange = false;
                    break;
                }

                if (regionOutOfRange) _regions.RemoveAt(i);
            }

            while (_regions.Count > 1)
            {
                float nearestRegion = Mathf.Infinity;
                int nearestRegionIndex = -1;
                Tuple<CavePoint, CavePoint, float> nearestRegionEdgePoints = null;
                Region a = _regions[0];
                _regions.RemoveAt(0);
                for (int i = 0; i < _regions.Count; ++i)
                {
                    Tuple<CavePoint, CavePoint, float> edgePoints = NearestEdgePoints(a, _regions[i]);
                    if (edgePoints == null || edgePoints.Item3 > nearestRegion) continue;
                    nearestRegion = edgePoints.Item3;
                    nearestRegionIndex = i;
                    nearestRegionEdgePoints = edgePoints;
                }

                Region b = _regions[nearestRegionIndex];
                _regions.RemoveAt(nearestRegionIndex);
                _regions.Add(a.Combine(b, nearestRegionEdgePoints));
            }
        }

        private static Tuple<CavePoint, CavePoint, float> NearestEdgePoints(Region a, Region b)
        {
            float nearestDistance = float.PositiveInfinity;
            CavePoint from = null;
            CavePoint to = null;
            foreach (CavePoint edge in a.Edges())
            {
                if (edge.OutOfRange()) continue;
                foreach (CavePoint otherEdge in b.Edges())
                {
                    if (otherEdge.OutOfRange()) continue;
                    float distance = edge.Distance(otherEdge);
                    if (distance >= nearestDistance) continue;
                    nearestDistance = distance;
                    from = edge;
                    to = otherEdge;
                }
            }

            return from == null ? null : Tuple.Create(from, to, nearestDistance);
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

        private void GenerateRegions(bool accessible)
        {
            FindCells(!accessible).ForEach(c => c.IsEdge = false);
            List<CavePoint> cells = FindCells(accessible);
            _regions.Clear();
            while (cells.Count > 0)
            {
                CavePoint startCell = cells[0];
                Region newRegion = GenerateRegion(startCell, accessible);
                cells.RemoveAll(c => newRegion.Points().Contains(c));
                if (!newRegion.IsValid()) continue;
                _regions.Add(newRegion);
            }
        }

        private static List<CavePoint> FindCells(bool accessible)
        {
            List<CavePoint> emptyCells = new List<CavePoint>();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Width; y++)
                {
                    if (_map[x, y].accessible != accessible) continue;
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

            public CavePoint(int x, int y)
            {
                X = x;
                Y = y;
                float radius = Width / 2f;
                float xCentreDist = x - radius;
                float yCentreDist = y - radius;
                radius -= 10;
                _inRange = xCentreDist * xCentreDist + yCentreDist * yCentreDist < radius * radius;
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