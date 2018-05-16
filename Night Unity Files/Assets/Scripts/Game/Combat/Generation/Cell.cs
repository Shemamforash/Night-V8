using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Cell : MonoBehaviour
    {
        private static GameObject _cellPrefab;
        public List<Cell> AllNeighbors = new List<Cell>();
        public bool Blocked;
        public int id;
        public Node Node;
        public Vector2 Position;
        public bool Reachable = true;
        public List<Cell> ReachableNeighbors = new List<Cell>();
        public int XIndex, YIndex;
        public float XPos, YPos;

        public static Cell Generate(int xIndex, int yIndex)
        {
            if (_cellPrefab == null) _cellPrefab = Resources.Load<GameObject>("Prefabs/Combat/Cell");
            GameObject sprite = Instantiate(_cellPrefab, PathingGrid.Instance().transform);
            Cell cell = sprite.GetComponent<Cell>();
            cell.SetXY(xIndex, yIndex);
            return cell;
        }

        private void AddNeighbor(Cell c)
        {
            if (c == null) return;
            AllNeighbors.Add(c);
            if (!c.Reachable) return;
            ReachableNeighbors.Add(c);
            Node.AddNeighbor(c.Node);
        }

        public void SetNeighbors()
        {
            if (XIndex - 1 >= 0) AddNeighbor(PathingGrid.Instance().Grid[XIndex - 1, YIndex]);
            if (YIndex - 1 >= 0) AddNeighbor(PathingGrid.Instance().Grid[XIndex, YIndex - 1]);
            if (XIndex + 1 < PathingGrid.GridWidth) AddNeighbor(PathingGrid.Instance().Grid[XIndex + 1, YIndex]);
            if (YIndex + 1 < PathingGrid.GridWidth) AddNeighbor(PathingGrid.Instance().Grid[XIndex, YIndex + 1]);
        }

        private void SetXY(int xIndex, int yIndex)
        {
            id = xIndex * 100 + yIndex;
            gameObject.layer = 9;
            gameObject.name = "Cell " + xIndex + " " + yIndex;
            XIndex = xIndex;
            YIndex = yIndex;
            XPos = (float) xIndex / PathingGrid.CellResolution - PathingGrid.CombatAreaWidth / 2f;
            YPos = (float) yIndex / PathingGrid.CellResolution - PathingGrid.CombatAreaWidth / 2f;
            Position = new Vector2(XPos, YPos);
            Node = new Node(Position);
//            BoxCollider2D col = gameObject.GetComponent<BoxCollider2D>();
            transform.position = new Vector3(XPos, YPos, 0);
            transform.localScale = new Vector3(PathingGrid.CellWidth, PathingGrid.CellWidth, 1);
//            col.size = new Vector2(1, 1);
//            col.isTrigger = true;
        }

        public float Distance(Cell other)
        {
            int xDiff = other.XIndex - XIndex;
            int yDiff = other.YIndex - YIndex;
            return Mathf.Sqrt(xDiff * xDiff + yDiff * yDiff);
        }
    }
}