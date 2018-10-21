using SamsHelper.Libraries;
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
            Vector2 position = AdvancedMaths.RandomDirection() * Random.Range(2f, 5f) + (Vector2)transform.position;
            Vector2 dir = position - (Vector2)transform.position;
            dir.Normalize();
            Enemy.MovementController.AddForce(dir * Enemy.Enemy.Template.Speed * 50);
        }
    }
}