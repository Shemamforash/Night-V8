using System.Linq;
using Game.Characters;
using UnityEngine;

namespace Game.Combat.Skills
{
    public class Sickness : Condition
    {
        public static int MaxStacks = 10;
        private float _timeSinceLastHit = 0;

        public Sickness(CharacterCombat character) : base(character, 1, 0)
        {
        }

        protected override void Tick()
        {
            if (StackList.Count == 0) return;
            StackList[0] -= 1;
            if (StackList[0] <= 0)
            {
                RemoveStack(0);
            }
        }

        public override void AddStack()
        {
            base.AddStack();
            if (StackList.Count == MaxStacks)
            {
                CharacterHealth.TakeDamage((int) (CharacterHealth.GetMaxHealth() * 0.25f));
                Clear();
                return;
            }
            for (int i = 0; i < StackList.Count; ++i)
            {
                StackList[i] = Duration;
            }
        }

        public float GetNormalisedValue()
        {
            float currentValue = StackList.Sum();
            return currentValue / MaxStacks;
        }
    }
}