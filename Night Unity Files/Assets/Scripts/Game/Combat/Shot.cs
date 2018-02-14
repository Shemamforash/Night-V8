using System;
using System.Collections.Generic;
using Facilitating.Audio;
using Game.Characters;
using Game.Characters.Player;
using Game.Combat.Enemies;
using Game.Gear.Weapons;
using SamsHelper;
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
        private Weapon _weapon;

        private int _noPellets = 1, _noShots = 1;
        private int _range;
        private int _pierceDepth;

        private bool _guaranteeHit, _guaranteeCritical;

        private float _criticalChance;

        private int _knockbackDistance;

        private float _pierceChance, _burnChance, _bleedChance, _sicknessChance, _knockDownChance;
        private float _finalDamageModifier = 1f;

        private event Action OnHitAction;
        private bool _didHit;

        public Shot(CharacterCombat target, CharacterCombat origin)
        {
            Assert.IsNotNull(target);
            _origin = origin;
            _target = target;

            if (_origin != null)
            {
                DetailedEnemyCombat enemy = origin as DetailedEnemyCombat;
                _distanceToTarget = enemy?.DistanceToPlayer ?? ((DetailedEnemyCombat) target).DistanceToPlayer;
                CacheWeaponAttributes();
                float distance = origin is Player ? 0 : _distanceToTarget;
                GunFire.Fire(origin.Weapon().WeaponType(), distance);
            }
        }

        public bool IsCritical { get; set; }

        private void CacheWeaponAttributes()
        {
            WeaponAttributes attributes = _origin.Weapon().WeaponAttributes;
            _damage = (int) attributes.GetCalculatedValue(AttributeType.Damage);
//            if (_origin is Enemy) _damage = (int)Mathf.Ceil(_damage / 2f);
            _range = (int) attributes.GetCalculatedValue(AttributeType.Range);
            _noPellets = (int) attributes.GetCalculatedValue(AttributeType.Pellets);
            _bleedChance = attributes.GetCalculatedValue(AttributeType.BleedChance);
            _burnChance = attributes.GetCalculatedValue(AttributeType.BurnChance);
            _sicknessChance = attributes.GetCalculatedValue(AttributeType.SicknessChance);
            _criticalChance = _origin.Weapon().WeaponAttributes.CriticalChance.CurrentValue();
        }

        private void SetDamage(int damage)
        {
            _damage = damage;
        }

        public void SetDamageModifier(float modifier)
        {
            _finalDamageModifier = modifier;
        }

        private bool WillHitTarget()
        {
            float hitChance = _origin?.GetHitChance(_target) ?? 1;
            if (_guaranteeHit && hitChance != 0) return true;
            return Random.Range(0f, 1f) < hitChance;
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

        private const float BulletSpeed = 500;

        public void Fire()
        {
            for (int j = 0; j < _noShots; ++j)
            {
                if (_origin != null)
                {
                    if (_origin.Weapon().Empty()) break;
                    _origin.Weapon().ConsumeAmmo(1);
                }

                CreateShotCooldown();
            }
        }

        private void CreateShotCooldown()
        {
            Cooldown shotCooldown = new Cooldown(CombatManager.CombatCooldowns);
            shotCooldown.Duration = _distanceToTarget / BulletSpeed;
            shotCooldown.SetEndAction(() =>
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
            });
            shotCooldown.Start();
        }

        private void ApplyDamage()
        {
            bool isCritical = WillCrit();
            int pelletDamage = isCritical ? _damage * 2 : _damage;
            pelletDamage = (int) (pelletDamage * _finalDamageModifier);
            _damageDealt += pelletDamage;
            ApplyPierce(pelletDamage);
            ApplyConditions();
            OnHitAction?.Invoke();
            (_origin as PlayerCombat)?.RageController.Increase(pelletDamage);
            PlayerCombat player = _target as PlayerCombat;
            if (player != null)
            {
                player.OnHit(this, pelletDamage);
            }
            else
            {
                _target.OnHit(pelletDamage, IsCritical);
            }
        }

        private void ApplyPierce(int pelletDamage)
        {
            if (_pierceDepth == 0) return;
            if (Random.Range(0f, 1f) > _pierceChance) return;
            if (_target is PlayerCombat) return;
            List<DetailedEnemyCombat> enemiesBehindTarget = CombatManager.GetEnemiesBehindTarget((DetailedEnemyCombat) _target);
            for (int i = 0; i < enemiesBehindTarget.Count; ++i)
            {
                if (i == _pierceDepth)
                {
                    break;
                }

                int pierceDamage = (int) (pelletDamage * Math.Pow(1, i + 1));
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

            if (Random.Range(0f, 1f) < _bleedChance) _target.Bleeding.AddStack();
            if (Random.Range(0f, 1f) < _burnChance) _target.Burn.AddStack();
            if (Random.Range(0f, 1f) < _sicknessChance) _target.Sick.AddStack();
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

        public CharacterCombat Origin()
        {
            return _origin;
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