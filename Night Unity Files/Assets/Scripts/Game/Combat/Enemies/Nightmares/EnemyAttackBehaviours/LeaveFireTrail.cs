using Game.Combat.Misc;
using NUnit.Framework;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class LeaveFireTrail : MonoBehaviour
    {
        private Vector2 _lastPosition = Vector2.negativeInfinity;
        private const float Interval = 0.75f;
        private const float FirePauseTime = 0.1f;
        private float _currentTime;
        private CanTakeDamage _canTakeDamage;
        private bool _initialised;

        public void Initialise()
        {
            _initialised = true;
            _lastPosition = transform.position;
            _canTakeDamage = GetComponent<CanTakeDamage>();
        }

        public void Update()
        {
            Assert.IsTrue(_initialised);
            _currentTime += Time.deltaTime;
            if (_currentTime < FirePauseTime) return;
            Vector2 currentPosition = transform.position;
            if (_lastPosition == currentPosition) return;
            float distance = Vector2.Distance(_lastPosition, currentPosition);
            Vector2 tempPos = _lastPosition;

            for (float i = Interval; i < distance; i += Interval)
            {
                float lerpVal = i / distance;
                tempPos = Vector2.Lerp(_lastPosition, currentPosition, lerpVal);
                TrailFireBehaviour.Create(tempPos).AddIgnoreTarget(_canTakeDamage);
            }

            _lastPosition = tempPos;
        }
    }
}