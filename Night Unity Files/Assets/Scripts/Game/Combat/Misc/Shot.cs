using System;
using System.Collections;
using System.Collections.Generic;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Exploration.Weather;
using Game.Gear.Weapons;
using QuickEngine.Extensions;
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
        private const float EnemyDamageModifier = 0.3f;

        private static readonly ObjectPool<Shot> _shotPool = new ObjectPool<Shot>("Shots", "Prefabs/Combat/Bullet");
        private float _accuracy;
        private float _age;

        private int _damage;
        private int _damageDealt;
        private Vector3 _direction;
        private float _finalDamageModifier = 1f;
        private bool _guaranteeHit;

        private bool _fired;
        public CharacterCombat _origin;
        private Vector3 _originPosition;
        private float _pierceChance, _burnChance, _decayChance, _sicknessChance;
        private Rigidbody2D _rigidBody;

        private Weapon _weapon;
        private bool _seekTarget, _leaveFireTrail;
        private float _knockBackForce;
        private float _knockBackModifier;
        private const float MaxAge = 3f;
        private event Action OnHitAction;
        private readonly RaycastHit2D[] _collisions = new RaycastHit2D[50];
        private Vector2 _lastPosition;

        public void Awake()
        {
            _rigidBody = GetComponent<Rigidbody2D>();
        }

        public float GetKnockBackForce()
        {
            return _knockBackForce;
        }

        private void OnDestroy()
        {
            _shotPool.Dispose(this);
        }

        private void ResetValues()
        {
            _finalDamageModifier = 1f;
            _guaranteeHit = false;
            _knockBackModifier = 1f;
            _knockBackForce = 10;
            _damageDealt = 0;
            _fired = false;
            _seekTarget = false;
            _leaveFireTrail = false;
            _pierce = false;
            OnHitAction = null;
        }

        public static Shot Create(CharacterCombat origin)
        {
            Shot shot = _shotPool.Create();
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

        public void OverrideDirection(Vector2 direction)
        {
            _direction = direction;
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
            ResetValues();
            CacheWeaponAttributes();
            CalculateAccuracy();
        }

        private void CalculateAccuracy()
        {
            if (_guaranteeHit) _accuracy = 0;
            else if (_origin == null) return;
            float accuracyDifference = Weapon.MaxAccuracyOffsetInDegrees - _accuracy;
            _accuracy += accuracyDifference * _origin.GetAccuracyModifier();
        }

        private void CacheWeaponAttributes()
        {
            WeaponAttributes attributes = _weapon.WeaponAttributes;
            _damage = (int) attributes.Val(AttributeType.Damage);
            _accuracy = _weapon.CalculateBaseAccuracy();
            _decayChance = attributes.Val(AttributeType.DecayChance);
            _burnChance = attributes.Val(AttributeType.BurnChance);
            _sicknessChance = attributes.Val(AttributeType.SicknessChance);
        }

        public void SetDamageModifier(float modifier)
        {
            _finalDamageModifier = modifier;
        }

        private void SeekTarget()
        {
            CanTakeDamage nearestEnemy = CombatManager.NearestEnemy(transform.position);
            if (nearestEnemy == null) return;
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

        private void CheckForPierce()
        {
            Vector2 newPosition = transform.position;
            ContactFilter2D cf = new ContactFilter2D();
            cf.layerMask = 1 << 10;
            Debug.DrawLine(_lastPosition, newPosition, Color.red, 0.02f);
            int hits = Physics2D.Linecast(_lastPosition, newPosition, cf, _collisions);
            for (int i = 0; i < hits; ++i)
            {
                RaycastHit2D hit = _collisions[i];
                DealDamage(hit.collider.gameObject);
            }

            _lastPosition = newPosition;
        }

        private void FixedUpdate()
        {
            if (!_fired) return;
            if (_seekTarget) SeekTarget();
            if (_pierce) CheckForPierce();
        }

        private IEnumerator WaitToDie()
        {
            _age = 0;
            while (_age < MaxAge)
            {
                float distanceTravelled = _originPosition.Distance(transform.position);
                if (distanceTravelled > 15f) _age = MaxAge;
                _age += Time.deltaTime;
                yield return null;
            }

            _rigidBody.velocity = Vector2.zero;
            DeactivateShot();
        }

        private bool _pierce;

        public void Pierce()
        {
            _pierce = true;
        }

        public void Fire(float distance = 0.15f)
        {
            if (_pierce) gameObject.layer = 20;
            float angleModifier = 1 - Mathf.Sqrt(Random.Range(0f, 1f));
            if (Random.Range(0, 2) == 0) angleModifier = -angleModifier;
            float angleOffset = angleModifier * _accuracy;
            transform.position = _originPosition + _direction * distance;
            _direction = Quaternion.AngleAxis(angleOffset, Vector3.forward) * _direction;
            if (_leaveFireTrail) gameObject.AddComponent<LeaveFireTrail>().Initialise();
            _fired = true;
            if (_origin != null) _origin.IncreaseRecoil();

            _bulletTrail = BulletTrailFade.Create();
            _bulletTrail.SetAlpha(1);
            _rigidBody.velocity = _direction * Speed * Random.Range(0.9f, 1.1f);
            _lastPosition = transform.position;
            _bulletTrail.SetPosition(transform);
            StartCoroutine(WaitToDie());
        }

        private void DeactivateShot()
        {
            Destroy(gameObject.GetComponent<LeaveFireTrail>());
            _bulletTrail.StartFade(0.2f);
            _shotPool.Return(this);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Hit(collision);
        }

        private void DealDamage(GameObject other)
        {
            _damageDealt = _damage;
            _damageDealt = (int) (_damageDealt * _finalDamageModifier);
            OnHitAction?.Invoke();
            CanTakeDamage hit = other.GetComponent<CanTakeDamage>();
            if (hit == null) return;
            PlayerCombat player = _origin as PlayerCombat;
            if (player != null) player.OnShotConnects(hit);
            ApplyDamage(hit);
        }

        private void Hit(Collision2D collision)
        {
            GameObject other = collision.gameObject;
            if (collision.contacts.Length > 0)
            {
                float angle = AdvancedMaths.AngleFromUp(Vector2.zero, _direction) + 180 + Random.Range(-10f, 10f);
                BulletImpactBehaviour.Create(collision.contacts[0].point, angle);
            }

            DealDamage(other);
            DeactivateShot();
            ApplyConditions();
        }

        private void ApplyDamage(CanTakeDamage hit)
        {
            CalculateKnockBackForce();
            hit.TakeShotDamage(this);
        }

        private void CalculateKnockBackForce()
        {
            float rainModifier = WeatherManager.CurrentWeather().Attributes.RainAmount;
            _knockBackModifier += rainModifier;
            _knockBackForce = _damageDealt / 4f * _knockBackModifier;
        }

        private void ApplyConditions()
        {
            float random = Random.Range(0f, 1f);
            bool canDecay = random < _decayChance / _weapon.GetAttributeValue(AttributeType.Pellets);
            bool canBurn = random < _burnChance / _weapon.GetAttributeValue(AttributeType.Pellets);
            bool canSicken = random < _sicknessChance / _weapon.GetAttributeValue(AttributeType.Pellets);
            List<int> conditions = new List<int>();
            if (canDecay) conditions.Add(0);
            if (canBurn) conditions.Add(1);
            if (canSicken) conditions.Add(2);
            if (conditions.Count == 0) return;
            int condition = conditions.GetRandomElement();
            float radius = _damageDealt / 400f;
            if (radius < 0.5f) radius = 0.5f;
            switch (condition)
            {
                case 0:
                    DecayBehaviour.Create(transform.position, radius);
                    break;
                case 1:
                    FireBehaviour.Create(transform.position, radius, 4f, false, false);
                    break;
                case 2:
                    SickenBehaviour.Create(transform.position, new List<CanTakeDamage> {_origin}, radius);
                    break;
            }
        }

        public void GuaranteeHit()
        {
            _guaranteeHit = true;
        }

        public void AddOnHit(Action a)
        {
            OnHitAction += a;
        }

        public void AddKnockBackModifier(float forceModifier)
        {
            _knockBackModifier += forceModifier;
        }

        public void SetBurnChance(float chance)
        {
            Assert.IsTrue(chance >= 0 && chance <= 1);
            _burnChance = chance;
        }

        public void SetDecayChance(float chance)
        {
            Assert.IsTrue(chance >= 0 && chance <= 1);
            _decayChance = chance;
        }

        public void SetSicknessChance(float chance)
        {
            Assert.IsTrue(chance >= 0 && chance <= 1);
            _sicknessChance = chance;
        }

        public int DamageDealt()
        {
            int damageDealt = _damageDealt;
            if (_origin is EnemyBehaviour) damageDealt = Mathf.FloorToInt(EnemyDamageModifier * _damageDealt);
            return damageDealt;
        }

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