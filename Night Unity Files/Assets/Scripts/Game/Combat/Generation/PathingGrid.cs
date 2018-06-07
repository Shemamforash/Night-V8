using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using SamsHelper;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Combat.Generation
{
    public static class PathingGrid
    {
        public const int CombatAreaWidth = 45;
        public const int CombatMovementDistance = CombatAreaWidth - 3;
        public const int CellResolution = 10;
        public const float CellWidth = 1f / CellResolution;
        public const int GridWidth = CombatAreaWidth * CellResolution;

        public static Cell[,] Grid;
        private static readonly List<Cell> _gridNodes = new List<Cell>();
        private static readonly HashSet<Cell> _invalidCells = new HashSet<Cell>();

        private static Vector2 _lastPlayerPosition;
        private static readonly HashSet<Cell> _hiddenCells = new HashSet<Cell>(new CellComparer());

        public static readonly List<Cell> _cellsInRange = new List<Cell>();
        private static Stopwatch _stopwatch;

        public static void InitialiseGrid()
        {
            _stopwatch = Stopwatch.StartNew();
            GenerateBaseGrid();
            _invalidCells.Clear();
        }

        public static bool AddBarrier(Barrier barrier, bool forcePlace)
        {
            HashSet<Cell> intersectingCells = GetIntersectingGridCells(barrier);
            if (intersectingCells.Intersect(_invalidCells).Count() != 0 && !forcePlace) return false;
            foreach (Cell cell in intersectingCells)
            {
                _invalidCells.Add(cell);
            }

            return true;
        }

        public static void FinaliseGrid()
        {
            _gridNodes.ForEach(n => n.SetNeighbors());
            _stopwatch.Stop();
            Debug.Log("no physics: " + _stopwatch.Elapsed.ToString("mm\\:ss\\.ff"));
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

        public static Cell WorldToCellPosition(Vector2 position)
        {
            Vector2Int gridPosition = WorldToGridPosition(position);
            int x = gridPosition.x;
            int y = gridPosition.y;
            try
            {
                return Grid[x, y];
            }
            catch (IndexOutOfRangeException)
            {
                Debug.Log("Invalid cell position: (" + x + " ," + y + ")");
                return null;
            }
        }

        public static bool IsLineObstructed(Vector2 start, Vector2 end, bool includeReachable = false)
        {
            float distance = Vector2.Distance(start, end);
            float obstructionInterval = 1f / (CellResolution * 2);
            int interval = (int) (distance / obstructionInterval);
            for (float j = 0; j <= interval; ++j)
            {
                float lerpVal = j / interval;
                Vector2 currentPosition = Vector2.Lerp(start, end, lerpVal);
                Vector2Int pos = WorldToGridPosition(currentPosition);
                if (pos.x < 0 || pos.x > GridWidth || pos.y < 0 || pos.y > GridWidth) continue;
                Cell cellHere = Grid[pos.x, pos.y];
                if (cellHere.Blocked) return true;
                if (!cellHere.Reachable && includeReachable) return true;
            }

            return false;
        }

        public static Thread RouteToCell(Cell from, Cell to, Queue<Cell> path)
        {
            Thread thread = new Thread(() =>
            {
                List<Node> nodePath = Pathfinding.AStar(from.Node, to.Node);
                List<Cell> newPath = new List<Cell>();
                nodePath.ForEach(n => newPath.Add(WorldToCellPosition(n.Position)));
                int currentIndex = 0;
                while (currentIndex < newPath.Count)
                {
                    Cell current = newPath[currentIndex];
                    List<Cell> cellsToRemove = new List<Cell>();
                    bool noneHit = false;
                    for (int i = newPath.Count - 1; i > currentIndex; --i)
                    {
                        Cell next = newPath[i];
                        if (noneHit)
                            cellsToRemove.Add(next);
                        else if (!IsLineObstructed(current.Position, next.Position, true))
                            noneHit = true;
                    }

                    cellsToRemove.ForEach(cell => newPath.Remove(cell));
                    ++currentIndex;
                }

                path.Clear();
                newPath.ForEach(path.Enqueue);
            });
            thread.Start();
            return thread;
        }

        public static Cell GetCellNearMe(Cell current, float distance)
        {
            return Helper.RandomInList(CellsInRange(current, WorldToGridDistance(distance)));
        }

        public static Cell FindCellToAttackPlayer(Cell currentCell, float maxRange, float minRange = 0)
        {
            int max = WorldToGridDistance(maxRange);
            int min = WorldToGridDistance(minRange);
            Cell playerCell = CombatManager.Player().CurrentCell();
            List<Cell> cellsNearPlayer = CellsInRange(playerCell, max, min);
            Cell nearestValidCell = FindNearestCell(cellsNearPlayer, false, currentCell);
            if (nearestValidCell == null) return currentCell;
            List<Cell> cellsNearTarget = CellsInRange(nearestValidCell, 3).FindAll(c =>
            {
                float distance = c.Distance(playerCell);
                bool outOfRange = distance < min || distance > max;
                if (outOfRange) return false;
                return !IsCellHidden(c);
            });
            return Helper.RandomInList(cellsNearTarget);
        }

        public static Cell FindCoverNearMe(Cell currentCell)
        {
            List<Cell> cellsNearby = CellsInRange(currentCell, 10);
            return FindNearestCell(cellsNearby, true, currentCell);
        }

        public static bool IsCellHidden(Cell c)
        {
            Vector2 currentPlayerPosition = CombatManager.Player().transform.position;
            if (_lastPlayerPosition != currentPlayerPosition) _hiddenCells.Clear();

            _lastPlayerPosition = currentPlayerPosition;

            if (_hiddenCells.Contains(c)) return true;
            bool hidden = IsLineObstructed(c.Position, currentPlayerPosition);
            if (hidden) _hiddenCells.Add(c);
            return hidden;
        }

        private static List<Cell> CellsInRange(Cell origin, int maxRange, int minRange = 0)
        {
            if (origin == null) Debug.Log("initial node was null");
            List<Cell> cellsInRange = new List<Cell>();
            int startX = origin.XIndex - maxRange;
            if (startX < 0) startX = 0;
            int startY = origin.YIndex - maxRange;
            if (startY < 0) startY = 0;
            int endX = origin.XIndex + maxRange;
            if (endX > GridWidth) endX = GridWidth;
            int endY = origin.YIndex + maxRange;
            if (endY > GridWidth) endY = GridWidth;

            for (int x = startX; x < endX; ++x)
            for (int y = startY; y < endY; ++y)
            {
                Cell current = Grid[x, y];
                if (current == null || !current.Reachable) continue;
                float distance = current.Distance(origin);
                if (distance < minRange || distance > maxRange) continue;
                cellsInRange.Add(current);
            }

            return cellsInRange;
        }

        public static bool DrawGrid;

        private static void OnDrawGizmos()
        {
            foreach (Cell cell in _cellsInRange)
            {
                Gizmos.color = IsCellHidden(cell) ? new Color(1, 0, 1, 0.4f) : new Color(0, 1, 1, 0.4f);
                Gizmos.DrawCube(new Vector3(cell.XPos, cell.YPos), new Vector3(CellWidth, CellWidth, 1));
            }

            if (!DrawGrid) return;
            Gizmos.color = new Color(1, 0, 1, 0.4f);
            if (Grid == null) return;
            for (int i = 0; i < GridWidth; ++i)
            {
                for (int j = 0; j < GridWidth; ++j)
                {
                    if (Grid[i, j] == null || !Grid[i, j].Reachable) continue;
                    Gizmos.DrawCube(new Vector3(Grid[i, j].XPos, Grid[i, j].YPos), new Vector3(CellWidth, CellWidth, 1));
                }
            }
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
                    if (Grid[x, y] == null) continue;
                    cellsInSquare.Add(Grid[x, y]);
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
//                if (!c.Reachable) continue;
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
                    if (c == null) return;
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
                Cell cellHere = Grid[pos.x, pos.y];
                cells.Add(cellHere);
            }

            Vector2Int endPos = WorldToGridPosition(end);
            if (!(endPos.x < 0 || endPos.x >= GridWidth || endPos.y < 0 || endPos.y >= GridWidth)) cells.Add(Grid[endPos.x, endPos.y]);
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
                float minDistance = 1000;
                for (int i = iteratorStart; i < cells.Count; ++i)
                {
                    float distance = currentCell.Distance(cells[i]);
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

        private static int WorldToGridDistance(float distance)
        {
            return Mathf.FloorToInt(distance * CellResolution);
        }

        private static void GenerateBaseGrid()
        {
            _gridNodes.Clear();
            Grid = new Cell[GridWidth, GridWidth];
            float worldRadius = CombatAreaWidth / 2f * CellResolution;
            for (int x = 0; x < GridWidth; ++x)
            {
                for (int y = 0; y < GridWidth; ++y)
                {
                    float xComp = Mathf.Pow(x - GridWidth / 2f, 2f);
                    float yComp = Mathf.Pow(y - GridWidth / 2f, 2f);
                    float distance = Mathf.Sqrt(xComp + yComp);
                    if (distance > worldRadius)
                    {
                        Grid[x, y] = null;
                        continue;
                    }

                    Grid[x, y] = Cell.Generate(x, y);
                    _gridNodes.Add(Grid[x, y]);
                }
            }
        }

        private class CellComparer : IEqualityComparer<Cell>
        {
            public bool Equals(Cell x, Cell y)
            {
                return x.id == y.id;
            }

            public int GetHashCode(Cell cell)
            {
                return cell.id;
            }
        }
    }
}