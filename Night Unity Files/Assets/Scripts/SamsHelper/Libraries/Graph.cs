using System;
using System.Collections.Generic;
using System.Linq;
using Game.Exploration.Regions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SamsHelper.Libraries
{
    public class Graph
    {
        private readonly List<Edge> _edges = new List<Edge>();
        private readonly List<Node> _nodes = new List<Node>();
        private Node _rootNode;

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

        public void SetRootNode(Node node)
        {
            _rootNode = node;
        }

        private float _maxNodeDepth = -1;
        
        public void CalculateNodeDepths()
        {
            _maxNodeDepth = -1;
            if (_rootNode == null) throw new NoRootNodeException();
            _rootNode.Depth = 0;
            HashSet<Node> seen = new HashSet<Node>();
            Queue<Node> unseen = new Queue<Node>();
            unseen.Enqueue(_rootNode);
            while (unseen.Count != 0)
            {
                Node node = unseen.Dequeue();
                seen.Add(node);
                node.Neighbors().ForEach(n =>
                {
                    if (seen.Contains(n))
                    {
                        if (n.Depth + 1 < node.Depth)
                        {
                            node.Depth = n.Depth + 1;
                        }

                        return;
                    }
                    unseen.Enqueue(n);
                    n.Depth = node.Depth + 1;
                });
            }
            _nodes.ForEach(n =>
            {
                if (n.Depth > _maxNodeDepth) _maxNodeDepth = n.Depth;
            });
        }

        public float MaxDepth()
        {
            return _maxNodeDepth;
        }
    }

    public class NoRootNodeException : Exception
    {
        public override string Message => "No root node set";
    }
}