using System.Collections.Generic;
using UnityEngine;

namespace Game.Exploration.Environment
{
    public class RoutePlotter : MonoBehaviour
    {
        private static Vertex LeastDistance(Dictionary<MapNode, Vertex> vertices)
        {
            Vertex least = null;
            foreach (Vertex v in vertices.Values)
                if (least == null || v.distance < least.distance)
                    least = v;
            return least;
        }

        private static Dictionary<MapNode, Vertex> GenerateVertices()
        {
            Dictionary<MapNode, Vertex> vertices = new Dictionary<MapNode, Vertex>();
            foreach (MapNode node in MapGenerator.DiscoveredNodes())
            {
                Vertex v = new Vertex {node = node};
                vertices[node] = v;
            }

            return vertices;
        }

        private static List<MapNode> FindPath(Vertex target)
        {
            List<MapNode> path = new List<MapNode>();
            Vertex current = target;
            path.Add(target.node);
            while (current.previous != null)
            {
                path.Insert(0, current.previous.node);
                current = current.previous;
            }

            return path;
        }

        private static Vertex Dijkstra(MapNode target, Dictionary<MapNode, Vertex> vertices)
        {
            while (vertices.Count > 0)
            {
                Vertex closest = LeastDistance(vertices);
                float currentRouteDistance = closest.distance;
                vertices.Remove(closest.node);
                if (closest.node == target) return closest;
                foreach (MapNode neighbor in closest.node.Neighbors())
                {
                    if (!vertices.ContainsKey(neighbor)) continue;
                    float distance = closest.node.DistanceToPoint(neighbor);
                    float altRouteDistance = currentRouteDistance + distance;
                    if (altRouteDistance >= vertices[neighbor].distance) continue;
                    vertices[neighbor].distance = altRouteDistance;
                    vertices[neighbor].previous = closest;
                }
            }

            return null;
        }

        private static Vertex GetEndVertex(MapNode origin, MapNode target)
        {
            Dictionary<MapNode, Vertex> vertices = GenerateVertices();
            vertices[origin].distance = 0;
            Vertex targetVertex = Dijkstra(target, vertices);
            return targetVertex;
        }

        public static float DistanceBetween(MapNode origin, MapNode target)
        {
            Vertex targetVertex = GetEndVertex(origin, target);
            return targetVertex.distance;
        }

        public static List<MapNode> RouteBetween(MapNode origin, MapNode target)
        {
            Vertex targetVertex = GetEndVertex(origin, target);
            return FindPath(targetVertex);
        }

        private class Vertex
        {
            public float distance = 1000000;
            public MapNode node;
            public Vertex previous;
        }
    }
}