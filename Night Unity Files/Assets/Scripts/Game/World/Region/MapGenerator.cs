using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.World.Region
{
    public class MapGenerator : MonoBehaviour
    {
        //10 radius unit = 0.5 hours
        public const int MapWidth = 100;
        private const int MinRadius = 10, MaxRadius = 40;
        private const int DesiredSamples = 50;
        private int _currentSamples;
        private readonly List<MapNode> activeNodes = new List<MapNode>();
        private static readonly List<MapNode> storedNodes = new List<MapNode>();

        public void Start()
        {
            GenerateNodes();
            UpdateNodeColor();
        }

        private float _currentTime;
        
        public void PlerpUpdate()
        {
            if (_currentTime > 0f)
            {
                _currentTime -= Time.deltaTime;
            }
            else
            {
                Camera.main.GetComponent<UiNodeFocusScript>().FocusOnNode(storedNodes[Random.Range(0, storedNodes.Count)].NodeObject);
                _currentTime = 2f;
            }
        }

        public static List<MapNode> GetVisibleNodes()
        {
            return storedNodes;
            return storedNodes.FindAll(n => n.Visible);
        }
        
        private void GenerateNodes()
        {
            activeNodes.Clear();
            CreateNewNode(new Vector2Int(MapWidth / 2, MapWidth / 2));
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
            Vector2 minCorner = GetMinCorner(origin.Position);
            Vector2 maxCorner = GetMaxCorner(origin.Position);

            List<Vector2Int> validPositions = new List<Vector2Int>();

            for (int x = (int) minCorner.x; x <= maxCorner.x; ++x)
            {
                for (int y = (int) minCorner.y; y <= maxCorner.y; ++y)
                {
                    Vector2Int candidatePosition = new Vector2Int(x, y);
                    float distance = Vector2Int.Distance(origin.Position, candidatePosition);
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
                    if (Vector2Int.Distance(node.Position, randomPoint) < MinRadius) tooClose = true;
                }

                if (!tooClose) return randomPoint;
            }

            return new Vector2Int(-1, -1);
        }

        private Vector2 GetMinCorner(Vector2Int origin)
        {
            int minX = origin.x - MaxRadius;
            int minY = origin.y - MaxRadius;
            if (minX < 0) minX = 0;
            if (minY < 0) minY = 0;
            return new Vector2(minX, minY);
        }

        private Vector2 GetMaxCorner(Vector2Int origin)
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

            return new Vector2(maxX, maxY);
        }

        public static void UpdateNodeColor()
        {
            foreach (MapNode n in storedNodes)
            {
                n.UpdateColor();
            }
        }
    }
}