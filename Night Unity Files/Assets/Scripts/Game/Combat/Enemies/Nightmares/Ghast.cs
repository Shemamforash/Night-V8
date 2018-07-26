using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
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
            MoveBehaviour.GoToCell(GetTarget().CurrentCell(), Random.Range(2f, 5f));
            CurrentAction = Attack;
        }
    }
}