using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SamsHelper.Libraries
{
    public class Graph
    {
        private readonly List<Edge> _edges = new List<Edge>();
        private readonly List<Node> _nodes = new List<Node>();

        public bool ContainsEdge(Edge edge)
        {
            return _edges.Any(e => e.Equals(edge));
        }

        public void AddEdge(Edge edge)
        {
            _edges.Add(edge);
        }

        public void AddNode(Node node)
        {
            _nodes.Add(node);
        }

        public List<Edge> Edges()
        {
            return _edges;
        }

        public List<Node> Nodes()
        {
            return _nodes;
        }

        public List<Edge> ComputeMinimumSpanningTree(Func<Node, Node, float> nodeWeightComparator = null)
        {
            return Pathfinding.MinimumSpanningTree(this, nodeWeightComparator);
        }

        public List<Edge> GenerateEdges()
        {
            _edges.Clear();
            _nodes.ForEach(n => n.MarkEdgesDirty());
            _nodes[0].GenerateEdges(this);
            return _edges;
        }

        public void DrawEdges()
        {
            _edges.ForEach(e =>
            {
                Color c = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                Debug.DrawLine(e.A.Position, e.B.Position, c, 500f);
            });
        }
    }
}