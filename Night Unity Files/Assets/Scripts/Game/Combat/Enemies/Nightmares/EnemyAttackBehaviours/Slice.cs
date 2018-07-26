using System.Collections;
using Game.Combat.Player;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Slice : TimedAttackBehaviour
    {
        private Collider2D _collider;
        private static GameObject _sliceTrailPrefab;
        private ParticleSystem _sliceParticles;
        private TrailRenderer _pathTrail;

        public override void Awake()
        {
            base.Awake();
            _collider = GetComponent<Collider2D>();
            if (_sliceTrailPrefab == null) _sliceTrailPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Slice Trail");
            GameObject sliceTrail = Instantiate(_sliceTrailPrefab);
            sliceTrail.transform.SetParent(transform, false);
            _sliceParticles = sliceTrail.GetComponent<ParticleSystem>();
            _pathTrail = GetComponent<TrailRenderer>();
        }

        protected override void Attack()
        {
            Vector2 dir = PlayerCombat.Instance.transform.position - transform.position;
            float distance = dir.magnitude;
            dir.Normalize();
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            Enemy.MovementController.AddForce(dir * distance * 500);
            StartCoroutine(DisableCollider());
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            PlayerCombat.Instance.HealthController.TakeDamage(5);
        }
        
        private IEnumerator DisableCollider()
        {
            PauseOthers();
            ParticleSystem.EmissionModule emission = _sliceParticles.emission;
            emission.rateOverTime = 100;
            _pathTrail.enabled = false;
            _collider.isTrigger = true;
            float disableTime = 0.25f;
            while (disableTime > 0f)
            {
                disableTime -= Time.deltaTime;
                yield return null;
            }
            UnpauseOthers();
            emission = _sliceParticles.emission;
            emission.rateOverTime = 0;
            _pathTrail.Clear();
            _pathTrail.enabled = true;
            _collider.isTrigger = false;
        }
    }
}