using System.Collections;
using System.Collections.Generic;
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

        private static GameObject _bulletPrefab;

        private readonly RaycastHit2D[] _collisions = new RaycastHit2D[50];

        private Rigidbody2D _rigidBody;
        private BulletTrail _bulletTrail, _conditionTrail;
        public CharacterCombat _origin;
        private Vector2 _direction, _originPosition, _lastPosition;
        private ShotAttributes _shotAttributes;
        
        public Rigidbody2D RigidBody2D()
        {
            return _rigidBody;
        }

        public void Awake()
        {
            _rigidBody = GetComponent<Rigidbody2D>();
        }

        public ShotAttributes Attributes()
        {
            return _shotAttributes;
        }

        public void OverrideDirection(Vector2 direction)
        {
            _direction = direction;
        }

        public void Initialise(CharacterCombat origin, Vector3 direction)
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

            CanTakeDamage nearestEnemy = CombatManager.Instance().NearestEnemy(transform.position);
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
            cf.layerMask = 1 << 10 | 1 << 24;
            int hits = Physics2D.Linecast(_lastPosition, newPosition, cf, _collisions);
            for (int i = 0; i < hits; ++i)
            {
                RaycastHit2D hit = _collisions[i];
                _shotAttributes.DealDamage(hit.collider.gameObject, this);
                _shotAttributes.ApplyConditions(transform.position);
            }

            _lastPosition = newPosition;
        }

        public void MyFixedUpdate()
        {
            if (!_shotAttributes.Fired) return;
            _lastPosition = transform.position;
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
            _shotAttributes.Fire();
            if (_origin != null) _origin.IncreaseRecoil();
            _bulletTrail = _shotAttributes.GetBulletTrail();
            _conditionTrail = _shotAttributes.GetConditionTrail();
            _rigidBody.velocity = _shotAttributes.GetSpeed() * Random.Range(0.9f, 1.1f) * _direction;
            _lastPosition = transform.position;
            _bulletTrail.SetTarget(transform);
            if (_conditionTrail != null) _conditionTrail.SetTarget(transform);
            StartCoroutine(WaitToDie());
        }

        private void DeactivateShot()
        {
            if (_conditionTrail != null)
            {
                _conditionTrail.SetFinalPosition(_lastPosition);
                _conditionTrail.StartFade();
            }

            _bulletTrail.SetFinalPosition(_lastPosition);
            _bulletTrail.StartFade();
            ShotManager.Return(this);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.gameObject.layer == 14) return;
            Hit(collision);
        }

        private void Hit(Collision2D collision)
        {
            GameObject other = collision.gameObject;
            if (_shotAttributes.HasHit) return;
            if (_shotAttributes.Piercing)
            {
                DeactivateShot();
                return;
            }

            _shotAttributes.HasHit = true;
            if (collision.contacts.Length > 0)
            {
                Vector2 collisionPosition = collision.contacts[0].point;
                float angle = AdvancedMaths.AngleFromUp(Vector2.zero, _direction) + 180 + Random.Range(-10f, 10f);
                BulletImpactBehaviour.Create(collisionPosition, angle);
                _lastPosition = collisionPosition;
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
            ShotManager.Dispose(this);
        }
    }
}