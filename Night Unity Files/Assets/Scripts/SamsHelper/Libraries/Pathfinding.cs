using System.Collections.Generic;

namespace SamsHelper
{
    public static class Pathfinding
    {
        public static List<T> AStar<T>(Node<T> from, Node<T> to, List<Node<T>> graph)
        {
            HashSet<Node<T>> visited = new HashSet<Node<T>>();
            HashSet<Node<T>> unvisited = new HashSet<Node<T>>();
            unvisited.Add(from);
            Dictionary<Node<T>, float> gScore = new Dictionary<Node<T>, float>();
            Dictionary<Node<T>, float> fScore = new Dictionary<Node<T>, float>();
            graph.ForEach(node =>
            {
                gScore[node] = float.MaxValue;
                fScore[node] = float.MaxValue;
            });
            gScore[from] = 0;
            fScore[from] = from.Distance(to);

            while (unvisited.Count != 0)
            {
                float minScore = float.MaxValue;
                Node<T> minNode = null;
                foreach (Node<T> n in unvisited)
                {
                    if (fScore[n] >= minScore) continue;
                    minScore = fScore[n];
                    minNode = n;
                }

                Node<T> current = minNode;
                if (current == to)
                {
                    return GetPath(current);
                }

                unvisited.Remove(current);
                visited.Add(current);

                foreach (Node<T> neighbor in current.Neighbors())
                {
                    if (visited.Contains(neighbor)) continue;
                    if (!unvisited.Contains(neighbor)) unvisited.Add(neighbor);

                    float tentativeGScore = gScore[current] + current.Distance(neighbor);
                    if (tentativeGScore >= gScore[neighbor]) continue;

                    neighbor.From = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + neighbor.Distance(to);
                }
            }

            return null;
        }

        private static List<T> GetPath<T>(Node<T> current)
        {
            List<T> path = new List<T>();
            while (current != null)
            {
                path.Add(current.Content);
                current = current.From;
            }

            path.Reverse();
            return path;
        }
    }
}