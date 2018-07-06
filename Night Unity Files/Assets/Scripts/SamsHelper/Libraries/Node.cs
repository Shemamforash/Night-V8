using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SamsHelper.Libraries
{
    public class Node
    {
        private readonly List<Tuple<float, Node>> _neighbors = new List<Tuple<float, Node>>();
        private readonly List<Node> _rawNeighbors = new List<Node>();
        private readonly List<Edge> _edges = new List<Edge>();
        public Vector2 Position;
        private bool _generatedEdges;

        public Node(Vector3 position)
        {
            Position = position;
        }

        public void AddNeighbor(Node neighbor, bool reciprocate = true)
        {
            if (_neighbors.Any(n => n.Item2 == neighbor)) return;
            float angle = 360 - AdvancedMaths.AngleFromUp(Position, neighbor.Position);
            _neighbors.Add(new Tuple<float, Node>(angle, neighbor));
            neighbor.AddNeighbor(this, false);
            _neighbors.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            _rawNeighbors.Clear();
            _neighbors.ForEach(n => _rawNeighbors.Add(n.Item2));
        }

        public void AddNeighborSimple(Node neighbor)
        {
            _rawNeighbors.Add(neighbor);
        }

        public float Distance(Node other) => Vector3.Distance(Position, other.Position);
        
        public List<Node> Neighbors() => _rawNeighbors;

        public Node NavigateClockwise(Node from)
        {
            for (int i = 0; i < _rawNeighbors.Count; ++i)
            {
                if (_rawNeighbors[i] != from) continue;
                int next = i + 1;
                if (next == _rawNeighbors.Count) next = 0;
                return _rawNeighbors[next];
            }

            return null;
        }

        public Edge GetEdge(Node n)
        {
            return _edges.Find(e => e.ConnectsTo(n));
        }

        public void DrawNeighbors()
        {
            Color from = Color.green;
            Color to = Color.red;
            foreach (Tuple<float, Node> t in _neighbors)
            {
                float lerpVal = t.Item1 / 360f;
                Debug.DrawLine(Position, (t.Item2.Position + Position) / 2f, Color.Lerp(from, to, lerpVal), 5f);
            }
        }

        public void GenerateEdges(Graph g)
        {
            _generatedEdges = true;
            _rawNeighbors.ForEach(n =>
            {
                if (n._generatedEdges) return;
                Edge e = new Edge(this, n);
                n._edges.Add(e);
                _edges.Add(e);
                g.AddEdge(new Edge(this, n));
            });
            _rawNeighbors.ForEach(n =>
            {
                if (n._generatedEdges) return;
                n.GenerateEdges(g);
            });
        }

        public void MarkEdgesDirty()
        {
            _generatedEdges = false;
            _edges.Clear();
        }

        public float ManhattanDistance(Node other)
        {
            return Mathf.Abs(Position.x - other.Position.x) + Mathf.Abs(Position.y - other.Position.y);
        }
    }
}