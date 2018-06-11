using System.Collections;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Teleport : BasicAttackBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private CircleCollider2D _collider;
        private ParticleSystem _teleportInParticles, _teleportOutParticles;
        private float _teleportTimer;
        private const float TeleportTimerMax = 0.5f;

        public void Start()
        {
            _teleportInParticles = Helper.FindChildWithName<ParticleSystem>(gameObject, "In");
            _teleportOutParticles = Helper.FindChildWithName<ParticleSystem>(gameObject, "Out");
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<CircleCollider2D>();
        }
        
        private IEnumerator TeleportNow()
        {
            PauseOthers();
            _teleportOutParticles.Play();
            while (_teleportOutParticles.isPlaying)
            {
                yield return null;
            }

            _teleportTimer = 0f;
            SetVisible(false);
            while (_teleportTimer < TeleportTimerMax)
            {
                _teleportTimer += Time.deltaTime;
                yield return null;
            }

            Cell c = PathingGrid.GetCellNearMe(CombatManager.Player().CurrentCell(), 4);
            transform.position = c.Position;
            _teleportInParticles.Play();
            SetVisible(true);
            UnpauseOthers();
        }

        protected override void Attack()
        {
            StartCoroutine(TeleportNow());
        }

        private void SetVisible(bool visible)
        {
            _spriteRenderer.enabled = visible;
            _collider.enabled = visible;
        }

        public void Initialise(int maxTimer)
        {
            MaxTimer = maxTimer;
        }
    }
}