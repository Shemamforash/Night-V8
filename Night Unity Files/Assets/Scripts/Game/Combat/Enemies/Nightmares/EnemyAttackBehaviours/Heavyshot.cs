using System.Collections;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Heavyshot : TimedAttackBehaviour
    {
        private static GameObject _particlesPrefab;
        private ParticleSystem _particles;
        private bool _firing;
        private float _speed;
        private float _offset;

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
            GameObject shotParticles = Instantiate(_particlesPrefab);
            shotParticles.transform.SetParent(transform);
            shotParticles.transform.localPosition = Vector3.zero;
            _particles = shotParticles.GetComponent<ParticleSystem>();
        }

        protected override void Attack()
        {
            Debug.Log("attacking");
            if (!_firing) return;
            StartCoroutine(FireMaelstromShot());
        }

        public void SetFiring(bool firing)
        {
            _firing = firing;
        }

        private IEnumerator FireMaelstromShot()
        {
            PauseOthers();
            _particles.Play();
            float shotTime = _particles.main.duration + 0.5f;
            while (shotTime > 0)
            {
                shotTime -= Time.deltaTime;
                yield return null;
            }

            Vector3 direction = GetComponent<Rigidbody2D>().velocity;
            MaelstromShotBehaviour.Create(direction, transform.position + direction * _offset, _speed);
            UnpauseOthers();
        }
    }
}