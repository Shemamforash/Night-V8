using System;
using System.Collections.Generic;
using Facilitating.Audio;
using Game.Characters;
using Game.Characters.Player;
using Game.Combat.Enemies;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Game.Combat
{
    public class Shot
    {
        private int _damage, _damageDealt;
        private readonly Character _target, _origin;
        private readonly float _distanceToTarget;
        private Weapon _weapon;

        private int _noPellets = 1, _noShots = 1;
        private int _maxRange;
        private int _pierceDepth;
        private float _pierceFalloff = 1;

        private bool _guaranteeHit, _guaranteeCritical;

        private float _criticalChance, _hitChance;

        private int _knockbackDistance;

        private float _pierceChance, _burnChance, _bleedChance, _sicknessChance, _knockDownChance;

        private event Action OnHitAction;
        private bool _didHit;

        public Shot(Character target, Character origin)
        {
            Assert.IsNotNull(target);
            _origin = origin;
            _target = target;

            if (_origin != null)
            {
                _distanceToTarget = CombatManager.DistanceBetween(_origin, _target);
                CacheWeaponAttributes();
                CalculateHitProbability();
                CalculateCriticalProbability();
                float distance = origin is Player ? 0 : _distanceToTarget;
                distance = distance / 150f;
                GunFire.Fire(origin.Weapon().WeaponType(), distance);
            }
            else
            {
                _hitChance = 1;
            }
        }

        private void CacheWeaponAttributes()
        {
            WeaponAttributes attributes = _origin.Weapon().WeaponAttributes;
            _damage = (int) attributes.GetCalculatedValue(AttributeType.Damage);
            _maxRange = (int) attributes.GetCalculatedValue(AttributeType.Accuracy);
            _noPellets = (int) attributes.GetCalculatedValue(AttributeType.Pellets);
        }

        public void SetDamage(int damage)
        {
            _damage = damage;
        }

        private bool WillHitTarget()
        {
            if (_hitChance <= 0f) return false;
            return Random.Range(0f, 1f) <= _hitChance;
        }

        private bool WillCrit()
        {
            if (_criticalChance <= 0f) return false;
            return Random.Range(0f, 1f) <= _criticalChance;
        }

        public void Fire()
        {
            for (int j = 0; j < _noShots; ++j)
            {
                for (int i = 0; i < _noPellets; ++i)
                {
                    if (!WillHitTarget())
                    {
                        _target.OnMiss();
                        continue;
                    }

                    ApplyDamage();
                }

                _origin?.Weapon().ConsumeAmmo(1);
            }
        }

        private void ApplyDamage()
        {
            bool isCritical = WillCrit();
            int pelletDamage = isCritical ? _damage * 2 : _damage;
            _damageDealt += pelletDamage;
            ApplyPierce(pelletDamage);
            ApplyConditions();
            OnHitAction?.Invoke();
            (_origin as Player)?.RageController.Increase(pelletDamage);
            _target.OnHit(this, pelletDamage, isCritical);
        }

        private void ApplyPierce(int pelletDamage)
        {
            if (_pierceDepth == 0) return;
            if (Random.Range(0f, 1f) > _pierceChance) return;
            if (_target is Player) return;
            List<Enemy> enemiesBehindTarget = CombatManager.GetEnemiesBehindTarget((Enemy) _target);
            for (int i = 0; i < enemiesBehindTarget.Count; ++i)
            {
                if (i == _pierceDepth)
                {
                    break;
                }

                int pierceDamage = (int) (pelletDamage * Math.Pow(_pierceFalloff, i + 1));
                Shot s = new Shot(enemiesBehindTarget[i], null);
                s.SetDamage(pierceDamage);
                s.GuaranteeHit();
                s.Fire();
            }
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
            if (Random.Range(0f, 1f) < _bleedChance) _target.AddBleedStack();
            if (Random.Range(0f, 1f) < _burnChance) _target.AddBleedStack();
            if (Random.Range(0f, 1f) < _sicknessChance) _target.AddSicknessStack();
        }

        private void CalculateHitProbability()
        {
            if (_guaranteeHit || _origin == null)
            {
                _hitChance = 1;
            }
            else
            {
                _hitChance = _maxRange / _distanceToTarget;
                if (_hitChance > 1)
                {
                    _hitChance = 1;
                }
            }
        }

        private void CalculateCriticalProbability()
        {
            if (_guaranteeCritical)
            {
                _criticalChance = 1;
            }
            else
            {
                float normalisedRange = Helper.Normalise(_distanceToTarget, _maxRange);
                _criticalChance = LogAndClamp(normalisedRange, _origin.Weapon().WeaponAttributes.CriticalChance.CurrentValue() / 100);
            }
        }

        private float LogAndClamp(float normalisedRange, float extra = 0)
        {
            float probability = (float) (-0.35f * Math.Log(normalisedRange));
            probability += extra;
            probability = Mathf.Clamp(probability, 0, 1);
            return probability;
        }

        public void SetPierceDepth(int depth)
        {
            if (depth < 0) depth = 0;
            _pierceDepth = depth;
        }

        public void SetPierceChance(float chance)
        {
            _pierceChance = Mathf.Clamp(chance, 0f, 1f);
        }

        public void SetPierceFalloff(float falloff)
        {
            _pierceFalloff = Mathf.Clamp(falloff, 0f, 1f);
        }

        public void SetNumberOfShots(int noShots)
        {
            _noShots = noShots;
        }

        public void UseRemainingShots()
        {
            _noShots = _origin.Weapon().GetRemainingAmmo();
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

        public Character Origin()
        {
            return _origin;
        }
        
        public Character Target()
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