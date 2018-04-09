using System.Collections;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Exploration.Environment;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Exploration.Ui
{
    public class CharacterVisionController : MonoBehaviour, IInputListener
    {
        private static CharacterVisionController _instance;
        private readonly float _moveSpeed = 3f;
        private bool _moving;
        private readonly float _rotateSpeed = 1f;
        private Transform _visionTransform;
        public MapNode CurrentNode;
        private Travel travelAction;

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (_moving) return;
            switch (axis)
            {
                case InputAxis.Horizontal:
                    float rotateAmount = Time.deltaTime * _rotateSpeed;
                    if (direction < 0) rotateAmount = -rotateAmount;
                    rotateAmount *= Mathf.Rad2Deg;
                    Vector3 rotation = transform.rotation.eulerAngles;
                    rotation.z += rotateAmount;
                    transform.rotation = Quaternion.Euler(rotation);
                    break;
                case InputAxis.Fire:
                    if (isHeld) return;
                    SetDestination();
                    break;
            }
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        public void Awake()
        {
            _instance = this;
            _visionTransform = transform.Find("Vision");
            travelAction = CharacterManager.SelectedCharacter.TravelAction;
            CurrentNode = travelAction.GetCurrentNode();
            transform.position = travelAction.GetCurrentPosition();
            _moving = travelAction.InTransit();
            Vector3 cameraPosition = Camera.main.transform.position;
            cameraPosition.x = transform.position.x;
            cameraPosition.y = transform.position.y;
            Camera.main.transform.position = cameraPosition;
            _moving = false;
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

        private MapNode FindTargetNode()
        {
            float smallestAngle = 500;
            MapNode closestNode = null;
            CurrentNode.Neighbors().ForEach(n =>
            {
                float angle = AdvancedMaths.AngleBetween(transform.position, n.Position, transform.rotation.eulerAngles.z);
                if (angle >= smallestAngle) return;
                smallestAngle = angle;
                closestNode = n;
            });
            return closestNode;
        }

        private void WalkToNowhere()
        {
            float angle = Mathf.Abs(transform.rotation.eulerAngles.z - 360);
            Vector3 dir = new Vector3();
            dir.x = Mathf.Sin(angle * Mathf.Deg2Rad);
            dir.y = Mathf.Cos(angle * Mathf.Deg2Rad);
            dir.Normalize();
            Vector3 targetPosition = transform.position += dir * Random.Range(2, 5);
            StartTravel(null, targetPosition);
        }

        private void StartTravel(MapNode nearestNode, Vector3 targetPosition)
        {
            travelAction.TravelTo(nearestNode, targetPosition);
            Debug.Log("started");
            SceneChanger.ChangeScene("Game");
        }
        
        private void SetDestination()
        {
            MapNode nearestNode = FindTargetNode();
            if (nearestNode == null)
            {
                WalkToNowhere();
            }
            else
            {
                StartTravel(nearestNode, nearestNode.Position);
            }
        }

        private float GetNearestNodeWeight()
        {
            float highestWeight = 0f;
            CurrentNode.Neighbors().ForEach(n =>
            {
                float angle = AdvancedMaths.AngleBetween(transform.position, n.Position, transform.rotation.eulerAngles.z);
                if (angle > 90) return;
                float distance = Vector2.Distance(transform.position, n.Position);
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
            return 1 - highestWeight;
        }
    }
}