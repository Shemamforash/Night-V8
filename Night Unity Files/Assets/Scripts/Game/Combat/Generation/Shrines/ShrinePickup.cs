using Game.Combat.Player;
using UnityEngine;

namespace Game.Combat.Generation.Shrines
{
    public class ShrinePickup : MonoBehaviour
    {
        private ChaseShrine _targetShrine;
        private PlayerCombat _player;
        private Rigidbody2D _rigidbody2D;
        private Vector2 _forceToAdd = Vector2.zero;
        private bool _returning;
        private ParticleSystem _particles;

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

        public void Update()
        {
            if (_player == null)
            {
                if (Vector2.Distance(transform.position, PlayerCombat.Instance.transform.position) > 0.2f) return;
                _player = PlayerCombat.Instance;
                _targetShrine.StartDropMarker();
            }
            else if (!_returning)
            {
                Vector2 dir = _player.transform.position - transform.position;
                _forceToAdd = dir * 10f;
                if (Vector2.Distance(transform.position, _targetShrine.transform.position) > 0.2f) return;
                _targetShrine.ReturnPickup();
                _particles.Stop();
                _returning = true;
            }
            else
            {
                Vector2 dir = _targetShrine.transform.position - transform.position;
                _forceToAdd = dir * 10f;
                if (_particles.particleCount == 0) Destroy(gameObject);
            }
        }
    }
}