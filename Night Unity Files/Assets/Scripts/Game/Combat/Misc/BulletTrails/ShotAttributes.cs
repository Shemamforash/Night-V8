using System;
using System.Collections.Generic;
using Game.Combat.Enemies;
using Game.Combat.Player;
using Game.Exploration.Weather;
using Game.Gear.Weapons;
using QuickEngine.Extensions;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Misc
{
    public class ShotAttributes
    {
        private const float MinimumAccuracyOffsetInDegrees = 20f;
        private const float MaximumAccuracyOffsetInDegrees = 40f;
        private const float MaxAge = 3f;
        private const float SeekDecay = 0.95f;

        private readonly CharacterCombat _origin;

        private Weapon _weapon;
        private event Action OnHitAction;

        private float _speed, _burnChance, _shatterChance, _sicknessChance, _accuracy, _knockBackForce, _knockBackModifier, _age, _seekModifier = 1;
        private float _finalDamageModifier = 1f;
        private bool _seekTarget;
        private int _damage, _damageDealt;
        public bool Piercing, Fired, HasHit;
        private int _condition = -1;

        public ShotAttributes(CharacterCombat origin)
        {
            _origin = origin;
            SetWeapon(origin.Weapon());
            CacheWeaponAttributes();
        }

        private void SetWeapon(Weapon weapon)
        {
            _weapon = weapon;
            switch (_weapon.WeaponType())
            {
                case WeaponType.Pistol:
                    _speed = 12.5f;
                    break;
                case WeaponType.Rifle:
                    _speed = 15;
                    break;
                case WeaponType.Shotgun:
                    _speed = 7.5f;
                    break;
                case WeaponType.SMG:
                    _speed = 10f;
                    break;
            }
        }

        public float GetSeekForce()
        {
            if (!_seekTarget) return -1;
            float force = 50;
            force *= _seekModifier;
            _seekModifier *= SeekDecay;
            return force;
        }

        private void CacheWeaponAttributes()
        {
            WeaponAttributes attributes = _weapon.WeaponAttributes;
            _damage = (int) attributes.Val(AttributeType.Damage);
            _accuracy = 1 - attributes.Val(AttributeType.Accuracy);
            _shatterChance = attributes.CalculateShatterChance();
            _burnChance = attributes.CalculateShatterChance();
            _sicknessChance = attributes.CalculateShatterChance();
            if (!(_origin is PlayerCombat)) return;
            _shatterChance += PlayerCombat.Instance.Player.Attributes.Val(AttributeType.Shatter);
            _burnChance += PlayerCombat.Instance.Player.Attributes.Val(AttributeType.Burn);
            _sicknessChance += PlayerCombat.Instance.Player.Attributes.Val(AttributeType.Sickness);
        }

        public float CalculateAccuracy()
        {
            if (_origin == null) return 0;
            float minAccuracy = MinimumAccuracyOffsetInDegrees * _accuracy;
            float maxAccuracy = MaximumAccuracyOffsetInDegrees * _accuracy;
            float recoilModifier = _origin.GetRecoilModifier();
            float currentAccuracy = Mathf.Lerp(minAccuracy, maxAccuracy, recoilModifier);
            return currentAccuracy;
        }

        public float GetSpeed()
        {
            return _speed;
        }

        public BulletTrail GetBulletTrail()
        {
            bool isPlayer = _origin is PlayerCombat;
            switch (_weapon.WeaponType())
            {
                case WeaponType.Pistol:
                    return PistolTrail.Create(isPlayer);
                case WeaponType.Rifle:
                    return RifleTrail.Create(isPlayer);
                case WeaponType.Shotgun:
                    return ShotgunTrail.Create(isPlayer);
                case WeaponType.SMG:
                    return SMGTrail.Create(isPlayer);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public BulletTrail GetConditionTrail()
        {
            switch (_condition)
            {
                case 0:
                    return ShatterTrail.Create();
                case 1:
                    return FireTrail.Create();
                case 2:
                    return VoidTrail.Create();
            }

            return null;
        }

        public void AddOnHit(Action a)
        {
            OnHitAction += a;
        }

        public void Seek()
        {
            _seekTarget = true;
        }

        public int DamageDealt()
        {
            return (int) (_damageDealt * _finalDamageModifier);
        }

        private void SetConditions()
        {
            float random = Random.Range(0f, 1f);
            float conditionModifier = _weapon.GetAttributeValue(AttributeType.Pellets) * _weapon.GetAttributeValue(AttributeType.Capacity);
            bool canDecay = random < _shatterChance / conditionModifier;
            bool canBurn = random < _burnChance / conditionModifier;
            bool canSicken = random < _sicknessChance / conditionModifier;
            List<int> conditions = new List<int>();
            if (canDecay) conditions.Add(0);
            if (canBurn) conditions.Add(1);
            if (canSicken) conditions.Add(2);
            if (conditions.Count == 0) return;
            _condition = conditions.GetRandomElement();
        }

        public void ApplyConditions(Vector2 position)
        {
            switch (_condition)
            {
                case 0:
                    DecayBehaviour.Create(position);
                    break;
                case 1:
                    FireBurstBehaviour.Create(position);
                    break;
                case 2:
                    SickenBehaviour.Create(position, new List<CanTakeDamage> {_origin});
                    break;
            }
        }

        public void DealDamage(GameObject other, Shot shot)
        {
            _damageDealt = _damage;
            OnHitAction?.Invoke();
            CanTakeDamage hit = other.GetComponent<CanTakeDamage>();
            if (hit == null) return;
            PlayerCombat player = _origin as PlayerCombat;
            CalculateKnockBackForce();
            hit.TakeShotDamage(shot);
            if (player != null) player.OnShotConnects(hit);
            _damage = Mathf.CeilToInt(_damage * 0.5f);
        }

        private void CalculateKnockBackForce()
        {
            float rainModifier = WeatherManager.CurrentWeather().Attributes.RainAmount;
            _knockBackModifier += rainModifier;
            _knockBackForce = _damageDealt / 4f * _knockBackModifier;
        }

        public void SetDamageModifier(float modifier)
        {
            _finalDamageModifier = modifier;
        }

        public float GetKnockBackForce()
        {
            return _knockBackForce;
        }

        public bool UpdateAge()
        {
            _age += Time.deltaTime;
            return _age < MaxAge;
        }

        public void Fire()
        {
            Fired = true;
            SetConditions();
        }
    }
}