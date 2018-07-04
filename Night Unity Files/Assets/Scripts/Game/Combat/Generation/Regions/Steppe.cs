using System;
using System.Collections.Generic;
using System.Linq;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Generation
{
    public class Steppe : RegionGenerator
    {
        private const int Width = 200;
        public int Seed = 1;
        private const int RegionAreaThreshold = 100;
        private static CavePoint[,] map;
        private readonly List<Region> _regions = new List<Region>();
        private List<Vector2> _verts;
        [Range(0, 100)] public int randomFillPercent;

        protected override void Generate()
        {
            for (int i = 0; i < 0; ++i)
            {
                _verts = new List<Vector2>();
                Vector2 start = new Vector2(-Random.Range(2f, 3f), 0);
                Vector2 end = new Vector2(Random.Range(2f, 3f), 0);
                _verts.Add(start);
                AddVerts(start, end, 1);
                _verts.Add(end);
                AddVerts(end, start, -1);
                AssignRockPosition(_verts, null);
            }
        }

        private void AddVerts(Vector2 start, Vector2 end, int polarity)
        {
            List<float> positionsOnLine = GetRandomPositions();
            positionsOnLine.ForEach(p =>
            {
                float xPos = AdvancedMaths.PointAlongLine(start, end, p).x;
                float noise = Mathf.PerlinNoise(p, 0) * 2f - 1f;
                float sinVal = Mathf.Sin(xPos);
                float height = polarity * noise * sinVal;
                Vector2 pos = new Vector2(xPos, height);
                _verts.Add(pos);
            });
        }

        private static List<float> GetRandomPositions()
        {
            List<float> positionsOnLine = new List<float>();
            for (int j = 0; j < Random.Range(10, 20); ++j) positionsOnLine.Add(Random.Range(0.01f, 0.99f));

            positionsOnLine.Sort((a, b) => a.CompareTo(b));
            return positionsOnLine;
        }

        private void Start()
        {
            GenerateMap();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                GenerateMap();
            }
        }

        private void GenerateMap()
        {
            map = new CavePoint[Width, Width];
            Random.InitState(Seed);
            RandomFillMap();

            for (int i = 0; i < 5; i++) SmoothMap();
            
            GenerateRegions(true);
            ConnectRegions();
//            GenerateRegions(false);
//            DrawEdges();
        }

        private static List<CavePoint> SmoothEdges(Region region)
        {
            List<CavePoint> edges = region.Edges().ToList();
            for (int i = edges.Count - 1; i >= 0; --i)
            {
                CavePoint edge = edges[i];
//                if (edge.IsOnScreenEdge) continue;
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
                    CavePoint neighbor = map[p.x, p.y];
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

        private CavePoint ClockWiseFrom(CavePoint current, CavePoint last)
        {
            int startIndex = -1;
            List<CavePoint> neighbors = current.Neighbors();
            if (last != null)
            {
                for (int i = 0; i < neighbors.Count; i++)
                {
                    CavePoint neighbor = neighbors[i];
                    if (neighbor != last) continue;
                    startIndex = i;
                    break;
                }
            }

            if (startIndex == -1)
            {
                for (int i = 0; i < neighbors.Count; i++)
                {
                    CavePoint neighbor = neighbors[i];
                    if (neighbor != null && !neighbor.accessible) continue;
                    startIndex = i;
                    break;
                }
            }

            for (int i = 0; i < neighbors.Count; ++i)
            {
                int currentIndex = startIndex + i;
                if (currentIndex >= neighbors.Count)
                {
                    currentIndex -= neighbors.Count;
                }

                if (neighbors[currentIndex] != null && !neighbors[currentIndex].accessible) return neighbors[currentIndex];
            }

            return null;
        }

        private void DrawEdges()
        {
            _regions.ForEach(region =>
            {
                HashSet<CavePoint> seenEdges = new HashSet<CavePoint>();
                List<CavePoint> edges = SmoothEdges(region);
                CavePoint current = edges[0];
//                CavePoint start = current;
//                CavePoint last = null;
//                int cnt = 1000;
//                while (cnt > 0)
//                {
//                    CavePoint next = ClockWiseFrom(current, last);
//                    last = current;
//                    current = next;
//                    Vector2 from = new Vector2(last.X - Width / 2, last.Y - Width / 2);
//                    Vector2 to = new Vector2(current.X - Width / 2, current.Y - Width / 2);
//                    Debug.DrawLine(from, to, Color.white, 10f);
//                    if (current == start) break;
//                    --cnt;
//                }
//
//                return;
                edges.RemoveAt(0);
                while (edges.Count > 0)
                {
                    seenEdges.Add(current);
                    float nearestEdgeDistance = float.PositiveInfinity;
                    int nearestEdgeIndex = -1;

//                    if (current.IsOnScreenEdge)
//                    {
//                        for (int i = 0; i < edges.Count; i++)
//                        {
//                            CavePoint e = edges[i];
//                            if (seenEdges.Contains(e)) continue;
//                            if (!e.IsOnScreenEdge) continue;
//                            bool connected = true;
//                            bool toLeft = e.X < current.X;
//                            bool toRight = e.X > current.X;
//                            bool above = e.Y > current.Y;
//                            bool below = e.Y < current.Y;
//                            bool sharesX = !toRight && !toLeft;
//                            bool sharesY = !below && !above;
//                            if (sharesX)
//                            {
//                                int y = current.Y;
//                                while (y != e.Y)
//                                {
//                                    y += above ? 1 : -1;
//                                    if (!map[current.X, y].accessible) continue;
//                                    connected = false;
//                                    break;
//                                }
//                            }
//                            else if (sharesY)
//                            {
//                                int x = current.X;
//                                while (x != e.X)
//                                {
//                                    x += toRight ? 1 : -1;
//                                    if (!map[x, current.Y].accessible) continue;
//                                    connected = false;
//                                    break;
//                                }
//                            }
//
//                            if (!connected) continue;
//                            current = e;
//                            nearestEdgeIndex = i;
//                            break;
//                        }
//                    }
                    if (nearestEdgeDistance == -1)
                    {
                        for (int i = 0; i < edges.Count; i++)
                        {
                            CavePoint e = edges[i];
                            if (seenEdges.Contains(e)) continue;
                            float distance = current.Distance(e);
                            if (distance > nearestEdgeDistance) continue;
                            nearestEdgeDistance = distance;
                            nearestEdgeIndex = i;
                        }
                    }

                    Vector2 from = new Vector2(current.X - Width / 2, current.Y - Width / 2);
                    current = edges[nearestEdgeIndex];
                    Vector2 to = new Vector2(current.X - Width / 2, current.Y - Width / 2);
                    Debug.DrawLine(from, to, Color.black, 10f);
                    edges.RemoveAt(nearestEdgeIndex);
                }
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

                CavePoint from = nearestRegionEdgePoints.Item1;
                CavePoint to = nearestRegionEdgePoints.Item2;
                Debug.DrawLine(new Vector2(from.X - Width / 2, from.Y - Width / 2), new Vector2(to.X - Width / 2, to.Y - Width / 2), Color.yellow, 10f);

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
                    if (map[x, y].accessible != accessible) continue;
                    emptyCells.Add(map[x, y]);
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
                    map[x, y] = new CavePoint(x, y);
                    if (map[x, y].IsOnScreenEdge)
                    {
                        map[x, y].accessible = false;
                    }
                    else
                    {
                        map[x, y].accessible = Random.Range(0, 100) >= randomFillPercent;
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
                    int neighbourWallTiles = GetSurroundingWallCount(map[x, y]);

                    if (neighbourWallTiles > 4)
                    {
                        map[x, y].accessible = false;
                    }
                    else if (neighbourWallTiles <
                             4)
                    {
                        map[x, y].accessible = true;
                    }
                }
            }
        }

        private int GetSurroundingWallCount(CavePoint point) => point.Neighbors().Count(p => p != null && !p.accessible);

        private void OnDrawGizmos()
        {
            if (map == null) return;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Width; y++)
                {
                    CavePoint point = map[x, y];
                    if (point.IsEdge)
                    {
                        Gizmos.color = new Color(0, 0, 0, 0.5f);
                    }
                    else if (point.accessible)
                    {
                        Gizmos.color = new Color(1, 1, 1, 0.5f);
                    }
                    else
                    {
                        continue;
                    }

                    Vector3 pos = new Vector3(-Width / 2 + x + 0.5f, -Width / 2 + y + 0.5f, 0);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }

            foreach (Region region in _regions)
            {
                Gizmos.color = region.Colour;
                foreach (CavePoint p in region.Points())
                {
                    if (p.OutOfRange()) continue;
                    Vector3 pos = new Vector3(-Width / 2 + p.X + .5f, -Width / 2 + p.Y + .5f, 0);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }

        private class Region
        {
            private readonly HashSet<CavePoint> _edges;
            private readonly HashSet<CavePoint> _points;
            public readonly Color Colour;

            public Region(HashSet<CavePoint> points, HashSet<CavePoint> edges)
            {
                _points = points;
                _edges = edges;
                foreach (CavePoint cavePoint in _points)
                    cavePoint.IsEdge = false;
                foreach (CavePoint cavePoint in _edges)
                    cavePoint.IsEdge = true;
                Colour = Color.HSVToRGB(Random.Range(0f, 1f), 1f, 1f);
                Colour.a = 0.5f;
            }

            public void ClearEdge()
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
                        map[drawX, drawY].accessible = true;
                    }
                }
            }

            private List<CavePoint> GetLine(CavePoint from, CavePoint to)
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
                    line.Add(map[x, y]);
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
                if (valid) return valid;
                MakeImpassable();
                foreach (CavePoint cavePoint in _edges)
                    cavePoint.IsEdge = false;

                return valid;
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

            public List<CavePoint> Neighbors()
            {
                if (_neighbors != null) return _neighbors;
                _neighbors = new List<CavePoint>();
                for (int x = X - 1; x <= X + 1; ++x)
                {
                    for (int y = Y - 1; y <= Y + 1; ++y)
                    {
                        if (x < 0 || x >= Width || y < 0 || y >= Width)
                        {
                            _neighbors.Add(null);
                            continue;
                        }

                        if (x == X && y == Y) continue;
                        _neighbors.Add(map[x, y]);
                    }
                }

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