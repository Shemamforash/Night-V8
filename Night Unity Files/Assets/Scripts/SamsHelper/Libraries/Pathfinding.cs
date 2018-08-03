using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SamsHelper.Libraries
{
    public static class Pathfinding
    {
        public static List<Node> AStar(Node from, Node to)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<Node> path = null;

            HashSet<Node> visited = new HashSet<Node>();
            HashSet<Node> unvisited = new HashSet<Node>();
            unvisited.Add(from);
            Dictionary<Node, Node> _fromDict = new Dictionary<Node, Node>();
            Dictionary<Node, float> gScore = new Dictionary<Node, float>();
            Dictionary<Node, float> fScore = new Dictionary<Node, float>();
            gScore[from] = 0;
            fScore[from] = Vector2.SqrMagnitude(from.Position - to.Position);

            while (unvisited.Count != 0)
            {
                if (stopwatch.Elapsed.Seconds >= 1) return new List<Node>();
                float minScore = float.MaxValue;
                Node minNode = null;
                for (int i = 0; i < unvisited.ToList().Count; i++)
                {
                    Node n = unvisited.ToList()[i];
                    if (fScore[n] >= minScore) continue;
                    minScore = fScore[n];
                    minNode = n;
                }

                Node current = minNode;
                if (current == to)
                {
                    path = GetPath(current, _fromDict);
                    break;
                }

                unvisited.Remove(current);
                visited.Add(current);

                foreach (Node neighbor in current.Neighbors())
                {
                    if (visited.Contains(neighbor)) continue;
                    unvisited.Add(neighbor);

                    float currentGScore = gScore[current];
                    float neighborGScore = gScore.ContainsKey(neighbor) ? gScore[neighbor] : float.MaxValue;
                    float tentativeGScore = currentGScore + 1;
                    if (tentativeGScore >= neighborGScore) continue;

                    _fromDict[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Vector2.SqrMagnitude(neighbor.Position - to.Position);
                }
            }

            if (path == null)
            {
                Debug.Log("No path exists between " + from.Position + " and " + to.Position);
            }

            return path;
        }

        private static List<Node> GetPath(Node current, Dictionary<Node, Node> fromDict)
        {
            List<Node> path = new List<Node>();
            while (current != null)
            {
                path.Add(current);
                fromDict.TryGetValue(current, out current);
            }

            path.Reverse();
            return path;
        }

        public static List<Edge> MinimumSpanningTree(Graph graph, Func<Node, Node, float> edgeWeightFunction = null)
        {
            List<Edge> minTreeEdges = new List<Edge>();
            HashSet<Node> tree = new HashSet<Node>();
            tree.Add(graph.Nodes()[0]);
            List<Node> uncheckedNodes = new List<Node>(graph.Nodes());
            uncheckedNodes.Remove(graph.Nodes()[0]);

            while (tree.Count != graph.Nodes().Count)
            {
                Node nearestNode = null;
                Node origin = null;
                float nearestNodeDistance = float.MaxValue;

                foreach (Node uncheckedNode in uncheckedNodes)
                {
                    foreach (Node treeNode in tree)
                    {
                        float candidateDistance = uncheckedNode.Distance(treeNode);
                        float edgeWeightMultiplier = 1;
                        if (edgeWeightFunction != null) edgeWeightMultiplier = edgeWeightFunction(treeNode, uncheckedNode);
                        if (candidateDistance * edgeWeightMultiplier >= nearestNodeDistance) continue;
                        nearestNode = uncheckedNode;
                        origin = treeNode;
                        nearestNodeDistance = candidateDistance;
                    }
                }

                tree.Add(nearestNode);
                uncheckedNodes.Remove(nearestNode);
                origin.AddNeighbor(nearestNode);
                minTreeEdges.Add(new Edge(origin, nearestNode));
            }

            return minTreeEdges;
        }
    }
}