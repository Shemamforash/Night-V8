using System;
using System.Collections.Generic;

namespace SamsHelper.Libraries
{
    public static class Pathfinding
    {
        public static List<T> AStar<T>(Node<T> from, Node<T> to)
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

        public static void MinimumSpanningTree<T>(Graph<T> graph, Func<Node<T>, Node<T>, float> edgeWeightFunction = null)
        {
            HashSet<Node<T>> tree = new HashSet<Node<T>>();
            tree.Add(graph.Nodes()[0]);
            List<Node<T>> uncheckedNodes = new List<Node<T>>(graph.Nodes());
            uncheckedNodes.Remove(graph.Nodes()[0]);

            while (tree.Count != graph.Nodes().Count)
            {
                Node<T> nearestNode = null;
                Node<T> origin = null;
                float nearestNodeDistance = float.MaxValue;

                foreach (Node<T> uncheckedNode in uncheckedNodes)
                {
                    foreach (Node<T> treeNode in tree)
                    {
                        float candidateDistance = uncheckedNode.Distance(treeNode);
                        float edgeWeightMultiplier = 1;
                        if (edgeWeightFunction != null) edgeWeightMultiplier = edgeWeightFunction(treeNode, uncheckedNode);
                        if(candidateDistance * edgeWeightMultiplier >= nearestNodeDistance) continue;
                        nearestNode = uncheckedNode;
                        origin = treeNode;
                        nearestNodeDistance = candidateDistance;
                    }
                }

                tree.Add(nearestNode);
                uncheckedNodes.Remove(nearestNode);
                origin.AddNeighbor(nearestNode);
                graph.AddEdge(new Edge<T>(origin, nearestNode));
            }
        }
    }
}