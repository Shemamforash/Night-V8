using UnityEngine;

namespace Game.World.Region
{
    public class MapNode : MonoBehaviour
    {
        public Vector2Int Position;
        public GameObject NodeObject;
        private SpriteRenderer _spriteRenderer;
        public bool Visible;
        
        public static MapNode CreateNode(Vector2Int position)
        {
            GameObject newNodeObject = Instantiate(Resources.Load("Prefabs/Map/Node") as GameObject);
            MapNode mapNode = newNodeObject.AddComponent<MapNode>();
            mapNode.NodeObject = newNodeObject;
            mapNode.SetPosition(position);
            return mapNode;
        }
        
        private void SetPosition(Vector2Int position)
        {
            Position = position;
            _spriteRenderer = NodeObject.GetComponent<SpriteRenderer>();
            float newX = (position.x - MapGenerator.MapWidth / 2) * 0.2f;
            float newY = (position.y - MapGenerator.MapWidth / 2) * 0.2f;
            NodeObject.transform.position = new Vector3(newX, newY, 0);
            NodeObject.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }

        public void UpdateColor()
        {
            float distanceToCamera = Vector2.Distance(Camera.main.transform.position, NodeObject.transform.position);
            float alpha = -0.25f * distanceToCamera + 1;
            Visible = true;
            if (alpha < 0)
            {
                alpha = 0;
                Visible = false;
            }
            _spriteRenderer.color = new Color(1,1,1,alpha);
        }
    }
}