using System.Linq;
using Game.Characters;
using UnityEngine;

namespace Game.Combat.Skills
{
    public class Sickness : Condition
    {
        private int _criticalStackCount = 10;
        private float _timeSinceLastHit = 0;

        public Sickness(Character character) : base(character, 1, 0)
        {
        }

        public override void Update()
        {
            if (StackList.Count != 0)
            {
                StackList[0] -= Time.deltaTime;
                if (StackList[0] <= 0)
                {
                    StackList.RemoveAt(0);
                }
            }
            base.Update();
        }

        public override void AddStack()
        {
            base.AddStack();
            if (StackList.Count == _criticalStackCount)
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
            return currentValue / _criticalStackCount;
        }
    }
}