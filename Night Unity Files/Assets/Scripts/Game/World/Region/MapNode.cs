using System.Collections.Generic;
using UnityEngine;

namespace Game.World.Region
{
    public class MapNode : MonoBehaviour
    {
        private Vector2Int Position;
        public GameObject NodeObject;
        private SpriteRenderer _spriteRenderer;
        public bool Discovered;
        public List<MapNode> Links = new List<MapNode>();

        public static MapNode CreateNode(Vector2Int position)
        {
            GameObject newNodeObject = Instantiate(Resources.Load("Prefabs/Map/Node") as GameObject);
            MapNode mapNode = newNodeObject.AddComponent<MapNode>();
            mapNode.NodeObject = newNodeObject;
            mapNode.SetPosition(position);
            return mapNode;
        }

        public Vector2Int GetMapPosition()
        {
            return Position;
        }

        private void SetPosition(Vector2Int position)
        {
            Position = position;
            _spriteRenderer = NodeObject.GetComponent<SpriteRenderer>();
            float newX = position.x - MapGenerator.MapWidth / 2;
            float newY = position.y - MapGenerator.MapWidth / 2;
            NodeObject.transform.position = new Vector3(newX, newY, 0);
            NodeObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }

        public void UpdateColor()
        {
            float distanceToCamera = Vector2.Distance(CharacterVisionController.Instance().transform.position, NodeObject.transform.position);
            float alpha = Discovered ? 1 : 1 - distanceToCamera / MapGenerator.VisionRadius;
            if (alpha < 0)
            {
                alpha = 0;
            }

            _spriteRenderer.color = new Color(1, 1, 1, alpha);
        }

        public float DistanceToPoint(Vector3 point)
        {
            return Vector3.Distance(point, transform.position);
        }
    }
}