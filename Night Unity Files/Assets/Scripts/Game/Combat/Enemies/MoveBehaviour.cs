using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Game.Combat.Generation;
using Game.Combat.Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    [RequireComponent(typeof(MovementController))]
    public class MoveBehaviour : MonoBehaviour
    {
        private Cell _currentCell;
        private Cell _destinationCell;
        private Cell _lastTargetCell;
        private Cell _nextCell;

        private float _currentTime;
        private float _lastRouteStartTime;
        private float _targetDistance;
        private bool _reachedTarget;

        private MovementController _movementController;
        private List<Cell> _route = new List<Cell>();
        private Thread _routeThread;
        private bool _shouldMove;
        private Coroutine RouteWaitCoroutine;
        private Transform _followTarget;
        private float _minDistance, _maxDistance;

        public void Awake()
        {
            _movementController = GetComponent<MovementController>();
        }

        public bool MoveToCover()
        {
            UpdateCurrentCell();
            if (PathingGrid.IsCellHidden(_currentCell)) return false;
            Cell safeCell = PathingGrid.FindCoverNearMe(_currentCell);
            if (safeCell == null) return false;
            GoToCell(safeCell);
            return true;
        }

        private void UpdateCurrentCell()
        {
            _currentCell = PathingGrid.WorldToCellPosition(transform.position);
        }

        public void SetRoute(List<Cell> route, float timeStarted)
        {
            if (timeStarted != _lastRouteStartTime) return;
            _route = route;
        }

        public bool Moving() => _destinationCell != null;

        public void GoToCell(Cell targetCell, float distanceFromTarget = 0)
        {
            if (targetCell == null) return;
            _destinationCell = targetCell;
            _targetDistance = distanceFromTarget;
        }

        private void FixedUpdate()
        {
            if (!CombatManager.InCombat()) return;
            UpdateCurrentCell();
            if (_nextCell == null) return;
            _reachedTarget = Vector2.Distance(_currentCell.Position, _nextCell.Position) < 0.5f;
        }

        private void Update()
        {
            if (!CombatManager.InCombat()) return;
            CheckForRequiredPathfind();
            Move();
        }

        private void UpdateFollowTargetPosition()
        {
            if (_followTarget == null) return;

            float distance = Vector2.Distance(transform.position, _followTarget.transform.position);
            bool outOfRange = distance > _maxDistance || distance < _minDistance;
            if (outOfRange)
            {
                GoToCell(PathingGrid.WorldToCellPosition(_followTarget.position), Random.Range(_minDistance, _maxDistance));
                return;
            }

            bool outOfSight = Physics2D.Linecast(transform.position, _followTarget.position, 1 << 8).collider != null;
            if (outOfSight)
            {
                GoToCell(PathingGrid.WorldToCellPosition(_followTarget.position), Random.Range(_minDistance, _maxDistance));
            }
        }

        public void FollowTarget(Transform target, float minDistance, float maxDistance)
        {
            _followTarget = target;
            _minDistance = minDistance;
            _maxDistance = maxDistance;
        }

        public void StopFollowing()
        {
            _followTarget = null;
        }

        private void MoveToNextCell()
        {
            Vector3 direction = ((Vector3) _nextCell.Position - transform.position).normalized;
            _movementController.Move(direction);
        }

        private IEnumerator WaitForRoute(float distanceFromTarget)
        {
            while (_routeThread.IsAlive) yield return null;
            if (_destinationCell == null) yield break;
            if (distanceFromTarget != 0)
            {
                for (int i = _route.Count - 1; i >= 0; --i)
                {
                    Cell c = _route[i];
                    float distance = Vector2.Distance(c.Position, _destinationCell.Position);
                    if (distance < distanceFromTarget)
                    {
                        _route.RemoveAt(i);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            PathingGrid.SmoothRoute(_route);
            Reposition();
        }

        private Queue<Cell> _routeQueue;

        private void Move()
        {
            if (!_shouldMove) return;
            if (_reachedTarget)
            {
                if (_routeQueue.Count == 0)
                {
                    _destinationCell = null;
                    _shouldMove = false;
                    return;
                }

                _nextCell = _routeQueue.Dequeue();
            }
            MoveToNextCell();
        }
        
        private void Reposition()
        {
            _routeQueue = new Queue<Cell>(_route);
            if (_routeQueue.Count == 0)
            {
                Debug.Log(name + " has no route");
                _destinationCell = null;
                return;
            }
            _nextCell = _routeQueue.Dequeue();
            _shouldMove = true;
        }

        private void CheckForRequiredPathfind()
        {
            if (_currentTime > 0f)
            {
                _currentTime -= Time.deltaTime;
                return;
            }
            
            UpdateFollowTargetPosition();
            _currentTime = 1f;
            if (_destinationCell == null) return;
            if (_destinationCell == _lastTargetCell) return;
            _lastTargetCell = _destinationCell;
            _lastRouteStartTime = Time.timeSinceLevelLoad;
            UpdateCurrentCell();
            Debug.DrawLine(_currentCell.Position, _destinationCell.Position, Color.red, 1f);
            _routeThread = PathingGrid.ThreadRouteToCell(_currentCell, _destinationCell, this, _lastRouteStartTime);
            if (RouteWaitCoroutine != null) StopCoroutine(RouteWaitCoroutine);
            RouteWaitCoroutine = StartCoroutine(WaitForRoute(_targetDistance));
        }
    }
}