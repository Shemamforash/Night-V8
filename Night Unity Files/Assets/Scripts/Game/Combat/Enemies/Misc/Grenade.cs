using System;
using System.Collections;
using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Misc
{
    public sealed class Grenade : MonoBehaviour
    {
        private static readonly float ThrowForce = 300f;
        private static readonly ObjectPool<Grenade> _grenadePool = new ObjectPool<Grenade>("Grenades", "Prefabs/Combat/Enemies/Grenade");
        private const int Damage = 20;
        private Action<List<EnemyBehaviour>> OnDetonate;
        private bool _incendiary, _decaying, _sickening;
        private float _radius = 1;
        private Rigidbody2D _rb2d;
        private bool _isPlayerGrenade;
        private readonly Collider2D[] _colliders = new Collider2D[50];

        public void Awake()
        {
            _rb2d = GetComponent<Rigidbody2D>();
        }

        public static Grenade CreateBasic(Vector2 origin, Vector2 target, bool isPlayerGrenade)
        {
            Grenade grenade = _grenadePool.Create();
            grenade.StartMoving(origin, target, isPlayerGrenade);
            return grenade;
        }

        private void ResetGrenade()
        {
            _incendiary = false;
            _decaying = false;
            _sickening = false;
            _radius = 1;
        }

        private void StartMoving(Vector2 origin, Vector2 target, bool isPlayerGrenade)
        {
            ResetGrenade();
            _isPlayerGrenade = isPlayerGrenade;
            transform.position = new Vector3(origin.x, origin.y, 0f);
            Vector2 direction = (target - origin).normalized;
            Vector2 force = direction * ThrowForce;
            _rb2d.AddForce(force);
        }

        public void SetExplosionRadius(float radius)
        {
            _radius = radius;
        }

        public static void CreateIncendiary(Vector2 origin, Vector2 target, bool isPlayerGrenade)
        {
            Grenade g = CreateBasic(origin, target, isPlayerGrenade);
            g._incendiary = true;
        }

        public static void CreateDecay(Vector2 origin, Vector2 target, bool isPlayerGrenade)
        {
            Grenade g = CreateBasic(origin, target, isPlayerGrenade);
            g._decaying = true;
        }

        public static void CreateSickness(Vector2 origin, Vector2 target, bool isPlayerGrenade)
        {
            Grenade g = CreateBasic(origin, target, isPlayerGrenade);
            g._sickening = true;
        }

        public void Update()
        {
            if (_rb2d.velocity.magnitude > 0.5f)
            {
                int layerMask = _isPlayerGrenade ? 1 << 24 | 1 << 10 : 1 << 17;
                int hits = Physics2D.OverlapCircleNonAlloc(transform.position, _radius / 2f, _colliders, layerMask);
                if (hits == 0) return;
                if (_isPlayerGrenade && transform.Distance(PlayerCombat.Instance.transform) < 1) return;
            }

            Detonate();
        }

        private void OnDestroy()
        {
            _grenadePool.Dispose(this);
        }

        private void Detonate()
        {
            int damage = Mathf.CeilToInt(Damage + WorldState.NormalisedDifficulty() * Damage);
            Explosion explosion = Explosion.CreateExplosion(transform.position, damage, _radius);
            explosion.AddOnDetonate(OnDetonate);
            if (_incendiary) explosion.SetBurn();
            if (_decaying) explosion.SetDecay();
            if (_sickening) explosion.SetSicken();
            explosion.InstantDetonate();
            _grenadePool.Return(this);
        }

        public void AddOnDetonate(Action<List<EnemyBehaviour>> action)
        {
            OnDetonate += action;
        }
    }
}