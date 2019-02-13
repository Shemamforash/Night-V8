using System.Collections.Generic;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class NightmareEnemyBehaviour : EnemyBehaviour
    {
        private Vector2 _targetPos = Vector2.zero;
        
        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            gameObject.layer = 24;
        }

        protected void Move()
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