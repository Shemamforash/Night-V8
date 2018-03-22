using System.Collections.Generic;
using SamsHelper;
using UnityEngine;

namespace Game.Combat
{
    public class Cell : MonoBehaviour
    {
        public float XPos, YPos;
        public int XIndex, YIndex;
        public Node<Cell> Node;
        public List<Cell> AllNeighbors = new List<Cell>();
        public List<Cell> ReachableNeighbors = new List<Cell>();
        public Vector2 Position;
        public AreaGenerator.Shape Barrier;
        private static GameObject _cellPrefab;
        public int id;

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
            AllNeighbors.Add(c);
            if (PathingGrid.UnreachableCells.Contains(c)) return;
            ReachableNeighbors.Add(c);
            Node.AddNeighbor(c.Node);
        }

        public void SetNeighbors()
        {
            if (XIndex - 1 >= 0) AddNeighbor(PathingGrid.Grid[XIndex - 1, YIndex]);
            if (YIndex - 1 >= 0) AddNeighbor(PathingGrid.Grid[XIndex, YIndex - 1]);
            if (XIndex + 1 < PathingGrid.GridWidth) AddNeighbor(PathingGrid.Grid[XIndex + 1, YIndex]);
            if (YIndex + 1 < PathingGrid.GridWidth) AddNeighbor(PathingGrid.Grid[XIndex, YIndex + 1]);
        }

        private void SetXY(int xIndex, int yIndex)
        {
            id = xIndex * 100 + yIndex;
            gameObject.layer = 9;
            gameObject.name = "Cell " + xIndex + " " + yIndex;
            XIndex = xIndex;
            YIndex = yIndex;
            XPos = (float) xIndex / PathingGrid.CellResolution - PathingGrid.GameWorldWidth / 2f;
            YPos = (float) yIndex / PathingGrid.CellResolution - PathingGrid.GameWorldWidth / 2f;
            Position = new Vector2(XPos, YPos);
            Node = new Node<Cell>(this, Position);
//            BoxCollider2D col = gameObject.GetComponent<BoxCollider2D>();
            transform.position = new Vector3(XPos, YPos, 0);
            transform.localScale = new Vector3(PathingGrid.CellWidth, PathingGrid.CellWidth, 1);
//            col.size = new Vector2(1, 1);
//            col.isTrigger = true;
        }
    }
}