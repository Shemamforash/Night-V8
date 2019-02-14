using System.Collections;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Heavyshot : TimedAttackBehaviour
    {
        private static GameObject _particlesPrefab;
        private ParticleSystem _particles;
        private bool _firing = true;
        private float _speed;
        private float _offset;
        private float _damageModifier = 1;

        public void Initialise(float maxTimer, float minTimer, float speed, float offset)
        {
            Initialise(maxTimer, minTimer);
            _speed = speed;
            _offset = offset;
        }

        public override void Awake()
        {
            base.Awake();
            if (_particlesPrefab == null) _particlesPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Shot Charge Particles");
            GameObject shotParticles = Instantiate(_particlesPrefab, transform, true);
            shotParticles.transform.localPosition = Vector3.zero;
            _particles = shotParticles.GetComponent<ParticleSystem>();
        }

        protected override void Attack()
        {
            if (!_firing) return;
            StartCoroutine(FireMaelstromShot());
        }

        public void SetFiring(bool firing)
        {
            _firing = firing;
        }

        private IEnumerator FireMaelstromShot()
        {
            if (_particles == null) yield break;
            PauseOthers();
            _particles.Play();
            float shotTime = _particles.main.duration + 0.5f;
            while (shotTime > 0)
            {
                shotTime -= Time.deltaTime;
                yield return null;
            }

            float rotation = -transform.rotation.z;
            Vector2 direction = AdvancedMaths.CalculatePointOnCircle(rotation, 1f, Vector2.zero);
            MaelstromShotBehaviour shot = MaelstromShotBehaviour.Create(direction, (Vector2) transform.position + direction * _offset, _speed, false);
            shot.SetDamageModifier(_damageModifier);
            UnpauseOthers();
        }

        public void SetDamageModifier(float damageModifier)
        {
            _damageModifier = damageModifier;
        }
    }
}