using System.Collections;
using Game.Combat.Generation;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Ghoul : EnemyBehaviour
    {
        private ParticleSystem _particles;
        private bool _drawingLife;

        private const float DistanceToTouch = 0.5f;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            Alerted = true;
            CurrentAction = SeekPlayer;
            _particles = Helper.FindChildWithName<ParticleSystem>(gameObject, "Dissolve Particles");
        }

        private void SeekPlayer()
        {
            Vector2 direction = CombatManager.Player().transform.position - transform.position;
            Move(direction.normalized);
        }

        public override void Update()
        {
            base.Update();
            if (DistanceToTarget() > DistanceToTouch) return;
            GetTarget().Sicken();
            Kill();
        }

        public void StartDrawLife(Nightmare target)
        {
            if (_drawingLife) return;
            StartCoroutine(DrawLife(target));
        }

        private Nightmare _target;
        
        public void OnDestroy()
        {
            if (_target == null) return;
            _target.DecreaseDrawLifeCount(0);
        }
        
        private IEnumerator DrawLife(Nightmare target)
        {
            _target = target;
            Immobilised(true);
            _drawingLife = true;
            _particles.Play();
            float timePassed = 0f;
            bool dead = false;
            while (_particles.isPlaying)
            {
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
                    target.DecreaseDrawLifeCount((int)HealthController.GetCurrentHealth());
                    _target = null;
                    dead = true;
                }

                timePassed += Time.deltaTime;
                yield return null;
            }
            Kill();
        }
    }
}