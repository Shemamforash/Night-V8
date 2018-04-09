using System.Collections;
using Game.Exploration.Environment;
using UnityEngine;

namespace Game.Exploration.Ui
{
    public class UiNodeFocusScript : MonoBehaviour
    {
        private const float PositionMoveTime = 0.5f;
        private Vector3 _startPosition, _targetPosition;

        public void FocusOnNode(GameObject node)
        {
            _startPosition = transform.position;
            Vector3 nodePosition = node.transform.position;
            _targetPosition = new Vector3(nodePosition.x, nodePosition.y, _startPosition.z);
            StartCoroutine(LerpToPosition());
        }

        public void Update()
        {
        }

        private IEnumerator LerpToPosition()
        {
            float elapsedTime = 0f;
            while (elapsedTime < PositionMoveTime)
            {
                elapsedTime += Time.deltaTime;
                float normalisedTime = elapsedTime / PositionMoveTime;
                transform.position = Vector3.Lerp(_startPosition, _targetPosition, normalisedTime);
                MapGenerator.UpdateNodeColor();
                yield return null;
            }
        }
    }
}