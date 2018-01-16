using Game.Characters;
using UnityEngine;

namespace Game.Combat
{
    public class Bleed : Condition
    {
        public Bleed(Character character) : base(character, 2, 1)
        {
        }

        public override void Update()
        {
            if (StackList.Count != 0)
            {
                for (int i = StackList.Count - 1; i >= 0; --i)
                {
                    StackList[i] -= Time.deltaTime;
                    if (StackList[i] > 0)
                    {
                        CharacterHealth.TakeDamage(Damage);
                        continue;
                    }

                    StackList.RemoveAt(i);
                }
            }
            base.Update();
        }
    }
}