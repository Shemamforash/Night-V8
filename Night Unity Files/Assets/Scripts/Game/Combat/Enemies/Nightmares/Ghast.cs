using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Misc;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Ghast : EnemyBehaviour
    {
        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            gameObject.AddComponent<Teleport>().Initialise(5);
            gameObject.AddComponent<Bombardment>().Initialise(7, 3, 4);
            CurrentAction = Attack;
        }

        private void Attack()
        {
            MoveBehaviour.GoToCell(((CharacterCombat)GetTarget()).CurrentCell(), 5f, 2f);
            CurrentAction = Attack;
        }
    }
}