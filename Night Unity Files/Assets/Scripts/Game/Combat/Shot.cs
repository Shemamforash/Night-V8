using System;
using System.Collections;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.U2D;
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
        private Vector3 _targetPosition;
        public bool IsCritical;
        private bool _moving, _fired;
        private float _speed;
        private float _age;
        private const float MaxAge = 3f;

        private float _knockDownChance;
        private float _finalDamageModifier = 1f;
        private bool _guaranteeHit, _guaranteeCritical;
        private int _knockbackDistance;
        private int _damageDealt;
        private int _pierceDepth;
        public bool DidHit;
        private event Action OnHitAction;
        private static GameObject _bulletPrefab;

        private static readonly List<Shot> _shotPool = new List<Shot>();
        
        private Transform _shotParent;

        public void Awake()
        {
            if(_shotParent == null) _shotParent = GameObject.Find("World").transform.Find("Footsteps");
        }

        private void OnDestroy()
        {
            _shotPool.Remove(this);
        }

        private void ResetValues()
        {
            _knockDownChance = 0;
            _finalDamageModifier = 1f;
            _guaranteeHit = false;
            _guaranteeCritical = false;
            _knockbackDistance = 0;
            _damageDealt = 0;
            _pierceDepth = 0;
            _moving = false;
            _fired = false;
            DidHit = false;
            OnHitAction = null;
        }

        public static Shot CreateShot(CharacterCombat origin)
        {
            Shot shot;
            if (_shotPool.Count == 0)
            {
                if (_bulletPrefab == null) _bulletPrefab = Resources.Load<GameObject>("Prefabs/Combat/Bullet");
                GameObject bullet = Instantiate(_bulletPrefab);
                shot = bullet.AddComponent<Shot>();
                bullet.layer = 11;
            }
            else
            {
                shot = _shotPool[0];
                shot.gameObject.SetActive(true);
                _shotPool.RemoveAt(0);
            }

            shot.Initialise(origin, origin.GetTarget());
            return shot;
        }

        private void Initialise(CharacterCombat origin, CharacterCombat target)
        {
            _origin = origin;
            _targetPosition = target.transform.position;
            ResetValues();
            CacheWeaponAttributes();
        }

        public bool DidPierce()
        {
            return Random.Range(0f, 1f) <= _pierceChance;
        }

        private void CacheWeaponAttributes()
        {
            WeaponAttributes attributes = _origin.Weapon().WeaponAttributes;
            _damage = (int) attributes.GetCalculatedValue(AttributeType.Damage);
            _accuracy = _origin.Weapon().CalculateBaseAccuracy();
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

        private void CalculateAccuracy()
        {
            if (_guaranteeHit) _accuracy = 0;
            else _accuracy *= _origin.RecoilManager.GetAccuracyModifier();
        }

        private bool WillCrit()
        {
            if (_guaranteeCritical)
            {
                IsCritical = true;
            }
            else
            {
                IsCritical = Random.Range(0f, 1f) < _criticalChance;
            }

            return IsCritical;
        }

        private Vector3 direction;
        
        private void FixedUpdate()
        {
            if (!_fired && !_moving) return;
            GetComponent<Rigidbody2D>().velocity = direction * _speed;
            _origin?.RecoilManager.Increment(_origin.Weapon());
            _moving = true;
        }

        public void Fire()
        {
            _age = 0;
            CalculateAccuracy();
            float angleOffset = Random.Range(-_accuracy, _accuracy);
            _speed = Random.Range(9f, 11f);

            direction = (_targetPosition - _origin.transform.position).normalized;
            transform.position = _origin.transform.position + direction * 0.2f;
            direction = Quaternion.AngleAxis(angleOffset, Vector3.forward) * direction;
            _fired = true;
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
            gameObject.SetActive(false);
            _shotPool.Add(this);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            GameObject other = collision.gameObject;
            EnemyBehaviour b = other.GetComponent<EnemyBehaviour>();
            if(b != null) ApplyDamage(b);
            DeactivateShot();
        }

        private void ApplyDamage(CharacterCombat hit)
        {
            WillCrit();
            _damageDealt = IsCritical ? _damage * 2 : _damage;
            _damageDealt = (int) (_damageDealt * _finalDamageModifier);
            ApplyConditions(hit);
            OnHitAction?.Invoke();
            hit.TakeDamage(this);
        }

        private void ApplyConditions(CharacterCombat hit)
        {
            if (_knockbackDistance != 0)
            {
                if (Random.Range(0, 1f) < _knockDownChance)
                {
                    hit.Knockback(_knockbackDistance);
                }
            }

            if (Random.Range(0f, 1f) < _bleedChance) hit.Bleeding.AddStack();
            if (Random.Range(0f, 1f) < _burnChance) hit.Burn.AddStack();
            if (Random.Range(0f, 1f) < _sicknessChance) hit.Sick.AddStack();
        }

        public void GuaranteeHit() => _guaranteeHit = true;

        public void GuaranteeCritical() => _guaranteeCritical = true;

        public void AddOnHit(Action a) => OnHitAction += a;

        public void SetKnockdownChance(float chance, int distance)
        {
            Assert.IsTrue(distance >= 0);
            _knockDownChance = Mathf.Clamp(chance, 0f, 1f);
            _knockbackDistance = distance;
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
    }
}