﻿using System;
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
        private float _timeOffset;
        private static float _timeOffsetCounter;

        private float _currentTime;
        private float _targetDistance;
        private bool _reachedTarget;

        private MovementController _movementController;
        private List<Cell> _route = new List<Cell>();
        private bool _shouldMove;
        private Transform _followTarget;
        private float _minDistance, _maxDistance;

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
                Cell cell =PathingGrid.GetCellNearMe(transform.position, _maxDistance / 2f, _minDistance);
                if (cell == null) return;
                GoToCell(cell, Random.Range(_minDistance, _maxDistance));
                return;
            }

            bool outOfSight = Physics2D.Linecast(transform.position, _followTarget.position, 1 << 8).collider != null;
            if (outOfSight)
            {
                GoToCell(targetCell, Random.Range(_minDistance, _maxDistance));
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
            Debug.DrawLine(_currentCell.Position, _destinationCell.Position, Color.red, 1f);
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