using System.Collections.Generic;
using Game.Combat.Generation;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

namespace Game.Combat.Enemies
{
    [RequireComponent(typeof(MovementController))]
    public class MoveBehaviour : MonoBehaviour
    {
        private const int BucketSize = 15;

        private static readonly List<List<MoveBehaviour>> _moveBuckets = new List<List<MoveBehaviour>>();
        private static int _currentUpdateBucket;

        private List<Cell> _route = new List<Cell>();
        private Queue<Cell> _routeQueue;
        private Cell _currentCell, _targetCell, _nextCell;
        private MovementController _movementController;
        private float _minDistance, _maxDistance;
        private bool _reachedTarget, _shouldMove, _pathNeedsUpdating;


        public void Awake()
        {
            _movementController = GetComponent<MovementController>();
            AddToBucket();
        }

        private void OnDestroy()
        {
            RemoveFromBucket();
        }

        private void AddToBucket()
        {
            bool added = false;
            foreach (List<MoveBehaviour> bucket in _moveBuckets)
            {
                if (bucket.Count >= BucketSize) continue;
                bucket.Add(this);
                added = true;
                break;
            }

            if (added) return;
            List<MoveBehaviour> newBucket = new List<MoveBehaviour>();
            newBucket.Add(this);
            _moveBuckets.Add(newBucket);
        }

        private void RemoveFromBucket()
        {
            List<MoveBehaviour> bucket = _moveBuckets.Find(b => b.Contains(this));
            Assert.IsNotNull(bucket);
            bucket.Remove(this);
            if (bucket.Count != 0) return;
            _moveBuckets.Remove(bucket);
            if (_moveBuckets.Count == 0) _currentUpdateBucket = 0;
        }

        public Cell MoveToCover()
        {
            UpdateCurrentCell();
            if (PathingGrid.IsCellHidden(_currentCell)) return null;
            Cell safeCell = PathingGrid.FindCoverNearMe(_currentCell);
            if (safeCell == null) return null;
            GoToCell(safeCell);
            return safeCell;
        }

        private void UpdateCurrentCell()
        {
            _currentCell = PathingGrid.WorldToCellPosition(transform.position);
        }

        public bool Moving() => _targetCell != null;

        public void GoToCell(Cell targetCell, float maxDistance = 0.1f, float minDistance = 0f)
        {
            if (targetCell == null || !targetCell.Reachable) return;
            _targetCell = targetCell;
            _pathNeedsUpdating = true;
            _minDistance = minDistance;
            _maxDistance = maxDistance;
        }

        private void FixedUpdate()
        {
            UpdateCurrentCell();
            if (_nextCell == null) return;
            _reachedTarget = Vector2.Distance(_currentCell.Position, _nextCell.Position) < 0.5f;
        }

        public static void UpdateMoveBehaviours()
        {
            if (_moveBuckets.Count == 0) return;
            _moveBuckets[_currentUpdateBucket].ForEach(m => m.UpdatePath());
            ++_currentUpdateBucket;
            if (_currentUpdateBucket < _moveBuckets.Count) return;
            _currentUpdateBucket = 0;
        }

        public void MyUpdate()
        {
            Move();
        }

        private void DoStraightLinePath()
        {
            Vector2 difference = _targetCell.Position - _currentCell.Position;
            float distanceToTargetCell = difference.magnitude;
            if (distanceToTargetCell <= _maxDistance && distanceToTargetCell >= _minDistance) return;
            Vector2 direction = difference.normalized;
            float distanceFromTarget = Random.Range(_minDistance, _maxDistance);
            float distance = distanceToTargetCell - distanceFromTarget;
            Vector2 targetPosition = _currentCell.Position + direction * distance;
            _targetCell = PathingGrid.WorldToCellPosition(targetPosition);
//            Debug.DrawLine(_currentCell.Position, _targetCell.Position, Color.yellow, 1f);
            _route = new List<Cell>(new[] {_currentCell, _targetCell});
        }

        private void UpdatePath()
        {
            if (_targetCell == null || !_pathNeedsUpdating) return;
            _pathNeedsUpdating = false;
            UpdateCurrentCell();
            bool outOfSight = Physics2D.Linecast(transform.position, _targetCell.Position, 1 << 8).collider != null;
//            Debug.DrawLine(_currentCell.Position, _targetCell.Position, outOfSight ? Color.red : Color.green, 1f);
            if (outOfSight) DoPathFind();
            else DoStraightLinePath();
            Reposition();
        }

        private void MoveToNextCell()
        {
            Vector3 direction = ((Vector3) _nextCell.Position - transform.position).normalized;
            _movementController.Move(direction);
        }

        private void Move()
        {
            if (!_shouldMove) return;
            if (_reachedTarget)
            {
                if (_routeQueue.Count == 0)
                {
                    _targetCell = null;
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
                _targetCell = null;
                return;
            }

            _nextCell = _routeQueue.Dequeue();
            _shouldMove = true;
        }

        private void DoPathFind()
        {
            _route = PathingGrid.JPS(_currentCell, _targetCell);
            if (_route.Count == 0) return;
            float targetDistance = Random.Range(_minDistance, _maxDistance);
            if (targetDistance != 0)
            {
                for (int i = _route.Count - 1; i >= 0; --i)
                {
                    Cell c = _route[i];
                    if (c == null) return;
                    float distance = Vector2.Distance(c.Position, _targetCell.Position);
                    if (distance < targetDistance) _route.RemoveAt(i);
                    else break;
                }
            }

            PathingGrid.SmoothRoute(_route);
        }
    }
}