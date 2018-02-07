using Game.Characters;
using UnityEngine;

namespace Game.Combat
{
    public class Burn : Condition
    {
        public Burn(Character character) : base(character, 5, 4)
        {
        }

        protected override void Tick()
        {
            if (StackList.Count == 0) return;
            int totalBurnDamage = 0;
            int stackMaxDamage = Damage;
            for (int i = StackList.Count - 1; i >= 0; --i)
            {
                StackList[i] -= Time.deltaTime;
                if (StackList[i] > 0)
                {
                    totalBurnDamage += stackMaxDamage;
                    stackMaxDamage /= 2;
                    continue;
                }

                StackList.RemoveAt(i);
            }
            CharacterHealth.TakeDamage(totalBurnDamage);
        }
    }
}