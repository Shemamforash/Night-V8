using Game.Combat.Misc;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Humans
{
    public class Brawler : UnarmedBehaviour
    {
        private const float MinMeleeDistance = 0.5f;
        private const int MeleeDamage = 5;
        private const float MeleeForce = 20;
        private ParticleSystem _slashParticles;
        private static GameObject _prefab;
        private float _meleeTime;
        private const float MaxMeleeTime = 0.25f;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Brawler Particles");
            GameObject slashObject = Instantiate(_prefab);
            _slashParticles = slashObject.GetComponent<ParticleSystem>();
            slashObject.transform.SetParent(transform);
            slashObject.transform.localPosition = Vector2.zero;
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            if (DistanceToTarget() > MinMeleeDistance) return;
            StrikePlayer();
        }

        private void StrikePlayer()
        {
            if (_meleeTime > 0f)
            {
                _meleeTime -= Time.deltaTime;
                return;
            }

            _meleeTime = MaxMeleeTime * Random.Range(0.8f, 1.2f);
            ParticleSystem.MainModule main = _slashParticles.main;
            int flip = Random.Range(0, 2);
            main.flipRotation = flip;
            main.startRotation = AdvancedMaths.AngleFromUp(transform.position, TargetPosition());
            Vector2 direction = transform.Direction(TargetPosition());
            float x = -direction.y;
            float y = direction.x;
            if (flip == 1)
            {
                x = -x;
                y = -x;
            }

            direction.x = x;
            direction.y = y;
            _slashParticles.Emit(1);
            GetTarget().TakeRawDamage(MeleeDamage, direction);
            direction = TargetPosition().Direction(transform);
            ((CharacterCombat) GetTarget()).MovementController.KnockBack(direction, MeleeForce);
        }
    }
}