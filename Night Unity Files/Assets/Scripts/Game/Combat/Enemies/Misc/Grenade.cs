using System;
using System.Collections;
using System.Collections.Generic;
using Game.Combat.Misc;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Enemies.Misc
{
    public class Grenade : MonoBehaviour
    {
        private static readonly float MaxSpeed = 2f;
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
            float distance = Vector2.Distance(origin, target);
            float timeToReach = distance / MaxSpeed;
            float currentTime = 0f;
            while (currentTime < timeToReach)
            {
                float normalisedTime = currentTime / timeToReach;
                transform.position = Vector2.Lerp(origin, target, normalisedTime);
                currentTime += Time.deltaTime;
                yield return null;
            }

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
            if (_incendiary) FireBehaviour.Create(transform.position, 2);
            if (_decaying) DecayBehaviour.Create(transform.position);
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