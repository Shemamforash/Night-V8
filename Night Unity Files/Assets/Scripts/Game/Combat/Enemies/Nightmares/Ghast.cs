using System.Collections;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Ghast : EnemyBehaviour
    {
        private float _teleportCooldown;
        private const float TeleportCooldownMax = 5f;
        private ParticleSystem _teleportInParticles, _teleportOutParticles;
        private const float TeleportTimerMax = 0.5f;
        private float _teleportTimer;
        private bool _teleporting;
        private SpriteRenderer _spriteRenderer;
        private CircleCollider2D _collider;

        private float _fireTimer = 3f;
        
        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            _teleportInParticles = Helper.FindChildWithName<ParticleSystem>(gameObject, "In");
            _teleportOutParticles = Helper.FindChildWithName<ParticleSystem>(gameObject, "Out");
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<CircleCollider2D>();
            ChooseNextAction();
        }

        public override void ChooseNextAction()
        {
            Reposition(PathingGrid.FindCellToAttackPlayer(CurrentCell(), 5f, 2f));
        }
        
        public override void Update()
        {
            if (_teleporting) return;
            base.Update();
            if (_fireTimer > 0) _fireTimer -= Time.deltaTime;
            else
            {
                for (int i = Random.Range(3, 7); i >= 0; --i)
                {
                    GhastProjectile.Create(transform.position, (GetTarget().transform.position - transform.position).normalized);
                }

                _fireTimer = 3f;
            }
            UpdateTeleport();
        }

        private void UpdateTeleport()
        {
            _teleportCooldown -= Time.deltaTime;
            if (_teleportCooldown > 0f) return;
            _teleportCooldown = TeleportCooldownMax + Random.Range(0f, TeleportCooldownMax);
            StartCoroutine(Teleport());
        }

        private void SetVisible(bool visible)
        {
            _spriteRenderer.enabled = visible;
            _collider.enabled = visible;
        }
        
        private IEnumerator Teleport()
        {
            _teleporting = true;
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
            _teleporting = false;
            ChooseNextAction();
        }
    }
}