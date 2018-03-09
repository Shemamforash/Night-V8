﻿using System;
using System.Collections;
using System.Collections.Generic;
using Game.World.Region;
using UnityEngine;

public class FitScreenToRoute : MonoBehaviour
{
    private const float TimeToMove = 0.5f;
    private float _timeElapsed;
    private IEnumerator coroutine;
    private const float MinSize = 5;

    public void FitRoute(List<MapNode> route)
    {
        Vector3 nodePosition = route[0].transform.position;
        float highestNode = nodePosition.y;
        float lowestNode = nodePosition.y;
        float leftmostNode = nodePosition.x;
        float rightmostNode = nodePosition.x;
        foreach (MapNode node in route)
        {
            if (node == route[0]) continue;
            nodePosition = node.transform.position;
            if (nodePosition.y > highestNode) highestNode = nodePosition.y;
            if (nodePosition.y < lowestNode) lowestNode = nodePosition.y;
            if (nodePosition.x < leftmostNode) leftmostNode = nodePosition.x;
            if (nodePosition.x > rightmostNode) rightmostNode = nodePosition.x;
        }

        float xCentre = (leftmostNode + rightmostNode) / 2f;
        float yCentre = (highestNode + lowestNode) / 2f;
        float height = (highestNode - lowestNode) / 2f;
        Vector3 centre = new Vector3(xCentre, yCentre, -10f);
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = MoveToCentre(centre, height);
        StartCoroutine(coroutine);
    }

    private IEnumerator MoveToCentre(Vector3 centre, float height)
    {
        float startSize = GetComponent<Camera>().orthographicSize;
        float targetSize = height * 1.5f;
        if (targetSize < MinSize) targetSize = MinSize;
        _timeElapsed = 0f;
        Vector3 origin = transform.position;
        while (_timeElapsed < TimeToMove)
        {
            _timeElapsed += Time.deltaTime;
            if (_timeElapsed > TimeToMove)
            {
                _timeElapsed = TimeToMove;
            }

            float easedTime = Mathf.SmoothStep(0, 1, _timeElapsed / TimeToMove);
            bool easingOff = easedTime > 0.5f;

            if (easingOff) easedTime = 1 - easedTime;
            easedTime = (float) (Math.Pow(easedTime * 2f, 2f) / 2);
            if (easingOff) easedTime = 1 - easedTime;

            Vector3 newPosition = Vector3.Lerp(origin, centre, easedTime);
            transform.position = newPosition;

            gameObject.GetComponent<Camera>().orthographicSize = Mathf.Lerp(startSize, targetSize, easedTime);
            yield return null;
        }
    }
}