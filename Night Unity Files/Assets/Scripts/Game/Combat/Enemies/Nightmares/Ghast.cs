using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Player;
using Game.Global;
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
            if(WorldState.Difficulty() > 15) gameObject.AddComponent<Teleport>().Initialise(5);
            gameObject.AddComponent<Bombardment>().Initialise(7, 3, 4);
            _distanceToTarget = Random.Range(2f, 5f);
            CurrentAction = Attack;
        }

        private void Attack()
        {
            float distanceToTarget = transform.position.Distance(GetTarget().transform.position);
            if (distanceToTarget <= _distanceToTarget) return;
            Vector2 direction = PlayerCombat.Position() - transform.position;
            MovementController.Move(direction.normalized);
        }
    }
}