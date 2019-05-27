using System.Collections;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Misc
{
	public class Shot : MonoBehaviour
	{
		private const float DistanceFromOrigin = 0.2f;

		private static GameObject _bulletPrefab;

		private BulletTrailController _bulletTrail;
		private Rigidbody2D           _rigidBody;
		private ShotAttributes        _shotAttributes;
		private Vector2               _direction;
		private Vector2               _originPosition;
		private Vector2               _lastPosition;

		public CharacterCombat _origin;

		public Rigidbody2D RigidBody2D() => _rigidBody;

		public void Awake()
		{
			_rigidBody = GetComponent<Rigidbody2D>();
		}

		public ShotAttributes Attributes() => _shotAttributes;

		public void OverrideDirection(Vector2 direction)
		{
			_direction = direction;
		}

		public void Initialise(CharacterCombat origin, Vector3 direction)
		{
			_shotAttributes = new ShotAttributes(origin);
			_origin         = origin;
			_direction      = direction;
			_originPosition = origin.transform.position;
		}

		private void SeekTarget()
		{
			float seekForce = _shotAttributes.GetSeekForce();
			if (seekForce < 0) return;

			CanTakeDamage nearestEnemy = CombatManager.Instance().NearestEnemy(transform.position);
			if (nearestEnemy == null) return;
			Vector2 velocity = _rigidBody.velocity;
			Vector2 dir      = new Vector2(-velocity.y, velocity.x).normalized;
			float   angle    = Vector2.Angle(dir, nearestEnemy.transform.position - transform.position);

			if (angle > 90) seekForce = -seekForce;
			velocity            += seekForce * Time.fixedDeltaTime * dir;
			_rigidBody.velocity =  velocity;
			_rigidBody.velocity =  velocity.normalized * _shotAttributes.GetSpeed();
		}

		public void MyFixedUpdate()
		{
			if (!_shotAttributes.Fired) return;
			_lastPosition = transform.position;
			SeekTarget();
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
			float angleModifier                        = 1 - Mathf.Sqrt(Random.Range(0f, 1f));
			if (Random.Range(0, 2) == 0) angleModifier = -angleModifier;
			float angleOffset                          = angleModifier * _shotAttributes.CalculateAccuracy();
			transform.position = _originPosition + _direction * DistanceFromOrigin;
			_direction         = Quaternion.AngleAxis(angleOffset, Vector3.forward) * _direction;
			_shotAttributes.Fire();
			if (_origin != null) _origin.IncreaseRecoil();
			_bulletTrail        = _shotAttributes.GetBulletTrail();
			_rigidBody.velocity = _shotAttributes.GetSpeed() * Random.Range(0.9f, 1.1f) * _direction;
			_lastPosition       = transform.position;
			_bulletTrail.SetTarget(transform);
			StartCoroutine(WaitToDie());
		}

		private void DeactivateShot()
		{
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
			if (collision.contacts.Length > 0)
			{
				Vector2 collisionPosition = collision.contacts[0].point;
				float   angle             = AdvancedMaths.AngleFromUp(Vector2.zero, _direction) + 180 + Random.Range(-10f, 10f);
				BulletImpactBehaviour.Create(collisionPosition, angle);
				_lastPosition = collisionPosition;
			}

			_shotAttributes.DealDamage(other, this);
			_shotAttributes.ApplyConditions(transform.position);

			if (_shotAttributes.DidPierce()) return;
			DeactivateShot();
		}

		public Vector2 Direction() => _direction;

		private void OnDestroy() => ShotManager.Dispose(this);
	}
}