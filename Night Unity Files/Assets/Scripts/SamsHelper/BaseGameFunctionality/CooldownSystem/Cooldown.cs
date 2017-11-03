using System;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.CooldownSystem
{
    public class Cooldown
    {
        private float _startTime;
        private float _duration;
        private Action _startOfCooldown, _endOfCooldown;
        private Action<float> _duringCooldown;
        private CooldownManager _manager;
        private bool _finished, _started;
        
        public Cooldown(CooldownManager manager, float duration = 0)
        {
            _duration = duration;
            _manager = manager;
        }

        public void Start()
        {
            _manager.RegisterCooldown(this);
            _startTime = Time.time;
            _startOfCooldown?.Invoke();
            _finished = false;
            _started = true;
        }
        
        public void Restart()
        {
            Start();
        }

        public void SetStartAction(Action a) => _startOfCooldown = a;
        public void SetEndAction(Action a) => _endOfCooldown = a;
        public void SetDuringAction(Action<float> a) => _duringCooldown = a;
        public void SetDuration(float duration) => _duration = duration;

        public bool Finished()
        {
            return _finished || !_started;
        }

        public bool Running()
        {
            return !_finished && _started;
        }

        public void Cancel()
        {
            _finished = true;
            _manager.RemoveCooldown(this);
        }

        public void Update()
        {
            float elapsed = Time.time - _startTime;
            if (elapsed >= _duration)
            {
                _finished = true;
                _started = false;
                _endOfCooldown?.Invoke();
                return;
            }
            _duringCooldown?.Invoke(_duration - elapsed);
        }
    }
}