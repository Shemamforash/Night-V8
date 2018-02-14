using Game.Characters;
using UnityEngine;

namespace Game.Combat
{
    public class Bleed : Condition
    {
        public Bleed(CharacterCombat character) : base(character, 2, 1)
        {
        }

        protected override void Tick()
        {
            if (StackList.Count == 0) return;
            for (int i = StackList.Count - 1; i >= 0; --i)
            {
                StackList[i] -= 1;
                if (StackList[i] > 0)
                {
                    CharacterHealth.TakeDamage(Damage);
                    if (StackList.Count == 0) break;
                    continue;
                }

                RemoveStack(i);
            }
        }
    }
}