using System;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.CooldownSystem
{
    public class Cooldown
    {
        private float _startTime;
        public float Duration;
        private Action _startOfCooldown, _endOfCooldown, _cancelCooldown;
        private Action<float> _duringCooldown;
        private CooldownManager _manager;
        private bool _finished, _started;
        protected CooldownController Controller;
        
        public Cooldown(CooldownManager manager, float duration = 0)
        {
            Duration = duration;
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

        public virtual void SetController(CooldownController controller)
        {
            Controller = controller;
        }

        public void SetStartAction(Action a) => _startOfCooldown = a;

        public void SetEndAction(Action a, bool isCancelAction = false)
        {
            if (isCancelAction) _cancelCooldown = a;
            _endOfCooldown = a;
        }

        public void SetCancelAction(Action a) => _cancelCooldown = a;
        public void SetDuringAction(Action<float> a) => _duringCooldown = a;

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
            _cancelCooldown?.Invoke();
            _manager.RemoveCooldown(this);
        }

        public void Update()
        {
            float elapsed = Time.time - _startTime;
            if (elapsed >= Duration)
            {
                _finished = true;
                _started = false;
                Controller?.Reset();
                _endOfCooldown?.Invoke();
                return;
            }
            _duringCooldown?.Invoke(Duration - elapsed);
            Controller?.UpdateCooldownFill(1 - (Duration - elapsed) / Duration);
        }
    }
}