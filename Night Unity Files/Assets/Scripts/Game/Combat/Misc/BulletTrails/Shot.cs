using System.Collections;
using Fastlights;
using Game.Combat.Generation;
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

		private Rigidbody2D     _rigidBody;
		private BulletTrail     _bulletTrail, _conditionTrail;
		public  CharacterCombat _origin;
		private Vector2         _direction, _originPosition, _lastPosition;
		private ShotAttributes  _shotAttributes;
		private Transform       _transform;
		private FastLight       _light;

		public Rigidbody2D RigidBody2D()
		{
			return _rigidBody;
		}

		public void Awake()
		{
			_rigidBody = GetComponent<Rigidbody2D>();
			_transform = transform;
			_light     = GetComponent<FastLight>();
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
			float pellets = origin.Weapon().GetAttributeValue(AttributeType.Pellets);
			_light.Colour = new Color(1, 1, 1, 0.1f / pellets);
			float normalisedDps = origin.Weapon().WeaponAttributes.DPS() / 500f;
			_light.Radius   = Mathf.Lerp(1f, 2f, normalisedDps);
			_origin         = origin;
			_direction      = direction;
			_originPosition = origin.transform.position;
		}

		private void SeekTarget()
		{
			float seekForce = _shotAttributes.GetSeekForce();
			if (seekForce < 0) return;

			CanTakeDamage nearestEnemy = CombatManager.Instance().NearestEnemy(_transform.position);
			if (nearestEnemy == null) return;
			Vector2 velocity = _rigidBody.velocity;
			Vector2 dir      = new Vector2(-velocity.y, velocity.x).normalized;
			float   angle    = Vector2.Angle(dir, nearestEnemy.transform.position - _transform.position);

			if (angle > 90) seekForce = -seekForce;
			velocity            += seekForce * Time.fixedDeltaTime * dir;
			_rigidBody.velocity =  velocity.normalized             * _shotAttributes.GetSpeed();
		}

		private void CheckForPierce()
		{
			if (!_shotAttributes.Piercing) return;
			Vector2         newPosition = _transform.position;
			ContactFilter2D cf          = new ContactFilter2D();
			cf.layerMask = 1 << 10 | 1 << 24;
			int hits = Physics2D.Linecast(_lastPosition, newPosition, cf, _collisions);
			for (int i = 0; i < hits; ++i)
			{
				RaycastHit2D hit = _collisions[i];
				_shotAttributes.DealDamage(hit.collider.gameObject, this);
				_shotAttributes.ApplyConditions(_transform.position);
			}

			_lastPosition = newPosition;
		}

		public void MyFixedUpdate()
		{
			if (!_shotAttributes.Fired) return;
			_lastPosition = _transform.position;
			SeekTarget();
			CheckForPierce();
		}

		private IEnumerator WaitToDie()
		{
			while (_shotAttributes.UpdateAge())
			{
				float distanceTravelled = _originPosition.Distance(_transform.position);
				if (distanceTravelled > 15f) break;
				yield return null;
			}

			_rigidBody.velocity = Vector2.zero;
			DeactivateShot();
		}

		public void Fire()
		{
			if (_shotAttributes.Piercing) gameObject.layer = 20;
			float angleModifier                            = 1 - Mathf.Sqrt(Random.Range(0f, 1f));
			if (Random.Range(0, 2) == 0) angleModifier     = -angleModifier;
			float angleOffset                              = angleModifier * _shotAttributes.CalculateAccuracy();
			_transform.position = _originPosition + _direction * DistanceFromOrigin;
			_direction          = Quaternion.AngleAxis(angleOffset, Vector3.forward) * _direction;
			_shotAttributes.Fire();
			if (_origin != null) _origin.IncreaseRecoil();
			_bulletTrail        = _shotAttributes.GetBulletTrail();
			_conditionTrail     = _shotAttributes.GetConditionTrail();
			_rigidBody.velocity = _shotAttributes.GetSpeed() * Random.Range(0.9f, 1.1f) * _direction;
			_lastPosition       = _transform.position;
			_bulletTrail.SetTarget(_transform);
			if (_conditionTrail != null) _conditionTrail.SetTarget(_transform);
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
				float   angle             = AdvancedMaths.AngleFromUp(Vector2.zero, _direction) + 180 + Random.Range(-10f, 10f);
				BulletImpactBehaviour.Create(collisionPosition, angle);
				_lastPosition = collisionPosition;
			}

			_shotAttributes.DealDamage(other, this);
			_shotAttributes.ApplyConditions(_transform.position);
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