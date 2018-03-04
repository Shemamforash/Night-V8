using System.Collections.Generic;
using Game.World.Region;
using NUnit.Framework;
using SamsHelper.Input;
using UnityEditor.VersionControl;
using UnityEngine;

public class CharacterVisionController : MonoBehaviour, IInputListener
{
    private float _rotateSpeed = 2f;
    public List<MapNode> _visibleNodes = new List<MapNode>();
    private Transform _visionTransform;

    public void Awake()
    {
        _visionTransform = transform.Find("Vision");
    }

    private void OnEnable()
    {
        InputHandler.RegisterInputListener(this);
    }

    private void OnDisable()
    {
        InputHandler.UnregisterInputListener(this);
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (axis != InputAxis.Horizontal) return;
        float rotateAmount = Time.deltaTime * _rotateSpeed;
        if (direction < 0)
        {
            rotateAmount = -rotateAmount;
        }

        rotateAmount *= Mathf.Rad2Deg;
        transform.Rotate(new Vector3(0, 0, rotateAmount));
        UpdateSize();
    }

    private float AngleBetweenMapNode(MapNode node)
    {
        Vector3 targetDirection = node.transform.position - transform.position;
        float xDir = -Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        float yDir = Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
        Vector3 characterDirection = new Vector2(xDir, yDir);
        return Vector2.Angle(characterDirection, targetDirection);
    }

    private void UpdateSize()
    {
        float nearestNode = GetNearestNode();
        float xScale = nearestNode;
        if (xScale < 0.05f) xScale = 0.05f;
        if (xScale > 2) xScale = 2;
        _visionTransform.localScale = new Vector3(xScale, 1, 1);
    }

    private float GetNearestNode()
    {
        float smallestTangeantDistance = 100f;
        float distanceToNearestNode = 100f;
        float angleToNearestNode = 100f;
        MapNode nearestNode = null;
        List<MapNode> sortedNodes = MapGenerator.GetVisibleNodes();
        sortedNodes.Sort((a, b) =>
        {
            float aDistance = Vector2.Distance(transform.position, a.transform.position);
            float bDistance = Vector2.Distance(transform.position, a.transform.position);
            if (aDistance < bDistance)
            {
                return 1;
            }

            if (aDistance > bDistance)
            {
                return -1;
            }

            return 0;
        });
        sortedNodes.ForEach(n =>
        {
            float angle = AngleBetweenMapNode(n);
            if (angle > 90) return;
            float distanceToNode = Vector2.Distance(transform.position, n.transform.position);
            if (distanceToNode < 0.1f) return;
            float tangeantDistance = Mathf.Sin(angle * Mathf.Deg2Rad) * distanceToNode;
            if (tangeantDistance >= smallestTangeantDistance) return;
            if (angle < angleToNearestNode && distanceToNode > distanceToNearestNode) return;
            distanceToNearestNode = distanceToNode;
            angleToNearestNode = angle;
            smallestTangeantDistance = tangeantDistance;
            nearestNode = n;
        });
        Assert.NotNull(nearestNode);
        return smallestTangeantDistance;
    }

//    private MapNode GetNearestNode()
//    {
//        float smallestAngle = 180;
//        MapNode nearestNode = null;
//        MapGenerator.GetVisibleNodes().ForEach(n =>
//        {
//            float angle = AngleBetweenMapNode(n);
//            float distanceToNode = Vector2.Distance(transform.position, n.transform.position);
//            if (distanceToNode < 0.2f) distanceToNode = 100;
//            angle *= distanceToNode / 10f;
//            if (angle >= smallestAngle) return;
//            smallestAngle = angle;
//            nearestNode = n;
//        });
//        Assert.NotNull(nearestNode);
//        return nearestNode;
//    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}