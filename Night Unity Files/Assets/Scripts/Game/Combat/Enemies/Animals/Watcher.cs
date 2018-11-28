using Game.Combat.Enemies.Nightmares;
using Game.Combat.Generation;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Animals
{
    public class Watcher : AnimalBehaviour
    {
        protected override void UpdateDistanceToTarget()
        {
            base.UpdateDistanceToTarget();
            bool outOfSight = Physics2D.Linecast(transform.position, PlayerCombat.Instance.transform.position, 1 << 8).collider != null;
            if (outOfSight) return;
            bool outOfRange = PlayerCombat.Instance.transform.Distance(transform.position) < DetectionRange;
            if (outOfRange) return;
            Alert(true);
        }

        public override void Alert(bool alertOthers)
        {
            Debug.Log("alerted");
            if (Alerted) return;
            Alerted = true;
            if (!alertOthers) return;
            CombatManager.Enemies().ForEach(e =>
            {
                Grazer enemy = e as Grazer;
                if (enemy == this || enemy == null) return;
                float distance = TargetTransform().Distance(enemy.transform);
                if (distance > DetectionRange) return;
                enemy.Alert(false);
            });
        }
    }
}