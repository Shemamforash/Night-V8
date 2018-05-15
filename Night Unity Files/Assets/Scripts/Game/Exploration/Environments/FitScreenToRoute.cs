﻿using System;
using System.Collections;
using System.Collections.Generic;
using Game.Characters;
using Game.Exploration.Regions;
using UnityEngine;

namespace Game.Exploration.Environment
{
    public class FitScreenToRoute : MonoBehaviour
    {
        private const float TimeToMove = 0.5f;
        private const float MinSize = 5;
        private float _timeElapsed;
        private IEnumerator coroutine;

        public void FitRoute(List<Region> route)
        {
            Vector3 nodePosition = route[0].Position;
            float highestNode = nodePosition.y;
            float lowestNode = nodePosition.y;
            float leftmostNode = nodePosition.x;
            float rightmostNode = nodePosition.x;
            foreach (Region node in route)
            {
                if (node == route[0]) continue;
                nodePosition = node.Position;
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
            coroutine = MoveToCentre(centre, height * 1.5f);
            StartCoroutine(coroutine);
        }

        public void Recenter()
        {
            Vector3 position = CharacterManager.SelectedCharacter.TravelAction.GetCurrentPosition();
            position.z = -10;
            coroutine = MoveToCentre(position, 5);
            StartCoroutine(coroutine);
        }

        private IEnumerator MoveToCentre(Vector3 centre, float height)
        {
            float startSize = GetComponent<Camera>().orthographicSize;
            if (height < MinSize) height = MinSize;
            _timeElapsed = 0f;
            Vector3 origin = transform.position;
            while (_timeElapsed < TimeToMove)
            {
                _timeElapsed += Time.deltaTime;
                if (_timeElapsed > TimeToMove) _timeElapsed = TimeToMove;

                float easedTime = Mathf.SmoothStep(0, 1, _timeElapsed / TimeToMove);
                bool easingOff = easedTime > 0.5f;

                if (easingOff) easedTime = 1 - easedTime;
                easedTime = (float) (Math.Pow(easedTime * 2f, 2f) / 2);
                if (easingOff) easedTime = 1 - easedTime;

                Vector3 newPosition = Vector3.Lerp(origin, centre, easedTime);
                transform.position = newPosition;

                gameObject.GetComponent<Camera>().orthographicSize = Mathf.Lerp(startSize, height, easedTime);
                yield return null;
            }
        }
    }
}