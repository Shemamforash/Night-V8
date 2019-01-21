using System.Collections.Generic;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Ghast : NightmareEnemyBehaviour
    {
        private Vector2 _targetPos = Vector2.zero;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            if (WorldState.Difficulty() > 15) gameObject.AddComponent<Teleport>().Initialise(5);
            int projectiles = (int) (WorldState.Difficulty() / 5f + 2);
            gameObject.AddComponent<Bombardment>().Initialise(projectiles, 4, 3);
            CurrentAction = Attack;
        }

        private void Attack()
        {
            if (_targetPos.Distance(transform.position) < 0.5f) GetRandomPointAroundPlayer();
            Vector2 direction = _targetPos - (Vector2) transform.position;
            MovementController.Move(direction.normalized);
        }

        private void GetRandomPointAroundPlayer()
        {
            float angle = 0;
            List<Vector2> positions = new List<Vector2>();
            Vector2 playerPos = PlayerCombat.Position();
            while (angle < 360)
            {
                angle += Random.Range(20, 50);
                Vector2 pos = AdvancedMaths.CalculatePointOnCircle(angle, Random.Range(1.5f, 4f), playerPos);
                positions.Add(pos);
            }

            float maxDistance = 10000;
            positions.ForEach(p =>
            {
                float distance = p.Distance(playerPos);
                if (distance > maxDistance) return;
                maxDistance = distance;
                _targetPos = p;
            });
        }
    }
}