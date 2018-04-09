using System.Collections.Generic;
using UnityEngine;

namespace SamsHelper.Libraries
{
    public class Node<T>
    {
        private readonly List<Node<T>> _neighbors = new List<Node<T>>();
        public readonly T Content;
        public readonly Vector3 Position;

        public Node(T content, Vector3 position)
        {
            Content = content;
            Position = position;
        }

        public void AddNeighbor(Node<T> neighbor)
        {
            if(_neighbors.Contains(neighbor)) return;
            _neighbors.Add(neighbor);
            neighbor._neighbors.Add(this);
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