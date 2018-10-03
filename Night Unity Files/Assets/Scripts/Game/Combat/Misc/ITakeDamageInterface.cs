using UnityEngine;

namespace Game.Combat.Misc
{
    public interface ITakeDamageInterface
    {
        GameObject GetGameObject();
        void TakeShotDamage(Shot shot);
        void TakeRawDamage(float damage, Vector2 direction);
        void TakeExplosionDamage(float damage, Vector2 origin, float radius);
        void Decay();
        void Burn();
        void Sicken(int stacks = 1);
        bool IsDead();
        void Kill();
        void MyUpdate();
    }
}