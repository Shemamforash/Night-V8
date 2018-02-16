using System;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Game.Combat
{
    public class Shot
    {
        private int _damage, _damageDealt;
        private readonly CharacterCombat _target, _origin;
        private readonly float _distanceToTarget;

        private int _pierceDepth;

        private bool _guaranteeHit, _guaranteeCritical;

        private float _criticalChance;

        private int _knockbackDistance;

        private float _pierceChance, _burnChance, _bleedChance, _sicknessChance, _knockDownChance;
        private float _finalDamageModifier = 1f;

        private event Action OnHitAction;
        private bool _didHit;
        private bool _isCritical;
        public bool DidHit;
        
        public Shot(CharacterCombat target, CharacterCombat origin)
        {
            Assert.IsNotNull(target);
            Assert.IsNotNull(origin);
            _origin = origin;
            _target = target;
            DetailedEnemyCombat enemy = origin as DetailedEnemyCombat;
            _distanceToTarget = enemy?.DistanceToPlayer ?? ((DetailedEnemyCombat) target).DistanceToPlayer;
            CacheWeaponAttributes();
        }

        private bool DidPierce()
        {
            return Random.Range(0f, 1f) < _pierceChance;
        }

        private void CacheWeaponAttributes()
        {
            WeaponAttributes attributes = _origin.Weapon().WeaponAttributes;
            _damage = (int) attributes.GetCalculatedValue(AttributeType.Damage);
//            if (_origin is Enemy) _damage = (int)Mathf.Ceil(_damage / 2f);
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

        private void CalculateIfWillHit()
        {
            if (_target.InCover) return;
            float hitChance = _origin?.GetHitChance(_target) ?? 1;
            if (_guaranteeHit && hitChance > 0) DidHit = true;
            DidHit = Random.Range(0f, 1f) < hitChance;
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

        private const float BulletSpeed = 500;

        public void Fire()
        {
            if (_origin.Weapon().Empty()) return;
            _origin.Weapon().ConsumeAmmo(1);
            CreateShotCooldown();
            CalculateIfWillHit();
            _origin?.RecoilManager.Increment(_origin.Weapon());
        }

        private void CreateShotCooldown()
        {
            Cooldown shotCooldown = new Cooldown(CombatManager.CombatCooldowns);
            shotCooldown.Duration = _distanceToTarget / BulletSpeed;
            shotCooldown.SetEndAction(() =>
            {
                if (!DidHit)
                {
                    _target.OnMiss();
                    return;
                }

                ApplyDamage();
            });
            shotCooldown.Start();
        }

        private void ApplyDamage()
        {
            bool isCritical = WillCrit();
            int totalDamage = isCritical ? _damage * 2 : _damage;
            totalDamage = (int) (totalDamage * _finalDamageModifier);
            _damageDealt = totalDamage;
            ApplyConditions();
            OnHitAction?.Invoke();
            (_origin as PlayerCombat)?.RageController.Increase(totalDamage);
            float armourModifier = DidPierce() ? 1 : 1 - _target.ArmourController.CurrentArmour() / 10f;
            float healthDamage = (int) (armourModifier * DamageDealt());
            float armourDamage = (int) ((1 - armourModifier) * DamageDealt());
            if (healthDamage != 0) _target.HealthController.TakeDamage(healthDamage);
            if (armourDamage != 0) _target.ArmourController.TakeDamage(armourDamage);
            if (_isCritical) (_target as DetailedEnemyCombat)?.UiHitController.RegisterCritical();
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
    }
}