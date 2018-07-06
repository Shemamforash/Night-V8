using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Cell : Polygon
    {
        public readonly Node Node;
        public readonly int XIndex, YIndex;
        public readonly int id;
        private static int CellNumber;

        public bool Reachable = true;
        public bool Blocked;

        public static Cell Generate(int xIndex, int yIndex)
        {
            List<Vector2> verts = new List<Vector2>();
            float xPos = (float) xIndex / PathingGrid.CellResolution - PathingGrid.CombatAreaWidth / 2f;
            float yPos = (float) yIndex / PathingGrid.CellResolution - PathingGrid.CombatAreaWidth / 2f;
            Vector2 position = new Vector2(xPos, yPos);
            verts.Add(new Vector2(-PathingGrid.CellWidth / 2f, -PathingGrid.CellWidth / 2f));
            verts.Add(new Vector2(-PathingGrid.CellWidth / 2f, PathingGrid.CellWidth / 2f));
            verts.Add(new Vector2(PathingGrid.CellWidth / 2f, -PathingGrid.CellWidth / 2f));
            verts.Add(new Vector2(PathingGrid.CellWidth / 2f, PathingGrid.CellWidth / 2f));
            Cell cell = new Cell(verts, position, xIndex, yIndex);
            return cell;
        }

        private void AddNeighbor(Cell c)
        {
            if (c == null) return;
            if (!c.Reachable) return;
            Node.AddNeighborSimple(c.Node);
        }

        public void SetNeighbors()
        {
            if (XIndex - 1 >= 0) AddNeighbor(PathingGrid.Grid[XIndex - 1][YIndex]);
            if (YIndex - 1 >= 0) AddNeighbor(PathingGrid.Grid[XIndex][YIndex - 1]);
            if (XIndex + 1 < PathingGrid.GridWidth) AddNeighbor(PathingGrid.Grid[XIndex + 1][YIndex]);
            if (YIndex + 1 < PathingGrid.GridWidth) AddNeighbor(PathingGrid.Grid[XIndex][YIndex + 1]);
        }

        public float SqrDistance(Cell other)
        {
            int xDiff = other.XIndex - XIndex;
            int yDiff = other.YIndex - YIndex;
            return xDiff * xDiff + yDiff * yDiff;
        }

        private Cell(List<Vector2> vertices, Vector2 position, int xIndex, int yIndex) : base(vertices, position)
        {
            XIndex = xIndex;
            YIndex = yIndex;
            Node = new Node(Position);
            id = CellNumber;
            ++CellNumber;
        }
    }
}