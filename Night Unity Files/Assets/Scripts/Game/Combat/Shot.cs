using System;
using System.Collections;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Game.Combat
{
    public class Shot : MonoBehaviour
    {
        private int _damage;
        private float _accuracy;
        private float _pierceChance, _burnChance, _bleedChance, _sicknessChance;
        private float _criticalChance;
        private CharacterCombat _origin;
        private Vector3 _direction;
        public bool IsCritical;
        private bool _moving, _fired;
        private float _speed;
        private float _age;
        private const float MaxAge = 3f;

        private float _knockDownChance;
        private float _finalDamageModifier = 1f;
        private bool _guaranteeHit, _guaranteeCritical;
        private int _knockbackForce;
        private int _damageDealt;
        private int _pierceDepth;
        public bool DidHit;
        private event Action OnHitAction;
        private static GameObject _bulletPrefab;
        private Rigidbody2D _rigidBody;
        private TrailRenderer _trailRenderer;
        private GameObject _fireTrail;

        private static readonly ObjectPool<Shot> _shotPool = new ObjectPool<Shot>("Prefabs/Combat/Bullet");

        private Transform _shotParent;
        private Weapon _weapon;
        private Vector3 _originPosition;

        public void Awake()
        {
            if (_shotParent == null) _shotParent = GameObject.Find("World").transform.Find("Bullets");
        }

        private void OnDestroy()
        {
            _shotPool.Dispose(this);
        }

        private void ResetValues()
        {
            _knockDownChance = 0;
            _finalDamageModifier = 1f;
            _guaranteeHit = false;
            _guaranteeCritical = false;
            _knockbackForce = 0;
            _damageDealt = 0;
            _pierceDepth = 0;
            _moving = false;
            _fired = false;
            DidHit = false;
            OnHitAction = null;
        }

        public static Shot Create(CharacterCombat origin)
        {
            Shot shot = _shotPool.Create();
            shot.gameObject.layer = 11;
            shot.Initialise(origin, origin.GetTarget().transform.position);
            return shot;
        }
        
        public static Shot Create(Shot origin)
        {
            Shot shot = _shotPool.Create();
            shot.gameObject.layer = 11;
            shot.Initialise(origin);
            return shot;
        }

        private void Initialise(CharacterCombat origin, Vector3 target)
        {
            _origin = origin;
            _direction = (target - _origin.transform.position).normalized;
            _weapon = origin.Weapon();
            _originPosition = origin.transform.position;
            SetUpComponents();
        }

        private void SetUpComponents()
        {
            if (_rigidBody == null) _rigidBody = GetComponent<Rigidbody2D>();
            if (_trailRenderer == null) _trailRenderer = GetComponent<TrailRenderer>();
            if (_fireTrail == null) _fireTrail = Helper.FindChildWithName(gameObject, "Fire Trail");
            _fireTrail.SetActive(false);
            _trailRenderer.Clear();
            ResetValues();
            CacheWeaponAttributes();
            CalculateAccuracy();
        }
        
        private void CalculateAccuracy()
        {
            if (_guaranteeHit) _accuracy = 0;
            else if(_origin != null) _accuracy *= _origin.GetAccuracyModifier();
        }

        private void Initialise(Shot shot)
        {
            _origin = null;
            _direction = shot._direction;
            _weapon = shot._weapon;
            _originPosition = shot.transform.position;
            SetUpComponents();
        }

        public void ActivateFireTrail()
        {
            _fireTrail.SetActive(true);
        }

        public bool DidPierce()
        {
            return Random.Range(0f, 1f) <= _pierceChance;
        }

        private void CacheWeaponAttributes()
        {
            WeaponAttributes attributes = _weapon.WeaponAttributes;
            _damage = (int) attributes.GetCalculatedValue(AttributeType.Damage);
            _accuracy = _weapon.CalculateBaseAccuracy();
            _bleedChance = attributes.GetCalculatedValue(AttributeType.BleedChance);
            _burnChance = attributes.GetCalculatedValue(AttributeType.BurnChance);
            _sicknessChance = attributes.GetCalculatedValue(AttributeType.SicknessChance);
            _pierceChance = attributes.GetCalculatedValue(AttributeType.PierceChance);
            _criticalChance = attributes.GetCalculatedValue(AttributeType.CriticalChance);
        }

        public void SetDamageModifier(float modifier)
        {
            _finalDamageModifier = modifier;
        }

        private void CheckWillCrit()
        {
            IsCritical = _guaranteeCritical || Random.Range(0f, 1f) < _criticalChance;
        }

        private void FixedUpdate()
        {
            if (!_fired || _moving) return;
            _rigidBody.velocity = _direction * _speed;
            _moving = true;
        }

        public void Fire(float distance = 0.2f)
        {
            _age = 0;
            float angleOffset = Random.Range(-_accuracy, _accuracy);
            _speed = Random.Range(9f, 11f);
            transform.position = _originPosition + _direction * distance;
            _direction = Quaternion.AngleAxis(angleOffset, Vector3.forward) * _direction;
            _fired = true;
            _origin?.IncreaseRecoil();
            StartCoroutine(WaitToDie());
        }

        private IEnumerator WaitToDie()
        {
            while (_age < MaxAge)
            {
                _age += Time.deltaTime;
                yield return null;
            }

            DeactivateShot();
        }

        private void DeactivateShot()
        {
            _fireTrail.SetActive(false);
            _shotPool.Return(this);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            GameObject other = collision.gameObject;
            if (Random.Range(0f, 1f) < _burnChance)
            {
                FireBehaviour.Create(transform.position, 1f);
            }
            OnHitAction?.Invoke();
            EnemyBehaviour b = other.GetComponent<EnemyBehaviour>();
            if (b != null) ApplyDamage(b);
            DeactivateShot();
        }

        private void ApplyDamage(CharacterCombat hit)
        {
            CheckWillCrit();
            _damageDealt = IsCritical ? _damage * 2 : _damage;
            _damageDealt = (int) (_damageDealt * _finalDamageModifier);
            ApplyConditions(hit);
            hit.TakeDamage(this);
        }

        private void ApplyConditions(CharacterCombat hit)
        {
            if (_knockbackForce != 0)
            {
                if (Random.Range(0, 1f) < _knockDownChance)
                {
                    hit.Knockback(transform.position, _knockbackForce);
                }
            }

            if (Random.Range(0f, 1f) < _bleedChance) hit.Bleeding.AddStack();
            if (Random.Range(0f, 1f) < _sicknessChance) hit.Sick.AddStack();
        }

        public void GuaranteeHit() => _guaranteeHit = true;

        public void GuaranteeCritical() => _guaranteeCritical = true;

        public void AddOnHit(Action a) => OnHitAction += a;

        public void SetKnockdownChance(float chance, int distance)
        {
            Assert.IsTrue(distance >= 0);
            _knockDownChance = Mathf.Clamp(chance, 0f, 1f);
            _knockbackForce = distance;
        }

        public void SetBurnChance(float chance)
        {
            Assert.IsTrue(chance >= 0 && chance <= 1);
            _burnChance = chance;
        }

        public void SetBleedChance(float chance)
        {
            Assert.IsTrue(chance >= 0 && chance <= 1);
            _bleedChance = chance;
        }

        public void SetSicknessChance(float chance)
        {
            Assert.IsTrue(chance >= 0 && chance <= 1);
            _sicknessChance = chance;
        }

        public int DamageDealt()
        {
            return _damageDealt;
        }

        public void SetPierceChance(float chance)
        {
            Assert.IsTrue(chance >= 0 && chance <= 1);
            _pierceChance = chance;
        }

        public void SetAccuracy(float accuracy)
        {
            Assert.IsTrue(accuracy >= 0 && accuracy <= 1);
            _accuracy = accuracy;
        }
    }
}