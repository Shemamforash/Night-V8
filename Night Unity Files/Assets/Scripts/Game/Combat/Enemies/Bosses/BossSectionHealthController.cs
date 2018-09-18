﻿using Game.Combat.Generation;
using Game.Combat.Misc;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses
{
    public abstract class BossSectionHealthController : MonoBehaviour, ITakeDamageInterface
    {
        private int InitialHealth;
        protected readonly HealthController HealthController = new HealthController();
        private DamageSpriteFlash _spriteFlash;
        private Boss _boss;

        public virtual void Awake()
        {
            _spriteFlash = GetComponent<DamageSpriteFlash>();
            HealthController.SetInitialHealth(GetInitialHealth(), this);
            CombatManager.Enemies().Add(this);
        }

        protected abstract int GetInitialHealth();

        protected void SetBoss(Boss boss)
        {
            _boss = boss;
        }

        public virtual void Start()
        {
            _boss.RegisterSection(this);
        }

        protected virtual void TakeDamage(float damage)
        {
            _spriteFlash.FlashSprite();
            HealthController.TakeDamage(damage);
        }

        public void TakeShotDamage(Shot shot)
        {
            TakeDamage(shot.DamageDealt());
        }

        public void TakeRawDamage(float damage, Vector2 direction)
        {
            TakeDamage(damage);
        }

        public void TakeExplosionDamage(float damage, Vector2 origin)
        {
            TakeDamage(damage);
        }

        public void Decay()
        {
        }

        public void Burn()
        {
        }

        public void Sicken(int stacks = 0)
        {
        }

        public bool IsDead()
        {
            return HealthController.GetCurrentHealth() == 0;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public virtual void Kill()
        {
            Destroy(gameObject);
            CombatManager.Remove(this);
            _boss.UnregisterSection(this);
        }

        public void MyUpdate()
        {
        }
    }
}