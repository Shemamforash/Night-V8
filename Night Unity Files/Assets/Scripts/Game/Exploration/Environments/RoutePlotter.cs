using System.Collections.Generic;
using Game.Exploration.Regions;
using SamsHelper.Libraries;

namespace Game.Exploration.Environment
{
    public static class RoutePlotter
    {
        private static Vertex LeastDistance(Dictionary<Region, Vertex> vertices)
        {
            Vertex least = null;
            foreach (Vertex v in vertices.Values)
                if (least == null || v.distance < least.distance)
                    least = v;
            return least;
        }

        private static Dictionary<Region, Vertex> GenerateVertices()
        {
            Dictionary<Region, Vertex> vertices = new Dictionary<Region, Vertex>();
            foreach (Region node in MapGenerator.DiscoveredRegions())
            {
                Vertex v = new Vertex {node = node};
                vertices[node] = v;
            }

            return vertices;
        }

        private static List<Region> FindPath(Vertex target)
        {
            List<Region> path = new List<Region>();
            Vertex current = target;
            path.Add(target.node);
            while (current.previous != null)
            {
                path.Insert(0, current.previous.node);
                current = current.previous;
            }

            return path;
        }

        private static Vertex Dijkstra(Region target, Dictionary<Region, Vertex> vertices)
        {
            while (vertices.Count > 0)
            {
                Vertex closest = LeastDistance(vertices);
                float currentRouteDistance = closest.distance;
                vertices.Remove(closest.node);
                if (closest.node == target) return closest;
                foreach (Region neighbor in closest.node.Neighbors())
                {
                    if (!vertices.ContainsKey(neighbor)) continue;
                    float distance = neighbor.Position.Distance(closest.node.Position);
                    float altRouteDistance = currentRouteDistance + distance;
                    if (altRouteDistance >= vertices[neighbor].distance) continue;
                    vertices[neighbor].distance = altRouteDistance;
                    vertices[neighbor].previous = closest;
                }
            }

            return null;
        }

        private static Vertex GetEndVertex(Region origin, Region target)
        {
            Dictionary<Region, Vertex> vertices = GenerateVertices();
            vertices[origin].distance = 0;
            Vertex targetVertex = Dijkstra(target, vertices);
            return targetVertex;
        }

        public static List<Region> RouteBetween(Region origin, Region target)
        {
            Vertex targetVertex = GetEndVertex(origin, target);
            return FindPath(targetVertex);
        }

        private class Vertex
        {
            public float distance = 1000000;
            public Region node;
            public Vertex previous;
        }
    }
}