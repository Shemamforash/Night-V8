using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies
{
    [RequireComponent(typeof(MovementController))]
    public class MoveBehaviour : MonoBehaviour
    {
        private Cell _currentCell;
        private float _currentTime;
        private Cell _destinationCell;
        private float _lastRouteStartTime;
        private Cell _lastTargetCell;
        private MovementController _movementController;
        private Cell _nextCell;
        private bool _reachedTarget;
        private List<Cell> _route = new List<Cell>();
        private Thread _routeThread;
        private float _targetDistance;

        private Action MoveAction;
        private Coroutine RouteWaitCoroutine;

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
            UpdateCurrentCell();
            if (_nextCell == null) return;
            _reachedTarget = Vector2.Distance(_currentCell.Position, _nextCell.Position) < 0.5f;
        }

        private void Update()
        {
            CheckForRequiredPathfind();
            MoveAction?.Invoke();
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


        private void Reposition()
        {
            Queue<Cell> newRoute = new Queue<Cell>(_route);
            if (newRoute.Count == 0)
            {
                Debug.Log(name + " has no route");
                _destinationCell = null;
                return;
            }
            
            _nextCell = newRoute.Dequeue();
            MoveAction = () =>
            {
                if (_reachedTarget)
                {
                    if (newRoute.Count == 0)
                    {
                        _destinationCell = null;
                        return;
                    }

                    _nextCell = newRoute.Dequeue();
                }

                MoveToNextCell();
            };
        }

        private void CheckForRequiredPathfind()
        {
            if (_currentTime > 0f)
            {
                _currentTime -= Time.deltaTime;
                return;
            }

            _currentTime = 1f;
            if (_destinationCell == null) return;
            if (_destinationCell == _lastTargetCell) return;
            _lastTargetCell = _destinationCell;
            _lastRouteStartTime = Time.timeSinceLevelLoad;
            UpdateCurrentCell();
            _routeThread = PathingGrid.ThreadRouteToCell(_currentCell, _destinationCell, this, _lastRouteStartTime);
            if (RouteWaitCoroutine != null) StopCoroutine(RouteWaitCoroutine);
            RouteWaitCoroutine = StartCoroutine(WaitForRoute(_targetDistance));
        }
    }
}