using System.Collections;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Heavyshot : TimedAttackBehaviour
    {
        private static GameObject _particlesPrefab;
        private ParticleSystem _particles;

        public override void Awake()
        {
            base.Awake();
            if (_particlesPrefab == null) _particlesPrefab = Resources.Load<GameObject>("Prefabs/Combat/Shot Charge Particles");
            GameObject shotParticles = Instantiate(_particlesPrefab);
            shotParticles.transform.SetParent(transform);
            shotParticles.transform.localPosition = Vector3.zero;
            _particles = shotParticles.GetComponent<ParticleSystem>();
        }

        protected override void Attack()
        {
            StartCoroutine(FireMaelstromShot());
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

            MaelstromShotBehaviour.Create(Enemy.GetTarget().transform.position - transform.position, transform.position);
            UnpauseOthers();
        }
    }
}