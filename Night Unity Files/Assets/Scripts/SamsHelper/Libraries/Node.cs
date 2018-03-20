using System.Collections.Generic;
using UnityEngine;

namespace SamsHelper
{
    public class Node<T>
    {
        public readonly T Content;
        public readonly Vector3 Position;
        public float Length;
        private readonly List<Node<T>> _neighbors = new List<Node<T>>();

        public Node(T content, Vector3 position)
        {
            Content = content;
            Position = position;
        }

        public void AddNeighbor(Node<T> neighbor)
        {
            _neighbors.Add(neighbor);
        }

        public float Distance(Node<T> other)
        {
            return Vector3.Distance(Position, other.Position);
        }

        public List<Node<T>> Neighbors()
        {
            return _neighbors;
        }
    }
}