using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Characters.Player;

namespace Game.Combat
{
    public abstract class Condition
    {
        protected readonly int Duration, Damage;
        protected readonly List<float> StackList = new List<float>();
        protected readonly HealthController CharacterHealth;
        public Action OnConditionEmpty, OnConditionNonEmpty;

        protected Condition(Character character, int duration, int damage)
        {
            CharacterHealth = character.HealthController;
            Duration = duration;
            Damage = damage;
        }

        public virtual void Update()
        {
            if (StackList.Count == 0)
            {
                OnConditionEmpty?.Invoke();
            }
            else
            {
                OnConditionNonEmpty?.Invoke();
            }
        }

        public virtual void AddStack()
        {
            StackList.Add(Duration);
            OnConditionNonEmpty?.Invoke();
        }

        protected void Clear()
        {
            StackList.Clear();
            OnConditionEmpty?.Invoke();
        }
    }
}