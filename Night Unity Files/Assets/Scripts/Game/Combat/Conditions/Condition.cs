using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public abstract class Condition
    {
        protected readonly int Duration, Damage;
        protected readonly List<float> StackList = new List<float>();
        protected readonly HealthController CharacterHealth;
        public Action OnConditionEmpty, OnConditionNonEmpty;
        private float _timeToNextTick;

        protected Condition(CharacterCombat character, int duration, int damage)
        {
            CharacterHealth = character.HealthController;
            Duration = duration;
            Damage = damage;
        }

        public bool Active()
        {
            return StackList.Count != 0;
        }

        public int Size()
        {
            return StackList.Count;
        }
        
        public void Update()
        {
            _timeToNextTick -= Time.deltaTime;
            if (_timeToNextTick > 0) return;
            _timeToNextTick = 1 + _timeToNextTick;
            Tick();
        }

        public void RemoveStack(int index)
        {
            StackList.RemoveAt(index);
            if (StackList.Count == 0)
            {
                OnConditionEmpty?.Invoke();
            }
        }

        protected abstract void Tick();

        public void AddStacks(int stacks)
        {
            for (int i = 0; i < stacks; ++i)
            {
                AddStack();
            }
        }

        public virtual void AddStack()
        {
            StackList.Add(Duration);
            if (StackList.Count == 1)
            {
                _timeToNextTick = 1f;
            }
            OnConditionNonEmpty?.Invoke();
        }

        public void Clear()
        {
            StackList.Clear();
            OnConditionEmpty?.Invoke();
        }
    }
}