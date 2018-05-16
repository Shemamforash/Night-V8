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
        public readonly Vector3 Position;
        private bool _isLeaf;
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
            _isLeaf = _neighbors.Count == 1;
        }

        public float Distance(Node other)
        {
            return Vector3.Distance(Position, other.Position);
        }

        public List<Node> Neighbors()
        {
            return _rawNeighbors;
        }

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

        public Node NavigateAnticlockwise(Node from)
        {
            for (int i = 0; i < _rawNeighbors.Count; ++i)
            {
                if (_rawNeighbors[i] != from) continue;
                int prev = i - 1;
                if (prev == -1) prev = _rawNeighbors.Count - 1;
                return _rawNeighbors[prev];
            }

            return null;
        }

        public bool IsLeaf()
        {
            return _isLeaf;
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
    }
}