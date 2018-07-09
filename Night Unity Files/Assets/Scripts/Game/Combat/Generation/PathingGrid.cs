using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Game.Combat.Enemies;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace Game.Combat.Generation
{
    public static class PathingGrid
    {
        public const int CombatAreaWidth = 40;
        public const int CombatMovementDistance = CombatAreaWidth - 3;
        public const int CellResolution = 4;
        public const float CellWidth = 1f / CellResolution;
        public const int GridWidth = CombatAreaWidth * CellResolution;

        public static Cell[][] Grid;
        private static readonly List<Cell> _gridNodes = new List<Cell>();
        private static readonly HashSet<Cell> _invalidCells = new HashSet<Cell>();

        private static Vector2 _lastPlayerPosition;
        private static readonly HashSet<Cell> _hiddenCells = new HashSet<Cell>(new CellComparer());

        private static readonly List<Cell> _cellsInRange = new List<Cell>();
        private static Stopwatch _stopwatch;

        public static void InitialiseGrid()
        {
            _stopwatch = Stopwatch.StartNew();
            GenerateBaseGrid();
            _invalidCells.Clear();
            _stopwatch.Stop();
            Debug.Log("grid: " + _stopwatch.Elapsed.ToString("mm\\:ss\\.ff"));
            _stopwatch = Stopwatch.StartNew();
        }

        public static bool AddBarrier(Barrier barrier, bool forcePlace)
        {
            HashSet<Cell> intersectingCells = GetIntersectingGridCells(barrier);
            if (intersectingCells.Intersect(_invalidCells).Count() != 0 && !forcePlace) return false;
            foreach (Cell cell in intersectingCells)
            {
                _invalidCells.Add(cell);
                _outOfRangeSet.Remove(cell);
            }

            return true;
        }

        public static void FinaliseGrid()
        {
            _stopwatch.Stop();
            Debug.Log("barriers: " + _stopwatch.Elapsed.ToString("mm\\:ss\\.ff"));
            _stopwatch = Stopwatch.StartNew();
            _gridNodes.ForEach(n => n.SetNeighbors());
            _stopwatch.Stop();
            Debug.Log("neighbors: " + _stopwatch.Elapsed.ToString("mm\\:ss\\.ff"));

            _outOfRangeList = _outOfRangeSet.ToList();
        }

        public static List<Cell> GetCellsInFrontOfMe(Cell current, Vector2 direction, float distance)
        {
            Vector2 positionInFront = current.Position + direction.normalized * distance;
            Cell cellInFrontOfMe = WorldToCellPosition(positionInFront);
            while (cellInFrontOfMe == null && distance > 0)
            {
                cellInFrontOfMe = WorldToCellPosition(positionInFront);
                distance -= 0.1f;
                positionInFront = current.Position + direction * distance;
            }

            return cellInFrontOfMe == null ? null : CellsInRange(cellInFrontOfMe, WorldToGridDistance(distance));
        }

        public static Cell GetCellOrbitingTarget(Cell current, Cell target, Vector2 direction, float distanceFromTarget, float distanceFromCurrent)
        {
            List<Cell> cellsAroundTarget = CellsInRange(target, WorldToGridDistance(distanceFromTarget), 3);
            List<Cell> cellsInFrontOfMe = GetCellsInFrontOfMe(current, direction, distanceFromCurrent);
            List<Cell> sharedCells = new List<Cell>(cellsAroundTarget.Intersect(cellsInFrontOfMe));
            if (sharedCells.Count == 0)
            {
                return FindNearestCell(cellsAroundTarget, false, current);
            }

            return Helper.RandomInList(sharedCells);
        }

        public static Cell WorldToCellPosition(Vector2 position, bool print = true)
        {
            Vector2Int gridPosition = WorldToGridPosition(position);
            int x = gridPosition.x;
            int y = gridPosition.y;
            try
            {
                return Grid[x][y];
            }
            catch (IndexOutOfRangeException)
            {
                if (print) Debug.Log("Invalid cell position: (" + x + " ," + y + ")");
                return null;
            }
        }


        //todo slow
        private static bool IsLineObstructed(Vector2 start, Vector2 end, bool includeReachable = false)
        {
            float distance = Vector2.Distance(start, end);
            float obstructionInterval = 1f / (CellResolution * 2);
            int interval = (int) (distance / obstructionInterval);
            for (float j = 0; j <= interval; ++j)
            {
                float lerpVal = j / interval;
                Vector2 currentPosition = Vector2.Lerp(start, end, lerpVal);
                Vector2Int pos = WorldToGridPosition(currentPosition);
                if (pos.x < 0 || pos.x >= GridWidth || pos.y < 0 || pos.y >= GridWidth) continue;
                Cell cellHere = Grid[pos.x][pos.y];
                if (cellHere.Blocked) return true;
                if (!cellHere.Reachable && includeReachable) return true;
            }

            return false;
        }

        public static void SmoothRoute(List<Cell> route)
        {
            int currentIndex = 0;
            while (currentIndex < route.Count)
            {
                Cell current = route[currentIndex];
                List<Cell> cellsToRemove = new List<Cell>();
                bool noneHit = false;
                for (int i = route.Count - 1; i > currentIndex; --i)
                {
                    Cell next = route[i];
                    if (noneHit)
                    {
                        cellsToRemove.Add(next);
                    }
                    else if (!IsLineObstructed(current.Position, next.Position, true))
                    {
                        noneHit = true;
                    }
                }

                cellsToRemove.ForEach(cell => route.Remove(cell));
                ++currentIndex;
            }
        }

        public static Thread ThreadRouteToCell(Cell from, Cell to, EnemyBehaviour enemy, float timeStarted)
        {
            Thread thread = new Thread(() =>
            {
                List<Cell> route = new List<Cell>();
                List<Node> nodePath = Pathfinding.AStar(from.Node, to.Node);
                nodePath.ForEach(n => route.Add(WorldToCellPosition(n.Position)));
                enemy.SetRoute(route, timeStarted);
            });
            thread.Start();
            return thread;
        }


        public static Cell GetCellNearMe(Vector2 position, float distanceMax, float distanceMin = 0)
        {
            return GetCellNearMe(WorldToCellPosition(position), distanceMax, distanceMin);
        }

        public static Cell GetCellNearMe(Cell current, float distanceMax, float distanceMin = 0) =>
            Helper.RandomInList(CellsInRange(current, WorldToGridDistance(distanceMax), WorldToGridDistance(distanceMin)));

        public static List<Cell> GetCellsNearMe(Vector2 position, int noCells, float distanceMax, float distanceMin = 0) =>
            GetCellsNearMe(WorldToCellPosition(position), noCells, distanceMax, distanceMin);

        public static List<Cell> GetCellsNearMe(Cell current, int noCells, float distanceMax, float distanceMin = 0)
        {
            List<Cell> cells = CellsInRange(current, WorldToGridDistance(distanceMax), WorldToGridDistance(distanceMin));
            Helper.Shuffle(cells);
            return cells.Take(noCells).ToList();
        }

        public static Cell FindCoverNearMe(Cell currentCell)
        {
            List<Cell> cellsNearby = CellsInRange(currentCell, 10);
            return FindNearestCell(cellsNearby, true, currentCell);
        }

        public static bool IsCellHidden(Cell c)
        {
            Vector2 currentPlayerPosition = PlayerCombat.Instance.transform.position;
            if (_lastPlayerPosition != currentPlayerPosition) _hiddenCells.Clear();

            _lastPlayerPosition = currentPlayerPosition;

            if (_hiddenCells.Contains(c)) return true;
//            bool hidden = IsLineObstructed(c.Position, currentPlayerPosition);
            List<Vector2> vertices = PlayerCombat.Instance._playerLight.Vertices();
            bool hidden = AdvancedMaths.IsPointInPolygon(c.Position, vertices);
            if (hidden) _hiddenCells.Add(c);
            return hidden;
        }

        public static Cell GetCellOutOfRange() => Helper.RandomInList(_outOfRangeList);

        private static List<Cell> CellsInRange(Cell origin, int maxRange, int minRange = 0)
        {
            Assert.IsTrue(minRange <= maxRange);
            if (origin == null) Debug.Log("initial node was null");
            _cellsInRange.Clear();
            int startX = origin.XIndex - maxRange;
            if (startX < 0) startX = 0;
            int startY = origin.YIndex - maxRange;
            if (startY < 0) startY = 0;
            int endX = origin.XIndex + maxRange;
            if (endX > GridWidth) endX = GridWidth;
            int endY = origin.YIndex + maxRange;
            if (endY > GridWidth) endY = GridWidth;

            minRange *= minRange;
            maxRange *= maxRange;
            for (int x = startX; x < endX; ++x)
            for (int y = startY; y < endY; ++y)
            {
                Cell current = Grid[x][y];
                if (!current.Reachable) continue;
                float distance = current.SqrDistance(origin);
                if (distance < minRange || distance > maxRange) continue;
                _cellsInRange.Add(current);
            }

            return _cellsInRange;
        }

        public static bool IsSpaceAvailable(Vector3 topLeft, Vector3 bottomRight)
        {
            List<Cell> cells = CellsInSquare(topLeft, bottomRight);
            return cells.All(c => c.Reachable);
        }

        private static List<Cell> CellsInSquare(Vector3 topLeft, Vector3 bottomRight)
        {
            Vector2Int topLeftGridPosition = WorldToGridPosition(topLeft);
            Vector2Int bottomRightGridPosition = WorldToGridPosition(bottomRight);
            int xMin = topLeftGridPosition.x;
            int yMin = topLeftGridPosition.y;
            int xMax = bottomRightGridPosition.x;
            int yMax = bottomRightGridPosition.y;
            xMin = Mathf.Clamp(xMin, 0, GridWidth - 1);
            yMin = Mathf.Clamp(yMin, 0, GridWidth - 1);
            xMax = Mathf.Clamp(xMax, 0, GridWidth - 1);
            yMax = Mathf.Clamp(yMax, 0, GridWidth - 1);
            List<Cell> cellsInSquare = new List<Cell>();
            for (int x = xMin; x <= xMax; ++x)
            {
                for (int y = yMin; y <= yMax; ++y)
                {
                    if (Grid[x][y] == null) continue;
                    cellsInSquare.Add(Grid[x][y]);
                }
            }

            return cellsInSquare;
        }

        private static HashSet<Cell> GetIntersectingGridCells(Polygon p)
        {
            HashSet<Cell> intersectingCells = new HashSet<Cell>();
            List<Cell> possibleCells = CellsInSquare(p.TopLeft, p.BottomRight);
            foreach (Cell c in possibleCells)
            {
                if (!AdvancedMaths.IsPointInPolygon(c.Position - p.Position, p.Vertices)) continue;
                c.Blocked = true;
                c.Reachable = false;
                intersectingCells.Add(c);
            }

            for (int i = 0; i < p.Vertices.Count; ++i)
            {
                Vector2 start = p.Vertices[i] + p.Position;
                Vector2 end = p.Vertices[i + 1 == p.Vertices.Count ? 0 : i + 1] + p.Position;
                List<Cell> cells = CellsOnLine(start, end);
                cells.ForEach(c =>
                {
                    c.Reachable = false;
                    intersectingCells.Add(c);
                });
            }

            return intersectingCells;
        }

        private static List<Cell> CellsOnLine(Vector2 start, Vector2 end)
        {
            HashSet<Cell> cells = new HashSet<Cell>();
            float distance = Vector2.Distance(start, end);
            float obstructionInterval = 1f / (CellResolution * 2);
            int interval = (int) (distance / obstructionInterval);
            for (float j = 0; j <= interval; ++j)
            {
                float lerpVal = j / interval;
                Vector2 currentPosition = Vector2.Lerp(start, end, lerpVal);
                Vector2Int pos = WorldToGridPosition(currentPosition);
                if (pos.x < 0 || pos.x >= GridWidth || pos.y < 0 || pos.y >= GridWidth) continue;
                Cell cellHere = Grid[pos.x][pos.y];
                cells.Add(cellHere);
            }

            Vector2Int endPos = WorldToGridPosition(end);
            if (!(endPos.x < 0 || endPos.x >= GridWidth || endPos.y < 0 || endPos.y >= GridWidth)) cells.Add(Grid[endPos.x][endPos.y]);
            return cells.ToList();
        }

        private static Vector2Int WorldToGridPosition(Vector2 position)
        {
            position.x += CellWidth / 2f;
            position.y += CellWidth / 2f;
            int x = Mathf.FloorToInt(position.x * CellResolution);
            int y = Mathf.FloorToInt(position.y * CellResolution);
            x += GridWidth / 2;
            y += GridWidth / 2;
            return new Vector2Int(x, y);
        }

        private static Cell FindNearestCell(List<Cell> cells, bool shouldCellBeHidden, Cell currentCell)
        {
            Cell nearestValidCell = null;
            int iteratorStart = 0;
            while (iteratorStart < cells.Count)
            {
                int minIndex = -1;
                float minDistance = float.PositiveInfinity;
                for (int i = iteratorStart; i < cells.Count; ++i)
                {
                    float distance = currentCell.SqrDistance(cells[i]);
                    if (distance >= minDistance) continue;
                    minIndex = i;
                    minDistance = distance;
                }

                Cell temp = cells[iteratorStart];
                cells[iteratorStart] = cells[minIndex];
                cells[minIndex] = temp;
                if (shouldCellBeHidden && IsCellHidden(cells[iteratorStart]))
                {
                    nearestValidCell = cells[iteratorStart];
                    break;
                }

                if (!shouldCellBeHidden && !IsCellHidden(cells[iteratorStart]))
                {
                    nearestValidCell = cells[iteratorStart];
                    break;
                }

                ++iteratorStart;
            }

            return nearestValidCell;
        }

        private static int WorldToGridDistance(float distance) => Mathf.FloorToInt(distance * CellResolution);

        private static readonly HashSet<Cell> _outOfRangeSet = new HashSet<Cell>();
        public static List<Cell> _outOfRangeList = new List<Cell>();

        private static void GenerateBaseGrid()
        {
            _gridNodes.Clear();
            if (Grid == null) Grid = new Cell[GridWidth][];
            int outOfRangeDistanceSqrd = (int) Mathf.Pow(CombatMovementDistance * CellResolution * 0.5f, 2f);
            for (int x = 0; x < GridWidth; ++x)
            {
                if (Grid[x] == null) Grid[x] = new Cell[GridWidth];
                for (int y = 0; y < GridWidth; ++y)
                {
                    float xComp = Mathf.Pow(x - GridWidth / 2f, 2f);
                    float yComp = Mathf.Pow(y - GridWidth / 2f, 2f);
                    float distanceSqrd = xComp + yComp;
                    Grid[x][y] = Cell.Generate(x, y);

                    if (distanceSqrd > outOfRangeDistanceSqrd)
                    {
                        _outOfRangeSet.Add(Grid[x][y]);
                    }

                    _gridNodes.Add(Grid[x][y]);
                }
            }
        }

        private class CellComparer : IEqualityComparer<Cell>
        {
            public bool Equals(Cell x, Cell y) => x.id == y.id;

            public int GetHashCode(Cell cell) => cell.id;
        }
    }
}