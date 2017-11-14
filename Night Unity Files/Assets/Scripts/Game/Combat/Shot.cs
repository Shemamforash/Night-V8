using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Gear.Weapons;
using SamsHelper;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat
{
    public class Shot
    {
        private int _damage;
        private readonly Character _target, _origin;
        private readonly float _distanceToTarget;

        private float _maxRange;
        private int _pierceDepth;
        private float _pierceFalloff = 1;
        
        private int _splinterRange;
        private float _splinterFalloff = 1;
        
        private bool _guaranteeHit, _guaranteeCritical;
        
        private float _criticalChance, _hitChance;
        private int _noPellets, _noShots = 1;

        private float _knockbackDistance;
        
        private float _pierceChance, _burnChance, _bleedChance, _decayChance, _knockDownChance;

        private Action _onHitAction;
        private bool _didHit;

        public Shot(Character origin, Character target)
        {
            _origin = origin;
            _target = target;
            _distanceToTarget = origin.DistanceToCharacter(_target);
            if (_origin.RageActivated()) _guaranteeCritical = true;
            CacheWeaponAttributes();
            CalculateHitProbability();
            CalculateCriticalProbability();
        }
        
        private void CacheWeaponAttributes()
        {
            WeaponAttributes attributes = _origin.Weapon().WeaponAttributes;
            _damage = (int) attributes.Damage.GetCalculatedValue();
            _maxRange = attributes.Accuracy.GetCalculatedValue();
            _noPellets = _origin.Weapon().Pellets;
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
                    if (!WillHitTarget()) continue;
                    _didHit = true;
                    ApplyDamage();
                    ApplyConditions();
                }
                _origin.Weapon().ConsumeAmmo(1);
            }
        }

        private void ApplyDamage()
        {
            int pelletDamage = WillCrit() ? _damage * 2 : _damage;
            _target.TakeDamage(this, pelletDamage);
            ApplyPierce(pelletDamage);
            ApplySplinter(pelletDamage);
            _onHitAction?.Invoke();
        }
        
        private void ApplyPierce(int pelletDamage)
        {
            if (_pierceDepth == 0) return;
            _pierceChance = pelletDamage;
            List<Enemy> enemiesBehindTarget = CombatManager.GetEnemiesBehindTarget(_target);
            for (int i = 0; i < enemiesBehindTarget.Count; ++i)
            {
                if (i == _pierceDepth)
                {
                    break;
                }
                _pierceChance = (int) (_pierceChance * _pierceFalloff);
                enemiesBehindTarget[i].TakeDamage(this, pelletDamage);
            }
        }

        private void ApplySplinter(int pelletDamage)
        {
            if (_splinterRange <= 0) return;
            List<Enemy> enemiesInRange = CombatManager.GetEnemiesInRange(_target, _splinterRange);
            foreach (Enemy enemy in enemiesInRange)
            {
                float distance = _origin.DistanceToCharacter(enemy);
                float damageModifier = 1 - Helper.Normalise(distance, _splinterRange);
                damageModifier = damageModifier * (1 - _splinterFalloff);
                damageModifier += _splinterFalloff;
                int splinterDamage = (int) (damageModifier * pelletDamage);
                enemy.TakeDamage(this, splinterDamage);
            }
        }

        private void ApplyConditions()
        {
            if (Random.Range(0f, 1f) < _knockDownChance && _knockDownChance > 0)
            {
                _target.KnockDown();
            }
            if (_knockbackDistance != 0)
            {
                _target.Knockback(_origin, _knockbackDistance);
            }
        }

        private void CalculateHitProbability()
        {
            if (_guaranteeHit)
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
                _criticalChance = LogAndClamp(normalisedRange, _origin.Weapon().WeaponAttributes.CriticalChance.GetCalculatedValue() / 100);
            }
        }

        private float LogAndClamp(float normalisedRange, float extra = 0)
        {
            float probability = (float) (-0.35f * Math.Log(normalisedRange));
            probability += extra;
            probability = Mathf.Clamp(probability, 1, 0);
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

        public void SetSplinterRange(int range)
        {
            if (range < 0) range = 0;
            _splinterRange = range;
        }

        public void SetSplinterFalloff(float falloff)
        {
            _splinterFalloff = Mathf.Clamp(falloff, 0f, 1f);
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

        public void SetOnHit(Action a) => _onHitAction = a;

        public void SetKnockdownChance(float chance)
        {
            _knockDownChance = Mathf.Clamp(chance, 0f, 1f);
        }
        
        public void SetKnockbackDistance(int distance)
        {
            _knockbackDistance = distance < 0 ? 0 : distance;
        }

        public bool DidHit()
        {
            return _didHit;
        }

        public Character Origin()
        {
            return _origin;
        }
    }
}