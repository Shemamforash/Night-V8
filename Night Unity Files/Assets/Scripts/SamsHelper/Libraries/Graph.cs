using System;
using System.Collections.Generic;
using System.Linq;

namespace SamsHelper.Libraries
{
    public class Graph<T>
    {
        private readonly List<Edge<T>> _edges = new List<Edge<T>>();
        private readonly List<Node<T>> _nodes = new List<Node<T>>();

        public bool ContainsEdge(Edge<T> edge)
        {
            return _edges.Any(e => e.Equals(edge));
        }

        public void AddEdge(Edge<T> edge)
        {
            _edges.Add(edge);
        }

        public void AddNode(Node<T> node)
        {
            _nodes.Add(node);
        }

        public List<Edge<T>> Edges()
        {
            return _edges;
        }

        public List<Node<T>> Nodes()
        {
            return _nodes;
        }

        public void ComputeMinimumSpanningTree(Func<Node<T>, Node<T>, float> nodeWeightComparator = null)
        {
            _edges.Clear();
            Pathfinding.MinimumSpanningTree(this, nodeWeightComparator);
        }
    }
}