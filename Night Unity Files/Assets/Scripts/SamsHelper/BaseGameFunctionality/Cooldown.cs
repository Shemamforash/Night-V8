using System;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality
{
    public class Cooldown
    {
        private readonly float _startTime, _duration;
        private readonly Action _endOfCooldown;
        private readonly Action<float> _duringCooldown;

        public Cooldown(float duration, Action endOfCooldown)
        {
            _startTime = Time.time;
            _duration = duration;
            _endOfCooldown = endOfCooldown;
            CooldownManager.RegisterCooldown(this);
        }

        public Cooldown(float duration, Action endOfCooldown, Action<float> duringCooldown) : this(duration, endOfCooldown)
        {
            _duringCooldown = duringCooldown;
        }

        public bool Update()
        {
            float elapsed = Time.time - _startTime;
            if (elapsed >= _duration)
            {
                _endOfCooldown();
                return true;
            }
            if (_duringCooldown != null)
            {
                _duringCooldown(_duration - elapsed);
            }
            return false;
        }
    }
}