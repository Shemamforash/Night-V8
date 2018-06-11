using System;
using System.Collections.Generic;
using SamsHelper;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Generation
{
    public class Ruins : RegionGenerator
    {
        private RuinNode[,] nodes;
        private const float CellWidth = 2f;

        private class RuinNode
        {
            public List<Vector2> RightEdge;
            public List<Vector2> BottomEdge;
            public RuinNode TopNeighbor;
            public RuinNode RightNeighbor;
            public RuinNode BottomNeighbor;
            public RuinNode LeftNeighbor;
            public readonly Vector2 Position;
            public bool Visited;
            public readonly int X, Y;

            public RuinNode(Vector3 position, int x, int y)
            {
                Position = position;
                X = x;
                Y = y;
            }

            public RuinNode GetNeighbor(Direction direction)
            {
                switch (direction)
                {
                    case Direction.Up:
                        return TopNeighbor;
                    case Direction.Down:
                        return BottomNeighbor;
                    case Direction.Left:
                        return LeftNeighbor;
                    case Direction.Right:
                        return RightNeighbor;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }
            }

            public void CreatePassage(Direction direction)
            {
                switch (direction)
                {
                    case Direction.Up:
                        TopNeighbor.BottomEdge = null;
                        break;
                    case Direction.Down:
                        BottomEdge = null;
                        break;
                    case Direction.Left:
                        LeftNeighbor.RightEdge = null;
                        break;
                    case Direction.Right:
                        RightEdge = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }

                if (RightEdge == null && BottomEdge == null) _nodesWithWalls.Remove(this);
            }

            public void SetNeighbors(RuinNode topNeighbor, RuinNode rightNeighbor, RuinNode bottomNeighbor, RuinNode leftNeighbor)
            {
                TopNeighbor = topNeighbor;
                LeftNeighbor = leftNeighbor;

                RightNeighbor = rightNeighbor;
                if (RightNeighbor == null) RightEdge = null;
                BottomNeighbor = bottomNeighbor;
                if (BottomNeighbor == null) BottomEdge = null;
                if (RightEdge == null && BottomEdge == null) _nodesWithWalls.Remove(this);
            }

            public void SetBottomEdge(List<Vector2> edge)
            {
                BottomEdge = edge;
            }

            public void SetRightEdge(List<Vector2> edge)
            {
                RightEdge = edge;
            }

            private void DrawEdges(List<Vector2> verts)
            {
                Vector2 mida = (verts[0] + verts[1]) / 2f;
                Vector2 midb = (verts[2] + verts[3]) / 2f;
                Debug.DrawLine(verts[0] + Position, mida + Position, Color.magenta, 10f);
                Debug.DrawLine(mida + Position, verts[1] + Position, Color.blue, 10f);
                Debug.DrawLine(verts[2] + Position, midb + Position, Color.yellow, 10f);
                Debug.DrawLine(midb + Position, verts[3] + Position, Color.green, 10f);
            }

            public void Clear()
            {
                BottomEdge = null;
                RightEdge = null;
                _nodesWithWalls.Remove(this);
            }
        }

        private static readonly List<RuinNode> _nodesWithWalls = new List<RuinNode>();

        private void CarvePassages(int x, int y)
        {
            RuinNode n = nodes[x, y];
            n.Visited = true;
            List<Direction> directions = new List<Direction> {Direction.Left, Direction.Down, Direction.Right, Direction.Up};
            Helper.Shuffle(ref directions);
            directions.ForEach(direction =>
            {
                RuinNode neighbor = n.GetNeighbor(direction);
                if (neighbor == null || neighbor.Visited) return;
                n.CreatePassage(direction);
                CarvePassages(neighbor.X, neighbor.Y);
            });
        }

        private void GenerateCell(int x, int y, int startPos, int width)
        {
            Vector2 cellPosition = new Vector2(x * CellWidth, y * CellWidth);
            RuinNode n = new RuinNode(cellPosition, x - startPos, y - startPos);
            _nodesWithWalls.Add(n);
            nodes[x - startPos, y - startPos] = n;

            List<Vector2> verts = new List<Vector2>();

            //bottom
            if (y > -width)
            {
                verts.Add(new Vector2(0.8f, -1.2f)); //0
                verts.Add(new Vector2(-0.8f, -1.2f)); //1
                verts.Add(new Vector2(-0.8f, -0.8f)); //2
                verts.Add(new Vector2(0.8f, -0.8f)); //3
                n.SetBottomEdge(verts);
            }

            if (x + 2 >= width) return;
            //right
            verts = new List<Vector2>();
            verts.Add(new Vector2(1.2f, -0.8f)); //0
            verts.Add(new Vector2(0.8f, -0.8f)); //1
            verts.Add(new Vector2(0.8f, 0.8f)); //2
            verts.Add(new Vector2(1.2f, 0.8f)); //3
            n.SetRightEdge(verts);
        }

        private const int WidthInCells = 30;

        protected override void Generate()
        {
            nodes = new RuinNode[WidthInCells, WidthInCells];
            int startPos = (int) -(WidthInCells / CellWidth);
            for (int x = startPos; x < startPos + WidthInCells; ++x)
            for (int y = startPos; y < startPos + WidthInCells; ++y)
                GenerateCell(x, y, startPos, WidthInCells);

            for (int x = 0; x < WidthInCells; ++x)
            {
                for (int y = 0; y < WidthInCells; ++y)
                {
                    RuinNode topNeighbor = y + 1 == WidthInCells ? null : nodes[x, y + 1];
                    RuinNode rightNeighbor = x + 1 == WidthInCells ? null : nodes[x + 1, y];
                    RuinNode bottomNeighbor = y - 1 == -1 ? null : nodes[x, y - 1];
                    RuinNode leftNeighbor = x - 1 == -1 ? null : nodes[x - 1, y];
                    nodes[x, y].SetNeighbors(topNeighbor, rightNeighbor, bottomNeighbor, leftNeighbor);
                }
            }

            CarvePassages(0, 0);
            CreateIslands(10);
            while (_nodesWithWalls.Count > 0) CombineWalls();
        }

//get random cell with a wall
//select any wall from cell
//progress to next wall:
//	if last wall was bottom wall:
//		if next wall is bottom wall:
//			if next wall.x > last wall.x add last wall 2,3 and next wall 2,3
//			else add last wall 3,0 and next wall 3,0
//		if next wall is right wall:
//			if next wall.x > last wall.x
//				if next wall.y > last wall.y add last wall 2,3 and next wall 1,2
//				else add last wall 2,3 and next wall 3,0
//			else
//				if next wall.y > last wall.y add last wall 3,2 and next wall 0,3
//				else add last wall 0,1 and next wall 3,0
//	if last wall was right wall:
//		if next wall is bottom wall:
//			if next wall.x > last wall.x
//				if next wall.y > last wall.y add last wall 3,2 and next wall 0,3
//				else add last wall 3,0 and next wall 2,3
//			else
//				if next wall.y > last wall.y add last wall 3,2 and next wall 0,1
//				else add last wall 3,0 and next wall 0,1
//		if next wall is right wall:
//			if next wall.x > last wall.x add last wall 2,3 and next wall 2,3
//			else add last wall 0,1 and next wall 0,1
//
//	if this is bottom wall:
//		if right wall is not null, it is the next wall, add 2,3
//		else if right cell, bottom wall is not null, it is the next wall, add 2,3
//		else if bottom cell, right wall is not null, it is the next wall, add 2,3
//		else if left cell, bottom cell, right wall is not null, it is the next wall, add 2,3,0,1
//		else if left cell, bottom wall is not null, it is the next wall, add 2,3,0,1
//		else if left cell, right wall is not null, it is the next wall, add 2,3,0,1
// 	if this is right wall:
//		else if right cell, bottom wall is not null, it is the next wall, add 3,0
//		else if bottom cell, right wall is not null, it is the next wall, add 3,0
//		else if the bottom wall is not null, it is the next wall, add 3,0
//		else if top cell, bottom wall is not null, it is the next wall, add 3,0,1,2
//		else if top cell, right wall is not null, it is the next wall, add 3,0,1,2
//		else if top cell, right cell, bottom wall is not null, it is the next wall, add 3,0,1,2
//		else finish
//		repeat from progress to the next wall


        private void AddFinalShapePoints(int a, int b, int c, int d)
        {
            Vector2 vertexA = _lastWall[a] + lastCell.Position;
            Vector2 vertexB = _lastWall[b] + lastCell.Position;
            Vector2 vertexC = _nextWall[c] + nextCell.Position;
            Vector2 vertexD = _nextWall[d] + nextCell.Position;

            if (_finalShape.Count >= 2)
            {
                Vector2 secondToLastVertex = _finalShape[_finalShape.Count - 2];
                Vector2 lastVertex = _finalShape[_finalShape.Count - 1];
                if (secondToLastVertex != vertexA)
                {
                    if (lastVertex != vertexA)
                    {
                        _finalShape.Add(vertexA);
                    }

                    _finalShape.Add(vertexB);
                }
            }
            else
            {
                _finalShape.Add(vertexA);
                _finalShape.Add(vertexB);
            }

            if (vertexB != vertexC) _finalShape.Add(vertexC);
            _finalShape.Add(vertexD);
        }

        private List<Vector2> _finalShape = new List<Vector2>();
        private List<Vector2> _lastWall, _nextWall;
        private RuinNode lastCell, nextCell;

        private void CombineWalls()
        {
            RuinNode start = _nodesWithWalls[0];
            List<Vector2> startWall = start.BottomEdge ?? start.RightEdge;
            nextCell = start;
            _nextWall = startWall;
            _lastWall = null;
            lastCell = null;
            _finalShape = new List<Vector2>();

            while (true)
            {
                _nodesWithWalls.Remove(nextCell);

                bool lastWallRight = false;
                bool lastWallAbove = false;

                if (_lastWall != null && lastCell != null)
                {
                    lastWallRight = (_lastWall[0].x + _lastWall[1].x) / 2f + lastCell.Position.x > (_nextWall[0].x + _nextWall[1].x) / 2f + nextCell.Position.x;
                    lastWallAbove = (_lastWall[0].y + _lastWall[3].y) / 2f + lastCell.Position.y > (_nextWall[0].y + _nextWall[3].y) / 2f + nextCell.Position.y;
                }

                _lastWall = _nextWall;
                lastCell = nextCell;

                if (_nextWall == nextCell.BottomEdge)
                {
                    if (lastWallRight)
                    {
                        CheckCellsLeft(() => CheckCellsRight());
                    }
                    else
                    {
                        CheckCellsRight(() => CheckCellsLeft());
                    }
                }
                else
                {
                    if (lastWallAbove)
                    {
                        CheckCellsBelow(() => CheckCellsAbove());
                    }
                    else
                    {
                        CheckCellsAbove(() => CheckCellsBelow());
                    }
                }

                if (nextCell == lastCell && _nextWall == _lastWall)
                {
                    _finalShape.Add(_lastWall[0] + lastCell.Position);
                    _finalShape.Add(_lastWall[1] + lastCell.Position);
                    _finalShape.Add(_lastWall[2] + nextCell.Position);
                    _finalShape.Add(_lastWall[3] + nextCell.Position);
                    break;
                }

                if (nextCell == start && _nextWall == startWall) break;
            }

            _finalShape.Reverse();

//            List<Vector2> shape = new List<Vector2>();
//            for (int i = 0; i < _finalShape.Count; ++i)
//            {
//                Vector2 current = _finalShape[i];
//                Vector2 next = Helper.NextElement(i, _finalShape);
//                shape.Add(current);
//                float distance = Vector2.Distance(current, next);
//                int numIntervals = Mathf.FloorToInt(distance / 5f);
//                List<float> intervals = new List<float>();
//                while (numIntervals > 0)
//                {
//                    intervals.Add(Random.Range(0f, 1f));
//                    --numIntervals;
//                }
//
//                intervals.Sort();
//                intervals.ForEach(interval =>
//                {
//                    Vector2 point = AdvancedMaths.PointAlongLine(current, next, interval);
//                    shape.Add(point);
//                });
//                shape.Add(next);
//            }
            //todo perturb me!

//            for (int i = 0; i < shape.Count; i++)
//            {
//                Vector2 s = shape[i];
//                s.x += 0.1f * Mathf.PerlinNoise(s.x, s.y);
//                s.y += 0.1f * Mathf.PerlinNoise(s.x, s.y);
//                shape[i] = s;
//            }

            Vector2 centre = Vector2.zero;
            _finalShape.ForEach(v => centre += v);
            centre /= _finalShape.Count;
            for (int i = 0; i < _finalShape.Count; ++i) _finalShape[i] -= centre;

            Barrier b = new Barrier(_finalShape, "Wall " + GetObjectNumber(), centre);
            barriers.Add(b);
        }

        private void DrawShape()
        {
            for (int i = 0; i < _finalShape.Count; ++i)
            {
                int next = Helper.NextIndex(i, _finalShape);
                float lerpVal = (float) i / _finalShape.Count;
                Debug.DrawLine(_finalShape[i], _finalShape[next], Color.Lerp(Color.green, Color.red, lerpVal), 10f);
            }

            Helper.PrintList(_finalShape);
        }

        private void CheckCellsRight(Action fallback = null)
        {
            if (nextCell.RightEdge != null)
            {
                _nextWall = nextCell.RightEdge;
                AddFinalShapePoints(2, 3, 1, 2); //a
//                Debug.Log("A");
            }
            else if (nextCell.RightNeighbor?.BottomEdge != null)
            {
                nextCell = nextCell.RightNeighbor;
                _nextWall = nextCell.BottomEdge;
                AddFinalShapePoints(2, 3, 2, 3);
//                Debug.Log("B");
            } //
            else if (nextCell.BottomNeighbor?.RightEdge != null)
            {
                nextCell = nextCell.BottomNeighbor;
                _nextWall = nextCell.RightEdge;
                AddFinalShapePoints(2, 3, 3, 0); //c
                //Debug.Log("C");
            }
            else
            {
                fallback?.Invoke();
            }
        }

        private void CheckCellsLeft(Action fallback = null)
        {
            if (nextCell.LeftNeighbor?.BottomNeighbor?.RightEdge != null)
            {
                nextCell = nextCell.LeftNeighbor.BottomNeighbor;
                _nextWall = nextCell.RightEdge;
                AddFinalShapePoints(0, 1, 3, 0); //d
                //Debug.Log("D");
            }
            else if (nextCell.LeftNeighbor?.BottomEdge != null)
            {
                nextCell = nextCell.LeftNeighbor;
                _nextWall = nextCell.BottomEdge;
                AddFinalShapePoints(0, 1, 0, 1); //e
                //Debug.Log("E");
            }
            else if (nextCell.LeftNeighbor?.RightEdge != null)
            {
                nextCell = nextCell.LeftNeighbor;
                _nextWall = nextCell.RightEdge;
                AddFinalShapePoints(0, 1, 1, 2); //f
                //Debug.Log("F");
            }
            else
            {
                fallback?.Invoke();
            }
        }

        private void CheckCellsAbove(Action fallback = null)
        {
            if (nextCell.TopNeighbor?.BottomEdge != null)
            {
                nextCell = nextCell.TopNeighbor;
                _nextWall = nextCell.BottomEdge;
                AddFinalShapePoints(1, 2, 0, 1); //h
                //Debug.Log("H");
            }
            else if (nextCell.TopNeighbor?.RightEdge != null)
            {
                nextCell = nextCell.TopNeighbor;
                _nextWall = nextCell.RightEdge;
                AddFinalShapePoints(1, 2, 1, 2); //i
                //Debug.Log("I");
            }
            else if (nextCell.TopNeighbor?.RightNeighbor?.BottomEdge != null)
            {
                nextCell = nextCell.TopNeighbor.RightNeighbor;
                _nextWall = nextCell.BottomEdge;
                AddFinalShapePoints(1, 2, 2, 3); //j
                //Debug.Log("J");
            }
            else
            {
                fallback?.Invoke();
            }
        }

        private void CheckCellsBelow(Action fallback = null)
        {
            if (nextCell.RightNeighbor?.BottomEdge != null)
            {
                nextCell = nextCell.RightNeighbor;
                _nextWall = nextCell.BottomEdge;
                AddFinalShapePoints(3, 0, 2, 3); //k
                //Debug.Log("K");
            }
            else if (nextCell.BottomNeighbor?.RightEdge != null)
            {
                nextCell = nextCell.BottomNeighbor;
                _nextWall = nextCell.RightEdge;
                AddFinalShapePoints(3, 0, 3, 0); //l
                //Debug.Log("L");
            }
            else if (nextCell.BottomEdge != null)
            {
                _nextWall = nextCell.BottomEdge;
                AddFinalShapePoints(3, 0, 0, 1); //m
                //Debug.Log("M");
            }
            else
            {
                fallback?.Invoke();
            }
        }

        private void CreateIslands(int i)
        {
            HashSet<RuinNode> nodesToKeep = new HashSet<RuinNode>();
            while (i > 0)
            {
                Vector2 randomPosition = AdvancedMaths.RandomVectorWithinRange(Vector2.zero, PathingGrid.CombatAreaWidth / 3f);
                randomPosition = FindAndRemoveValidPosition().Value;
                float range = Random.Range(5, 10);
                for (int x = 0; x < WidthInCells; ++x)
                {
                    for (int y = 0; y < WidthInCells; ++y)
                    {
                        float distance = Vector2.Distance(randomPosition, nodes[x, y].Position);
                        if (distance > range) continue;
                        float chanceToRemove = 1f - distance / range;
                        if (Random.Range(0f, 1f) > chanceToRemove) continue;
                        nodesToKeep.Add(nodes[x, y]);
                    }
                }

                --i;
            }

            for (int x = 0; x < WidthInCells; ++x)
            {
                for (int y = 0; y < WidthInCells; ++y)
                {
                    if (nodesToKeep.Contains(nodes[x, y])) continue;
                    nodes[x, y].Clear();
                }
            }
        }
    }
}