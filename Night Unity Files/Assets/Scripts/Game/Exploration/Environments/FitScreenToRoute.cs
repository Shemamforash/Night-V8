using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Characters;
using Game.Exploration.Regions;
using UnityEngine;

namespace Game.Exploration.Environment
{
    public class FitScreenToRoute : MonoBehaviour
    {
        private const float TimeToMove = 0.5f;
        private const float MinSize = 5;

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
            float width = (rightmostNode - leftmostNode) / 2f;
            Vector3 centre = new Vector3(xCentre, yCentre, -10f);
            MoveToCentre(centre, height * 1.5f);
        }

        public void Recenter()
        {
            Vector3 position = CharacterManager.SelectedCharacter.TravelAction.GetCurrentNode().Position;
            position.z = -10;
            MoveToCentre(position, 5);
        }

        private void MoveToCentre(Vector3 centre, float height)
        {
            transform.DOMove(centre, TimeToMove);
            if (height < MinSize) height = MinSize;
            Camera.main.DOOrthoSize(height, TimeToMove);
        }
    }
}