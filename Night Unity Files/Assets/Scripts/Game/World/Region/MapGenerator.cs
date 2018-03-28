using System.Collections.Generic;
using SamsHelper;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.World.Region
{
    public class MapGenerator : MonoBehaviour
    {
        public const int MapWidth = 120;
        public const int MinRadius = 6, MaxRadius = 9;
        public const int VisionRadius = 1;
        private const int DesiredSamples = 50;
        private int _currentSamples;
        private static readonly List<MapNode> storedNodes = new List<MapNode>();
        private static MapNode initialNode;
        private static float _currentAlpha;
        private static float _flashSpeed = 1;

        public void Start()
        {
            GenerateNodes();
            UpdateNodeColor();
            CharacterVisionController.Instance().SetNode(initialNode);
//            storedNodes.ForEach(n => UiPathDrawController.CreatePathBetweenNodes(initialNode, n));
        }

        public static void SetRoute(MapNode from, MapNode to)
        {
            if (route != null)
            {
                for (int i = 0; i < route.Count; ++i)
                {
                    if (i < route.Count - 1)
                    {
                        route[i].GetPathTo(route[i + 1]).StopGlowing();
                    }
                }
            }

            route = RoutePlotter.RouteBetween(from, to);
            Camera.main.GetComponent<FitScreenToRoute>().FitRoute(route);
        }

        private static List<MapNode> route;

        public void Update()
        {
            if (route == null || route.Count == 1) return;
            for (int i = 1; i < route.Count; ++i)
            {
                float scaledAlpha = _currentAlpha / 2f + 0.5f;
                route[i - 1].GetPathTo(route[i]).GlowSegments(scaledAlpha);
            }

            _currentAlpha += Time.deltaTime * _flashSpeed;
            if (_currentAlpha > 1)
            {
                _currentAlpha = 1;
                _flashSpeed = -_flashSpeed;
            }
            else if (_currentAlpha < 0)
            {
                _currentAlpha = 0;
                _flashSpeed = -_flashSpeed;
            }
        }

        public static List<MapNode> GetVisibleNodes(MapNode origin)
        {
            return storedNodes.FindAll(n =>
            {
                if (n.Discovered()) return false;
                return Vector2.Distance(n.transform.position, origin.transform.position) <= MaxRadius;
            });
        }

        public static List<MapNode> DiscoveredNodes()
        {
            return storedNodes.FindAll(n => n.Discovered());
        }

        private void GenerateNodes()
        {
            List<Region> regions = RegionManager.GenerateRegions(DesiredSamples);

            List<Vector2> samples = AdvancedMaths.GetPoissonDiscDistribution(DesiredSamples, MinRadius, MaxRadius, MapWidth / 2f, true);
            for (int i = 0; i < samples.Count; ++i)
            {
                Vector2Int point = new Vector2Int((int)samples[i].x, (int)samples[i].y);
                CreateNewNode(point, regions[i]);
                if(i == 0) initialNode = storedNodes[0];
            }
        }
        
        private void CreateNewNode(Vector2Int point, Region region)
        {
            MapNode newMapNode = MapNode.CreateNode(point, region);
            newMapNode.NodeObject.name = "Node " + _currentSamples;
            newMapNode.NodeObject.transform.SetParent(transform);
            storedNodes.Add(newMapNode);
            --_currentSamples;
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