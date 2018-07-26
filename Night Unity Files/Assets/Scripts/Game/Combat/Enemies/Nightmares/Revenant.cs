using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Revenant : EnemyBehaviour
    {
        private Split _split;
        private LeaveFireTrail _fireTrail;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            _split = gameObject.AddComponent<Split>();
            _fireTrail = gameObject.AddComponent<LeaveFireTrail>();
            _fireTrail.Initialise();
            _split.Initialise(3, 200, EnemyType.Revenant, 1000, -1, true);
            CurrentAction = Orbit;
        }

        private void Orbit()
        {
            Cell target = PathingGrid.GetCellOrbitingTarget(CurrentCell(), GetTarget().CurrentCell(), GetComponent<Rigidbody2D>().velocity, 4f, 0.5f);
            MoveBehaviour.GoToCell(target, Orbit);
        }

        public override void Kill()
        {
            base.Kill();
            int newHealth = (int) (HealthController.GetMaxHealth() / 3f);
            if (newHealth < 3) return;
            _split.LastSplitEnemies().ForEach(e =>
            {
                Revenant r = (Revenant) e;
                r.HealthController.SetInitialHealth(newHealth, r);
                r._fireTrail.Initialise();
            });
        }
    }
}