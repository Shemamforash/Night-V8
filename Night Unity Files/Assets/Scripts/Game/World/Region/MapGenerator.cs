using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.World.Region
{
    public class MapGenerator : MonoBehaviour
    {
        public const int MapWidth = 30;
        public const int MinRadius = 6, MaxRadius = 9;
        public const int VisionRadius = 1;
        private const int DesiredSamples = 50;
        private int _currentSamples;
        private readonly List<MapNode> activeNodes = new List<MapNode>();
        private static readonly List<MapNode> storedNodes = new List<MapNode>();
        private MapNode initialNode;

        public void Start()
        {
            GenerateNodes();
            UpdateNodeColor();
            CharacterVisionController.Instance().SetNode(initialNode);
//            storedNodes.ForEach(n => UiPathDrawController.CreatePathBetweenNodes(initialNode, n));
        }

        public static List<MapNode> GetVisibleNodes(MapNode origin)
        {
            return storedNodes.FindAll(n =>
            {
                if (n.Discovered) return false;
                return Vector2.Distance(n.transform.position, origin.transform.position) <= MaxRadius;
            });
        }

        private void GenerateNodes()
        {
            activeNodes.Clear();
            CreateNewNode(new Vector2Int(MapWidth / 2, MapWidth / 2));
            initialNode = storedNodes[0];
            _currentSamples = DesiredSamples - 1;

            while (activeNodes.Count != 0)
            {
                MapNode randomMapNode = activeNodes[Random.Range(0, activeNodes.Count)];
                Vector2Int point = GenerateSamplePoint(randomMapNode);
                if (point.x == -1)
                {
                    activeNodes.Remove(randomMapNode);
                }
                else
                {
                    CreateNewNode(point);
                }
            }
        }

        private void CreateNewNode(Vector2Int point)
        {
            MapNode newMapNode = MapNode.CreateNode(point);
            newMapNode.NodeObject.name = "Node " + _currentSamples;
            newMapNode.NodeObject.transform.SetParent(transform);
            activeNodes.Add(newMapNode);
            storedNodes.Add(newMapNode);
            --_currentSamples;
        }

        private List<Vector2Int> GetCandidatePositions(MapNode origin)
        {
            Vector2Int minCorner = GetMinCorner(origin.GetMapPosition());
            Vector2Int maxCorner = GetMaxCorner(origin.GetMapPosition());

            List<Vector2Int> validPositions = new List<Vector2Int>();

            for (int x = (int) minCorner.x; x <= maxCorner.x; ++x)
            {
                for (int y = (int) minCorner.y; y <= maxCorner.y; ++y)
                {
                    Vector2Int candidatePosition = new Vector2Int(x, y);
                    float distance = Vector2Int.Distance(origin.GetMapPosition(), candidatePosition);
                    if (distance >= MinRadius && distance <= MaxRadius)
                    {
                        validPositions.Add(candidatePosition);
                    }
                }
            }

            return validPositions;
        }

        private Vector2Int GenerateSamplePoint(MapNode origin)
        {
            List<Vector2Int> validPositions = GetCandidatePositions(origin);

            for (int i = 0; i < _currentSamples && validPositions.Count > 0; ++i)
            {
                int randomPosition = Random.Range(0, validPositions.Count);
                Vector2Int randomPoint = validPositions[randomPosition];
                validPositions.RemoveAt(randomPosition);
                bool tooClose = false;
                foreach (MapNode node in storedNodes)
                {
                    if (Vector2Int.Distance(node.GetMapPosition(), randomPoint) < MinRadius) tooClose = true;
                }

                if (!tooClose) return randomPoint;
            }

            return new Vector2Int(-1, -1);
        }

        private Vector2Int GetMinCorner(Vector2Int origin)
        {
            int minX = origin.x - MaxRadius;
            int minY = origin.y - MaxRadius;
            if (minX < 0) minX = 0;
            if (minY < 0) minY = 0;
            return new Vector2Int(minX, minY);
        }

        private Vector2Int GetMaxCorner(Vector2Int origin)
        {
            int maxX = origin.x + MaxRadius;
            int maxY = origin.y + MaxRadius;
            if (maxX >= MapWidth)
            {
                maxX = MapWidth - 1;
            }

            if (maxY >= MapWidth)
            {
                maxY = MapWidth - 1;
            }

            return new Vector2Int(maxX, maxY);
        }

        public static void UpdateNodeColor()
        {
            foreach (MapNode n in storedNodes)
            {
                n.UpdateColor();
            }
        }

        public static MapNode GetNearestNode(Vector3 position, MapNode excludedNode)
        {
            float nearestDistance = 100;
            MapNode nearestNode = null;
            storedNodes.ForEach(n =>
            {
                if (n == excludedNode) return;
                float distance = n.DistanceToPoint(position);
                if (distance >= nearestDistance) return;
                nearestDistance = distance;
                nearestNode = n;
            });
            return nearestNode;
        }
    }
}