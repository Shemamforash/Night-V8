using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EpPathFinding.cs;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace Game.Combat.Generation
{
    public static class PathingGrid
    {
        public static int CombatAreaWidth = 30;
        public static int CombatMovementDistance = CombatAreaWidth - 3;
        public const int CellResolution = 4;
        public const float CellWidth = 1f / CellResolution;
        public static int GridWidth;

        public static void SetCombatAreaWidth(int width)
        {
            CombatAreaWidth = width;
            CombatMovementDistance = width - 3;
            GridWidth = CombatAreaWidth * CellResolution;
        }

        public static Cell[][] Grid;
        public static readonly HashSet<Cell> _invalidCells = new HashSet<Cell>();

        private static Vector2 _lastPlayerPosition;
        private static readonly HashSet<Cell> _hiddenCells = new HashSet<Cell>(new CellComparer());

        private static readonly List<Cell> _cellsInRange = new List<Cell>();
        private static Stopwatch _stopwatch;

        public static void InitialiseGrid()
        {
            _stopwatch = Stopwatch.StartNew();
            GenerateBaseGrid();
            UiCombatRingBoundDrawer.Draw();
            _stopwatch.Stop();
            Helper.PrintTime("Grid: ", _stopwatch);
            _stopwatch = Stopwatch.StartNew();
        }

        public static bool AddBarrier(Polygon barrier)
        {
            HashSet<Cell> intersectingCells = GetIntersectingGridCells(barrier);
            if (intersectingCells.Intersect(_invalidCells).Count() != 0) return false;

            _outOfRangeList.RemoveAll(e => intersectingCells.Contains(e));
            _edgePositionList.RemoveAll(e => intersectingCells.Contains(e));

            foreach (Cell cell in intersectingCells) _invalidCells.Add(cell);

            return true;
        }

        private static List<Vector2> _blockingVerts = new List<Vector2>();

        public static void AddBlockingArea(Vector2 origin, float radius)
        {
            _blockingVerts.Clear();
            for (int angle = 0; angle < 360; angle += 20)
            {
                Vector2 vert = AdvancedMaths.CalculatePointOnCircle(angle, radius, Vector2.zero);
                _blockingVerts.Add(vert);
            }

            Polygon polygon = new Polygon(_blockingVerts, origin);
            AddBarrier(polygon);
        }

        public static void FinaliseGrid()
        {
            _stopwatch.Stop();
            Helper.PrintTime("Barriers: ", _stopwatch);
            _stopwatch = Stopwatch.StartNew();
            _stopwatch.Stop();
            Helper.PrintTime("Neighbors: ", _stopwatch);
            _outOfRangeList.ForEach(c => c.OutOfRange = true);
            _edgePositionList.ForEach(c => c.IsEdgeCell = true);

            int i = 0;
            Parallel.For(i, GridWidth, x =>
            {
                for (int y = 0; y < GridWidth; ++y)
                {
                    _searchGrid.SetWalkableAt(x, y, Grid[x][y].Reachable);
                }
            });
        }

        public static Cell GetEdgeCell()
        {
            return Helper.RandomElement(_edgePositionList);
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

        private static List<Cell> _sharedCells = new List<Cell>();

        public static Cell GetCellOrbitingTarget(Cell current, Cell target, Vector2 direction, float distanceFromTarget, float distanceFromCurrent)
        {
            List<Cell> cellsAroundTarget = CellsInRange(target, WorldToGridDistance(distanceFromTarget), 3);
            List<Cell> cellsInFrontOfMe = GetCellsInFrontOfMe(current, direction, distanceFromCurrent);
            _sharedCells.Clear();
            _sharedCells.AddRange(cellsAroundTarget.Intersect(cellsInFrontOfMe));
            if (_sharedCells.Count != 0) return _sharedCells.RandomElement();
            return FindNearestCell(cellsAroundTarget, false, current);
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

        private static List<Cell> _cellsToRemove = new List<Cell>();

        public static void SmoothRoute(List<Cell> route)
        {
            int currentIndex = 0;
            while (currentIndex < route.Count)
            {
                Cell current = route[currentIndex];
                _cellsToRemove.Clear();
                bool noneHit = false;
                for (int i = route.Count - 1; i > currentIndex; --i)
                {
                    Cell next = route[i];
                    if (noneHit)
                    {
                        _cellsToRemove.Add(next);
                    }
                    else if (!IsLineObstructed(current.Position, next.Position, true))
                    {
                        noneHit = true;
                    }
                }

                _cellsToRemove.ForEach(cell => route.Remove(cell));
                ++currentIndex;
            }
        }

        private static List<Cell> _cellPath = new List<Cell>();

        public static List<Cell> JPS(Cell start, Cell end)
        {
            _searchGrid.Reset();
            GridPos startPos = new GridPos(start.x, start.y);
            GridPos endPos = new GridPos(end.x, end.y);
            JumpPointParam jpParam = new JumpPointParam(_searchGrid, startPos, endPos);
            List<GridPos> path = JumpPointFinder.FindPath(jpParam);
            _cellPath.Clear();
            path.ForEach(pos =>
            {
                float x = (pos.x - GridWidth / 2f) / CellResolution;
                float y = (pos.y - GridWidth / 2f) / CellResolution;
                _cellPath.Add(WorldToCellPosition(new Vector2(x, y)));
            });
            return _cellPath;
        }

        public static Cell GetCellNearMe(Vector2 position, float distanceMax, float distanceMin = 0)
        {
            return GetCellNearMe(WorldToCellPosition(position), distanceMax, distanceMin);
        }

        public static Cell GetCellNearMe(Cell current, float distanceMax, float distanceMin = 0) =>
            CellsInRange(current, WorldToGridDistance(distanceMax), WorldToGridDistance(distanceMin)).RandomElement();

        public static List<Cell> GetCellsNearMe(Vector2 position, int noCells, float distanceMax, float distanceMin = 0) =>
            GetCellsNearMe(WorldToCellPosition(position), noCells, distanceMax, distanceMin);

        public static List<Cell> GetCellsNearMe(Cell current, int noCells, float distanceMax, float distanceMin = 0)
        {
            List<Cell> cells = CellsInRange(current, WorldToGridDistance(distanceMax), WorldToGridDistance(distanceMin));
            cells.Shuffle();
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

        public static Cell GetCellOutOfRange(Vector2 position)
        {
            Cell nearest = null;
            float nearestDistance = 1000000f;
            _outOfRangeList.ForEach(c =>
            {
                float distance = Vector2.SqrMagnitude(position - c.Position);
                if (distance > nearestDistance) return;
                nearestDistance = distance;
                nearest = c;
            });
            return nearest;
        }

        private static List<Cell> CellsInRange(Cell origin, int maxRange, int minRange = 0)
        {
            Assert.IsTrue(minRange <= maxRange);
            if (origin == null) Debug.Log("initial node was null");
            _cellsInRange.Clear();
            int startX = origin.x - maxRange;
            if (startX < 0) startX = 0;
            int startY = origin.y - maxRange;
            if (startY < 0) startY = 0;
            int endX = origin.x + maxRange;
            if (endX > GridWidth) endX = GridWidth;
            int endY = origin.y + maxRange;
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

        private static List<Cell> _cellsInSquare = new List<Cell>();

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
            _cellsInSquare.Clear();
            for (int x = xMin; x <= xMax; ++x)
            {
                for (int y = yMin; y <= yMax; ++y)
                {
                    if (Grid[x][y] == null) continue;
                    _cellsInSquare.Add(Grid[x][y]);
                }
            }

            return _cellsInSquare;
        }

        private static HashSet<Cell> _intersectingCells = new HashSet<Cell>();

        private static HashSet<Cell> GetIntersectingGridCells(Polygon p)
        {
            _intersectingCells.Clear();
            List<Cell> possibleCells = CellsInSquare(p.TopLeft, p.BottomRight);
            foreach (Cell c in possibleCells)
            {
                if (!AdvancedMaths.IsPointInPolygon(c.Position - p.Position, p.Vertices)) continue;
                c.Blocked = true;
                c.Reachable = false;
                _intersectingCells.Add(c);
            }

            for (int i = 0; i < p.Vertices.Count; ++i)
            {
                Vector2 start = p.Vertices[i] + p.Position;
                Vector2 end = p.Vertices[i + 1 == p.Vertices.Count ? 0 : i + 1] + p.Position;
                List<Cell> cells = CellsOnLine(start, end);
                cells.ForEach(c =>
                {
                    c.Reachable = false;
                    _intersectingCells.Add(c);
                });
            }

            return _intersectingCells;
        }

        private static HashSet<Cell> _cellsOnLine = new HashSet<Cell>();


        private static List<Cell> CellsOnLine(Vector2 start, Vector2 end)
        {
            _cellsOnLine.Clear();
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
                _cellsOnLine.Add(cellHere);
            }

            Vector2Int endPos = WorldToGridPosition(end);
            if (!(endPos.x < 0 || endPos.x >= GridWidth || endPos.y < 0 || endPos.y >= GridWidth)) _cellsOnLine.Add(Grid[endPos.x][endPos.y]);
            return _cellsOnLine.ToList();
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

        public static readonly List<Cell> _outOfRangeList = new List<Cell>();

        public static readonly List<Cell> _edgePositionList = new List<Cell>();

        private static StaticGrid _searchGrid;

        private static void GenerateBaseGrid()
        {
            _outOfRangeList.Clear();
            _edgePositionList.Clear();
            _invalidCells.Clear();
            _hiddenCells.Clear();
            _cellsInRange.Clear();
            Grid = new Cell[GridWidth][];
            _searchGrid = new StaticGrid(GridWidth, GridWidth);
            int outOfRangeDistanceSqrd = (int) Mathf.Pow(CombatMovementDistance * CellResolution * 0.5f, 2f);
            int edgeDistanceSquared = (int) Mathf.Pow((CombatMovementDistance - 1) * CellResolution * 0.5f, 2f);

            int i = 0;
            for(int x = 0; x < GridWidth; ++x)
            {
                if (Grid[x] == null) Grid[x] = new Cell[GridWidth];
                for (int y = 0; y < GridWidth; ++y)
                {
                    float xComp = Mathf.Pow(x - GridWidth / 2f, 2f);
                    float yComp = Mathf.Pow(y - GridWidth / 2f, 2f);
                    float distanceSqrd = xComp + yComp;
                    Cell c = Cell.Generate(x, y);
                    Grid[x][y] = c;
                    if (distanceSqrd > outOfRangeDistanceSqrd) _outOfRangeList.Add(c);
                    else if (distanceSqrd > edgeDistanceSquared) _edgePositionList.Add(c);
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