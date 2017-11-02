using System;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.CooldownSystem
{
    public class Cooldown
    {
        private readonly float _startTime, _duration;
        private readonly Action _endOfCooldown;
        private readonly Action<float> _duringCooldown;
        private CooldownManager _manager;
        private bool _finished;

        public Cooldown(CooldownManager manager, float duration, Action endOfCooldown)
        {
            _startTime = Time.time;
            _duration = duration;
            _endOfCooldown = endOfCooldown;
            _manager = manager;
            manager.RegisterCooldown(this);
        }

        public Cooldown(CooldownManager manager, float duration, Action endOfCooldown, Action<float> duringCooldown) : this(manager, duration, endOfCooldown)
        {
            _duringCooldown = duringCooldown;
        }

        public bool IsFinished()
        {
            return _finished;
        }

        public void Cancel()
        {
            _manager.RemoveCooldown(this);
        }

        public bool Update()
        {
            float elapsed = Time.time - _startTime;
            if (elapsed >= _duration)
            {
                _finished = true;
                _endOfCooldown();
                return true;
            }
            _duringCooldown?.Invoke(_duration - elapsed);
            return false;
        }
    }
}