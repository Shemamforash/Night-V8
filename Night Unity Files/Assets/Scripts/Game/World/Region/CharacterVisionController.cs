using System;
using System.Collections;
using Game.World.Region;
using SamsHelper;
using SamsHelper.Input;
using UnityEngine;

public class CharacterVisionController : MonoBehaviour, IInputListener
{
    private float _rotateSpeed = 2f;
    private Transform _visionTransform;
    private static CharacterVisionController _instance;
    private bool _moving;
    public MapNode CurrentNode;
    private float _distanceTravelled;
    private const float DistanceToFootPrint = 0.25f;
    private float _moveSpeed = 3f;
    private bool _leftLast;

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
        InputHandler.RegisterInputListener(this);
        _visionTransform.gameObject.SetActive(!_moving);
    }

    private void OnDisable()
    {
        InputHandler.UnregisterInputListener(this);
        _visionTransform.gameObject.SetActive(false);
    }

    public void Update()
    {
        UpdateSize();
        if (!_moving) return;
        Vector3 charPos = Instance().transform.position;
        charPos.z = -10f;
        Camera.main.transform.position = charPos;
    }
    
    private void UpdateSize()
    {
        float nearestNode = GetNearestNodeWeight();
        float xScale = nearestNode;
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
                transform.Rotate(new Vector3(0, 0, rotateAmount));
                UpdateSize();
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

    private void UpdateFootprintMaker(Vector3 lastPosition)
    {
        _distanceTravelled += Vector3.Distance(lastPosition, transform.position);
        if (_distanceTravelled < DistanceToFootPrint) return;
        GameObject newFootPrint = Instantiate(Resources.Load("Prefabs/Map/Footprint") as GameObject, transform.position, transform.rotation);
        if (_leftLast)
        {
            newFootPrint.transform.Translate(Vector3.left * 0.03f);
            _leftLast = false;
        }
        else
        {
            newFootPrint.transform.Translate(Vector3.right * 0.03f);
            _leftLast = true;
        }

        _distanceTravelled = 0;
    }

    private MapNode nearestNode;
    
    private IEnumerator MoveForward()
    {
        _moving = true;
        _visionTransform.gameObject.SetActive(false);
        nearestNode = null;
        while (true)
        {
            Vector3 lastPosition = transform.position;
            transform.Translate(Vector3.up * Time.deltaTime *_moveSpeed);
            if (nearestNode == null)
            {
                MapGenerator.UpdateNodeColor();
                MapNode nearestNodeCanditate = MapGenerator.GetNearestNode(transform.position, CurrentNode);
                float distance = nearestNodeCanditate.DistanceToPoint(transform.position);
                if (distance < MapGenerator.VisionRadius)
                {
                    nearestNode = nearestNodeCanditate;
                }
            }
            else
            {
                float distance = nearestNode.DistanceToPoint(transform.position);
                float timeToTarget = distance / 4;
                float angle = AngleBetweenMapNode(nearestNode, false);
                float angleDelta = angle / timeToTarget * Time.deltaTime;
                transform.Rotate(new Vector3(0, 0, angleDelta));
                if (AdvancedMaths.DoesLinePassThroughPoint(lastPosition, transform.position, nearestNode.transform.position))
                {
                    ReachNode(nearestNode);
                    break;
                }
            }
            UpdateFootprintMaker(lastPosition);
            yield return null;
        }
    }

    private float AngleBetweenMapNode(MapNode node, bool absolute = true)
    {
        Vector3 targetDirection = node.transform.position - transform.position;
        float xDir = -Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        float yDir = Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        Vector3 characterDirection = new Vector2(xDir, yDir);
        float angle = Vector2.Angle(characterDirection, targetDirection);
        if (absolute) return angle;
        Vector3 cross = Vector3.Cross(characterDirection, targetDirection);
        if (cross.z < 0)
        {
            angle = -angle;
        }

        return angle;
    }
    
    private float GetNearestNodeWeight()
    {
        float highestWeight = 0f;
        MapGenerator.GetVisibleNodes(CurrentNode).ForEach(n =>
        {
            float angle = AngleBetweenMapNode(n);
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