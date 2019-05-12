using System;
using Extensions;
using Game.Combat.Generation;

using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
	public class Orbit : MonoBehaviour
	{
		public enum SpinDirection
		{
			Clockwise,
			Anticlockwise
		}

		private float           _currentOrbitRadius;
		private Action<Vector2> _forceAction;
		private float           _orbitRadiusMax = 5f;
		private float           _orbitRadiusMin = 2.5f;
		private float           _speed;
		private SpinDirection   _spinDir = SpinDirection.Clockwise;
		private Vector2         _targetPosition;
		private Transform       _targetTransform;

		public void Initialise(Transform target, Action<Vector2> forceAction, float speed, float orbitRadiusMin, float orbitRadiusMax = -1)
		{
			_forceAction     = forceAction;
			_targetTransform = target;
			_orbitRadiusMin  = orbitRadiusMin;
			_orbitRadiusMax  = orbitRadiusMax < 0 ? _orbitRadiusMin : orbitRadiusMax;
			if (NumericExtensions.RollDie(0, 2)) _spinDir = SpinDirection.Anticlockwise;
			_speed = speed;
		}

		private float GetOrbitRadius()
		{
			float noise = Mathf.PerlinNoise(Time.timeSinceLevelLoad, 0f);
			_currentOrbitRadius = _orbitRadiusMin + noise * (_orbitRadiusMax - _orbitRadiusMin);
			return _currentOrbitRadius;
		}

		public void SetSpin(SpinDirection spinDir)
		{
			_spinDir = spinDir;
		}

		public void Update()
		{
			if (!CombatManager.Instance().IsCombatActive()) return;
			_targetPosition = _targetTransform == null ? _targetPosition : (Vector2) _targetTransform.position;
			Vector2 currentPosition = transform.position;
			Vector2 dirToTarget     = _targetPosition - currentPosition;
			dirToTarget.Normalize();
			Vector2 tangent                                   = new Vector2(-dirToTarget.y, dirToTarget.x);
			Vector2 pos                                       = -dirToTarget * GetOrbitRadius() + _targetPosition;
			float   mult                                      = 2;
			if (_spinDir == SpinDirection.Anticlockwise) mult = -mult;
			pos += tangent * mult;
			Vector2 dirToTangent = (pos - currentPosition).normalized;
			_forceAction.Invoke(dirToTangent * GetSpeed());
		}

		private float GetSpeed()
		{
			float noise = Mathf.PerlinNoise(0f, Time.timeSinceLevelLoad);
			noise = 0.9f + noise / 5f;
			return _speed * noise;
		}
	}
}