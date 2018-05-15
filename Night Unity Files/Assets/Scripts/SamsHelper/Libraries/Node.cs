using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SamsHelper.Libraries
{
    public class Node<T>
    {
        private readonly List<Tuple<float, Node<T>>> _neighbors = new List<Tuple<float, Node<T>>>();
        private readonly List<Node<T>> _rawNeighbors = new List<Node<T>>();
        public readonly T Content;
        public readonly Vector3 Position;
        private bool _isLeaf;

        public Node(T content, Vector3 position) : this(position)
        {
            Content = content;
        }

        public Node(Vector3 position)
        {
            Position = position;
        }

        public void AddNeighbor(Node<T> neighbor, bool reciprocate = true)
        {
            if (_neighbors.Any(n => n.Item2 == neighbor)) return;
            float angle = AdvancedMaths.AngleFromUp(Position, neighbor.Position);
            _neighbors.Add(new Tuple<float, Node<T>>(angle, neighbor));
            neighbor.AddNeighbor(this, false);
            _neighbors.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            _rawNeighbors.Clear();
            _neighbors.ForEach(n => _rawNeighbors.Add(n.Item2));
            _isLeaf = _neighbors.Count == 1;
        }

        public float Distance(Node<T> other)
        {
            return Vector3.Distance(Position, other.Position);
        }

        public List<Node<T>> Neighbors()
        {
            return _rawNeighbors;
        }

        public Node<T> NavigateLeft(Node<T> from)
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

        public Node<T> NavigateRight(Node<T> from)
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
    }
}