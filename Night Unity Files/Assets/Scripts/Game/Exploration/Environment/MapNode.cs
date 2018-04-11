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
        private string _name;
        public Vector2 Position;
        public Region.Region Region;

        public static MapNode CreateNode(Vector2Int position, Region.Region region)
        {
            MapNode mapNode = new MapNode();
            mapNode.Region = region;
            mapNode.SetPosition(position);
            mapNode._name = region == null ? "Vehicle" : region.Name;
            return mapNode;
        }

        public string GetRegionName()
        {
            return _name;
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
            GameObject nodeObject = GameObject.Instantiate(_nodePrefab);
            nodeObject.transform.SetParent(MapGenerator.MapTransform);
            nodeObject.name = _name;
            nodeObject.transform.position = new Vector3(Position.x, Position.y, 0);
            nodeObject.transform.localScale = Vector3.one;
            UpdatePaths();
            MapNodeController mapNodeController = nodeObject.transform.GetComponentInChildren<MapNodeController>(true);
            mapNodeController.gameObject.SetActive(true);
            mapNodeController.SetName(_name);
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
//            float distanceToCamera = Vector2.Distance(CharacterVisionController.Instance().transform.position, Position);
//            float alpha = Discovered() ? 1 : 1 - distanceToCamera / MapGenerator.VisionRadius;
//            if (alpha < 0) alpha = 0;

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