using System.Collections.Generic;
using System.Linq;
using SamsHelper;
using UnityEngine;

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

        private List<Cell> _reachableCells = new List<Cell>();
        private static List<Cell> _unreachableCells = new List<Cell>();

        private readonly HashSet<Cell> _hiddenCells = new HashSet<Cell>();
        private List<AreaGenerator.Shape> _shapes;
        private ContactFilter2D cf;

        private readonly List<PolygonCollider2D> _shapeColliders = new List<PolygonCollider2D>();
        private readonly Collider2D[] colliders = new Collider2D[5000];

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
                }
            }
        }

        public static Cell PositionToCell(Vector2 position)
        {
            Collider2D point = Physics2D.OverlapPoint(position, 1 << 9);
            return point != null ? point.GetComponent<Cell>() : null;
        }

        
        public static Cell PositionToCell2(Vector2 position)
        {
            Debug.Log(position);
            float x = position.x;
            float y = position.y;
            x *= CellResolution;
            y *= CellResolution;
            x += GridWidth / 2f;
            y += GridWidth / 2f;
            int xIndex = Mathf.FloorToInt(x);
            int yIndex = Mathf.FloorToInt(y);
            return _grid[xIndex, yIndex];
        }

        public static List<Cell> RouteToCell(Cell from, Cell to)
        {
            return Pathfinding.AStar(from.Node, to.Node, _gridNodes);
        }

        public static Cell GetCellNearMe(Cell current, int distance)
        {
            HashSet<Cell> visited = new HashSet<Cell>();
            List<Cell> currentLayer = new List<Cell>();
            List<Cell> nextLayer = new List<Cell>();
            currentLayer.Add(current);
            while (distance >= 0)
            {
                nextLayer.Clear();

                foreach (Cell c in currentLayer)
                {
                    foreach (Cell neighbor in c.Neighbors)
                    {
                        if (_unreachableCells.Contains(neighbor)) continue;
                        if (visited.Contains(neighbor) || currentLayer.Contains(neighbor)) continue;
                        nextLayer.Add(neighbor);
                    }

                    visited.Add(c);
                }

                currentLayer = nextLayer.ToList();
                --distance;
            }

            return Helper.RandomInList(visited.ToList());
        }

        public List<Cell> GetHiddenCells()
        {
            _hiddenCells.Clear();
            Vector2 playerPosition = CombatManager.Player.CharacterController.transform.position;
            for (int i = 0; i < _shapes.Count; i++)
            {
                AreaGenerator.Shape shape = _shapes[i];
                if (shape.OccupiedCells.All(a => _hiddenCells.Contains(a))) continue;
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

                Vector2 leftPointDistant = leftPoint + (leftPoint - playerPosition).normalized * 20f;
                Vector2 rightPointDistant = rightPoint + (rightPoint - playerPosition).normalized * 20f;

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
                    if (_hiddenCells.Contains(c)) continue;
                    _hiddenCells.Add(c);
                }

                collider2d.enabled = false;
            }

            _unreachableCells.ForEach(cell => _hiddenCells.Add(cell));

            return _hiddenCells.ToList();
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
                    _reachableCells.Add(_grid[x, y]);
                    _gridNodes.Add(_grid[x, y].Node);
                }
            }

            _gridNodes.ForEach(n => n.Content.SetNeighbors());
        }

//        private void OnDrawGizmos()
//        {
//            Gizmos.color = new Color(1, 0, 0, 0.4f);
//            foreach (Cell cell in _reachableCells)
//            {
//                Gizmos.DrawCube(new Vector3(cell.XPos, cell.YPos), new Vector3(CellWidth, CellWidth, 1));
//            }
//        }

        public class Cell : MonoBehaviour
        {
            public float XPos, YPos;
            public int XIndex, YIndex;
            public Node<Cell> Node;
            public List<Cell> Neighbors = new List<Cell>();
            public Vector2 Position;

            public static Cell Generate(int xIndex, int yIndex)
            {
                GameObject sprite = new GameObject();
                sprite.AddComponent<Cell>().SetXY(xIndex, yIndex);
                return sprite.GetComponent<Cell>();
            }

            private void AddNeighbor(Cell c)
            {
                Neighbors.Add(c);
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
                XIndex = xIndex;
                YIndex = yIndex;
                XPos = (float) xIndex / CellResolution - GameWorldWidth / 2f;
                YPos = (float) yIndex / CellResolution - GameWorldWidth / 2f;
                Position = new Vector2(XPos, YPos);
                Node = new Node<Cell>(this, new Vector2(XPos, YPos));
                BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
                transform.position = new Vector3(XPos, YPos, 0);
                transform.localScale = new Vector3(CellWidth, CellWidth, 1);
                col.size = new Vector2(0.8f, 0.8f);
                col.isTrigger = true;
            }
        }
    }
}