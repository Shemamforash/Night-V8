using System;
using System.Collections.Generic;
using System.Threading;
using SamsHelper;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Combat
{
    public class PathingGrid : MonoBehaviour
    {
        public const int GameWorldWidth = 20;
        public const int CellResolution = 5;
        public const float CellWidth = 1f / CellResolution;
        public const int GridWidth = GameWorldWidth * CellResolution;


        public static Cell[,] Grid;
        private static readonly List<Node<Cell>> _gridNodes = new List<Node<Cell>>();
        private static PathingGrid _instance;

        private static List<AreaGenerator.Shape> _shapes;
        private static ContactFilter2D cf;

        private static readonly Collider2D[] colliders = new Collider2D[5000];
        private static Vector2 _lastPlayerPosition;
        private static readonly HashSet<Cell> _hiddenCells = new HashSet<Cell>(new CellComparer());

        public void Awake()
        {
            _instance = this;
        }

        public static PathingGrid Instance()
        {
            return _instance;
        }

        public void GenerateGrid(List<AreaGenerator.Shape> barriers)
        {
            _shapes = barriers;
            cf.useTriggers = true;
            cf.SetLayerMask(1 << 9);
            GenerateBaseGrid();
            foreach (AreaGenerator.Shape shape in _shapes)
            {
                int areas = shape.Collider.OverlapCollider(cf, colliders);
                for (int i = 0; i < areas; ++i)
                {
                    Cell c = colliders[i].GetComponent<Cell>();
                    shape.OccupiedCells.Add(c);
                    c.Reachable = false;
                    if (AdvancedMaths.IsPointInPolygon(c.Position, shape.WorldVerts)) c.Blocked = true;
                }
            }

            _gridNodes.ForEach(n => n.Content.SetNeighbors());
        }

        public static Cell PositionToCell(Vector2 position)
        {
            position.x += CellWidth / 2f;
            position.y += CellWidth / 2f;
            int x = Mathf.FloorToInt(position.x * CellResolution);
            int y = Mathf.FloorToInt(position.y * CellResolution);
            x += GridWidth / 2;
            y += GridWidth / 2;
            try
            {
                return Grid[x, y];
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.Log("(" + x + " ," + y + ")");
                Debug.Log(e.StackTrace);
                return null;
            }
        }

        public static bool IsLineObstructed(Vector3 start, Vector3 end, bool includeReachable = false)
        {
            Vector3 direction = (end - start).normalized;
            float distance = Vector2.Distance(start, end);
            for (float j = 0; j <= distance; j += 0.05f)
            {
                Vector3 currentPosition = start + direction * j;
                Cell cellHere = PositionToCell(currentPosition);
                if (cellHere.Blocked) return true;
                if (!cellHere.Reachable && includeReachable) return true;
            }

            return false;
        }

        public static Thread RouteToCell(Cell from, Cell to, Queue<Cell> path)
        {
            Thread thread = new Thread(() =>
            {
                List<Cell> newPath = Pathfinding.AStar(from.Node, to.Node, _gridNodes);
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
                        {
                            cellsToRemove.Add(next);
                        }
                        else if (!IsLineObstructed(current.Position, next.Position, true))
                        {
                            noneHit = true;
                        }
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
            {
                for (int y = startY; y < endY; ++y)
                {
                    Cell current = Grid[x, y];
                    if (!current.Reachable) continue;
                    float distance = current.Distance(origin);
                    if (distance < minRange || distance > maxRange) continue;
                    cellsInRange.Add(current);
                }
            }

            return cellsInRange;
        }

        public static Cell GetCellNearMe(Cell current, int distance)
        {
            return Helper.RandomInList(CellsInRange(current, distance));
        }

        private void OnDrawGizmos()
        {
            foreach (Cell cell in _cellsInRange)
            {
                Gizmos.color = IsCellHidden(cell) ? new Color(1, 0, 1, 0.4f) : new Color(0, 1, 1, 0.4f);
                Gizmos.DrawCube(new Vector3(cell.XPos, cell.YPos), new Vector3(CellWidth, CellWidth, 1));
            }
        }

        private static List<Cell> _cellsInRange = new List<Cell>();

        public static Cell FindCellToAttackPlayer(Cell currentCell, int maxRange, int minRange = 0)
        {
            Cell playerCell = CombatManager.Player.CurrentCell();
            List<Cell> cellsNearPlayer = CellsInRange(playerCell, maxRange, minRange);
            Cell nearestValidCell = FindNearestCell(cellsNearPlayer, false, currentCell);
            if (nearestValidCell == null) return currentCell;
            List<Cell> cellsNearTarget = CellsInRange(nearestValidCell, 3).FindAll(c =>
            {
                float distance = c.Distance(playerCell);
                bool outOfRange = distance < minRange || distance > maxRange;
                if (outOfRange) return false;
                return !IsCellHidden(c);
            });
            return Helper.RandomInList(cellsNearTarget);
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

        public static Cell FindCoverNearMe(Cell currentCell)
        {
            List<Cell> cellsNearby = CellsInRange(currentCell, 10);
            return FindNearestCell(cellsNearby, true, currentCell);
        }

        public static int WorldToGridDistance(float distance)
        {
            return Mathf.FloorToInt(distance * CellResolution);
        }

        public static bool IsCellHidden(Cell c)
        {
            Vector2 currentPlayerPosition = CombatManager.Player.transform.position;
            if (_lastPlayerPosition != currentPlayerPosition)
            {
                _hiddenCells.Clear();
            }

            _lastPlayerPosition = currentPlayerPosition;

            if (_hiddenCells.Contains(c)) return true;
            bool hidden = IsLineObstructed(c.Position, currentPlayerPosition);
            if (hidden) _hiddenCells.Add(c);
            return hidden;
        }

        private void GenerateBaseGrid()
        {
            _gridNodes.Clear();
            Grid = new Cell[GridWidth, GridWidth];
            for (int x = 0; x < GridWidth; ++x)
            {
                for (int y = 0; y < GridWidth; ++y)
                {
                    Grid[x, y] = Cell.Generate(x, y);
                    _gridNodes.Add(Grid[x, y].Node);
                }
            }
        }
    }
}