using System.Collections;
using Game.Combat.Misc;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace Game.Combat.Enemies.Misc
{
    public class Grenade : MonoBehaviour
    {
        private static readonly float MaxSpeed = 2f;
        private static readonly ObjectPool<Grenade> _grenadePool = new ObjectPool<Grenade>("Prefabs/Combat/Grenade");
        private readonly int _damage = 20;

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
            Explosion explosion = Explosion.CreateExplosion(transform.position, _damage);
            explosion.SetKnockbackDistance(5);
            explosion.Detonate();
        }
    }
}