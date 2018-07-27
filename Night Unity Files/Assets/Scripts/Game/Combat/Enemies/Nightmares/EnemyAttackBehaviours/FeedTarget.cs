﻿using System.Collections;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class FeedTarget : MonoBehaviour
    {
        private bool _drawingLife;
        private Feed _target;
        private ParticleSystem _particles;
        private EnemyBehaviour _enemy;
        private GameObject _dissolvePrefab;

        public void Awake()
        {
            if (_dissolvePrefab == null) _dissolvePrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Disappear");
            GameObject dissolveObject = Instantiate(_dissolvePrefab);
            dissolveObject.transform.SetParent(transform);
            dissolveObject.transform.position = transform.position;
            _particles = dissolveObject.GetComponent<ParticleSystem>();
            _enemy = GetComponent<EnemyBehaviour>();
        }

        public void StartDrawLife(Feed target)
        {
            if (_drawingLife) return;
            StartCoroutine(DrawLife(target));
        }

        private IEnumerator DrawLife(Feed target)
        {
            _target = target;
            _drawingLife = true;
            _particles.Play();
            float timePassed = 0f;
            bool dead = false;
            while (_particles.isPlaying)
            {
                if (target == null) break;
                _particles.transform.rotation = Quaternion.Euler(new Vector3(0, 0, AdvancedMaths.AngleFromUp(transform.position, target.transform.position) - transform.parent.rotation.z));
                float distance = Vector2.Distance(transform.position, target.transform.position);
                float duration = distance / _particles.main.startSpeed.constant;
                ParticleSystem.MainModule main = _particles.main;
                main.startLifetime = duration;
                if (!dead && timePassed > _particles.main.duration)
                {
                    Explosion.CreateExplosion(transform.position, 10, 0.1f).InstantDetonate();
                    GetComponent<SpriteRenderer>().enabled = false;
                    GetComponent<CircleCollider2D>().enabled = false;
                    target.DecreaseDrawLifeCount((int) _enemy.HealthController.GetCurrentHealth());
                    _target = null;
                    dead = true;
                }

                timePassed += Time.deltaTime;
                yield return null;
            }

            if (target != null)
            {
                _enemy.Kill();
            }
        }

        public void OnDestroy()
        {
            if (_target == null) return;
            _target.DecreaseDrawLifeCount(0);
        }
    }
}