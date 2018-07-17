using System.Collections;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Push : TimedAttackBehaviour
    {
        private ParticleSystem _pushParticles;
        private GameObject _pushPrefab;
        
        public void Start()
        {
            Initialise(2f);
            if (_pushPrefab == null) _pushPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Push Burst");
            GameObject pushObject = Instantiate(_pushPrefab);
            pushObject.transform.SetParent(transform, false);
            _pushParticles = pushObject.GetComponent<ParticleSystem>();
        }
        
        protected override void Attack()
        {
            float angle = AdvancedMaths.AngleFromUp(transform.position, PlayerCombat.Instance.transform.position);
            _pushParticles.transform.rotation = Quaternion.Euler(0, 0, angle + 80f);
            _pushParticles.Emit(50);
            StartCoroutine(CheckHit());
        }

        private IEnumerator CheckHit()
        {
            float lifeTime = _pushParticles.main.startLifetime.constant;
            while (lifeTime > 0)
            {
                lifeTime -= Time.deltaTime;
                yield return null;
            }
        }
    }
}