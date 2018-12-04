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
        private const float MaxAge = 3f;
        private const float EnemyDamageModifier = 0.25f;
        private const float SeekDecay = 0.95f;

        private readonly CharacterCombat _origin;

        private Weapon _weapon;
        private event Action OnHitAction;

        private float _speed, _burnChance, _decayChance, _sicknessChance, _accuracy, _knockBackForce, _knockBackModifier, _age, _seekModifier = 1;
        private float _finalDamageModifier = 1f;
        private bool _seekTarget;
        private int _damage, _damageDealt;
        public bool Piercing, Fired, HasHit;

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
                    _speed = 20f;
                    break;
                case WeaponType.Rifle:
                    _speed = 25;
                    break;
                case WeaponType.Shotgun:
                    _speed = 15f;
                    break;
                case WeaponType.SMG:
                    _speed = 15f;
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
            _accuracy = _weapon.CalculateBaseAccuracy();
            _decayChance = attributes.Val(AttributeType.DecayChance);
            _burnChance = attributes.Val(AttributeType.BurnChance);
            _sicknessChance = attributes.Val(AttributeType.SicknessChance);
            if (!(_origin is PlayerCombat)) return;
            _decayChance += PlayerCombat.Instance.Player.Attributes.Val(AttributeType.DecayChance);
            _burnChance += PlayerCombat.Instance.Player.Attributes.Val(AttributeType.BurnChance);
            _sicknessChance += PlayerCombat.Instance.Player.Attributes.Val(AttributeType.SicknessChance);
        }

        public float CalculateAccuracy()
        {
            if (_origin == null) return 0;
            float accuracyDifference = Weapon.MaxAccuracyOffsetInDegrees - _accuracy;
            _accuracy += accuracyDifference * _origin.GetAccuracyModifier();
            return _accuracy;
        }

        public float GetSpeed()
        {
            return _speed;
        }

        public BulletTrail GetBulletTrail()
        {
            switch (_weapon.WeaponType())
            {
                case WeaponType.Pistol:
                    return PistolTrail.Create();
                case WeaponType.Rifle:
                    return RifleTrail.Create();
                case WeaponType.Shotgun:
                    return ShotgunTrail.Create();
                case WeaponType.SMG:
                    return SMGTrail.Create();
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            int damageDealt = _damageDealt;
            if (_origin is EnemyBehaviour) damageDealt = Mathf.CeilToInt(EnemyDamageModifier * _damageDealt);
            return damageDealt;
        }

        public void ApplyConditions(Vector2 position)
        {
            float random = Random.Range(0f, 1f);
            float conditionModifier = _weapon.GetAttributeValue(AttributeType.Pellets) * _weapon.GetAttributeValue(AttributeType.Capacity);
            bool canDecay = random < _decayChance / conditionModifier;
            bool canBurn = random < _burnChance / conditionModifier;
            bool canSicken = random < _sicknessChance / conditionModifier;
            List<int> conditions = new List<int>();
            if (canDecay) conditions.Add(0);
            if (canBurn) conditions.Add(1);
            if (canSicken) conditions.Add(2);
            if (conditions.Count == 0) return;
            int condition = conditions.GetRandomElement();
            switch (condition)
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
            _damageDealt = (int) (_damageDealt * _finalDamageModifier);
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
    }
}