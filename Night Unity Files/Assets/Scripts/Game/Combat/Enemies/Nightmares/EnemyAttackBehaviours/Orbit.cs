using System;
using Game.Combat.Generation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Orbit : MonoBehaviour
    {
        private Transform _targetTransform;
        private Vector2 _targetPosition;
        private float _orbitRadiusMin = 3f;
        private float _orbitRadiusMax = 6f;
        private float _changeOrbitTime;
        private float _currentOrbitRadius;
        private bool _oppositeSpin;
        private Action<Vector2> _forceAction;
        private float _speed;

        public void Initialise(Transform target, Action<Vector2> forceAction, float speed, float orbitRadiusMin, float orbitRadiusMax = -1)
        {
            _forceAction = forceAction;
            _targetTransform = target;
            _orbitRadiusMin = orbitRadiusMin;
            _orbitRadiusMax = orbitRadiusMax < 0 ? _orbitRadiusMin : orbitRadiusMax;
            _oppositeSpin = Random.Range(0, 2) == 0;
            _speed = speed;
        }

        private float GetOrbitRadius()
        {
            _changeOrbitTime -= Time.deltaTime;
            if (!(_changeOrbitTime <= 0)) return _currentOrbitRadius;
            _changeOrbitTime = Random.Range(2, 4);
            _currentOrbitRadius = Random.Range(_orbitRadiusMin, _orbitRadiusMax);
            return _currentOrbitRadius;
        }

        public void SwitchSpin()
        {
            _oppositeSpin = !_oppositeSpin;
        }

        public void Update()
        {
            if (!CombatManager.IsCombatActive()) return;
            _targetPosition = _targetTransform == null ? _targetPosition : (Vector2) _targetTransform.position;
            Vector2 currentPosition = transform.position;
            Vector2 dirToTarget = _targetPosition - currentPosition;
            dirToTarget.Normalize();
            Vector2 tangent = new Vector2(-dirToTarget.y, dirToTarget.x);
            Vector2 pos = -dirToTarget * GetOrbitRadius() + _targetPosition;
            float mult = 2;
            if (_oppositeSpin) mult = -mult;
            pos += tangent * mult;
            Vector2 dirToTangent = (pos - currentPosition).normalized;
            _forceAction.Invoke(dirToTangent * _speed);
        }
    }
}