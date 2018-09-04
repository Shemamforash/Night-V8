using System.Collections;
using Game.Combat.Generation;
using Game.Combat.Player;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Teleport : TimedAttackBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private CircleCollider2D _collider;
        private ParticleSystem _teleportInParticles, _teleportOutParticles;
        private float _teleportTimer;
        private const float TeleportTimerMax = 0.5f;
        private static GameObject _teleportInPrefab, _teleportOutPrefab;
        private TrailRenderer _trail;

        public void Start()
        {
            if (_teleportInPrefab == null)
            {
                _teleportInPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Teleport In");
                _teleportOutPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Teleport Out");
            }

            GameObject teleportIn = Instantiate(_teleportInPrefab);
            teleportIn.transform.SetParent(transform, false);
            _teleportInParticles = teleportIn.GetComponent<ParticleSystem>();

            GameObject teleportOut = Instantiate(_teleportOutPrefab);
            teleportOut.transform.SetParent(transform, false);
            _teleportOutParticles = teleportOut.GetComponent<ParticleSystem>();
            
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<CircleCollider2D>();
            _trail = transform.GetComponentInChildren<TrailRenderer>();
        }

        private IEnumerator TeleportNow()
        {
            PauseOthers();
            _teleportOutParticles.Play();
            _trail.Clear();
            while (_teleportOutParticles.isPlaying)
            {
                yield return null;
            }

            _teleportTimer = 0f;
            SetVisible(false);
            PlayerCombat.Instance.SetTarget(null);
            while (_teleportTimer < TeleportTimerMax)
            {
                _teleportTimer += Time.deltaTime;
                yield return null;
            }

            Cell c = PathingGrid.GetCellNearMe(PlayerCombat.Instance.CurrentCell(), 4);
            transform.position = c.Position;
            _trail.Clear();
            
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
            if(visible) _trail.Clear();
        }
    }
}