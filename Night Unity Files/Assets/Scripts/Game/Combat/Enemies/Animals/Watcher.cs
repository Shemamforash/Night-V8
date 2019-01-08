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
            bool outOfSight = Physics2D.Linecast(transform.position, PlayerCombat.Position(), 1 << 8).collider != null;
            if (outOfSight) return;
            bool outOfRange = PlayerCombat.Instance.transform.Distance(transform.position) < DetectionRange;
            if (outOfRange) return;
            Alert();
        }

        public override void Alert()
        {
            if (!Alerted)
            {
                Cell target = PathingGrid.GetCellOutOfRange(transform.position);
                MoveBehaviour.GoToCell(target);
            }

            Alerted = true;
            CombatManager.Enemies().ForEach(e =>
            {
                if (!(e is Grazer enemy)) return;
                if (enemy.Alerted) return;
                float distance = TargetTransform().Distance(enemy.transform);
                if (distance > DetectionRange) return;
                enemy.Alert();
            });
        }
    }
}