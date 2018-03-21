using System.Collections.Generic;
using System.Threading;

namespace SamsHelper
{
    public static class Pathfinding
    {
        public static List<T> AStar<T>(Node<T> from, Node<T> to, List<Node<T>> graph)
        {
            List<T> path = null;
            
            HashSet<Node<T>> visited = new HashSet<Node<T>>();
            HashSet<Node<T>> unvisited = new HashSet<Node<T>>();
            unvisited.Add(from);
            Dictionary<Node<T>, Node<T>> _fromDict = new Dictionary<Node<T>, Node<T>>();
            Dictionary<Node<T>, float> gScore = new Dictionary<Node<T>, float>();
            Dictionary<Node<T>, float> fScore = new Dictionary<Node<T>, float>();
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
                    path = GetPath(current, _fromDict);
                    break;
                }

                unvisited.Remove(current);
                visited.Add(current);

                foreach (Node<T> neighbor in current.Neighbors())
                {
                    if (visited.Contains(neighbor)) continue;
                    if (!unvisited.Contains(neighbor)) unvisited.Add(neighbor);

                    float currentGScore = gScore.ContainsKey(current) ? gScore[current] : float.MaxValue;
                    float neighborGScore = gScore.ContainsKey(neighbor) ? gScore[neighbor] : float.MaxValue;
                    float tentativeGScore = currentGScore + current.Distance(neighbor);
                    if (tentativeGScore >= neighborGScore) continue;

                    _fromDict[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + neighbor.Distance(to);
                }
            }

            return path;
        }

        private static List<T> GetPath<T>(Node<T> current, Dictionary<Node<T>, Node<T>> fromDict)
        {
            List<T> path = new List<T>();
            while (current != null)
            {
                path.Add(current.Content);
                fromDict.TryGetValue(current, out current);
            }

            path.Reverse();
            return path;
        }
    }
}