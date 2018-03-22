using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
            Stopwatch watch = Stopwatch.StartNew();
            GenerateBaseGrid();
            watch.Stop();
            Debug.Log("base grid: " + watch.Elapsed.ToString("mm\\:ss\\.ff"));
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
        
        public static Thread RouteToCell(Cell from, Cell to, List<Cell> path)
        {
            Thread thread = new Thread(() =>
            {
                Stopwatch watch = Stopwatch.StartNew();
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
                newPath.ForEach(path.Add);
                watch.Stop();
//            Debug.Log(watch.Elapsed.ToString("mm\\:ss\\.ff"));
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

        private static List<Cell> CellsInRange(Cell current, int distance, bool includeUnreachable = false)
        {
            HashSet<Cell> visited = new HashSet<Cell>(new CellComparer());
            HashSet<Cell> nextLayer = new HashSet<Cell>(new CellComparer());
            if (current == null) Debug.Log("initial node was null");
            nextLayer.Add(current);

            while (nextLayer.Count != 0)
            {
                List<Cell> prevLayer = nextLayer.ToList();
                nextLayer.Clear();
                foreach (Cell c in prevLayer)
                {
                    List<Cell> neighbors = includeUnreachable ? c.AllNeighbors : c.ReachableNeighbors;
                    foreach (Cell neighbor in neighbors)
                    {
                        int xDiff = Math.Abs(neighbor.XIndex - current.XIndex);
                        int yDiff = Math.Abs(neighbor.YIndex - current.YIndex);
                        if (xDiff + yDiff > distance) continue;
                        if (visited.Contains(neighbor)) continue;
                        nextLayer.Add(neighbor);
                        visited.Add(neighbor);
                    }
                }
            }
            return visited.ToList();
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

        public static Cell GetCellInRange(Cell currentCell, int maxRange, int minRange = 0)
        {
            Cell playerCell = PositionToCell(CombatManager.Player.transform.position);
            List<Cell> cellsInRange = CellsInRange(playerCell, Random.Range(minRange, maxRange));
            float minDistance = float.MaxValue;
            Cell nearestValidCell = null;
            _cellsInRange = cellsInRange;
            cellsInRange.ForEach(c =>
            {
                float distance = Vector2.Distance(currentCell.Position, c.Position);
                if (distance >= minDistance) return;
                if (IsCellHidden(c)) return;
                minDistance = distance;
                nearestValidCell = c;
            });
            return Helper.RandomInList(CellsInRange(nearestValidCell, 2));
            return nearestValidCell;
        }

        public static int WorldToGridDistance(float distance)
        {
            return Mathf.FloorToInt(distance * CellResolution);
        }

        private static Vector2 _lastPlayerPosition;

        private static bool IsCellHidden(Cell c)
        {
            Vector2 currentPlayerPosition = CombatManager.Player.transform.position;
            if (_lastPlayerPosition != currentPlayerPosition)
            {
                UpdateHiddenCells(currentPlayerPosition);
            }
            _lastPlayerPosition = currentPlayerPosition;
            return _hiddenCellPolys.Any(poly => AdvancedMaths.IsPointInPolygon(c.transform.position, poly));
        }

        private static readonly List<List<Vector2>> _hiddenCellPolys = new List<List<Vector2>>();

        private static void UpdateHiddenCells(Vector2 playerPosition)
        {
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