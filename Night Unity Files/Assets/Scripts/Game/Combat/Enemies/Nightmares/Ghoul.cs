using System.Collections;
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
            gameObject.AddComponent<LifeDrain>();
        }

        private void SeekPlayer()
        {
            Vector2 direction = CombatManager.Player().transform.position - transform.position;
            Move(direction.normalized);
        }

        public override void Update()
        {
            base.Update();
            if (DistanceToTarget() > _distanceToTouch) return;
            GetTarget().Sicken();
            Kill();
        }
    }
}