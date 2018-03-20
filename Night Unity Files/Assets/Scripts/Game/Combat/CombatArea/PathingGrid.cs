using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using SamsHelper;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Combat
{
    public class PathingGrid : MonoBehaviour
    {
        private const int GameWorldWidth = 20;
        private const int CellResolution = 5;
        private const float CellWidth = 1f / CellResolution;
        private const int GridWidth = GameWorldWidth * CellResolution;


        private static Cell[,] _grid;
        private static readonly List<Node<Cell>> _gridNodes = new List<Node<Cell>>();
        private static PathingGrid _instance;

        private HashSet<Cell> _reachableCells = new HashSet<Cell>();
        private static HashSet<Cell> _unreachableCells = new HashSet<Cell>();

        private static List<AreaGenerator.Shape> _shapes;
        private static ContactFilter2D cf;

        private static readonly List<PolygonCollider2D> _shapeColliders = new List<PolygonCollider2D>();
        private static readonly Collider2D[] colliders = new Collider2D[5000];

        public void Awake()
        {
            _instance = this;
        }

        public static PathingGrid Instance()
        {
            return _instance;
        }

        public void SetShapes(List<AreaGenerator.Shape> barriers)
        {
            _shapes = barriers;
            _shapeColliders.Clear();
            cf.useTriggers = true;
            cf.SetLayerMask(1 << 9);
            GenerateBaseGrid();
            foreach (AreaGenerator.Shape shape in _shapes)
            {
                GameObject tempCOl = new GameObject();
                tempCOl.name = "Collider shape";
                tempCOl.transform.localScale = Vector3.one;
                PolygonCollider2D collider2d = tempCOl.AddComponent<PolygonCollider2D>();
                _shapeColliders.Add(collider2d);
                collider2d.isTrigger = true;
                int areas = shape.Collider.OverlapCollider(cf, colliders);
                for (int i = 0; i < areas; ++i)
                {
                    Cell c = colliders[i].GetComponent<Cell>();
                    shape.OccupiedCells.Add(c);
                    _reachableCells.Remove(c);
                    _unreachableCells.Add(c);
                    c.Barrier = shape;
                }
            }

            _gridNodes.ForEach(n => n.Content.SetNeighbors());
        }

        public static Cell PositionToCell(Vector2 position)
        {
            Collider2D point = Physics2D.OverlapPoint(position, 1 << 9);
            return point != null ? point.GetComponent<Cell>() : null;
        }

        public static List<Cell> RouteToCell(Cell from, Cell to)
        {
            Stopwatch watch = Stopwatch.StartNew();
            List<Cell> path = Pathfinding.AStar(from.Node, to.Node, _gridNodes);

            int currentIndex = 0;
            while (currentIndex < path.Count)
            {
                Cell current = path[currentIndex];
                List<Cell> cellsToRemove = new List<Cell>();
                bool noneHit = false;
                for (int i = path.Count - 1; i > currentIndex; --i)
                {
                    Cell next = path[i];
                    if (noneHit)
                    {
                        cellsToRemove.Add(next);
                    }
                    else
                    {
                        RaycastHit2D[] hits = Physics2D.RaycastAll(current.Position, next.Position - current.Position, Vector2.Distance(current.Position, next.Position), 1 << 9);
                        Debug.DrawRay(current.Position, next.Position - current.Position, Color.cyan, 10f);
                        if (hits.Any(hit => _unreachableCells.Contains(hit.collider.GetComponent<Cell>())))
                        {
                            continue;
                        }

                        noneHit = true;
                    }
                }

                cellsToRemove.ForEach(cell => path.Remove(cell));
                ++currentIndex;
            }

            watch.Stop();
//            Debug.Log(watch.Elapsed.ToString("mm\\:ss\\.ff"));
            return path;
        }

        private static List<Cell> CellsInRange(Cell current, int distance, bool includeUnreachable = false)
        {
            HashSet<Cell> visited = new HashSet<Cell>();
            HashSet<Cell> currentLayer = new HashSet<Cell>();
            HashSet<Cell> nextLayer = new HashSet<Cell>();
            if (current == null) Debug.Log("initial node was null");
            currentLayer.Add(current);
            while (distance >= 0)
            {
                nextLayer.Clear();
                foreach (Cell c in currentLayer)
                {
                    List<Cell> neighbors = includeUnreachable ? c.AllNeighbors : c.ReachableNeighbors;
                    foreach (Cell neighbor in neighbors)
                    {
                        if (visited.Contains(neighbor) || currentLayer.Contains(neighbor) || neighbor == null) continue;
                        nextLayer.Add(neighbor);
                    }

                    visited.Add(c);
                }

                currentLayer.Clear();
                foreach (Cell cell in nextLayer)
                {
                    currentLayer.Add(cell);
                }

                --distance;
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
            foreach (Cell cell in _unreachableCells)
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
            List<Cell> cellsInRange = CellsInRange(playerCell, Random.Range(minRange, maxRange), true);
            cellsInRange = RemoveHiddenCells(cellsInRange, maxRange);
            float minDistance = float.MaxValue;
            Cell nearestValidCell = null;
            _cellsInRange = cellsInRange;
            cellsInRange.ForEach(c =>
            {
                float distance = Vector2.Distance(currentCell.Position, c.Position);
                if (distance >= minDistance) return;
                minDistance = distance;
                nearestValidCell = c;
            });
            return nearestValidCell;
        }

        private static List<Cell> RemoveHiddenCells(List<Cell> cells, int range)
        {
            Vector2 playerPosition = CombatManager.Player.CharacterController.transform.position;
            HashSet<Cell> hiddenCells = new HashSet<Cell>();

            List<AreaGenerator.Shape> shapes = new List<AreaGenerator.Shape>();
            cells.ForEach(c =>
            {
                if (c.Barrier != null) shapes.Add(c.Barrier);
            });

            for (int i = 0; i < shapes.Count; i++)
            {
                AreaGenerator.Shape shape = shapes[i];
                if (shape.OccupiedCells.All(a => hiddenCells.Contains(a))) continue;
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

                Vector2 leftPointDistant = leftPoint + (leftPoint - playerPosition).normalized * range * 1.5f;
                Vector2 rightPointDistant = rightPoint + (rightPoint - playerPosition).normalized * range * 1.5f;

                PolygonCollider2D collider2d = _shapeColliders[i];
                collider2d.enabled = true;
                collider2d.points = new[] {leftPoint, rightPoint, rightPointDistant, leftPointDistant};
                int collisions = collider2d.OverlapCollider(cf, colliders);
                for (int j = 0; j < collisions; ++j)
                {
                    Collider2D d = colliders[j];
                    if (d == null) break;
                    Cell c = d.GetComponent<Cell>();
                    if (c == null) continue;
                    if (hiddenCells.Contains(c)) continue;
                    hiddenCells.Add(c);
                }

                collider2d.enabled = false;
            }

            cells.RemoveAll(c => _unreachableCells.Contains(c) || hiddenCells.Contains(c));

            return cells;
        }

        private void GenerateBaseGrid()
        {
            _gridNodes.Clear();
            _grid = new Cell[GridWidth, GridWidth];
            for (int x = 0; x < GridWidth; ++x)
            {
                for (int y = 0; y < GridWidth; ++y)
                {
                    _grid[x, y] = Cell.Generate(x, y);
                    _grid[x, y].transform.SetParent(transform);
                    _reachableCells.Add(_grid[x, y]);
                    _gridNodes.Add(_grid[x, y].Node);
                }
            }
        }

        public class Cell : MonoBehaviour
        {
            public float XPos, YPos;
            public int XIndex, YIndex;
            public Node<Cell> Node;
            public List<Cell> AllNeighbors = new List<Cell>();
            public List<Cell> ReachableNeighbors = new List<Cell>();
            public Vector2 Position;
            public AreaGenerator.Shape Barrier;

            public static Cell Generate(int xIndex, int yIndex)
            {
                GameObject sprite = new GameObject();
                sprite.AddComponent<Cell>().SetXY(xIndex, yIndex);
                return sprite.GetComponent<Cell>();
            }

            private void AddNeighbor(Cell c)
            {
                AllNeighbors.Add(c);
                if (_unreachableCells.Contains(c)) return;
                ReachableNeighbors.Add(c);
                Node.AddNeighbor(c.Node);
            }

            public void SetNeighbors()
            {
                if (XIndex - 1 >= 0) AddNeighbor(_grid[XIndex - 1, YIndex]);
                if (YIndex - 1 >= 0) AddNeighbor(_grid[XIndex, YIndex - 1]);
                if (XIndex + 1 < GridWidth) AddNeighbor(_grid[XIndex + 1, YIndex]);
                if (YIndex + 1 < GridWidth) AddNeighbor(_grid[XIndex, YIndex + 1]);
            }

            private void SetXY(int xIndex, int yIndex)
            {
                gameObject.layer = 9;
                gameObject.name = "Cell " + xIndex + " " + yIndex;
                XIndex = xIndex;
                YIndex = yIndex;
                XPos = (float) xIndex / CellResolution - GameWorldWidth / 2f;
                YPos = (float) yIndex / CellResolution - GameWorldWidth / 2f;
                Position = new Vector2(XPos, YPos);
                Node = new Node<Cell>(this, Position);
                BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
                transform.position = new Vector3(XPos, YPos, 0);
                transform.localScale = new Vector3(CellWidth, CellWidth, 1);
                col.size = new Vector2(1, 1);
                col.isTrigger = true;
            }
        }
    }
}