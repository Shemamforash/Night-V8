using System;
using System.Collections.Generic;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.CooldownSystem
{
    public class Cooldown
    {
        private Action<float> _duringCooldown;
        private bool _finished, _started;
        private readonly CooldownManager _manager;
        private Action _startOfCooldown, _endOfCooldown, _cancelCooldown;
        private float _startTime;
        private readonly List<CooldownController> Controllers = new List<CooldownController>();
        public float Duration;

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
            Controllers.Add(controller);
        }

        public void SetStartAction(Action a)
        {
            _startOfCooldown = a;
        }

        public void SetEndAction(Action a, bool isCancelAction = false)
        {
            if (isCancelAction) _cancelCooldown = a;
            _endOfCooldown = a;
        }

        public void SetCancelAction(Action a)
        {
            _cancelCooldown = a;
        }

        public void SetDuringAction(Action<float> a)
        {
            _duringCooldown = a;
        }

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
        }

        public void Update()
        {
            float elapsed = Time.time - _startTime;
            if (elapsed >= Duration)
            {
                _finished = true;
                _started = false;
                Controllers.ForEach(c => c.Reset());
                _endOfCooldown?.Invoke();
                return;
            }

            _duringCooldown?.Invoke(Duration - elapsed);
            float normalisedDuration = 1 - (Duration - elapsed) / Duration;
        }
    }
}