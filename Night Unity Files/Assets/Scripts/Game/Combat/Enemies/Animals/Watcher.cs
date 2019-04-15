using Game.Combat.Enemies.Nightmares;
using Game.Combat.Generation;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Animals
{
    public class Watcher : AnimalBehaviour
    {
        private float _lastUsedSkill = 0f;

        protected override void UpdateDistanceToTarget()
        {
            if (_lastUsedSkill > 0f) _lastUsedSkill -= Time.deltaTime;
            base.UpdateDistanceToTarget();
            bool outOfSight = Physics2D.Linecast(transform.position, PlayerCombat.Position(), 1 << 8).collider != null;
            if (outOfSight) return;
            float distanceToPlayer = PlayerCombat.Instance.transform.Distance(transform.position);
            bool outOfRange = distanceToPlayer > DetectionRange;
            if (outOfRange) return;
            if (distanceToPlayer < 2f && _lastUsedSkill <= 0f)
            {
                PushController.Create(transform.position, 0, false, 360f);
                _lastUsedSkill = 5f;
            }

            Alert();
        }

        public override void Alert()
        {
            base.Alert();
            if (Alerted) return;
            Cell target = WorldGrid.GetEdgeCell(transform.position);
            MoveBehaviour.GoToCell(target);
            Alerted = true;
            CombatManager.Instance().Enemies().ForEach(e =>
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