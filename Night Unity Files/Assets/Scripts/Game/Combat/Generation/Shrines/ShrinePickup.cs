using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public class ShrinePickup : MonoBehaviour
    {
        private ChaseShrine _targetShrine;
        private Rigidbody2D _rigidbody2D;
        private Vector2 _forceToAdd = Vector2.zero;
        private bool _returning;
        private ParticleSystem _particles;
        private bool _attractedToShrine, _followPlayer;

        public void SetShrine(ChaseShrine chaseShrine)
        {
            _targetShrine = chaseShrine;
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _particles = GetComponent<ParticleSystem>();
        }

        public void FixedUpdate()
        {
            _rigidbody2D.AddForce(_forceToAdd);
            _forceToAdd = Vector2.zero;
        }

        private void TryAttractToShrine()
        {
            if (_returning) return;
            float shrineDistance = transform.Distance(_targetShrine.transform);
            if (!_attractedToShrine)
            {
                if (shrineDistance > 0.5f) return;
                _attractedToShrine = true;
            }

            if (shrineDistance < 0.1f)
            {
                _targetShrine.ReturnPickup();
                _particles.Stop();
                _returning = true;
            }

            Vector2 dir = _targetShrine.transform.position - transform.position;
            _forceToAdd = dir * 10f;
        }

        private void TryFollowPlayer()
        {
            if (_returning) return;
            if (_attractedToShrine) return;
            if (_followPlayer == false)
            {
                if (Vector2.Distance(transform.position, PlayerCombat.Position()) > 0.25f) return;
                _followPlayer = true;
                _targetShrine.StartDropMarker();
            }

            Vector2 dir = PlayerCombat.Position() - transform.position;
            _forceToAdd = dir * 10f;
        }

        public void Update()
        {
            if (!CombatManager.IsCombatActive()) return;
            TryFollowPlayer();
            TryAttractToShrine();
            if (_returning && _particles.particleCount == 0) Destroy(gameObject);
        }
    }
}