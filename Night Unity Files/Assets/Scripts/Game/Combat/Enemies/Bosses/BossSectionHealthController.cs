using Game.Combat.Generation;
using Game.Combat.Misc;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses
{
    public abstract class BossSectionHealthController : MonoBehaviour, ITakeDamageInterface
    {
        private int InitialHealth;
        private readonly HealthController _healthController = new HealthController();
        private DamageSpriteFlash _spriteFlash;
        private Boss _boss;

        public virtual void Awake()
        {
            _spriteFlash = GetComponent<DamageSpriteFlash>();
            _healthController.SetInitialHealth(GetInitialHealth(), this);
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

        private void TakeDamage(float damage)
        {
            _spriteFlash.FlashSprite();
            _healthController.TakeDamage(damage);
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
            return _healthController.GetCurrentHealth() == 0;
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
    }
}