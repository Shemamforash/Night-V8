using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation
{
    public class Cell : Polygon
    {
        public int x;
        public int y;
        public readonly int id;
        private static int CellNumber;

        public bool Reachable = true;
        public bool Blocked;
        public bool IsEdgeCell;
        public bool OutOfRange;

        public static Cell Generate(int xIndex, int yIndex)
        {
            List<Vector2> verts = new List<Vector2>();
            float xPos = (float) xIndex / WorldGrid.CellResolution - WorldGrid.CombatAreaWidth / 2f;
            float yPos = (float) yIndex / WorldGrid.CellResolution - WorldGrid.CombatAreaWidth / 2f;
            Vector2 position = new Vector2(xPos, yPos);
            verts.Add(new Vector2(-WorldGrid.CellWidth / 2f, -WorldGrid.CellWidth / 2f));
            verts.Add(new Vector2(-WorldGrid.CellWidth / 2f, WorldGrid.CellWidth / 2f));
            verts.Add(new Vector2(WorldGrid.CellWidth / 2f, -WorldGrid.CellWidth / 2f));
            verts.Add(new Vector2(WorldGrid.CellWidth / 2f, WorldGrid.CellWidth / 2f));
            Cell cell = new Cell(verts, position, xIndex, yIndex);
            return cell;
        }

        public float SqrDistance(Cell other)
        {
            int xDiff = other.x - x;
            int yDiff = other.y - y;
            return xDiff * xDiff + yDiff * yDiff;
        }

        private Cell(List<Vector2> vertices, Vector2 position, int x, int y) : base(vertices, position)
        {
            this.x = x;
            this.y = y;
            id = CellNumber;
            ++CellNumber;
        }
    }
}