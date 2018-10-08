using Game.Combat.Misc;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class LeaveFireTrail : MonoBehaviour
    {
        private Vector2 _lastPosition = Vector2.negativeInfinity;
        private const float Interval = 0.4f;
        private const float FirePauseTime = 0.1f;
        private float _currentTime;
        private float _lifeTime;

        public void Initialise(float lifeTime = 2f)
        {
            _lastPosition = transform.position;
            _lifeTime = lifeTime;
        }

        public void Update()
        {
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
                FireBehaviour.Create(tempPos, 0.25f, _lifeTime, false, false).AddIgnoreTarget(GetComponent<CanTakeDamage>());
            }

            _lastPosition = tempPos;
        }
    }
}