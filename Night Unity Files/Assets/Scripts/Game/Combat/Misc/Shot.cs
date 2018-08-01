using System;
using System.Collections;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Game.Combat.Misc
{
    public class Shot : MonoBehaviour
    {
        private float Speed = 25f;
        private static GameObject _bulletPrefab;
        private BulletTrailFade _bulletTrail;

        private static readonly ObjectPool<Shot> _shotPool = new ObjectPool<Shot>("Prefabs/Combat/Bullet");
        private float _accuracy;
        private float _age;

        private int _damage;
        private int _damageDealt;
        private Vector3 _direction;
        private float _finalDamageModifier = 1f;
        private bool _guaranteeHit;
        private int _knockbackForce = 10;

        private bool _fired;
        public CharacterCombat _origin;
        private Vector3 _originPosition;
        private float _pierceChance, _burnChance, _decayChange, _sicknessChance;
        private Rigidbody2D _rigidBody;
        
        private static Transform _shotParent;
        private Weapon _weapon;
        private bool _seekTarget, _leaveFireTrail;
        private const float MaxAge = 3f;
        private event Action OnHitAction;

        private void OnDestroy()
        {
            _shotPool.Dispose(this);
            if (_shotPool.Empty()) _shotParent = null;
        }

        private void ResetValues()
        {
            _finalDamageModifier = 1f;
            _guaranteeHit = false;
            _knockbackForce = 10;
            _damageDealt = 0;
            _fired = false;
            _seekTarget = false;
            _leaveFireTrail = false;
            OnHitAction = null;
        }

        public static Shot Create(CharacterCombat origin)
        {
            if (_shotParent == null) _shotParent = GameObject.Find("World").transform.Find("Bullets");
            Shot shot = _shotPool.Create(_shotParent);
            shot.gameObject.layer = origin is PlayerCombat ? 16 : 15;
            Vector3 direction = origin.Direction();
            shot.Initialise(origin, direction);
            return shot;
        }

        public static Shot Create(Shot origin)
        {
            Shot shot = _shotPool.Create();
            shot.gameObject.layer = origin.gameObject.layer;
            shot.Initialise(origin);
            return shot;
        }

        private void Initialise(CharacterCombat origin, Vector3 direction)
        {
            _origin = origin;
            _direction = direction;
            _weapon = origin.Weapon();
            switch (_weapon.WeaponType())
            {
                case WeaponType.Pistol:
                    Speed = 20f;
                    break;
                case WeaponType.Rifle:
                    Speed = 25;
                    break;
                case WeaponType.Shotgun:
                    Speed = 15f;
                    break;
                case WeaponType.SMG:
                    Speed = 15f;
                    break;
                case WeaponType.LMG:
                    Speed = 20f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _originPosition = origin.transform.position;
            SetUpComponents();
        }

        private void Initialise(Shot shot)
        {
            _origin = null;
            _direction = shot._direction;
            _weapon = shot._weapon;
            _originPosition = shot.transform.position;
            SetUpComponents();
        }

        private void SetUpComponents()
        {
            if (_rigidBody == null) _rigidBody = GetComponent<Rigidbody2D>();
            ResetValues();
            CacheWeaponAttributes();
            CalculateAccuracy();
        }

        private void CalculateAccuracy()
        {
            if (_guaranteeHit) _accuracy = 0;
        }

        private void CacheWeaponAttributes()
        {
            WeaponAttributes attributes = _weapon.WeaponAttributes;
            _damage = (int) attributes.Val(AttributeType.Damage);
            _accuracy = _weapon.CalculateBaseAccuracy();
            _decayChange = attributes.Val(AttributeType.DecayChance);
            _burnChance = attributes.Val(AttributeType.BurnChance);
            _sicknessChance = attributes.Val(AttributeType.SicknessChance);
        }

        public void SetDamageModifier(float modifier)
        {
            _finalDamageModifier = modifier;
        }

        private void FixedUpdate()
        {
            if (!_fired || !_seekTarget) return;
            EnemyBehaviour nearestEnemy = CombatManager.NearestEnemy(transform.position);
            Vector2 dir = new Vector2(-_rigidBody.velocity.y, _rigidBody.velocity.x).normalized;
            float angle = Vector2.Angle(dir, nearestEnemy.transform.position - transform.position);
            float force = 50;
            if (angle > 90)
            {
                force = -force;
            }
            _rigidBody.velocity += force * dir * Time.fixedDeltaTime;
            _rigidBody.velocity = _rigidBody.velocity.normalized * Speed;
        }

        private IEnumerator WaitToDie()
        {
            _age = 0;
            while (_age < MaxAge)
            {
                _age += Time.deltaTime;
                _bulletTrail.SetAlpha(1f - _age / MaxAge);
                yield return null;
            }

            _rigidBody.velocity = Vector2.zero;
            _bulletTrail.StartFade(0f);
            DeactivateShot();
        }

        public void Fire(float distance = 0.15f)
        {
            float angleOffset = Random.Range(-_accuracy, _accuracy);
            if (_origin != null) angleOffset *= _origin.GetAccuracyModifier();
            transform.position = _originPosition + _direction * distance;
            _direction = Quaternion.AngleAxis(angleOffset, Vector3.forward) * _direction;
            if (_leaveFireTrail) gameObject.AddComponent<LeaveFireTrail>().Initialise();
            _fired = true;
            if (_origin != null) _origin.IncreaseRecoil();

            _bulletTrail = BulletTrailFade.Create();
            _bulletTrail.SetAlpha(1);

            _bulletTrail.SetPosition(transform);
            _rigidBody.velocity = _direction * Speed * Random.Range(0.9f, 1.1f);
            StartCoroutine(WaitToDie());
        }

        private void DeactivateShot()
        {
            Destroy(gameObject.GetComponent<LeaveFireTrail>());
            _shotPool.Return(this);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            GameObject other = collision.gameObject;
            if (collision.contacts.Length > 0)
            {
                float angle = AdvancedMaths.AngleFromUp(Vector2.zero, _direction) + 180 + Random.Range(-10f, 10f);
                BulletImpactBehaviour.Create(collision.contacts[0].point, angle);
            }

            if (Random.Range(0f, 1f) < _burnChance) FireBehaviour.Create(transform.position, 1f);
            OnHitAction?.Invoke();
            CharacterCombat hit = other.GetComponent<CharacterCombat>();
            (_origin as PlayerCombat)?.OnShotConnects(hit);
            if (hit != null) ApplyDamage(hit);
            DeactivateShot();
            _bulletTrail.StartFade(0.2f);
        }

        private void ApplyDamage(CharacterCombat hit)
        {
            _damageDealt = _damage;
            _damageDealt = (int) (_damageDealt * _finalDamageModifier);
            ApplyConditions(hit);
            hit.TakeDamage(this);
        }

        private void ApplyConditions(CharacterCombat hit)
        {
            if (_knockbackForce != 0)
            {
                hit.MovementController.Knockback(transform.position, _knockbackForce);
            }

            if (Random.Range(0f, 1f) < _decayChange) hit.Decay();
            if (Random.Range(0f, 1f) < _sicknessChance) hit.Sicken();
            if (Random.Range(0f, 1f) < _burnChance) hit.Burn();
        }

        public void GuaranteeHit()
        {
            _guaranteeHit = true;
        }

        public void AddOnHit(Action a)
        {
            OnHitAction += a;
        }

        public void SetKnockbackForce(int force)
        {
            Assert.IsTrue(force >= 0);
            _knockbackForce = force;
        }

        public void SetBurnChance(float chance)
        {
            Assert.IsTrue(chance >= 0 && chance <= 1);
            _burnChance = chance;
        }

        public void SetDecayChance(float chance)
        {
            Assert.IsTrue(chance >= 0 && chance <= 1);
            _decayChange = chance;
        }

        public void SetSicknessChance(float chance)
        {
            Assert.IsTrue(chance >= 0 && chance <= 1);
            _sicknessChance = chance;
        }

        public int DamageDealt() => _damageDealt;

        public void SetAccuracy(float accuracy)
        {
            Assert.IsTrue(accuracy >= 0 && accuracy <= 180);
            _accuracy = accuracy;
        }

        public void Seek()
        {
            _seekTarget = true;
        }

        public void LeaveFireTrail()
        {
            _leaveFireTrail = true;
        }

        public Vector2 Direction()
        {
            return _direction;
        }
    }
}