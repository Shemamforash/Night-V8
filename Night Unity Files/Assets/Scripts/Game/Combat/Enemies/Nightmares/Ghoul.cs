using System.Collections;
using System.Collections.Generic;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Ghoul : EnemyBehaviour
    {
        private float _distanceToTouch = 0.5f;
        
        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            CurrentAction = SeekPlayer;
            gameObject.AddComponent<FeedTarget>();
        }

        private void SeekPlayer()
        {
            Vector2 direction = CombatManager.Player().transform.position - transform.position;
            Move(direction.normalized);
        }

        public override void Update()
        {
            base.Update();
            List<CharacterCombat> chars = CombatManager.GetCharactersInRange(transform.position, 1f);
            Vector2 forceDir = Vector2.zero;
            chars.ForEach(c =>
            {
                if (c == this) return;
                Vector2 dir = c.transform.position - transform.position;
                float force = 1 / dir.magnitude;
                forceDir += -dir * force;
            });
            GetComponent<Rigidbody2D>().AddForce(forceDir);
            if (DistanceToTarget() > _distanceToTouch) return;
            GetTarget().Sicken();
            Kill();
        }
    }
}