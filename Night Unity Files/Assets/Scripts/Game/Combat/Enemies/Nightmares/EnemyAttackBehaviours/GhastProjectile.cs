using System.Collections;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class GhastProjectile : MonoBehaviour
    {
        private static GameObject _projectilePrefab;
        private Vector3 targetPosition;
        private Vector3 _direction;
        private const float MaxVelocity = 10f;
        private Rigidbody2D _rigidbody2D;
        private float _duration;
        
        public static void Create(Vector3 position, Vector3 direction)
        {
            if (_projectilePrefab == null) _projectilePrefab = Resources.Load<GameObject>("Prefabs/Combat/Enemies/Ghast Projectile");
            GameObject projectile = Instantiate(_projectilePrefab);
            projectile.transform.position = position;
            projectile.GetComponent<GhastProjectile>().Init(direction);
        }

        private void Init(Vector3 direction)
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            direction.Normalize();
            _direction = direction;
            targetPosition = AdvancedMaths.RandomVectorWithinRange(PlayerCombat.Position(), 1f);
            Vector3 startingVelocity = AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 2f);
            _rigidbody2D.velocity = startingVelocity;
            _duration = Random.Range(10f, 12f);
            StartCoroutine(Steer());
        }

        private IEnumerator Steer()
        {
            while (_duration > 0f)
            {
                Vector2 desiredVelocity = (targetPosition - transform.position).normalized * MaxVelocity;
                Vector2 steeringForce = desiredVelocity - _rigidbody2D.velocity;
                float angle = AdvancedMaths.AngleFromUp(Vector3.zero, _direction);
                transform.rotation = Quaternion.Euler(new Vector3(0,0, angle));
                _rigidbody2D.AddForce(steeringForce);
                _rigidbody2D.velocity = Vector2.ClampMagnitude(_rigidbody2D.velocity, MaxVelocity);
                _duration -= Time.deltaTime;
                if (Vector2.Distance(transform.position, targetPosition) < 0.5f) break;
                yield return null;
            }

            Explosion.CreateExplosion(transform.position, 0.25f).InstantDetonate();
            Destroy(gameObject);
        }
    }
}