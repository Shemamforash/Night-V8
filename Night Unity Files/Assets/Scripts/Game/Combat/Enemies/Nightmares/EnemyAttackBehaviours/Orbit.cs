using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Orbit : MonoBehaviour
    {
        private Transform _targetTransform;
        private Vector2 _targetPosition;
        private EnemyBehaviour _enemy;
        private float _orbitRadiusMin = 3f;
        private float _orbitRadiusMax = 6f;
        private float _changeOrbitTime;
        private float _currentOrbitRadius;
        private bool _oppositeSpin;

        public void Initialise(Transform target, float orbitRadiusMin, float orbitRadiusMax = -1)
        {
            _targetTransform = target;
            _orbitRadiusMin = orbitRadiusMin;
            _orbitRadiusMax = orbitRadiusMax < 0 ? _orbitRadiusMin : orbitRadiusMax;
            _enemy = GetComponent<EnemyBehaviour>();
            _oppositeSpin = Random.Range(0, 2) == 0;
        }

        private float GetOrbitRadius()
        {
            _changeOrbitTime -= Time.deltaTime;
            if (!(_changeOrbitTime <= 0)) return _currentOrbitRadius;
            _changeOrbitTime = Random.Range(2, 4);
            _currentOrbitRadius = Random.Range(_orbitRadiusMin, _orbitRadiusMax);
            return _currentOrbitRadius;
        }

        public void Update()
        {
            _targetPosition = _targetTransform == null ? _targetPosition : (Vector2) _targetTransform.position;
            Vector2 currentPosition = transform.position;
            Vector2 dirToTarget = _targetPosition - currentPosition;
            dirToTarget.Normalize();
            Vector2 tangeant = new Vector2(-dirToTarget.y, dirToTarget.x);
            Vector2 pos = -dirToTarget * GetOrbitRadius() + _targetPosition;
            float mult = 2;
            if (_oppositeSpin) mult = -mult;
            pos += tangeant * mult;
            Vector2 dirToTangeant = (pos - currentPosition).normalized;
            _enemy.MovementController.AddForce(dirToTangeant * 10f);
        }
    }
}