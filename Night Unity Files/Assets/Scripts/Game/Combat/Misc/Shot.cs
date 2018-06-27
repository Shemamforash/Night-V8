using System;
using System.Collections;
using Game.Combat.Enemies;
using Game.Combat.Player;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Game.Combat.Misc
{
    public class Shot : MonoBehaviour
    {
        private const float Speed = 25f;
        private static GameObject _bulletPrefab;
        private BulletTrailFade _bulletTrail;

        private static readonly ObjectPool<Shot> _shotPool = new ObjectPool<Shot>("Prefabs/Combat/Bullet");
        private float _accuracy;
        private float _age;

        private int _damage;
        private int _damageDealt;
        private Vector3 _direction;
        private float _finalDamageModifier = 1f;
        private GameObject _fireTrail;
        private bool _guaranteeHit;
        private int _knockbackForce = 10;

        private bool _moving, _fired;
        private CharacterCombat _origin;
        private Vector3 _originPosition;
        private float _pierceChance, _burnChance, _decayChange, _sicknessChance;
        private Rigidbody2D _rigidBody;

        private static Transform _shotParent;
        private Weapon _weapon;
        public bool DidHit;
        private const float MaxAge = 3f;
        private event Action OnHitAction;


        private void OnDestroy()
        {
            _shotPool.Dispose(this);
            if (_shotPool.Empty()) _shotParent = null;
        }

        private void ResetValues()
        {
            _finalDamageModifier = 1f;
            _guaranteeHit = false;
            _knockbackForce = 10;
            _damageDealt = 0;
            _moving = false;
            _fired = false;
            DidHit = false;
            OnHitAction = null;
        }

        public static Shot Create(CharacterCombat origin)
        {
            if (_shotParent == null) _shotParent = GameObject.Find("World").transform.Find("Bullets");
            Shot shot = _shotPool.Create(_shotParent);
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

        private void Initialise(CharacterCombat origin, Vector3 direction)
        {
            _origin = origin;
            _direction = direction;
            _weapon = origin.Weapon();
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
            if (_rigidBody == null) _rigidBody = GetComponent<Rigidbody2D>();
            if (_fireTrail == null) _fireTrail = Helper.FindChildWithName(gameObject, "Fire Trail");
            _fireTrail.SetActive(false);
            ResetValues();
            CacheWeaponAttributes();
            CalculateAccuracy();
        }

        private void CalculateAccuracy()
        {
            if (_guaranteeHit) _accuracy = 0;
        }

        public void ActivateFireTrail()
        {
            _fireTrail.SetActive(true);
        }

        private void CacheWeaponAttributes()
        {
            WeaponAttributes attributes = _weapon.WeaponAttributes;
            _damage = (int) attributes.Val(AttributeType.Damage);
            _accuracy = _weapon.CalculateBaseAccuracy();
            _decayChange = attributes.Val(AttributeType.DecayChance);
            _burnChance = attributes.Val(AttributeType.BurnChance);
            _sicknessChance = attributes.Val(AttributeType.SicknessChance);
        }

        public void SetDamageModifier(float modifier)
        {
            _finalDamageModifier = modifier;
        }

        private void FixedUpdate()
        {
            if (!_fired || _moving) return;
            _rigidBody.velocity = _direction * Speed * Random.Range(0.9f, 1.1f);
            _moving = true;
        }

        private IEnumerator WaitToDie()
        {
            _age = 0;
            while (_age < MaxAge)
            {
                _age += Time.deltaTime;
                _bulletTrail.SetAlpha(1f - _age / MaxAge);
                yield return null;
            }

            _rigidBody.velocity = Vector2.zero;
            _bulletTrail.StartFade(0f);
            DeactivateShot();
        }

        public void Fire(float distance = 0.15f)
        {
            float angleOffset = Random.Range(-_accuracy, _accuracy);
            if (_origin != null) angleOffset *= _origin.GetAccuracyModifier();
            transform.position = _originPosition + _direction * distance;
            _direction = Quaternion.AngleAxis(angleOffset, Vector3.forward) * _direction;
            _fired = true;
            if (_origin != null) _origin.IncreaseRecoil();

            _bulletTrail = BulletTrailFade.Create();
            _bulletTrail.SetAlpha(1);

            _bulletTrail.SetPosition(transform);
            StartCoroutine(WaitToDie());
        }

        private void DeactivateShot()
        {
            _fireTrail.SetActive(false);
            _shotPool.Return(this);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            GameObject other = collision.gameObject;
            if (other.gameObject != null)
            {
                if (collision.contacts.Length > 0)
                {
                    float angle = AdvancedMaths.AngleFromUp(Vector2.zero, _direction) + 180 + Random.Range(-10f, 10f);
                    BulletImpactBehaviour.Create(collision.contacts[0].point, angle);
                }

                if (Random.Range(0f, 1f) < _burnChance) FireBehaviour.Create(transform.position, 1f);
                OnHitAction?.Invoke();
                EnemyBehaviour b = other.GetComponent<EnemyBehaviour>();
                if (b != null) ApplyDamage(b);
            }

            DeactivateShot();
            _bulletTrail.StartFade(0.2f);
        }

        private void ApplyDamage(CharacterCombat hit)
        {
            _damageDealt = _damage;
            _damageDealt = (int) (_damageDealt * _finalDamageModifier);
            ApplyConditions(hit);
            hit.TakeDamage(this);
        }

        private void ApplyConditions(CharacterCombat hit)
        {
            if (_knockbackForce != 0)
            {
                hit.Knockback(transform.position, _knockbackForce);
            }

            if (Random.Range(0f, 1f) < _decayChange) hit.Decay();
            if (Random.Range(0f, 1f) < _sicknessChance) hit.Sicken();
            if (Random.Range(0f, 1f) < _burnChance) hit.Burn();
        }

        public void GuaranteeHit()
        {
            _guaranteeHit = true;
        }

        public void AddOnHit(Action a)
        {
            OnHitAction += a;
        }

        public void SetKnockbackForce(int force)
        {
            Assert.IsTrue(force >= 0);
            _knockbackForce = force;
        }

        public void SetBurnChance(float chance)
        {
            Assert.IsTrue(chance >= 0 && chance <= 1);
            _burnChance = chance;
        }

        public void SetDecayChance(float chance)
        {
            Assert.IsTrue(chance >= 0 && chance <= 1);
            _decayChange = chance;
        }

        public void SetSicknessChance(float chance)
        {
            Assert.IsTrue(chance >= 0 && chance <= 1);
            _sicknessChance = chance;
        }

        public int DamageDealt() => _damageDealt;

        public void SetAccuracy(float accuracy)
        {
            Assert.IsTrue(accuracy >= 0 && accuracy <= 180);
            _accuracy = accuracy;
        }
    }
}