using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Game.Characters.Player;
using SamsHelper;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

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

        private readonly HashSet<Cell> _reachableCells = new HashSet<Cell>(new CellComparer());
        public static HashSet<Cell> UnreachableCells = new HashSet<Cell>(new CellComparer());

        private static List<AreaGenerator.Shape> _shapes;
        private static ContactFilter2D cf;

        private static readonly Collider2D[] colliders = new Collider2D[5000];
        private static List<List<Vector2>> _hiddenCellPolys = new List<List<Vector2>>();

        public void Awake()
        {
            UnreachableCells = new HashSet<Cell>(new CellComparer());
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
                    _reachableCells.Remove(c);
                    UnreachableCells.Add(c);
                    c.Barrier = shape;
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
            return Grid[x, y];
        }

        public static bool IsLineObstructed(Vector3 start, Vector3 end)
        {
            Vector3 direction = (end - start).normalized;
            float distance = Vector2.Distance(start, end);
            for (float j = 0; j <= distance; j += 0.05f)
            {
                Vector3 currentPosition = start + direction * j;
                Cell cellHere = PositionToCell(currentPosition);
                if (UnreachableCells.Contains(cellHere)) return true;
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
                        else if (!IsLineObstructed(current.Position, next.Position))
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
                    if (UnreachableCells.Contains(current)) continue;
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
            Gizmos.color = new Color(1, 0, 0, 0.4f);
            foreach (Cell cell in UnreachableCells)
            {
                Gizmos.DrawCube(new Vector3(cell.XPos, cell.YPos), new Vector3(CellWidth, CellWidth, 1));
            }

            Gizmos.color = new Color(0, 1, 1, 0.4f);
            foreach (Cell cell in _cellsInRange)
            {
                Gizmos.DrawCube(new Vector3(cell.XPos, cell.YPos), new Vector3(CellWidth, CellWidth, 1));
            }
        }

        private static List<Cell> _cellsInRange = new List<Cell>();

        public static Cell FindCellToAttackPlayer(Cell currentCell, int maxRange, int minRange = 0)
        {
            Cell playerCell = CombatManager.Player.CurrentCell();
            _cellsInRange = CellsInRange(playerCell, maxRange, minRange);
            Cell nearestValidCell = null;
            int iteratorStart = 0;
            int minIndex = -1;
            float minDistance = 1000;
            for (int i = iteratorStart; i < _cellsInRange.Count; ++i)
            {
                float distance = currentCell.Distance(_cellsInRange[i]);
                if (distance < minDistance)
                {
                    minIndex = i;
                    minDistance = distance;
                }

                if (i != _cellsInRange.Count - 1) continue;
                Cell temp = _cellsInRange[iteratorStart];
                _cellsInRange[iteratorStart] = _cellsInRange[minIndex];
                _cellsInRange[minIndex] = temp;
                if (!IsCellHidden(_cellsInRange[iteratorStart]))
                {
                    nearestValidCell = _cellsInRange[iteratorStart];
                    break;
                }
                ++iteratorStart;
            }
            if (nearestValidCell == null) return currentCell;
            List<Cell> cellsNearTarget = CellsInRange(nearestValidCell, 3, 0).FindAll(c =>
            {
                float distance = c.Distance(playerCell);
                bool outOfRange = distance < minRange || distance > maxRange;
                if (outOfRange) return false;
                return !IsCellHidden(c);
            });
            return Helper.RandomInList(cellsNearTarget);
        }

        public static Cell FindCoverNearMe(Cell currentCell, int maxRange = 10)
        {
            List<Cell> cellsInRange = CellsInRange(currentCell, maxRange, 0);
            List<Cell> hiddenCells = cellsInRange.FindAll(IsCellHidden);
            Cell nearestHiddenCell = null;
            float nearestCellDistance = 1000;
            hiddenCells.ForEach(c =>
            {
                float distance = currentCell.Distance(c);
                if (distance >= nearestCellDistance) return;
                nearestCellDistance = distance;
                nearestHiddenCell = c;
            });
            return nearestHiddenCell;
        }

        public static int WorldToGridDistance(float distance)
        {
            return Mathf.FloorToInt(distance * CellResolution);
        }

        private static Vector2 _lastPlayerPosition;
        private static readonly HashSet<Cell> _hiddenCells = new HashSet<Cell>(new CellComparer());

        public static bool IsCellHidden(Cell c)
        {
            Vector2 currentPlayerPosition = CombatManager.Player.transform.position;
            if (_lastPlayerPosition != currentPlayerPosition)
            {
                UpdateHiddenCellPolys(currentPlayerPosition);
            }
            _lastPlayerPosition = currentPlayerPosition;

            if (_hiddenCells.Contains(c)) return true;
            bool hidden = IsLineObstructed(c.Position, currentPlayerPosition);
            if (hidden) _hiddenCells.Add(c);
            return hidden;
        }

        private static void UpdateHiddenCellPolys(Vector2 playerPosition)
        {
            _hiddenCells.Clear();
            _hiddenCellPolys.Clear();
            _shapes.ForEach(shape =>
            {
                Vector2 dirToShape = shape.ShapeObject.transform.position;
                dirToShape -= playerPosition;

                float leftAngleMax = 0;
                float rightAngleMax = 0;
                Vector2 leftPoint = Vector2.zero;
                Vector2 rightPoint = Vector2.zero;

                foreach (Vector2 worldPoint in shape.WorldVerts)
                {
                    Vector2 dirToPoint = worldPoint - playerPosition;
                    float angle = Vector2.SignedAngle(dirToShape, dirToPoint);
                    if (angle < leftAngleMax)
                    {
                        leftAngleMax = angle;
                        leftPoint = worldPoint;
                        continue;
                    }

                    if (angle <= rightAngleMax) continue;
                    rightAngleMax = angle;
                    rightPoint = worldPoint;
                }

                Vector2 leftPointDistant = leftPoint + (leftPoint - playerPosition).normalized * 20 * 1.5f;
                Vector2 rightPointDistant = rightPoint + (rightPoint - playerPosition).normalized * 20 * 1.5f;

                List<Vector2> hiddenPoly = new List<Vector2> {leftPoint, rightPoint, rightPointDistant, leftPointDistant};
                _hiddenCellPolys.Add(hiddenPoly);
            });
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
                    _reachableCells.Add(Grid[x, y]);
                    _gridNodes.Add(Grid[x, y].Node);
                }
            }
        }
    }
}