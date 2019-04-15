using Game.Combat.Misc;
using Game.Global;
using UnityEngine;

namespace Game.Combat.Enemies.Humans
{
    public class Martyr : UnarmedBehaviour
    {
        private const float ExplosionDamage = 25;
        private bool _detonated, _dontKill;
        private const float MinExplodeDistance = 0.5f;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            HealthController.SetOnKill(Detonate);
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            if (DistanceToTarget() > MinExplodeDistance) return;
            DetonateWithSkill();
        }

        private void DetonateWithSkill()
        {
            if (_detonated) return;
            _detonated = true;
            _dontKill = true;
            Vector2 currentPosition = transform.position;
            SkillAnimationController.Create(transform, "Martyr", 0.5f, () =>
            {
                Explosion.CreateExplosion(currentPosition, 2).InstantDetonate();
                _dontKill = false;
                Kill();
            });
        }

        public override void Kill()
        {
            if (_dontKill) return;
            Detonate();
            base.Kill();
        }

        private void Detonate()
        {
            if (_detonated) return;
            _detonated = true;
            Vector2 currentPosition = transform.position;
            Explosion.CreateExplosion(currentPosition).InstantDetonate();
        }
    }
}