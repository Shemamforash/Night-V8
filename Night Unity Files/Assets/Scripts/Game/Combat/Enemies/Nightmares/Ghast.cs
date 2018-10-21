using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Ghast : NightmareEnemyBehaviour
    {
        private float _distanceToTarget;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            gameObject.AddComponent<Teleport>().Initialise(5);
            gameObject.AddComponent<Bombardment>().Initialise(7, 3, 4);
            _distanceToTarget = Random.Range(2f, 5f);
            CurrentAction = Attack;
        }

        private void Attack()
        {
            float distanceToTarget = transform.position.Distance(GetTarget().transform.position);
            if (distanceToTarget <= _distanceToTarget) return;
            Vector2 direction = PlayerCombat.Instance.transform.position - transform.position;
            MovementController.Move(direction.normalized);
        }
    }
}