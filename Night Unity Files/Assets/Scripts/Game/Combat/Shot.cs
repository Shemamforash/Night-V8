using System;
using System.Collections;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Game.Combat
{
    public class Shot : MonoBehaviour
    {
        private int _damage, _damageDealt;
        private CharacterCombat _target, _origin;

        private int _pierceDepth;

        private bool _guaranteeHit, _guaranteeCritical;

        private float _criticalChance;
        private float _accuracy;

        private int _knockbackDistance;

        private float _pierceChance, _burnChance, _bleedChance, _sicknessChance, _knockDownChance;
        private float _finalDamageModifier = 1f;

        private event Action OnHitAction;
        private bool _didHit;
        private bool _isCritical;
        public bool DidHit;

        private float _speed = 5;
        private float _age;
        private const float MaxAge = 5f;

        public static Shot CreateShot(CharacterCombat origin)
        {
            Assert.IsNotNull(origin.GetTarget());
            Assert.IsNotNull(origin);
            GameObject bullet = Instantiate(Resources.Load<GameObject>("Prefabs/Combat/Bullet"));
            Shot shot = bullet.AddComponent<Shot>();
            shot.Initialise(origin, origin.GetTarget());
            return shot;
        }

        private void Initialise(CharacterCombat origin, CharacterCombat target)
        {
            _origin = origin;
            _target = target;
            CacheWeaponAttributes();
        }

        private bool DidPierce()
        {
            return Random.Range(0f, 1f) <= _pierceChance;
        }

        private void CacheWeaponAttributes()
        {
            WeaponAttributes attributes = _origin.Weapon().WeaponAttributes;
            _damage = (int) attributes.GetCalculatedValue(AttributeType.Damage);
            _accuracy = 1f - attributes.GetCalculatedValue(AttributeType.Range) / 100f;
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
            _accuracy *= 45f;
            if (_guaranteeHit) _accuracy = 0;
            else
            {
                _accuracy *= _origin.RecoilManager.GetAccuracyModifier();
            }
        }

        private bool WillCrit()
        {
            if (_guaranteeCritical)
            {
                _isCritical = true;
            }
            else
            {
                _isCritical = Random.Range(0f, 1f) < _criticalChance;
            }

            return _isCritical;
        }

        public void Fire()
        {
            transform.position = _origin.CharacterController.Position();
            CalculateAccuracy();
            float angleOffset = Random.Range(-_accuracy, _accuracy);
            float angleToTarget = -AdvancedMaths.AngleFromUp(transform, _target.CharacterController.transform);
            angleToTarget += angleOffset;
            Vector3 bulletRot = new Vector3(0, 0, angleToTarget);
            _speed = Random.Range(4.8f, 5.2f);
            transform.rotation = Quaternion.Euler(bulletRot);
            StartCoroutine(Move());
            _origin?.RecoilManager.Increment(_origin.Weapon());
        }


        private IEnumerator Move()
        {
            while (_age < MaxAge)
            {
                transform.Translate(Vector3.up * Time.deltaTime * _speed);
                _age += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<CombatCharacterController> ()== _origin.CharacterController) return;
            ApplyDamage(other.GetComponent<CombatCharacterController>().Owner());
            Destroy(gameObject);
        }

        private void ApplyDamage(CharacterCombat hit)
        {
            bool isCritical = WillCrit();
            int totalDamage = isCritical ? _damage * 2 : _damage;
            totalDamage = (int) (totalDamage * _finalDamageModifier);
            _damageDealt = totalDamage;
            ApplyConditions();
            OnHitAction?.Invoke();
            (_origin as PlayerCombat)?.RageController.Increase(totalDamage);
            float armourModifier = DidPierce() ? 1 : 1 - hit.ArmourController.CurrentArmour() / 10f;
            float healthDamage = (int) (armourModifier * DamageDealt());
            float armourDamage = (int) ((1 - armourModifier) * DamageDealt());
            if (healthDamage != 0) hit.HealthController.TakeDamage(healthDamage);
            if (armourDamage != 0) hit.ArmourController.TakeDamage(armourDamage);
            if (_isCritical) (hit as DetailedEnemyCombat)?.UiHitController.RegisterCritical();
        }

        private void ApplyConditions()
        {
            if (_knockbackDistance != 0)
            {
                if (Random.Range(0, 1f) < _knockDownChance)
                {
                    _target.Knockback(_knockbackDistance);
                }
            }

            if (Random.Range(0f, 1f) < _bleedChance) _target.Bleeding.AddStack();
            if (Random.Range(0f, 1f) < _burnChance) _target.Burn.AddStack();
            if (Random.Range(0f, 1f) < _sicknessChance) _target.Sick.AddStack();
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

        public CharacterCombat Target()
        {
            return _target;
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