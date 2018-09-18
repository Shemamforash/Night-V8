using System.Collections.Generic;
using System.Diagnostics;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    [RequireComponent(typeof(MovementController))]
    public class MoveBehaviour : MonoBehaviour
    {
        private float _minDistance, _maxDistance;
        private Cell _currentCell;
        private Cell _destinationCell;
        private Cell _lastTargetCell;
        private Cell _nextCell;
        private float _timeOffset;
        private static float _timeOffsetCounter;

        private float _currentTime;
        private float _targetDistance;
        private bool _reachedTarget;

        private MovementController _movementController;
        private List<Cell> _route = new List<Cell>();
        private bool _shouldMove;
        private Transform _followTarget;

        public void Awake()
        {
            _movementController = GetComponent<MovementController>();
            _timeOffset = _timeOffsetCounter;
            _timeOffsetCounter += 0.02f;
            if (_timeOffsetCounter >= 1) _timeOffsetCounter = 0f;
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

        public bool Moving() => _destinationCell != null;

        public void GoToCell(Cell targetCell, float distanceFromTarget = 0)
        {
            if (targetCell == null || !targetCell.Reachable) return;
            _destinationCell = targetCell;
            _targetDistance = distanceFromTarget;
        }

        private void FixedUpdate()
        {
            UpdateCurrentCell();
            if (_nextCell == null) return;
            _reachedTarget = Vector2.Distance(_currentCell.Position, _nextCell.Position) < 0.5f;
        }

        public void MyUpdate()
        {
            UpdateTargetCell();
            CheckForRequiredPathfind();
            Move();
        }

        private void UpdateTargetCell()
        {
            if (_followTarget == null) return;
            if (!_outOfSight) return;
            _followCell = PathingGrid.WorldToCellPosition(_followTarget.position);
            if (_followCell.Reachable) return;
            List<Cell> cellsNearMe = PathingGrid.GetCellsNearMe(_followCell.Position, 1, 1);
            if (cellsNearMe.Count == 0) return;
            _followCell = cellsNearMe[0];
        }

        private void UpdateFollowTargetPosition()
        {
            if (_followTarget == null) return;
            if (_outOfSight)
            {
                GoToCell(_followCell, Random.Range(1, 3));
                return;
            }

            Cell targetCell = PathingGrid.WorldToCellPosition(_followTarget.position);
            if (!targetCell.Reachable) return;
            float distance = Vector2.Distance(transform.position, _followTarget.transform.position);
            if (distance > _maxDistance)
            {
                GoToCell(targetCell, Random.Range(_minDistance, _maxDistance));
                return;
            }

            if (distance < _minDistance)
            {
                Cell cell = PathingGrid.GetCellNearMe(transform.position, _maxDistance / 2f, _minDistance);
                if (cell == null) return;
                GoToCell(cell);
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

        private Queue<Cell> _routeQueue;
        private Cell _followCell;
        private bool _outOfSight;

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
                _destinationCell = null;
                return;
            }

            _nextCell = _routeQueue.Dequeue();
            _shouldMove = true;
        }

        private void CheckForRequiredPathfind()
        {
            if (_currentTime > _timeOffsetCounter)
            {
                _currentTime -= Time.deltaTime;
                return;
            }

            UpdateFollowTargetPosition();
            _currentTime = 0.5f + _timeOffsetCounter;
            if (_destinationCell == null) return;
            if (_destinationCell == _lastTargetCell) return;
            _lastTargetCell = _destinationCell;
            UpdateCurrentCell();
            Debug.DrawLine(_currentCell.Position, _destinationCell.Position, _outOfSight ? Color.red : Color.green, 1f);
            _route = PathingGrid.JPS(_currentCell, _destinationCell);

            if (_route.Count == 0) return;
            if (_targetDistance != 0)
            {
                for (int i = _route.Count - 1; i >= 0; --i)
                {
                    Cell c = _route[i];
                    if (c == null) return;
                    float distance = Vector2.Distance(c.Position, _destinationCell.Position);
                    if (distance < _targetDistance)
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
            for (int i = 1; i < _route.Count; i++)
            {
                Vector2 last = new Vector2(_route[i - 1].x, _route[i - 1].y);
                Vector2 current = new Vector2(_route[i].x, _route[i].y);
                Debug.DrawLine(last, current, Color.green, 5f);
            }

            Reposition();
        }
    }
}