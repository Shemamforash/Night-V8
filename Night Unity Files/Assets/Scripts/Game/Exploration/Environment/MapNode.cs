using System.Collections.Generic;
using System.Linq;
using Game.Exploration.Ui;
using UnityEngine;

namespace Game.Exploration.Environment
{
    public class MapNode
    {
        private readonly Dictionary<MapNode, Path> _paths = new Dictionary<MapNode, Path>();
        private readonly HashSet<MapNode> _neighbors = new HashSet<MapNode>();
        public string Name;
        public GameObject NodeObject;
        public Vector2 Position;
        public Region.Region Region;

        public static MapNode CreateNode(Vector2Int position, Region.Region region)
        {
            MapNode mapNode = new MapNode();
            mapNode.Region = region;
            mapNode.SetPosition(position);
            return mapNode;
        }

        public void AddNeighbor(MapNode neighbor)
        {
            _neighbors.Add(neighbor);
            neighbor._neighbors.Add(this);
        }

        public List<MapNode> Neighbors()
        {
            return _neighbors.ToList();
        }

        private static GameObject _nodePrefab;
        
        public void CreateObject()
        {
            if (!Discovered()) return;
            if (_nodePrefab == null) _nodePrefab = Resources.Load<GameObject>("Prefabs/Map/Node");
            NodeObject = GameObject.Instantiate(_nodePrefab);
            NodeObject.transform.SetParent(MapGenerator.MapTransform);
            NodeObject.name = Name;
            NodeObject.transform.position = new Vector3(Position.x, Position.y, 0);
            NodeObject.transform.localScale = Vector3.one;
            UpdatePaths();
            string regionName = Region == null ? "Vehicle" : Region.Name;
            MapNodeController mapNodeController = NodeObject.transform.GetComponentInChildren<MapNodeController>(true);
            mapNodeController.gameObject.SetActive(true);
            mapNodeController.SetName(regionName);
        }

        public void AddPathTo(MapNode node, Path path)
        {
            _paths[node] = path;
        }

        public Path GetPathTo(MapNode node)
        {
            return _paths[node];
        }

        public bool Discovered()
        {
            return Region == null || Region.Discovered();
        }

        private void UpdatePaths()
        {
            foreach (MapNode node in Neighbors())
            {
                if (node.Discovered() && !_paths.ContainsKey(node))
                {
                    UiPathDrawController.CreatePathBetweenNodes(this, node);
                }
            }
        }

        private void SetPosition(Vector2Int position)
        {
            Position = position;
        }

        public void UpdateColor()
        {
            float distanceToCamera = Vector2.Distance(CharacterVisionController.Instance().transform.position, Position);
            float alpha = Discovered() ? 1 : 1 - distanceToCamera / MapGenerator.VisionRadius;
            if (alpha < 0) alpha = 0;

//            _spriteRenderer.color = new Color(1, 1, 1, alpha);
        }

        public float DistanceToPoint(MapNode node)
        {
            return DistanceToPoint(node.Position);
        }

        public float DistanceToPoint(Vector3 point)
        {
            return Vector3.Distance(point, Position);
        }
    }
}