using System;
using System.Collections;
using System.Collections.Generic;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Misc
{
    public class Grenade : MonoBehaviour
    {
        private static readonly float ThrowForce = 75f;
        private static readonly ObjectPool<Grenade> _grenadePool = new ObjectPool<Grenade>("Grenades", "Prefabs/Combat/Enemies/Grenade");
        private readonly int _damage = 20;
        private Action<List<EnemyBehaviour>> OnDetonate;
        private bool _incendiary, _decaying, _sickening;
        private bool _instantDetonate;
        private float _radius = 1;
        private Rigidbody2D _rb2d;
        private Transform _target;
        private Vector2 _origin;

        public void Awake()
        {
            _rb2d = GetComponent<Rigidbody2D>();
        }

        public static Grenade CreateBasic(Vector2 origin, Transform target, bool instantDetonate = false)
        {
            Grenade grenade = _grenadePool.Create();
            grenade._instantDetonate = instantDetonate;
            grenade.StartMoving(origin, target);
            return grenade;
        }

        public static Grenade CreateBasic(Vector2 origin, Vector2 target, bool instantDetonate = false)
        {
            Grenade grenade = _grenadePool.Create();
            grenade._instantDetonate = instantDetonate;
            grenade.StartMoving(origin, target);
            return grenade;
        }

        private void StartMoving(Vector2 origin, Vector2 target)
        {
            transform.position = origin;
            Vector2 direction = target - origin;
            float distance = direction.magnitude;
            direction /= distance;
            Vector2 force = direction * distance * ThrowForce;
            _rb2d.AddForce(force);
        }

        public void SetExplosionRadius(float radius)
        {
            _radius = radius;
        }

        public static void CreateIncendiary(Vector2 origin, Transform target, bool instantDetonate = false)
        {
            Grenade g = CreateBasic(origin, target, instantDetonate);
            g._incendiary = true;
        }

        public static void CreateDecay(Vector2 origin, Transform target, bool instantDetonate = false)
        {
            Grenade g = CreateBasic(origin, target, instantDetonate);
            g._decaying = true;
        }

        public static void CreateSickness(Vector2 origin, Transform target, bool instantDetonate = false)
        {
            Grenade g = CreateBasic(origin, target, instantDetonate);
            g._sickening = true;
        }

        public static void CreateIncendiary(Vector2 origin, Vector2 target, bool instantDetonate = false)
        {
            Grenade g = CreateBasic(origin, target, instantDetonate);
            g._incendiary = true;
        }

        public static void CreateDecay(Vector2 origin, Vector2 target, bool instantDetonate = false)
        {
            Grenade g = CreateBasic(origin, target, instantDetonate);
            g._decaying = true;
        }

        public static void CreateSickness(Vector2 origin, Vector2 target, bool instantDetonate = false)
        {
            Grenade g = CreateBasic(origin, target, instantDetonate);
            g._sickening = true;
        }

        private void StartMoving(Vector3 origin, Transform target)
        {
            _origin = origin;
            _target = target;
            StartMoving(origin, target.position);
        }

        private void Update()
        {
            bool closeEnough = _target != null && _target.Distance(transform) <= 0.5f;
            bool slowEnough = _rb2d.velocity.magnitude <= 0.25f;
            if (!closeEnough && !slowEnough) return;
            _instantDetonate = true;
            Detonate();
        }

        private void OnDestroy()
        {
            _grenadePool.Dispose(this);
        }

        private void Detonate()
        {
            CreateExplosion();
        }

        public void Deactivate()
        {
            _grenadePool.Return(this);
        }

        protected virtual void CreateExplosion()
        {
            Explosion explosion = Explosion.CreateExplosion(transform.position, _damage, _radius);
            explosion.AddOnDetonate(OnDetonate);
            if (_incendiary) explosion.SetBurn();
            if (_decaying) explosion.SetDecay();
            if (_sickening) explosion.SetSicken();
            if (_instantDetonate)
            {
                explosion.InstantDetonate();
                Deactivate();
            }
            else explosion.Detonate(this);
        }

        public void AddOnDetonate(Action<List<EnemyBehaviour>> action)
        {
            OnDetonate += action;
        }
    }
}