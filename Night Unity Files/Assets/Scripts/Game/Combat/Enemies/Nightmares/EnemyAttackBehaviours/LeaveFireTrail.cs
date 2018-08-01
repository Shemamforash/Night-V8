using Game.Combat.Misc;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class LeaveFireTrail : MonoBehaviour
    {
        private Vector2 _lastPosition = Vector2.negativeInfinity;
        private const float Interval = 1f;
        private const float FirePauseTime = 0.1f;
        private float _currentTime;

        public void Initialise()
        {
            _lastPosition = transform.position;
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
                FireBehaviour.Create(tempPos, 0.2f, false, false);
            }

            _lastPosition = tempPos;
        }
    }
}