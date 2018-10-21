using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Misc;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Revenant : NightmareEnemyBehaviour
    {
        private Split _split;
        private LeaveFireTrail _fireTrail;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            _split = gameObject.AddComponent<Split>();
            _fireTrail = gameObject.AddComponent<LeaveFireTrail>();
            gameObject.AddComponent<Orbit>().Initialise(GetTarget().transform, v => MovementController.Move(v), 1, 0.5f, 4f);
            _fireTrail.Initialise();
            _split.Initialise(3, 200, EnemyType.Revenant, 1000, -1, true);
        }

        public override void Kill()
        {
            base.Kill();
            int newHealth = (int) (HealthController.GetMaxHealth() / 3f);
            if (newHealth < 3) return;
            _split.LastSplitEnemies().ForEach(e =>
            {
                Revenant r = e as Revenant;
                if (r == null) return;
                r.HealthController.SetInitialHealth(newHealth, r);
                r._fireTrail.Initialise();
            });
        }
    }
}