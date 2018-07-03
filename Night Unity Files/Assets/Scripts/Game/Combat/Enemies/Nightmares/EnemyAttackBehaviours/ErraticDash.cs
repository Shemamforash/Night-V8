using Game.Combat.Generation;
using Game.Combat.Player;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class ErraticDash : TimedAttackBehaviour
    {
        public void Start()
        {
            Initialise(0.4f, 0.6f);
        }

        protected override void Attack()
        {
            Cell c = PathingGrid.GetCellOrbitingTarget(Enemy.CurrentCell(), PlayerCombat.Instance.CurrentCell(), transform.forward, 5, 2);
            Vector2 dir = c.Position - (Vector2)transform.position;
            dir.Normalize();
            Enemy.AddForce(dir * Enemy.Speed * 50);
        }
    }
}