using System.Collections;
using Game.Combat.Generation;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Feed : TimedAttackBehaviour
    {
        private float _distanceToPlayer;
        private const float MinDistanceToAttack = 4f;
        private const float AttackTime = 5f;
        private bool _attacking;
        private static GameObject _feedParticlePrefab;
        private ParticleSystem _feedParticles;
        private const float MaxEmission = 75;

        public override void Update()
        {
            _distanceToPlayer = transform.position.Distance(PlayerCombat.Instance.transform.position);
            if (_distanceToPlayer > MinDistanceToAttack) return;
            if (_attacking) return;
            base.Update();
        }

        private void UpdateFeedParticles()
        {
            float maxSpeed = _distanceToPlayer;
            float minSpeed = _distanceToPlayer / 2f;
            float rotation = AdvancedMaths.AngleFromUp(PlayerCombat.Instance.transform.position, transform.position);
            rotation += 90;
            _feedParticles.transform.rotation = Quaternion.Euler(0, 0, rotation);
            ParticleSystem.MainModule main = _feedParticles.main;
            ParticleSystem.MinMaxCurve startSpeed = main.startSpeed;
            startSpeed.constantMin = minSpeed;
            startSpeed.constantMax = maxSpeed;
        }

        private void SetEmissionRate(float rate)
        {
            float emissionRate = rate * MaxEmission;
            ParticleSystem.EmissionModule emission = _feedParticles.emission;
            emission.rateOverTime = emissionRate;
        }

        private void LateUpdate()
        {
            if (_feedParticles == null) return;
            _feedParticles.transform.position = PlayerCombat.Instance.transform.position;
        }

        private IEnumerator DoFeed()
        {
            if (_feedParticlePrefab == null) _feedParticlePrefab = Resources.Load<GameObject>("Prefabs/Combat/Effects/Life Draw");
            _feedParticles = Instantiate(_feedParticlePrefab).GetComponent<ParticleSystem>();
            _feedParticles.transform.SetAsDynamicChild();

            float currentTime = 0f;
            _attacking = true;
            while (currentTime < AttackTime)
            {
                if (!CombatManager.IsCombatActive()) yield return null;
                currentTime += Time.deltaTime;
                UpdateFeedParticles();
                SetEmissionRate(currentTime <= 1 ? currentTime : 1);
                PlayerCombat.Instance.ReduceAdrenaline(1 * Time.deltaTime);
                if (_distanceToPlayer > MinDistanceToAttack) break;
                yield return null;
            }

            SetEmissionRate(0);
            while (_feedParticles.particleCount > 0) yield return null;
            Destroy(_feedParticles.gameObject);
            _attacking = false;
        }

        protected override void Attack()
        {
            StartCoroutine(DoFeed());
        }
    }
}