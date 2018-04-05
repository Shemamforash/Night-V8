using System.Collections;
using Game.World.Region;
using SamsHelper;
using SamsHelper.Input;
using UnityEngine;

public class CharacterVisionController : MonoBehaviour, IInputListener
{
    private float _rotateSpeed = 1f;
    private Transform _visionTransform;
    private static CharacterVisionController _instance;
    private bool _moving;
    public MapNode CurrentNode;
    private readonly float _moveSpeed = 3f;

    public void Awake()
    {
        _instance = this;
        _visionTransform = transform.Find("Vision");
    }

    public static CharacterVisionController Instance()
    {
        return _instance;
    }

    private void OnEnable()
    {
        InputHandler.SetCurrentListener(this);
        _visionTransform.gameObject.SetActive(!_moving);
    }

    private void OnDisable()
    {
        InputHandler.SetCurrentListener(this);
        _visionTransform.gameObject.SetActive(false);
    }

    public void Update()
    {
        UpdateSize();
    }

    private void UpdateSize()
    {
        float nearestNodeWeight = GetNearestNodeWeight();
        float xScale = nearestNodeWeight;
        float randomOffset = Mathf.PerlinNoise(Time.time, 0);
        xScale += randomOffset / 5f;
        xScale = Mathf.Clamp(xScale, 0.05f, 1);
        _visionTransform.localScale = new Vector3(xScale, 1, 1);
    }
    
    public void SetNode(MapNode node)
    {
        if (CurrentNode != null)
        {
            if (CurrentNode.Links.Contains(node)) return;
            CurrentNode.Links.Add(node);
            node.Links.Add(CurrentNode);
        }
        Debug.Log(node.transform.position);
        transform.position = node.transform.position;
        CurrentNode = node;
        CurrentNode.Discover();
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (_moving) return;
        switch (axis)
        {
            case InputAxis.Horizontal:
                float rotateAmount = Time.deltaTime * _rotateSpeed;
                if (direction < 0)
                {
                    rotateAmount = -rotateAmount;
                }

                rotateAmount *= Mathf.Rad2Deg;
                Vector3 rotation = transform.rotation.eulerAngles;
                rotation.z += rotateAmount;
                transform.rotation = Quaternion.Euler(rotation);
                break;
            case InputAxis.Fire:
                if (isHeld) return;
                StartCoroutine(MoveForward());
                break;
        }
    }

    private void ReachNode(MapNode newNode)
    {
        _moving = false;
        _visionTransform.gameObject.SetActive(true);
        UiPathDrawController.CreatePathBetweenNodes(CurrentNode, newNode);
        SetNode(newNode);
    }

    private MapNode nearestNode;
    
    private IEnumerator MoveForward()
    {
        _moving = true;
        _visionTransform.gameObject.SetActive(false);
        nearestNode = null;
        Vector2 originalPosition = Vector2.zero;
        Vector3 direction = new Vector3();
        float angle = Mathf.Abs(transform.rotation.eulerAngles.z - 360);
        direction.x = Mathf.Sin(angle * Mathf.Deg2Rad);
        direction.y = Mathf.Cos(angle * Mathf.Deg2Rad);
        direction.Normalize();
        Debug.Log(angle + " " +direction);
        while (true)
        {
            transform.position += direction * Time.deltaTime * _moveSpeed;
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
            if (nearestNode == null)
            {
                MapGenerator.UpdateNodeColor();
                MapNode nearestNodeCanditate = MapGenerator.GetNearestNode(transform.position, CurrentNode);
                float distance = nearestNodeCanditate.DistanceToPoint(transform.position);
                if (distance < MapGenerator.VisionRadius)
                {
                    nearestNode = nearestNodeCanditate;
                    nearestNode.transform.position = transform.position + direction;
                    originalPosition = transform.position;
                }
            }
            else
            {
                float normalisedDistance = _moveSpeed * Time.deltaTime / Vector2.Distance(originalPosition, nearestNode.transform.position);
                normalisedDistance = Mathf.Clamp(normalisedDistance, 0f, 1f);
                transform.position = Vector2.Lerp(originalPosition, nearestNode.transform.position, normalisedDistance);
                Debug.Log(normalisedDistance);
                if (normalisedDistance >= 0.995)
                {
                    ReachNode(nearestNode);
                    break;
                }
            }
            yield return null;
        }
    }

    private float GetNearestNodeWeight()
    {
        float highestWeight = 0f;
        MapGenerator.GetVisibleNodes(CurrentNode).ForEach(n =>
        {
            float angle = AdvancedMaths.AngleBetween(transform, n.transform);
            if (angle > 90) return;
            float distance = Vector2.Distance(transform.position, n.transform.position);
            if (distance < 0.1f || distance > MapGenerator.MaxRadius) return;
            float tangeantDistance = Mathf.Sin(angle * Mathf.Deg2Rad) * distance;

            distance -= MapGenerator.MinRadius;
            float distanceWeight = distance / (MapGenerator.MaxRadius - MapGenerator.MinRadius);
            distanceWeight = 1 - distanceWeight;
            
            float tangeantWeight = 1 - tangeantDistance / MapGenerator.MinRadius;
            if (tangeantWeight < 0) tangeantWeight = 0;
            float angleWeight = 1 - angle / 90f;
            float nodeWeight = distanceWeight * tangeantWeight * angleWeight;
            
            if (nodeWeight <= highestWeight) return;
            highestWeight = nodeWeight;
        });
        float perception = 10f;
        float damping = 0.5f + 0.05f * perception;
        highestWeight *= damping;
        return 1- highestWeight;
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}