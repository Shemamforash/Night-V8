﻿using UnityEngine;

namespace Game.Combat.Misc
{
    public interface ITakeDamageInterface
    {
        void TakeShotDamage(Shot shot);
        void TakeRawDamage(float damage, Vector2 direction);
        void TakeExplosionDamage(float damage, Vector2 origin);
        void Decay();
        void Burn();
        void Sicken(int stacks = 0);
        bool IsDead();
        void Kill();
    }
}