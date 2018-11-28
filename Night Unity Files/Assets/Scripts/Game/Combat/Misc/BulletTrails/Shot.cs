using System.Collections;
using Game.Combat.Generation;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Misc
{
    public class Shot : MonoBehaviour
    {
        private const float DistanceFromOrigin = 0.2f;

        private static readonly ObjectPool<Shot> _shotPool = new ObjectPool<Shot>("Shots", "Prefabs/Combat/Shots/Bullet");
        private static GameObject _bulletPrefab;

        private readonly RaycastHit2D[] _collisions = new RaycastHit2D[50];

        private Rigidbody2D _rigidBody;
        private BulletTrail _bulletTrail;
        public CharacterCombat _origin;
        private Vector2 _direction, _originPosition, _lastPosition;
        private ShotAttributes _shotAttributes;

        public void Awake()
        {
            _rigidBody = GetComponent<Rigidbody2D>();
        }

        public ShotAttributes Attributes()
        {
            return _shotAttributes;
        }

        public static Shot Create(CharacterCombat origin)
        {
            Shot shot = _shotPool.Create();
            shot.gameObject.layer = origin is PlayerCombat ? 16 : 15;
            Vector3 direction = origin.Direction();
            shot.Initialise(origin, direction);
            return shot;
        }

        public void OverrideDirection(Vector2 direction)
        {
            _direction = direction;
        }

        private void Initialise(CharacterCombat origin, Vector3 direction)
        {
            _shotAttributes = new ShotAttributes(origin);
            _origin = origin;
            _direction = direction;
            _originPosition = origin.transform.position;
        }

        private void SeekTarget()
        {
            float seekForce = _shotAttributes.GetSeekForce();
            if (seekForce < 0) return;

            CanTakeDamage nearestEnemy = CombatManager.NearestEnemy(transform.position);
            if (nearestEnemy == null) return;
            Vector2 dir = new Vector2(-_rigidBody.velocity.y, _rigidBody.velocity.x).normalized;
            float angle = Vector2.Angle(dir, nearestEnemy.transform.position - transform.position);

            if (angle > 90) seekForce = -seekForce;
            _rigidBody.velocity += seekForce * dir * Time.fixedDeltaTime;
            _rigidBody.velocity = _rigidBody.velocity.normalized * _shotAttributes.GetSpeed();
        }

        private void CheckForPierce()
        {
            if (!_shotAttributes.Piercing) return;
            Vector2 newPosition = transform.position;
            ContactFilter2D cf = new ContactFilter2D();
            cf.layerMask = 1 << 10;
            int hits = Physics2D.Linecast(_lastPosition, newPosition, cf, _collisions);
            for (int i = 0; i < hits; ++i)
            {
                RaycastHit2D hit = _collisions[i];
                _shotAttributes.DealDamage(hit.collider.gameObject, this);
            }

            _lastPosition = newPosition;
        }

        private void FixedUpdate()
        {
            if (!_shotAttributes.Fired) return;
            SeekTarget();
            CheckForPierce();
        }

        private IEnumerator WaitToDie()
        {
            while (_shotAttributes.UpdateAge())
            {
                float distanceTravelled = _originPosition.Distance(transform.position);
                if (distanceTravelled > 15f) break;
                yield return null;
            }

            _rigidBody.velocity = Vector2.zero;
            DeactivateShot();
        }

        public void Fire()
        {
            if (_shotAttributes.Piercing) gameObject.layer = 20;
            float angleModifier = 1 - Mathf.Sqrt(Random.Range(0f, 1f));
            if (Random.Range(0, 2) == 0) angleModifier = -angleModifier;
            float angleOffset = angleModifier * _shotAttributes.CalculateAccuracy();
            transform.position = _originPosition + _direction * DistanceFromOrigin;
            _direction = Quaternion.AngleAxis(angleOffset, Vector3.forward) * _direction;
            _shotAttributes.Fired = true;
            if (_origin != null) _origin.IncreaseRecoil();
            _bulletTrail = _shotAttributes.GetBulletTrail();
            _rigidBody.velocity = _direction * _shotAttributes.GetSpeed() * Random.Range(0.9f, 1.1f);
            _lastPosition = transform.position;
            _bulletTrail.SetTarget(transform);
            StartCoroutine(WaitToDie());
        }

        private void DeactivateShot()
        {
            _bulletTrail.StartFade(0.2f);
            _shotPool.Return(this);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.gameObject.layer == 14) return;
            Hit(collision);
        }

        private void Hit(Collision2D collision)
        {
            if (_shotAttributes.HasHit) return;
            _shotAttributes.HasHit = true;
            GameObject other = collision.gameObject;
            if (collision.contacts.Length > 0)
            {
                Vector2 collisionPosition = collision.contacts[0].point;
                float angle = AdvancedMaths.AngleFromUp(Vector2.zero, _direction) + 180 + Random.Range(-10f, 10f);
                BulletImpactBehaviour.Create(collisionPosition, angle);
                _bulletTrail.SetFinalPosition(collisionPosition);
            }

            _shotAttributes.DealDamage(other, this);
            _shotAttributes.ApplyConditions(transform.position);
            DeactivateShot();
        }

        public Vector2 Direction()
        {
            return _direction;
        }

        private void OnDestroy()
        {
            _shotPool.Dispose(this);
        }
    }
}