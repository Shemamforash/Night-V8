using Game.Combat.Misc;
using UnityEngine;

namespace Game.Combat.Enemies.Humans
{
    public class Martyr : UnarmedBehaviour
    {
        private bool _detonated;
        private const float MinExplodeDistance = 0.2f;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            HealthController.SetOnKill(Detonate);
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            if (!Alerted) return;
            if (DistanceToTarget() > MinExplodeDistance) return;
            Kill();
        }

        public override void Kill()
        {
            Detonate();
            base.Kill();
        }

        private void Detonate()
        {
            if (_detonated) return;
            _detonated = true;
            SkillAnimationController.Create(transform, "Martyr", 0.5f, () =>
            {
                Debug.Log("Fart" + transform.position);
                Explosion.CreateExplosion(transform.position, 50, 2).InstantDetonate();
            });
        }
    }
}