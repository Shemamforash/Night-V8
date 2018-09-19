using System;
using System.Collections;
using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Enemies.Misc
{
    public class Grenade : MonoBehaviour
    {
        private static readonly float ThrowForce = 5f;
        private static readonly ObjectPool<Grenade> _grenadePool = new ObjectPool<Grenade>("Grenades", "Prefabs/Combat/Enemies/Grenade");
        private readonly int _damage = 20;
        private Action<List<EnemyBehaviour>> OnDetonate;
        private bool _incendiary, _decaying;
        private bool _instantDetonate;
        private float _radius = 1;

        public static Grenade CreateBasic(Vector2 origin, Vector2 target, bool instantDetonate = false)
        {
            Grenade grenade = _grenadePool.Create();
            grenade._instantDetonate = instantDetonate;
            grenade.StartMoving(origin, target);
            return grenade;
        }

        public void SetExplosionRadius(float radius)
        {
            _radius = radius;
        }

        public static Grenade CreateIncendiary(Vector2 origin, Vector2 target,bool instantDetonate = false)
        {
            Grenade g = CreateBasic(origin, target, instantDetonate);
            g._incendiary = true;
            return g;
        }

        public static Grenade CreateDecay(Vector2 origin, Vector2 target,bool instantDetonate = false)
        {
            Grenade g = CreateBasic(origin, target, instantDetonate);
            g._decaying = true;
            return g;
        }

        private void StartMoving(Vector2 origin, Vector2 target)
        {
            StartCoroutine(MoveToPosition(origin, target));
        }

        private IEnumerator MoveToPosition(Vector2 origin, Vector2 target)
        {
            Vector2 direction = target - origin;
            float distance = direction.magnitude;
            direction.Normalize();
            Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
            rb2d.AddForce(direction * distance * ThrowForce);
            while (rb2d.velocity.magnitude > 0.001f) yield return null;
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
            if (_incendiary) explosion.SetIncendiary();
            if (_decaying) explosion.SetDecay();
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