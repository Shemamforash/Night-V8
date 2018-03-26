using System;
using System.Collections;
using System.Collections.Generic;
using Facilitating.UIControllers;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using TMPro;
using UnityEngine;

namespace Game.Combat.Enemies.EnemyTypes.Misc
{
    public class Grenade : MonoBehaviour
    {
        private int _damage = 20;
        private static float MaxSpeed = 2f;
        private static readonly ObjectPool<Grenade> _grenadePool = new ObjectPool<Grenade>("Prefabs/Combat/Grenade");

        public static void Create(Vector2 origin, Vector2 target)
        {
            Grenade grenade = _grenadePool.Create();
            grenade.StartMoving(origin, target);
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
            _grenadePool.Return(this);
        }

        protected virtual void CreateExplosion()
        {
            Explosion explosion = Explosion.CreateExplosion(transform.position, 5, _damage);
            explosion.SetKnockbackDistance(5);
            explosion.Detonate();
        }
    }
}