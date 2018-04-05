using System.Collections.Generic;
using UnityEngine;

namespace Game.World.Region
{
    public class MapNode : MonoBehaviour
    {
        private Vector2Int Position;
        public GameObject NodeObject;
        public List<MapNode> Links = new List<MapNode>();
        private readonly Dictionary<MapNode, Path> _paths = new Dictionary<MapNode, Path>();
        public Region Region;

        public static MapNode CreateNode(Vector2Int position, Region region)
        {
            GameObject newNodeObject = Instantiate(Resources.Load("Prefabs/Map/Node") as GameObject);
            MapNode mapNode = newNodeObject.AddComponent<MapNode>();
            mapNode.Region = region;
            mapNode.NodeObject = newNodeObject;
            mapNode.SetPosition(position);
            newNodeObject.transform.localScale = Vector3.one;
            return mapNode;
        }

        public void AddPathTo(MapNode node, Path path)
        {
            _paths[node] = path;
        }
        
        public Path GetPathTo(MapNode node)
        {
            return _paths[node];
        }

        public Vector2Int GetMapPosition()
        {
            return Position;
        }

        public bool Discovered()
        {
            return Region == null || Region.Discovered();
        }

        public void Discover()
        {
            Region?.Discover();
            MapNodeController mapNodeController = transform.GetComponentInChildren<MapNodeController>(true);
            mapNodeController.gameObject.SetActive(true);
            string regionName = Region == null ? "Vehicle" : Region.Name;
            mapNodeController.SetName(regionName);
        }

        private void SetPosition(Vector2Int position)
        {
            Position = position;
            NodeObject.transform.position = new Vector3(position.x, position.y, 0);
            NodeObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }

        public void UpdateColor()
        {
            float distanceToCamera = Vector2.Distance(CharacterVisionController.Instance().transform.position, NodeObject.transform.position);
            float alpha = Discovered() ? 1 : 1 - distanceToCamera / MapGenerator.VisionRadius;
            if (alpha < 0)
            {
                alpha = 0;
            }

//            _spriteRenderer.color = new Color(1, 1, 1, alpha);
        }

        public float DistanceToPoint(MapNode node)
        {
            return DistanceToPoint(node.transform.position);
        }
        
        public float DistanceToPoint(Vector3 point)
        {
            return Vector3.Distance(point, transform.position);
        }
    }
}