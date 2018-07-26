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
        private Cell _destinationCell;
        private float _targetDistance;
        private bool _reachedTarget;
        private float _currentTime;
        private Action _reachTargetAction;
        private Cell _nextCell;
        private Cell _currentCell;
        private List<Cell> _route = new List<Cell>();
        private float _lastRouteStartTime;
        private Thread _routeThread;
        private Cell _lastTargetCell;
        private Coroutine RouteWaitCoroutine;
        private MovementController _movementController;

        public void Awake()
        {
            _currentCell = PathingGrid.WorldToCellPosition(transform.position);
            _movementController = GetComponent<MovementController>();
        }
        
        public bool MoveToCover(Action reachCoverAction)
        {
            if (PathingGrid.IsCellHidden(_currentCell)) return false;
            Cell safeCell = PathingGrid.FindCoverNearMe(_currentCell);
            if (safeCell == null) return false;
            GoToCell(safeCell, reachCoverAction);
            return true;
        }

        private void MoveToNextCell()
        {
            Vector3 direction = ((Vector3) _nextCell.Position - transform.position).normalized;
            _movementController.Move(direction);
        }

        //pathfind

        public void SetRoute(List<Cell> route, float timeStarted)
        {
            if (timeStarted != _lastRouteStartTime) return;
            _route = route;
        }

        private IEnumerator WaitForRoute(Action reachTargetAction, float distanceFromTarget)
        {
            while (_routeThread.IsAlive) yield return null;
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
                    else break;
                }
            }

            PathingGrid.SmoothRoute(_route);
            Reposition(reachTargetAction);
        }

        public bool Moving()
        {
            return _destinationCell != null;
        }

        public void GoToCell(Cell targetCell, Action reachTargetAction, float distanceFromTarget = 0)
        {
            _destinationCell = targetCell;
            _targetDistance = distanceFromTarget;
            _reachTargetAction = reachTargetAction;
        }

        public void FixedUpdate()
        {
            _currentCell = PathingGrid.WorldToCellPosition(transform.position);
            if (_nextCell == null) return;
            _reachedTarget = _currentCell == _nextCell;
        }

        public void Update()
        {
            CheckForRequiredPathfind();
            MoveAction?.Invoke();
        }

        private Action MoveAction;
        
        private void Reposition(Action reachTargetAction)
        {
            Queue<Cell> newRoute = new Queue<Cell>(_route);
            if (newRoute.Count == 0)
            {
                Debug.Log(name + " has no route");
                return;
            }

            Debug.DrawLine(_currentCell.Position, _route[_route.Count - 1].Position, Color.red, 5f);
            _nextCell = newRoute.Dequeue();
            MoveAction = () =>
            {
                if (_reachedTarget)
                {
                    if (newRoute.Count == 0)
                    {
                        _destinationCell = null;
                        reachTargetAction();
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
            _routeThread = PathingGrid.ThreadRouteToCell(_currentCell, _destinationCell, this, _lastRouteStartTime);
            if (RouteWaitCoroutine != null) StopCoroutine(RouteWaitCoroutine);
            RouteWaitCoroutine = StartCoroutine(WaitForRoute(_reachTargetAction, _targetDistance));
        }
    }
}