using System.Collections.Generic;
using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies
{
	[RequireComponent(typeof(MovementController))]
	public class MoveBehaviour : MonoBehaviour
	{
		private Cell               _currentCell, _targetCell, _nextCell;
		private float              _minDistance, _maxDistance;
		private MovementController _movementController;
		private bool               _needsUpdating;
		private bool               _reachedTarget, _outOfSight, _tooClose, _tooFar;
		private Queue<Cell>        _routeQueue = new Queue<Cell>();

		public void Awake()
		{
			_movementController = GetComponent<MovementController>();
			AIMoveManager.AddToBucket(this);
		}

		public void GoToCell(Cell targetCell, float maxDistance = 0.1f, float minDistance = 0f)
		{
			if (targetCell == null || !targetCell.Reachable) return;
			_targetCell    = targetCell;
			_minDistance   = minDistance;
			_maxDistance   = maxDistance;
			_needsUpdating = true;
			UpdateCurrentCell();
		}

		private void UpdateCurrentCell()
		{
			_currentCell = WorldGrid.WorldToCellPosition(transform.position, false);
			if (_currentCell == null || _targetCell == null) return;

			_outOfSight = Physics2D.Linecast(transform.position, _targetCell.Position, 1 << 8).collider != null;
			Vector2 difference           = _targetCell.Position - _currentCell.Position;
			float   distanceToTargetCell = difference.magnitude;
			_tooFar   = distanceToTargetCell > _maxDistance;
			_tooClose = distanceToTargetCell < _minDistance;
		}

		private void FixedUpdate()
		{
			if (_targetCell == null) return;
			UpdateCurrentCell();
			if (_currentCell == _targetCell) return;
			DoMove();
		}

		private void DoMove()
		{
			if (_nextCell == null) return;
			if (_currentCell == null || Vector2.Distance(_currentCell.Position, _nextCell.Position) > 0.25f)
			{
				Vector3 direction = ((Vector3) _nextCell.Position - transform.position).normalized;
				_movementController.Move(direction);
				return;
			}

			if (_routeQueue.Count == 0) return;
			_nextCell = _routeQueue.Dequeue();
		}

		private List<Cell> DoStraightLinePath()
		{
			if (_targetCell  == null) return null;
			if (_currentCell == null) return null;
			Vector2 direction      = (_targetCell.Position - _currentCell.Position).normalized;
			Vector2 targetPosition = _targetCell.Position + _minDistance * 1.2f * direction;
			_targetCell = WorldGrid.WorldToCellPosition(targetPosition, false);
			if (_targetCell == null) return null;
			return new List<Cell>(new[] {_targetCell});
		}

		public void UpdatePath()
		{
			if (!_needsUpdating) return;
			_needsUpdating = false;
			List<Cell> route;
			if (_outOfSight || _tooClose)
			{
				route = DoPathFind();
			}
			else if (_tooFar)
			{
				route = DoStraightLinePath();
			}
			else
			{
				_targetCell = _currentCell;
				route       = new List<Cell> {_targetCell};
			}

			if (route == null) return;
			_routeQueue = new Queue<Cell>(route);
			if (_routeQueue.Count != 0) _nextCell = _routeQueue.Dequeue();
		}

		private List<Cell> DoPathFind()
		{
			List<Cell> route = WorldGrid.JPS(_currentCell, _targetCell);
			if (route.Count == 0) return null;
			float targetDistance = _minDistance * 1.2f;
			if (targetDistance != 0)
			{
				for (int i = route.Count - 1; i >= 0; --i)
				{
					Cell c = route[i];
					if (c == null) return null;
					float distance = Vector2.Distance(c.Position, _targetCell.Position);
					if (distance > targetDistance) break;
					route.RemoveAt(i);
				}
			}

			WorldGrid.SmoothRoute(route);
			return route;
		}

		private void OnDestroy()
		{
			AIMoveManager.RemoveFromBucket(this);
		}

		public bool Moving() => _currentCell != _targetCell;
	}
}