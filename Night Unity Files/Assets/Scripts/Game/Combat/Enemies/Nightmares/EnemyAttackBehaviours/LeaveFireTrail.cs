using System.Collections.Generic;
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
        private bool _initialised;
        private readonly List<CanTakeDamage> _ignoreTargets = new List<CanTakeDamage>();

        public void Initialise()
        {
            _initialised = true;
            _lastPosition = transform.position;
            _ignoreTargets.Add(GetComponent<CanTakeDamage>());
        }

        public void Update()
        {
            Assert.IsTrue(_initialised);
            _currentTime += Time.deltaTime;
            if (_currentTime < FirePauseTime) return;
            Vector2 currentPosition = transform.position;
            if (_lastPosition == currentPosition) return;
            float distance = Vector2.Distance(_lastPosition, currentPosition);
            if (distance < 1)
            {
                Vector2 tempPos = _lastPosition;
                for (float i = Interval; i < distance; i += Interval)
                {
                    float lerpVal = i / distance;
                    tempPos = Vector2.Lerp(_lastPosition, currentPosition, lerpVal);
                    _ignoreTargets.RemoveAll(c => c == null || c.HealthController.GetCurrentHealth() == 0);
                    TrailFireBehaviour.Create(tempPos).AddIgnoreTargets(_ignoreTargets);
                }

                _lastPosition = tempPos;
            }
            else
            {
                _lastPosition = transform.position;
            }
        }

        public void AddIgnoreTargets<T>(List<T> ignoreTargets) where T : CanTakeDamage
        {
            _ignoreTargets.AddRange(ignoreTargets);
        }
    }
}